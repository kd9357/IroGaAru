using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kechibi : Enemy
{

    [Tooltip("The minimum distance above the player flying dude will try to reach (0: player center)")]
    public float MinDistanceAbovePlayer;
    [Tooltip("The maximum distance above the player flying dude will try to reach (0: player center)")]
    public float MaxDistanceAbovePlayer;
    [Tooltip("Projectile to be launched by Firebird")]
    public GameObject Projectile;

    #region Helper Methods

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

    //Constantly move towards player until initiating attack
    protected override void TrackPlayer()
    {
        if (!FacingTarget())
        {
            Flip();
        }
        if (!InRange())
            MoveTowardsPlayer();
        _xMov = 0;
        _yMov = 0;
        if (InRange())
        {
            Attack();
        }
    }

    //Move until hit wall
    protected override void LeftRight()
    {
        MoveForward();
    }

    protected override void Stationary()
    {
        if (!FacingTarget())
            Flip();
        if (InRange())
        {
            Attack();
        }
    }

    //Determines Enemy is in zone above player
    protected override bool InRange()
    {
        //Shouldn't hardcode values but oh well
        bool xInRange = Mathf.Abs(_target.position.x - transform.position.x) < AttackRange;
        bool yInRange = Mathf.Abs((_target.position.y + 3f) - transform.position.y) < AttackRange / 4;
        return xInRange && yInRange;
    }

    protected override void Attack()
    {
        base.Attack();
        //Vector3 direction = _facingRight ? Vector2.right : Vector2.left;
        Vector3 direction = _target.position - transform.position;
        //Spawn projectile
        GameObject projectile = Instantiate(Projectile, transform.position, transform.rotation);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript == null)
            Debug.LogError("Prefab does not have projectile script");

        projectileScript.Launch(direction.normalized);
    }

    //Also apply gravity on enemy
    protected override void ApplyStun()
    {
        base.ApplyStun();
        gameObject.layer = 11; //Set to Enemy (Should make const)
        _rb.gravityScale = 30;
    }

    protected override void ResetColorStatus()
    {
        if (_colorTimer < 0 && _currentColor != DefaultColor)
        {
            _colorTimer = 0;
            _currentColor = DefaultColor;
            _sprite.color = _currentColor;
            //Reset ailments
            _currentSpeed = Speed;
            _currentKnockbackForce = EnemyKnockbackForce;
            _rb.gravityScale = 0;
            gameObject.layer = 14;  //Set to Transient Enemy
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
}
