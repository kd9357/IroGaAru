using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Player : MonoBehaviour {

    // Invincibility Mode -- For Testing!
    public bool GodMode = false;

    // Demo Vars -- will get rid of once we decide on platforming physics
    public bool Slide;
    public bool StaticJump;

    // Health vars
    public int MaxHealth;
    public float InvincibilityCooldown = 3f;

    // Movement vars
    public float Speed;
    public float Jump;

    // Recoil vars
    [Tooltip("Force player experiences from enemy")]
    public float PlayerKnockbackForce = 20;
    public float KnockbackCooldown = 0.3f;

    // Audio vars
    public AudioClip[] PlayerSoundEffects;

    // Components on player
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    //private PolygonCollider2D _footCollider;
    private Animator _anim;
    private AudioSource _audioSource;

    // Flags
    private bool _onGround;
    private bool _onWall;
    private bool _jumping;
    private bool _jumpCancel;
    private bool _facingRight;
    private bool _hit;
    private int _knockbackDirection;
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
	    //_footCollider = GetComponent<PolygonCollider2D>();
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        _onGround = true;
        _onWall = false;
        _jumping = false;
        _jumpCancel = false;
        _facingRight = true;
        _hit = false;
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
            float x_mov, y_mov;
#if (UNITY_ANDROID || UNITY_IPHONE)
            x_mov = MobileUI.Instance.GetLeft() && MobileUI.Instance.GetRight() ? 0 :
                    MobileUI.Instance.GetLeft() ? -Speed :
                    MobileUI.Instance.GetRight() ? Speed : 0;
#else
            x_mov = Slide ? Input.GetAxis("Horizontal") * Speed :
                Input.GetAxisRaw("Horizontal") * Speed;
#endif
            y_mov = _rb.velocity.y;
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
                _rb.AddForce(new Vector2(PlayerKnockbackForce * _knockbackDirection, PlayerKnockbackForce), ForceMode2D.Impulse);
                _hit = false;
            }
        }

        //_anim.SetFloat("Horizontal Speed", _rb.velocity.x);;
        if(_rb.velocity.x > 0.25f || _rb.velocity.x < -0.25f)
            _anim.SetBool("Walking", true);
        else
            _anim.SetBool("Walking", false);
        _anim.SetBool("OnGround", _onGround);
    }

    // Update runs once per frame
    private void Update()
    {
#if (UNITY_ANDROID || UNITY_IPHONE)
        if (MobileUI.Instance.GetJump() && _onGround)
        {
            _jumping = true;

            // Reset gamepad attack flag
            MobileUI.Instance.SetJump(false);
        }

        _jumpCancel = MobileUI.Instance.GetJump();
#else
        // JUMPING
        if (Input.GetButtonDown("Jump") && _onGround)
        {
            _jumping = true;
        }

        _jumpCancel = Input.GetButtonUp("Jump");
#endif

        // HEALTH
        if (_currentHealth <= 0)
        {
            // HACK TO RESTART GAME QUICKLY
            // There is a bug where if you hold left or right while it reload,
            // you'll start going that direction until you press the direction again
            GameController.instance.GameOver();

//            Destroy(gameObject.transform.GetChild(1));
//            transform.DetachChildren();
//
//            Destroy(gameObject);
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

    #region Helper Methods
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
        _knockbackDirection = fromRight ? -1 : 1;
        _hit = true;
    }
    #endregion

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

    public void PlayerDamaged(int damage)
    {
        _currentHealth -= damage;
        _invinceTimer = InvincibilityCooldown;

        _invincible = true;

        //For now, just half the transparency when hit + invincible
        _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, 0.5f);

        // Player hurt sound
        _audioSource.clip = PlayerSoundEffects[1];
        _audioSource.Play();
    }

    public bool IsInvincible()
    {
        return _invincible;
    }

    #endregion
}
