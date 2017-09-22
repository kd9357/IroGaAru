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

    //For testing purposes
    [Tooltip("Check to display enemy stats")]
    public bool DebugMode = false;

    [Tooltip("To determine where the player is in relation to the enemy")]
    public Transform Target;

    [Tooltip("Type in the movement behavior of the enemy")]
    public string AI_Type;
    public float MaxHealth = 5;
    [Tooltip("How much damage the enemy deals on contact")]
    public int TouchDamage = 1;
    public Color DefaultColor;
    public float Speed;

    [Tooltip("Greater values indicate larger attack ranges")]
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
    protected ColorStatus _currentStatus = ColorStatus.None;

    // Movement and Orientation
    protected float _currentSpeed;
    protected float _currentKnockbackForce;
    protected bool _facingRight;

    // Debugging variables
    protected TextMesh _textMesh;

    // Use this for initialization
    protected virtual void Start()
    {
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();

        _currentHealth = MaxHealth;
        _currentColor = DefaultColor;
        _currentSpeed = Speed;
        _currentKnockbackForce = EnemyKnockbackForce;
        _recoilTimer = 0;
        _actionTimer = 0;

        //For testing purposes
        _textMesh = gameObject.GetComponentInChildren<TextMesh>();
    }

    #region Updates

    void FixedUpdate()
    {
        //This unfortunately flips the debug text as well
        if ((Target.transform.position.x - transform.position.x > 0 && !_facingRight)
            || (Target.transform.position.x - transform.position.x < 0 && _facingRight))
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
                    else if (InRange())
                        Attack();
                    else
                        MoveToPlayer();
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
            message += "Color Timer: " + _colorTimer.ToString("F2");
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
    public virtual void UpdateTimers()
    {
        //Update stagger/knockback time
        if (_recoilTimer > 0)
            _recoilTimer -= Time.deltaTime;
        //Update status ailment time
        if (_colorTimer > 0)
            _colorTimer -= Time.deltaTime;  //What happens if colorTimer somehow becomes exactly 0?
        else if (_colorTimer < 0)
            ResetColorStatus();
    }
    
    public virtual void ResetColorStatus()
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

    protected virtual void ApplyAilment()
    {
        //When special color first applied, reset timer
        //TOOD: add some special effects, flas + sound effect or something to indicate change
        _colorTimer = ColorCooldown;
        switch(_currentStatus)
        {
            case ColorStatus.Stun:
                _currentSpeed = 0;
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
        //Only flip when not stunned (looks better)
        if (_currentStatus != ColorStatus.Stun)
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
        return Vector3.Distance(transform.position, Target.transform.position) < AttackRange;
    }

    //Move in the direction of the player
    protected virtual void MoveToPlayer()
    {
        _rb.velocity = _facingRight ? new Vector2(_currentSpeed, _rb.velocity.y)
                                            : new Vector2(-_currentSpeed, _rb.velocity.y);
    }

    protected virtual void Attack()
    {
        _actionTimer = ActionCooldown;
        _anim.SetTrigger("Attack");
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
