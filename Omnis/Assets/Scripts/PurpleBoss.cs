﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurpleBoss : Enemy {
    //TODO: need to do a lot of fixes to account for scaling + flipping

    //Components
    private BoxCollider2D _slapBox;

    //Combat variables
    private int _phaseNum;
    private float _defaultMass;
    private Vector3 _defaultScale;

    // Use this for initialization
    protected override void Start()
    {
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();

        _currentHealth = MaxHealth;
        _currentColor = DefaultColor;
        _sprite.color = _currentColor;
        _currentSpeed = Speed;
        _currentKnockbackForce = EnemyKnockbackForce;
        _recoilTimer = 0;
        _attacking = false;
        _actionTimer = ActionCooldown;  //May set this only when player in range

        //For testing purposes
        _textMesh = gameObject.GetComponentInChildren<TextMesh>();

        _colorParticleEffects = gameObject.GetComponentsInChildren<ParticleSystem>();

        //Unique to boss
        _phaseNum = 0;
        _defaultMass = _rb.mass;
        _defaultScale = transform.localScale;

        _slapBox = gameObject.GetComponent<BoxCollider2D>();
        _slapBox.enabled = false;
    }

    #region Updates
    private void FixedUpdate()
    {
        //This unfortunately flips the debug text as well
        if ((_target.transform.position.x - transform.position.x > 0 && !_facingRight)
            || (_target.transform.position.x - transform.position.x < 0 && _facingRight))
            Flip();

        if (_recoilTimer <= 0)
        {
            if (_actionTimer > 0)
                _actionTimer -= Time.deltaTime;
            else if (InRange() && !_attacking) //Need to account for changing size
            {
                _rb.velocity = Vector2.zero;
                Attack();
            }
            else if(!_attacking)
                MoveForward();
        }
    }

    #endregion

    #region Helper Methods

    private void EnableHurtbox()
    {
        _slapBox.enabled = true;
    }

    protected override void EndAttack()
    {
        _slapBox.enabled = false;
        _attacking = false;
        _actionTimer = ActionCooldown;
    }

    public override void EnemyDamaged(int damage, Color color, int direction)
    {
        //Only set the timer on first hit
        if (_colorTimer == 0)
            _colorTimer = ColorCooldown;
        //Unique to boss: do not decrease health or be staggered

        SetColor(color);
        _rb.AddForce(Vector2.right * direction * _currentKnockbackForce, ForceMode2D.Impulse);
    }

    //Combine colors and determine if status changed
    protected override void SetColor(Color color)
    {
        //Only allow color mixing when NOT under some ailment
        if (_currentStatus == ColorStatus.None)
        {
            _currentColor = (_currentColor + color) / 2;

            // Now determine other special effects
            Vector3 cc = new Vector3(_currentColor.r, _currentColor.g, _currentColor.b);
            Vector3 sc = Vector3.zero;

            int i;
            float threshold = 0.34f;
            //Unique to boss: Ignore i = 0, purple
            for (i = 1; i < SpecialColors.Count; i++)
            {
                sc.Set(SpecialColors[i].r, SpecialColors[i].g, SpecialColors[i].b);
                float distance = Vector3.Distance(sc, cc);
                if (distance < threshold)
                    break;
            }
            _currentStatus = (ColorStatus)(i);
            if (_currentStatus != ColorStatus.None)
                ApplyAilment();

            _sprite.color = _currentColor;
        }
    }

    //Unique to boss: Ignore purple effect
    protected override void ApplyAilment()
    {
        //When special color first applied, reset timer
        _colorTimer = ColorCooldown;
        if (!_colorParticleEffects[(int)_currentStatus].isPlaying)
            _colorParticleEffects[(int)_currentStatus].Play();
        switch (_currentStatus)
        {
            case ColorStatus.WindRecoil:
                _currentKnockbackForce *= _currentKnockbackForce;
                Debug.Log("Force: " + _currentKnockbackForce);
                //For now, knockback ^ 2 for boss
                //May change this to a one time hit
                //Don't mess with mass since we want the boss to be variably affected due to his increased mass
                _currentColor = Color.green;
                return;
            case ColorStatus.DamageOverTime:
                //Instead of damage, shrink to some minimum value
                StartCoroutine(ShrinkOverTime());
                return;
        }
    }

    //Maybe find a better method than using coroutines
    #region Coroutines
    IEnumerator ShrinkOverTime()
    {
        float timer = 0;
        float startMass = _rb.mass;
        Vector3 startScale = transform.localScale;

        while (_currentStatus == ColorStatus.DamageOverTime && startMass != _defaultMass) //TODO: Fix _facingRight scale bug when jumping over during shrinking
        {
            float proportionCompleted = timer / ColorCooldown;
            _rb.mass = Mathf.Lerp(startMass, _defaultMass, proportionCompleted);
            //Need to account for flip while shrinking (not good enough)
            if((startScale.x > 0 && _facingRight) || (startScale.x < 0 && !_facingRight))
                startScale.x *= -1;
            //Shrinking doesn't change location, so each time this calls the boss drops a bit
            //Basically it looks really shaky
            transform.localScale = Vector3.Lerp(startScale, _defaultScale, proportionCompleted);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    //When shocked, jump back to middle of arena and grow in size
    IEnumerator StartNextPhase()
    {
        //Should change color to purple?
        _anim.SetBool("Recoil", true);
        yield return new WaitForSeconds(1.5f);  //Wait 1.5 seconds until jumping back (should make public or something)
        _anim.SetBool("Recoil", false);
        //Reset status
        _currentStatus = ColorStatus.None;
        _currentColor = DefaultColor;
        _sprite.color = _currentColor;
        //TODO: Need to jump back into arena
        //For now just move directly above
        transform.position = new Vector3(10, 40, 0);
        //scale size
        yield return new WaitForSeconds(1.5f); //Wait 1.5 seconds until grow
        StartCoroutine(GrowOverTime());
    }

    IEnumerator GrowOverTime()
    {
        _anim.SetBool("Growing", true);
        float timer = 0;
        float endMass = _defaultMass * 10;
        Vector3 endScale = new Vector3(transform.localScale.x * 2, transform.localScale.y * 2, 1);
        //For now, just grow to twice size, 10 times mass over 5 seconds
        while (timer < 3f)
        {
            float proportionCompleted = timer / 5f;
            _rb.mass = Mathf.Lerp(_defaultMass, endMass, proportionCompleted);
            //Need to account for flip while shrinking
            if ((endScale.x > 0 && _facingRight) || (endScale.x < 0 && !_facingRight))
                endScale.x *= -1;
            transform.localScale = Vector3.Lerp(_defaultScale, endScale, proportionCompleted);
            timer += Time.deltaTime;
            yield return null;
        }
        _anim.SetBool("Growing", false);
    }
    #endregion

    #endregion

    #region Collisions
    // DON'T Hurt player on contact
    //Only care about water and knockback
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        //Check if enter water
        if(collision.gameObject.CompareTag("Hazard") && _recoilTimer <= 0)
        {
            _currentHealth--;
            _phaseNum++;
            _recoilTimer = RecoilCooldown;
            StartCoroutine(StartNextPhase());
        }
    }

    //Right now just override enemy's method
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        return;
    }

    //Slap connected
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().Knockback(!_facingRight);
        }
    }
    #endregion


}