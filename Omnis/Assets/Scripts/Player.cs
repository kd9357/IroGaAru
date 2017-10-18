using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_ANDROID || UNITY_IPHONE
using CnControls;
#endif

public class Player : MonoBehaviour
{

    #region Public Attributes

    // Invincibility Mode -- For Testing!
    public bool GodMode = false;

    // Demo Vars -- will get rid of once we decide on platforming physics
    public bool Slide;
    public bool StaticJump;

    // Health vars
    public int MaxHealth;
    [Tooltip("Time player remains invincible after being hit")]
    public float InvincibilityCooldown = 3f;

    // Movement vars
    public float Speed; //originally was 20
    public float Jump;  //originally was 55 at 20 gravity

    // Recoil vars
    [Tooltip("Force player experiences from enemy")]
    public float PlayerKnockbackForce = 20;
    [Tooltip("Time until player may move after recoil")]
    public float KnockbackCooldown = 0.3f;

    // Weapon var
    public GameObject Weapon;

    // Audio vars
    public AudioClip[] PlayerSoundEffects;

    #endregion

    #region Private Attributes

    // Components on player
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    private Animator _anim;
    private AudioSource _audioSource;

    // Flags
    private bool _paused;
    private bool _onGround;
    private bool _onWall;
    private bool _jumping;
    private bool _tapJump;
    private bool _jumpCancel;
    private bool _facingRight;
    private int _knockbackDirection;
    private bool _invincible;

    // Timers
    private float _knockbackTimer;
    private float _invincibleTimer;

    // Health
    private int _currentHealth;

    // Weapon components and private vars
    private PolygonCollider2D _weaponCollider;
    private AttackTrigger _trigg;

    private bool _attacking = false;
    private WeaponColor _weaponColor;

    #endregion

    #region Initialization
    // Use this for initialization
    void Start ()
    {
        _sprite = GetComponent<SpriteRenderer>();
	    _rb = GetComponent<Rigidbody2D>();
	    //_footCollider = GetComponent<PolygonCollider2D>();
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        _paused = false;
        _onGround = true;
        _onWall = false;
        _jumping = false;
        _jumpCancel = false;
        _tapJump = false;
        _facingRight = true;
        _invincible = false;

        _knockbackTimer = 0f;
        _invincibleTimer = 0f;

	    _currentHealth = MaxHealth;

        //Setup weapon stuff
        _trigg = Weapon.GetComponent<AttackTrigger>();
        _weaponCollider = Weapon.GetComponent<PolygonCollider2D>();
        _weaponCollider.enabled = false;
	}

    #endregion

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
            x_mov = Slide ? CnInputManager.GetAxis("Horizontal") * Speed:
                CnInputManager.GetAxisRaw("Horizontal") * Speed;
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
                    y_mov = _rb.velocity.y * 0.2f;
                }
            }

            _rb.velocity = new Vector2(x_mov, y_mov);
            _anim.SetFloat("Y_Mov", y_mov);
        }
        else
        {
            _knockbackTimer -= Time.deltaTime;
        }

        //_anim.SetFloat("Horizontal Speed", _rb.velocity.x);;
        if(_rb.velocity.x > 0.25f || _rb.velocity.x < -0.25f)
            _anim.SetBool("Walking", true);
        else
            _anim.SetBool("Walking", false);
        _anim.SetBool("OnGround", _onGround);
    }

    // Update runs once per frame
    //Used to detect jump and attack (subject to change)
    private void Update()
    {
        if (_paused)
            return;

        //Pulled from PlayerAttack.cs
        //Check weapon color
        if(_weaponColor != GameController.Instance.EquippedColor)
        {
            _weaponColor = GameController.Instance.EquippedColor;
            _trigg.SetColor(_weaponColor);
        }

        bool doAttack = false;

#if (UNITY_ANDROID || UNITY_IPHONE)
        doAttack = (CnInputManager.GetButtonDown("Red") || 
                    CnInputManager.GetButtonDown("Yellow") ||
                    CnInputManager.GetButtonDown("Blue")) && !_attacking;

        if (CnInputManager.GetButtonDown("Jump") && _onGround)
        {
            _jumping = true;
            _jumpCancel = false;
            _tapJump = true;
            _anim.SetTrigger("Jumping");
        }

        _jumpCancel = CnInputManager.GetButtonUp("Jump");
#else
        // ATTACK
        doAttack = (Input.GetButtonDown("Red") || Input.GetButtonDown("Yellow") || 
                    Input.GetButtonDown("Blue")) && !_attacking;

        // JUMPING
        if (Input.GetButtonDown("Jump") && _onGround)
        {
            _jumping = true;
            _anim.SetTrigger("Jumping");
        }

        _jumpCancel = Input.GetButtonUp("Jump");
#endif

        // ATTACK Continued
        if(doAttack)
        {
            GameController.Instance.IncrementAttacksMade();
            Attack();
        }
        _anim.SetBool("Attacking", _attacking);

        // HEALTH
        if (_currentHealth <= 0)
        {
            Die();
        }

        if (_invincibleTimer > 0)
        {
            _invincibleTimer -= Time.deltaTime;
        }
        else if (!GodMode)
        {
            _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, 1f);
            _invincible = false;
        }
    }

    #endregion

    #region Helper Methods

    #region Movement & Orientation
    void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void FreezeMovement(bool pause)
    {
        _paused = pause;
    }

    #endregion

    #region Combat
    public void Knockback(bool fromRight)
    {
        _knockbackTimer = KnockbackCooldown;
        _knockbackDirection = fromRight ? -1 : 1;
        _rb.AddForce(new Vector2(PlayerKnockbackForce * _knockbackDirection, PlayerKnockbackForce), ForceMode2D.Impulse);
        _anim.SetTrigger("Recoil");
    }
    //TODO: Maybe allow enemy/amount of damage to determine knockback distance

    //Activate weapon + animations + sound
    void Attack()
    {
        _attacking = true;
        _weaponCollider.enabled = _attacking;

        _audioSource.clip = PlayerSoundEffects[0];
        _audioSource.Play();

        _anim.SetBool("Attacking", _attacking);
    }

    //Called at end of animation
    void EndAttack()
    {
        _attacking = false;
        _weaponCollider.enabled = _attacking;
        _anim.SetBool("Attacking", _attacking);
    }

    public bool IsAttacking()
    {
        return _attacking;
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
        if (damage > 0)
        {
            _currentHealth -= damage;
            _invincibleTimer = InvincibilityCooldown;

            _invincible = true;

            //For now, just half the transparency when hit + invincible
            _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, 0.5f);

            // Player hurt sound
            _audioSource.clip = PlayerSoundEffects[1];
            _audioSource.Play();
        }
    }

    public bool IsInvincible()
    {
        return _invincible;
    }

    public void Die()
    {
        GameController.Instance.GameOver();
        Destroy(gameObject.GetComponent<Player>());
    }

    #endregion

    #endregion

    #region Collisions

    // Collisions wtih Player
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherCollider is CircleCollider2D)
        {
            _onGround = true;
        }

        switch (collision.collider.tag)
        {
            case "Wall":
                _onWall = true;
                break;
            case "Instant Death":
                Die();
                break;
            default:
                break;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.otherCollider is CircleCollider2D)
        {
            _onGround = true;
        }

        switch (collision.collider.tag)
        {
            case "Wall":
                _onWall = true;
                break;
            case "Instant Death":
                Die();
                break;
            default:
                break;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.otherCollider is CircleCollider2D)
        {
            _onGround = false;
        }

        switch (collision.collider.tag)
        {
            case "Wall":
                _onWall = false;
                break;
            case "Instant Death":
                Die();
                break;
            default:
                break;
        }
    }

    #endregion
}
