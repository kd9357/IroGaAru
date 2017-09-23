using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy {

    private float _a;
    private float _h;
    private float _k;
    private float _distanceX;

    void FixedUpdate()
    {
        //This unfortunately flips the debug text as well
        if ((Target.position.x - transform.position.x > 0 && !_facingRight)
            || (Target.position.x - transform.position.x < 0 && _facingRight))
            Flip();

        if (_recoilTimer <= 0)
        {
            //Constantly move towards player until in range
            //Should use lerp for more smooth stop
            if(!InRange())
            {
                float xMov = _facingRight ? _currentSpeed : -_currentSpeed;
                float yMov = 0;
                //Zone above player with 2-3 units
                if (transform.position.y < Target.position.y + 2f)
                    yMov = _currentSpeed;
                else if (transform.position.y > Target.position.y + 3f)
                    yMov = -_currentSpeed;
                _rb.velocity = new Vector2(xMov, yMov);
            }
            else
                _rb.velocity = Vector2.zero;

            if (_actionTimer > 0)
                _actionTimer -= Time.deltaTime;
            else
            {
                if (!_attacking && InRange())  //Set up arc
                {
                    SetParabolaEquation();
                    _attacking = true;
                }
            }

            if(_attacking)
            {
                MoveInArc();
                //when traveled length of parabola && outside of range stop
                if (Mathf.Abs(transform.position.x - _h) >= _distanceX && !InRange())
                {
                    _attacking = false;
                    _actionTimer = ActionCooldown;
                    //Possibly add a wait delay
                }
            }

        }
    }

    #region Helper Methods

    protected void SetParabolaEquation()
    {
        // y = a(x - h)^2 + k
        //where (h,k) is vertex, (x,y) is enemy current position
        // a = (y-k) / (x-h)^2
        _h = Target.position.x;
        _k = Target.position.y;

        _distanceX = Mathf.Abs(transform.position.x - _h);

        _a = (transform.position.y - _k) / Mathf.Pow((transform.position.x - _h), 2);
    } 

    //Parabolic movement
    protected void MoveInArc()
    {
        //Need to account for distance
        float xPos = _facingRight ? transform.position.x + _currentSpeed * Time.deltaTime * _distanceX
                                  : transform.position.x - _currentSpeed * Time.deltaTime * _distanceX;
        float yPos = _a * Mathf.Pow(xPos - _h, 2) + _k;
        transform.position = new Vector2(xPos, yPos);
    }

    #endregion

    #region Overrides

    //Determines Enemy is in zone above player
    protected override bool InRange()
    {
        bool xInRange = Mathf.Abs(Target.position.x - transform.position.x) < AttackRange;
        bool yInRange = Mathf.Abs((Target.position.y + 2f) - transform.position.y) < AttackRange / 2;
        return xInRange && yInRange;
    }

    public override void EnemyDamaged(int damage, Color color, int direction)
    {
        //Only set the timer on first hit
        if (_colorTimer == 0)
            _colorTimer = ColorCooldown;
        _recoilTimer = RecoilCooldown;
        //When hit stop attacking
        _attacking = false;
        _actionTimer = ActionCooldown;
        _anim.SetBool("Recoil", true);
        _currentHealth -= damage;

        SetColor(color);
        _rb.AddForce(Vector2.right * direction * _currentKnockbackForce, ForceMode2D.Impulse);
    }

    //TODO: add sound effect if using
    protected override void ApplyAilment()
    {
        //When special color first applied, reset timer
        _colorTimer = ColorCooldown;
        if (!_colorParticleEffects[(int)_currentStatus].isPlaying)
            _colorParticleEffects[(int)_currentStatus].Play();
        switch (_currentStatus)
        {
            case ColorStatus.Stun:
                _currentSpeed = 0;
                _recoilTimer = ColorCooldown;
                _anim.SetBool("Recoil", true);
                //Enemy should fall and collide with objects
                _rb.gravityScale = 30;
                gameObject.GetComponent<Collider2D>().isTrigger = false;
                return;
            case ColorStatus.WindRecoil:
                _currentKnockbackForce *= 2;    //For now, just double on normal enemies
                _currentColor = Color.green;
                return;
            case ColorStatus.DamageOverTime:
                StartCoroutine(DamageOverTime());
                return;
        }
    }

    protected override void ResetColorStatus()
    {
        if (_colorTimer < 0)
        {
            _colorTimer = 0;
            _currentColor = DefaultColor;
            _sprite.color = _currentColor;
            //Reset ailments
            _currentSpeed = Speed;
            _currentKnockbackForce = EnemyKnockbackForce;
            _rb.gravityScale = 0;
            gameObject.GetComponent<Collider2D>().isTrigger = true;
            _currentStatus = ColorStatus.None;
            foreach (ParticleSystem ps in _colorParticleEffects)
            {
                if (ps.isPlaying)
                    ps.Stop();
            }
        }
    }

    #endregion

    #region Collisions

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player.IsInvincible())
                return;

            player.PlayerDamaged(TouchDamage);
            player.Knockback(collision.transform.position.x < transform.position.x);

            // Enemy hit sound
            _audioSource.clip = EnemySoundEffects[0];
            _audioSource.Play();
        }
    }

    protected void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player.IsInvincible())
                return;

            player.PlayerDamaged(TouchDamage);
            player.Knockback(collision.transform.position.x < transform.position.x);

            // Enemy hit sound
            _audioSource.clip = EnemySoundEffects[0];
            _audioSource.Play();
        }
    }

    #endregion
}
