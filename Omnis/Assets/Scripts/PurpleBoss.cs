using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurpleBoss : Enemy {

    //Use this to know the player's location
    //May instead use some static variable?
    public GameObject Player;

    [Tooltip("Amount of time until enemy makes an action")]
    public float ActionCooldown;

    //Components
    private BoxCollider2D _slapBox;

    //Combat variables
    protected float _actionTimer;
    private int _phaseNum;
    private float _defaultMass;
    private float _attackCooldown = 0.3f;
    private float _attackTimer;

    //Movement and Orientation
    private bool _facingRight;


    // Use this for initialization
    protected override void Start()
    {
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();

        _currentHealth = MaxHealth;
        _currentColor = DefaultColor;
        _currentSpeed = Speed;
        _currentKnockbackForce = EnemyKnockbackForce;
        _recoilTimer = 0;
        _actionTimer = ActionCooldown;  //Maybe set this zero until enemy is ready

        _phaseNum = 0;
        _defaultMass = _rb.mass;
        _attackTimer = 0;

        _slapBox = gameObject.GetComponent<BoxCollider2D>();
        _slapBox.enabled = false;

        //For testing purposes
        _textMesh = gameObject.GetComponentInChildren<TextMesh>();
    }

    #region Updates
    //Will want to use this kind of AI on generic enemies
    private void FixedUpdate()
    {
        //This unfortunately flips the debug text as well
        if ((Player.transform.position.x - transform.position.x > 0 && !_facingRight) 
            || (Player.transform.position.x - transform.position.x < 0 && _facingRight))
            Flip();

        if (_attackTimer < 0)
        {
            _slapBox.enabled = false;
            _attackTimer = 0;
        }
        else
            _attackTimer -= Time.deltaTime;

        if (_recoilTimer <= 0)
        {
            if (_actionTimer > 0)
            {
                _actionTimer -= Time.deltaTime;
            }
            if (_actionTimer <= 0)
            {
                //Move to player
                //If in range, attack
                SlapAttack();
            }
        }
    }
    #endregion

    #region Helper Methods

    private void SlapAttack()
    {
        
        //Enable hurtbox
        _slapBox.enabled = true;
        _attackTimer = _attackCooldown;
        _actionTimer = ActionCooldown;
    }

    public override void EnemyDamaged(int damage, Color color, int direction)
    {
        //Only set the timer on first hit
        if (_colorTimer == 0)
            _colorTimer = ColorCooldown;

        //Only allow color mixing when not under some ailment
        if (_currentStatus == ColorStatus.None)
        {
            _currentColor = (_currentColor + color) / 2;

            // Now determine other special effects
            Vector3 cc = new Vector3(_currentColor.r, _currentColor.g, _currentColor.b);
            Vector3 sc = Vector3.zero;

            //ignore i == 0, purple
            int i;
            float threshold = 0.34f;
            for (i = 1; i < SpecialColors.Count; i++)
            {
                sc.Set(SpecialColors[i].r, SpecialColors[i].g, SpecialColors[i].b);
                float distance = Vector3.Distance(sc, cc);
                if (distance < threshold)
                {
                    Debug.Log("Applying " + (ColorStatus)(i));
                    break;
                }

            }
            _currentStatus = (ColorStatus)(i);
            if (_currentStatus != ColorStatus.None)
                ApplyAilment();
        }

        _sprite.color = _currentColor;
        _rb.AddForce(Vector2.right * direction * _currentKnockbackForce, ForceMode2D.Impulse);
    }

    //Ignore purple effect
    protected override void ApplyAilment()
    {
        //When special color first applied, reset timer
        _colorTimer = ColorCooldown;
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
                //StartCoroutine();
                return;
        }
    }

    void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    #endregion

    #region Collisions
    // DON'T Hurt player on contact
    //Only care about water and knockback
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        //Check if enter water
        if(collision.gameObject.CompareTag("Hazard"))
        {
            _currentHealth--;
            _phaseNum++;
            _recoilTimer = RecoilCooldown;
        }
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Slap connected with " + collision.tag);
    }
    #endregion


}
