using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour {

    // Constants
    private const int PASS_THROUGH_PLATFORMS = 13;
    private const int PLAYER_LAYER = 12;

    // Invincibility Mode -- For Testing!
    public bool GodMode = false;

    // Demo Vars -- will get rid of once we decide on platforming physics
    public bool Slide;
    public bool StaticJump;

    // Health vars
    public int MaxHealth;
    public float InvincibilityCooldown = 3f;

    // Movement parameters
    public float Speed;
    public float Jump;

    // Recoil parameters
    [Tooltip("Force player experiences from enemy")]
    public float PlayerKnockbackForce = 20;
    public float KnockbackCooldown = 0.3f;

    // Components on player
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    private PolygonCollider2D _footCollider;
    private Animator _anim;

    // Flags
    private bool _onGround;
    private bool _onWall;
    private bool _jumping;
    private bool _jumpCancel;
    private bool _facingRight;
    private bool _hit;
    private bool _knockFromRight;
    private bool _invincible;

    // Timers
    private float _knockbackTimer;
    private float _invinceTimer;

    // Health
    private int _currentHealth;


    // Use this for initialization
    void Start ()
    {
        _sprite = GetComponent<SpriteRenderer>();
	    _rb = GetComponent<Rigidbody2D>();
	    _footCollider = GetComponent<PolygonCollider2D>();
        _anim = GetComponent<Animator>();

        _onGround = true;
        _onWall = false;
        _jumping = false;
        _jumpCancel = false;
        _facingRight = true;
        _hit = false;
        _knockFromRight = false;
        _invincible = false;

        _knockbackTimer = 0f;
        _invinceTimer = 0f;

	    _currentHealth = MaxHealth;
	}

    #region Updates

    // Fixed Update runs once per physics tick
    void FixedUpdate()
    {
        // MOVEMENT
        // If knockback over, resume movement
        if(_knockbackTimer <= 0)
        { 
            float x_mov = Slide ? Input.GetAxis("Horizontal") * Speed :
                                  Input.GetAxisRaw("Horizontal") * Speed;
            float y_mov = _rb.velocity.y;

            x_mov = _onGround || (!_onGround && !_onWall) ? x_mov : _rb.velocity.x;

            if ((x_mov > 0 && !_facingRight) || (x_mov < 0 && _facingRight))
                Flip();

            if(_jumping)
            {
                y_mov = Jump;
                _jumping = false;
            }

            if(!StaticJump)
            {
                if (_jumpCancel && _rb.velocity.y > 0)
                {
                    y_mov = Mathf.Lerp(_rb.velocity.y, 0, 0.8f);
                }
            }

            _rb.velocity = new Vector2(x_mov, y_mov);
        }
        else
        {
            _knockbackTimer -= Time.deltaTime;
            if(_hit)
            {
                if(_knockFromRight)
                {
                    _rb.AddForce(new Vector2(-PlayerKnockbackForce, PlayerKnockbackForce), ForceMode2D.Impulse);
                }
                else
                {
                    _rb.AddForce(new Vector2(PlayerKnockbackForce, PlayerKnockbackForce), ForceMode2D.Impulse);
                }
                _hit = false;
            }
        }

        _anim.SetFloat("Horizontal Speed", _rb.velocity.x);
        _anim.SetFloat("Vertical Speed", _rb.velocity.y);
    }

    // Update runs once per frame
    private void Update()
    {
        // JUMPING
        // HACK: This is a little gacky but I swear it feels more responsive and idk why
        if (Input.GetButtonDown("Jump") && _onGround)
        {
            _jumping = true;
        }

        _jumpCancel = Input.GetButtonUp("Jump");

        // HEALTH
        if (_currentHealth <= 0)
        {
            //Initiate Game Over
            //GameController.instance.GameOver();

            //For now just destroy game object (will want to move this to game over function)
            Destroy(gameObject);
        }

        if (_invinceTimer > 0)
        {
            _invinceTimer -= Time.deltaTime;
        }
        else if (!GodMode)
        {
            _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, 1f);
            _invincible = false;
        }
    }

#endregion

    void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void Knockback(bool fromRight)
    {
        _knockbackTimer = KnockbackCooldown;
        _knockFromRight = fromRight;
        _hit = true;
        _anim.SetTrigger("Recoil"); //Do i need to send in a timer as well?
    }

    #region Collisions

    // Collisions wtih Player
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Wall")
        {
            _onWall = true;
        }

        if (collision.otherCollider is PolygonCollider2D)
        {
//            if (collision.contacts)
//            {
//                this.gameObject.layer = PASS_THROUGH_PLATFORMS;
//                _forcePassingThrough = true;
//            }
//            else
                _onGround = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.otherCollider is PolygonCollider2D)
        {
            _onGround = false;
        }

        if (collision.collider.tag == "Wall")
        {
            _onWall = false;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.otherCollider is PolygonCollider2D)
        {
            _onGround = true;
        }

        if (collision.collider.tag == "Wall")
        {
            _onWall = true;
        }
    }

    #endregion

    #region Health

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    public void RestoreHealth(int health)
    {
        _currentHealth = _currentHealth + health > MaxHealth ? MaxHealth : _currentHealth + health;
    }

    public void Damage(int damage)
    {
        _currentHealth -= damage;
        _invinceTimer = InvincibilityCooldown;
        //Knockback animation here or from enemy call?
        //Need to figure out direction then.
        //var playerMovement = gameObject.GetComponent<Player>();

        _invincible = true;

        //For now, just half the transparency when hit + invincible
        _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, 0.5f);
    }

    public bool IsInvincible()
    {
        return _invincible;
    }

    #endregion
}
