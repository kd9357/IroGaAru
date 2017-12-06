using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kappa : Enemy {

    [Tooltip("The height this enemy jumps straight up")]
    public float Jump = 60f;

    protected bool _onGround;

    protected override void TrackPlayer()
    {
        if (!FacingTarget())
            Flip();
        if (InRange())
        {
            _rb.velocity = CalculateArcVelocity();
            Attack();
        }
    }

    protected override void Stationary()
    {
        if (InRange())
        {
            _rb.AddForce(Vector2.up * Jump, ForceMode2D.Impulse);
            Attack();
        }
    }

    //Modifed from: http://answers.unity3d.com/questions/148399/shooting-a-cannonball.html
    protected Vector3 CalculateArcVelocity()
    {
        Vector3 dir = _target.position - transform.position;
        float h = dir.y;
        dir.y = 0;
        float dist = dir.magnitude;
        float a = Mathf.Lerp(85, 70, dist / AttackRange) * Mathf.Deg2Rad;
        dir.y = dist * Mathf.Tan(a);
        dist += h / Mathf.Tan(a);
        float vel = Mathf.Sqrt(dist * (Physics.gravity.magnitude * _rb.gravityScale) / Mathf.Sin(2 * a));
        if (float.IsNaN(vel))
            vel = 0;
        return vel * dir.normalized * 1.4f;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            case "Player":
                var player = collision.gameObject.GetComponent<Player>();
                if (player == null)
                    Debug.LogError("Player script doesn't exist!");
                if (player.IsInvincible() || _currentColorStatus == ColorStatus.Stun)
                    return;

                player.PlayerDamaged(TouchDamage);
                if (TouchDamage > 0)
                    player.Knockback(collision.transform.position.x < transform.position.x);

                //Push Kappa off of player if attacking
                if (_currentState == EnemyState.Attacking)
                {
                    Vector3 dir = transform.position - _target.position;
                    _rb.AddForce(dir.normalized * WindKnockbackForce, ForceMode2D.Impulse);
                }

                // Enemy hit sound
                _audioSource.clip = EnemySoundEffects[0];
                _audioSource.Play();
                break;
            case "Wall":
                if (EnemyBehavior == Behavior.LeftRight)
                {
                    _currentState = EnemyState.Waiting;
                    Flip();
                }
                break;
            case "Instant Death":
            case "Spikes":
                Die();
                break;
            default:
                break;
        }
        //Detect if grounded
        if (collision.otherCollider is CircleCollider2D)
        {
            _onGround = true;
            _anim.SetBool("Grounded", _onGround);
        }
        //Set land animation parameter
        //land animation plays, calls EndAttack() at end of animation
        //temporarily set now:
        EndAttack();
    }

    //Ignore water instadeath
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            case "Player":
                var player = collision.gameObject.GetComponent<Player>();
                if (player == null)
                    Debug.LogError("Player script doesn't exist!");
                if (player.IsInvincible() || _currentColorStatus == ColorStatus.Stun)
                    return;

                player.PlayerDamaged(TouchDamage);
                if (TouchDamage > 0)
                    player.Knockback(collision.transform.position.x < transform.position.x);

                // Enemy hit sound
                _audioSource.clip = EnemySoundEffects[0];
                _audioSource.Play();
                break;
            case "Instant Death":
            case "Spikes":
                Die();
                break;
            default:
                break;
        }
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.otherCollider is CircleCollider2D)
        {
            _onGround = false;
            _anim.SetBool("Grounded", _onGround);
        }
    }
}
