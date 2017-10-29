using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spearfish : Enemy {

    public float AttackSpeed = 1f;

    private Vector3 _initialPosition;
    private float _distanceToAttack;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (_currentState == EnemyState.Attacking)
        {
            if (_actionTimer > 0)
                _actionTimer -= Time.deltaTime;
            else
                Charge();
        }
    }

    protected override void LeftRight()
    {
        if (_actionTimer > 0)
            return;
        //If not in range OR not facing target OR in range but not on similar y planes
        if (!InRange() || !FacingTarget() || (InRange() && Mathf.Abs(_target.position.y - transform.position.y) > 1))
            MoveForward();
        else if(FacingTarget())
        {
            //Set target location
            _initialPosition = transform.position;
            _distanceToAttack = Vector3.Distance(_target.position, transform.position);
            _actionTimer = 1;
            Attack();
        }

        if (_distanceTraveled >= PatrolDistance)
        {
            Flip();
        }
    }

    private void Charge()
    {
        if (Mathf.Abs(transform.position.x - _initialPosition.x) >= _distanceToAttack)
        {
            EndAttack();
            _anim.SetTrigger("EndAttack");
        }
        else
        {
            _xMov += _facingRight ? _currentSpeed * AttackSpeed : -_currentSpeed * AttackSpeed;
            _yMov = _rb.velocity.y;

            _xMov = Mathf.Clamp(_xMov, -_currentSpeed * AttackSpeed, _currentSpeed * AttackSpeed);
            //_yMov = Mathf.Clamp(_yMov, -_currentSpeed, _currentSpeed);

            _distanceTraveled += Mathf.Abs(transform.position.x - _lastPos);
            _lastPos = transform.position.x;

            _rb.velocity = new Vector2(_xMov, _yMov);
        }
    }

    // When the enemy gets hit by something
    public override bool EnemyDamaged(int damage, Color color, int direction,
                                     int additionalForce = 1)
    {
        if (color == GameController.Instance.GetColor(ColorOutline))
            return false;

        //Only set the timer on first hit
        if (_colorTimer <= 0)
            _colorTimer = ColorCooldown;

        //For fish, only be staggered when not charging
        if(_currentState != EnemyState.Attacking
            || (_currentState == EnemyState.Attacking && _actionTimer > 0))
        {
            //Only mess with recoil when not stunned already
            if (_currentColorStatus != ColorStatus.Stun)
                _recoilTimer = RecoilCooldown;

            _anim.SetBool("Recoil", true);
            _currentState = EnemyState.Staggered;
        }
        
        _currentHealth -= damage;

        SetColor(color);

        GameController.Instance.IncrementAttacksConnected();

        if (_currentState != EnemyState.Attacking)
            _rb.AddForce(Vector2.right * direction * _currentKnockbackForce * additionalForce,
                         ForceMode2D.Impulse);
        return true;
    }
}
