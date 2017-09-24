using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorStatus
{
    Stun,
    DamageOverTime,
    WindRecoil,     //There must be a better name than this
    None
}

public class Enemy : MonoBehaviour
{
    protected static List<Color> SpecialColors = new List<Color>
    {
        new Color(.5f, 0f, .5f),        //Purple
        new Color(1f, .5f, 0f),         //Orange
        new Color(0.5f, 0.5f, 0.5f)     //Fake green
        
    };

    #region Public Attributes
    //For testing purposes
    [Tooltip("Check to display enemy stats")]
    public bool DebugMode = false;

    //[Tooltip("To determine where the player is in relation to the enemy")]
    //public Transform Target;    //Maybe just use gameobject.findwithtag instead of having it as a public var

    [Tooltip("Type in the movement behavior of the enemy")]
    public string AI_Type;
    public float MaxHealth = 5;
    [Tooltip("How much damage the enemy deals on contact")]
    public int TouchDamage = 1;
    public Color DefaultColor;
    public float Speed;

    [Tooltip("Greater values mean enemy will attack when player is further away")]
    public float AttackRange;
    [Tooltip("Force enemy experiences from player")]
    public float EnemyKnockbackForce;
    [Tooltip("Time before enemy can recover from being damaged")]
    public float RecoilCooldown = 0.5f;
    [Tooltip("Time before enemy returns to default color")]
    public float ColorCooldown = 5f;
    [Tooltip("Time before enemy takes another action")]
    public float ActionCooldown = 5f;

    // Audio vars
    public AudioClip[] EnemySoundEffects;

    //Assuming 0: Purple, 1: Orange, 2: Green
    protected ParticleSystem[] _colorParticleEffects;

    #endregion

    #region Protected Attributes

    // Components on enemy
    protected Animator _anim;
    protected SpriteRenderer _sprite;
    protected Rigidbody2D _rb;
    protected AudioSource _audioSource;

    // Combat Variables
    protected float _currentHealth;
    protected Color _currentColor;
    protected float _recoilTimer;
    protected float _colorTimer;
    protected float _actionTimer;
    protected bool _attacking;
    protected ColorStatus _currentStatus = ColorStatus.None;

    // Movement and Orientation
    protected Transform _target;
    protected float _currentSpeed;
    protected float _currentKnockbackForce;
    protected bool _facingRight;

    // Debugging variables
    protected TextMesh _textMesh;

    #endregion

    // Use this for initialization
    //Maybe have a helper method specifically for unique components?
    protected virtual void Start()
    {
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        _target = GameObject.FindGameObjectWithTag("Player").transform;

        _currentHealth = MaxHealth;
        _currentColor = DefaultColor;
        _sprite.color = _currentColor;
        _currentSpeed = Speed;
        _currentKnockbackForce = EnemyKnockbackForce;
        _recoilTimer = 0;
        _attacking = false;
        _actionTimer = ActionCooldown;  //May set this only when player in range

        //For testing purposes
        _textMesh = gameObject.GetComponentInChildren<TextMesh>();

        _colorParticleEffects = gameObject.GetComponentsInChildren<ParticleSystem>();
    }

    #region Updates

    void FixedUpdate()
    {
        //This unfortunately flips the debug text as well
        if ((_target.position.x - transform.position.x > 0 && !_facingRight)
            || (_target.position.x - transform.position.x < 0 && _facingRight))
            Flip();

        if (_recoilTimer <= 0)
        {
            //Leave AI_Type alone for now, replace later
            switch (AI_Type)
            {
                case "Lefty":
                    _rb.velocity = new Vector2(-_currentSpeed, 0);
                    break;
                case "Righty":
                    _rb.velocity = new Vector2(_currentSpeed, 0);
                    break;
                case "Stand":
                    _rb.velocity = Vector2.zero;
                    break;
                default:
                    //Nothing specified in AI_Type, default to move to player
                    if (_actionTimer > 0)
                        _actionTimer -= Time.deltaTime;
                    else if (InRange() && !_attacking)
                        Attack();
                    else
                        MoveForward();
                    break;
            }
        }
    }

    // Update is called once per frame
    //Turn this protected so that children can just update individual functions
    protected void Update()
    {
        UpdateTimers();
        ResetColorStatus();
        //Update health status
        if (_currentHealth <= 0)
        {
            Die();
        }

        if (DebugMode)
        {
            //Debug stuff
            string message = "";
            message += "HP: " + _currentHealth.ToString("F2") + "/" + MaxHealth + "\n";
            message += "AI: " + AI_Type + "\n";
            message += "Color: (" + _currentColor.r + ", " + _currentColor.g + ", " + _currentColor.b + ")\n";
            message += "Status: " + _currentStatus + "\n";
            message += "Color Timer: " + _colorTimer.ToString("F2") + "\n";
            message += "Action Timer: " + _actionTimer.ToString("F2") + "\n";
            message += "Recoil Timer: " + _recoilTimer.ToString("F2") + "\n";
            _textMesh.text = message;
        }
        else
        {
            _textMesh.text = "";
        }
    }

    #endregion

    #region Helper Methods
    
    #region Bookkeeping methods
    protected virtual void UpdateTimers()
    {
        //Update stagger/knockback time
        if (_recoilTimer > 0)
            _recoilTimer -= Time.deltaTime;
        else
            _anim.SetBool("Recoil", false);

        //Update status ailment time
        if (_colorTimer > 0)
            _colorTimer -= Time.deltaTime;  //What happens if colorTimer somehow becomes exactly 0?
        else if (_colorTimer < 0)
            ResetColorStatus();
    }
    
    protected virtual void ResetColorStatus()
    {
        if(_colorTimer < 0)
        {
            _colorTimer = 0;
            _currentColor = DefaultColor;
            _sprite.color = _currentColor;
            //Reset ailments
            _currentSpeed = Speed;
            _currentKnockbackForce = EnemyKnockbackForce;
            _currentStatus = ColorStatus.None;
            foreach(ParticleSystem ps in _colorParticleEffects)
            {
                if (ps.isPlaying)
                    ps.Stop();
            }
        }
    }
    #endregion

    #region Enemy Health and Combat Status methods
    // When the enemy gets hit by something
    public virtual void EnemyDamaged(int damage, Color color, int direction)
    {
        //Only set the timer on first hit
        if(_colorTimer == 0)
            _colorTimer = ColorCooldown;
        _recoilTimer = RecoilCooldown;
        _anim.SetBool("Recoil", true);
        _currentHealth -= damage;

        SetColor(color);
        _rb.AddForce(Vector2.right * direction * _currentKnockbackForce, ForceMode2D.Impulse);
    }

    //Combine colors and determine if status changed
    protected virtual void SetColor(Color color)
    {
        //Only allow color mixing when NOT under some ailment
        if (_currentStatus == ColorStatus.None)
        {
            _currentColor = (_currentColor + color) / 2;

            // Now determine other special effects
            Vector3 cc = new Vector3(_currentColor.r, _currentColor.g, _currentColor.b);
            Vector3 sc = Vector3.zero;

            int i;
            float threshold = 0.4f; //For purple
            for (i = 0; i < SpecialColors.Count; i++)
            {
                sc.Set(SpecialColors[i].r, SpecialColors[i].g, SpecialColors[i].b);
                float distance = Vector3.Distance(sc, cc);
                if (i != 0)
                    threshold = 0.34f; //gack, change threshold for orange and green
                if (distance < threshold)
                    break;
            }
            _currentStatus = (ColorStatus)(i);
            if (_currentStatus != ColorStatus.None)
                ApplyAilment();

            _sprite.color = _currentColor;
        }
    }

    //TODO: add sound effect if using
    protected virtual void ApplyAilment()
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

    //Reduce the enemy's health by 10% of currentHealth every second (Maxhealth instead?)
    protected virtual IEnumerator DamageOverTime()
    {
        while(_currentStatus == ColorStatus.DamageOverTime)
        {
            _currentHealth *= 0.90f;
            yield return new WaitForSeconds(1);
        }
    }

    //Trigger the attack animation if using and lock direction
    protected virtual void Attack()
    {
        _attacking = true;
        _anim.SetTrigger("Attack");
    }

    //Called at animation's end and resets status
    protected virtual void EndAttack()
    {
        _attacking = false;
        _actionTimer = ActionCooldown;
    }

    protected virtual void Die()
    {
        _anim.SetTrigger("Death");  //TODO: May instead trigger death animation, and on last frame call Die()
        Destroy(gameObject);
    }
    #endregion
    
    #region Movement & AI methods
    //Reorient enemy to face player
    protected virtual void Flip()
    {
        //Only flip when not stunned (looks better) && not attacking
        if (_currentStatus != ColorStatus.Stun && !_attacking)
        {
            _facingRight = !_facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            //Delay Enemy's actions
            if (_actionTimer != 0)
                _actionTimer = ActionCooldown / 2;
        }
    }

    //Determines if distance between player and enemy is within AttackRange
    protected virtual bool InRange()
    {
        return Vector3.Distance(transform.position, _target.position) < AttackRange;
    }

    //Move in the direction the enemy is facing
    protected virtual void MoveForward()
    {
        _rb.velocity = _facingRight ? new Vector2(_currentSpeed, _rb.velocity.y)
                                    : new Vector2(-_currentSpeed, _rb.velocity.y);
    }

    #endregion

    #endregion

    #region Collisions
    // Hurt player on contact
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.collider.tag)
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
            case "Wall":
                // TODO: Make this more dynamic
                AI_Type = AI_Type == "Righty" ? "Lefty" : "Righty";
                break;
            case "Instant Death":
                Die();
                break;
            default:
                break;
        }
    }

    //This is necessary if the player is pushing against the enemy while invincible, and their invincibility wears off
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        switch (collision.collider.tag)
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
            case "Instant Death":
                Die();
                break;
            default:
                break;
        }
    }

#endregion
}
