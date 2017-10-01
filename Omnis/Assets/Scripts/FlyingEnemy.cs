using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy {

    [Tooltip("The minimum distance above the player flying dude will try to reach (0: player center)")]
    public float MinDistanceAbovePlayer;
    [Tooltip("The maximum distance above the player flying dude will try to reach (0: player center)")]
    public float MaxDistanceAbovePlayer;
    private float _a;
    private float _h;
    private float _k;
    private float _distanceX;

    void FixedUpdate()
    {
        if (_currentState == EnemyState.Inactive)
            return;

        if (_currentState != EnemyState.Staggered)
        {
            //This unfortunately flips the debug text as well
            if ((_target.position.x - transform.position.x > 0 && !_facingRight)
                || (_target.position.x - transform.position.x < 0 && _facingRight))
                Flip();

            //Constantly move towards player until in range
            //Should use lerp for more smooth stop
            if (_currentState != EnemyState.Attacking)
            {
                if (!InRange())
                    MoveTowardsPlayer();
                else
                {
                    if (_currentState != EnemyState.Waiting)
                        _currentState = EnemyState.Waiting;
                    _rb.velocity = new Vector2(_xMov, _yMov);
                }
                _xMov = 0;
                _yMov = 0;
            }

            if (_actionTimer > 0)
                _actionTimer -= Time.deltaTime;
            else
            {
                if(_currentState != EnemyState.Attacking && InRange())
                {
                    SetParabolaEquation();
                    _currentState = EnemyState.Attacking;
                }
            }

            if(_currentState == EnemyState.Attacking)
            {
                MoveInArc();
                //when traveled length of parabola && outside of range stop
                if (Mathf.Abs(transform.position.x - _h) >= _distanceX && !InRange())
                {
                    _currentState = EnemyState.Waiting;
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
        _h = _target.position.x;
        _k = _target.position.y;

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

    //Move to the zone above the player
    protected void MoveTowardsPlayer()
    {
        if (_currentState != EnemyState.Moving)
            _currentState = EnemyState.Moving;
        _xMov += _facingRight ? _currentSpeed : -_currentSpeed;
        //Move to some zone around player
        if (transform.position.y < _target.position.y + MinDistanceAbovePlayer)
            _yMov += _currentSpeed;
        else if (transform.position.y > _target.position.y + MaxDistanceAbovePlayer)
            _yMov += -_currentSpeed;
        _xMov = Mathf.Clamp(_xMov, -_currentSpeed, _currentSpeed);
        _yMov = Mathf.Clamp(_yMov, -_currentSpeed, _currentSpeed);
        _rb.velocity = new Vector2(_xMov, _yMov);
    }
    #endregion

    #region Overrides

    //Determines Enemy is in zone above player
    protected override bool InRange()
    {
        //Shouldn't hardcode values but oh well
        bool xInRange = Mathf.Abs(_target.position.x - transform.position.x) < AttackRange;
        bool yInRange = Mathf.Abs((_target.position.y + 3f) - transform.position.y) < AttackRange / 4;
        return xInRange && yInRange;
    }

    public override void EnemyDamaged(int damage, Color color, int direction,
                                      int additionalForce = 1)
    {
        //Only set the timer on first hit
        if (_colorTimer == 0)
            _colorTimer = ColorCooldown;
        _recoilTimer = RecoilCooldown;
        //When hit stop attacking
        _currentState = EnemyState.Staggered;
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
        if (!_colorParticleEffects[(int)_currentColorStatus].isPlaying)
            _colorParticleEffects[(int)_currentColorStatus].Play();
        switch (_currentColorStatus)
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
                _currentKnockbackForce = WindKnockbackForce;
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
            _currentColorStatus = ColorStatus.None;
            _currentState = EnemyState.Waiting;
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
        switch (collision.tag)
        {
            case "Player":
                var player = collision.gameObject.GetComponent<Player>();
                if (player.IsInvincible())
                    return;

                player.PlayerDamaged(TouchDamage);
                player.Knockback(collision.transform.position.x < transform.position.x);

                // Enemy hit sound
                _audioSource.clip = EnemySoundEffects[0];
                _audioSource.Play();
                break;
        }
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Player":
                var player = collision.gameObject.GetComponent<Player>();
                if (player.IsInvincible())
                    return;

                player.PlayerDamaged(TouchDamage);
                player.Knockback(collision.transform.position.x < transform.position.x);

                // Enemy hit sound
                _audioSource.clip = EnemySoundEffects[0];
                _audioSource.Play();
                break;
            case "Active Zone":
                float dist = Vector2.Distance(transform.position, collision.transform.position);
                if (dist < AvoidanceDistance)
                {
                    Vector2 dir = transform.position - collision.transform.position;
                    _xMov += Mathf.Lerp(dir.x, 0, dist / AvoidanceDistance);
                    _yMov += Mathf.Lerp(dir.y, 0, dist / AvoidanceDistance);
                }
                break;

        }
    }

    #endregion
}
