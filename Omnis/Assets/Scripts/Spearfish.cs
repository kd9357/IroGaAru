using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spearfish : Enemy {

    private Vector3 _initialPosition;
    private float _distanceToTravel;

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
        if(!InRange() || !FacingTarget())
            MoveForward();
        else if(FacingTarget())
        {
            //Set target location
            _initialPosition = transform.position;
            _distanceToTravel = Vector3.Distance(_target.position, transform.position);
            _actionTimer = 1;
            Attack();
        }
    }

    private void Charge()
    {
        if (Mathf.Abs(transform.position.x - _initialPosition.x) >= _distanceToTravel + 2)
            EndAttack();
        else
        {
            _xMov += _facingRight ? _currentSpeed * 2 : -_currentSpeed * 2;
            _yMov = _rb.velocity.y;

            _xMov = Mathf.Clamp(_xMov, -_currentSpeed * 2, _currentSpeed * 2);
            //_yMov = Mathf.Clamp(_yMov, -_currentSpeed, _currentSpeed);
            _rb.velocity = new Vector2(_xMov, _yMov);
        }
    }

    // When the enemy gets hit by something
    public override void EnemyDamaged(int damage, Color color, int direction,
                                     int additionalForce = 1)
    {
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
    }
}
