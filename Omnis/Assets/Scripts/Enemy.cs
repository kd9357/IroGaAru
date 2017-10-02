using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public enum ColorStatus
{
    Stun,
    DamageOverTime,
    WindRecoil,     //There must be a better name than this
    None
}

public enum Behavior
{
    TrackPlayer,
    Stationary
}

public class Enemy : MonoBehaviour
{
    protected enum EnemyState
    {
        Inactive,
        Waiting,
        Moving,
        Attacking,
        Staggered,
        Dying
    }
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

    [Tooltip("Select the AI of the enemy")]
    public Behavior EnemyBehavior;
    [Tooltip("The maximum health the enemy starts at")]
    public float MaxHealth = 5;
    [Tooltip("How much damage the enemy deals on contact")]
    public int TouchDamage = 1;
    [Tooltip("The color the enemy will reset to")]
    public Color DefaultColor = Color.white;
    [Tooltip("The default movement speed this enemy moves at")]
    public float Speed;
    [Tooltip("The distance this enemy wants to be from other enemies")]
    public float AvoidanceDistance = 5f;

    // Combat public variables
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
    //Color effects
    [Tooltip("How strong the enemy is knockbacked while Green")]
    public float WindKnockbackForce;

    // Audio vars
    public AudioClip[] EnemySoundEffects;

    // Health pack prefab
    public GameObject HealthDrop;
    [Tooltip("Percent chance of dropping health")]
    public int Percent = 30;

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
    protected ColorStatus _currentColorStatus;

    // Movement and Orientation
    protected Transform _target;
    protected float _currentSpeed;
    protected float _xMov;
    protected float _yMov;
    protected float _currentKnockbackForce;
    protected bool _facingRight;

    //Assuming 0: Purple, 1: Orange, 2: Green
    protected ParticleSystem[] _colorParticleEffects;

    protected EnemyState _currentState;

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
        _currentColorStatus = ColorStatus.None;
        _sprite.color = _currentColor;
        _currentSpeed = Speed;
        _xMov = 0;
        _yMov = 0;
        _currentKnockbackForce = EnemyKnockbackForce;
        _recoilTimer = 0;
        _currentState = EnemyState.Inactive;
        _actionTimer = 0;

        //For testing purposes
        _textMesh = gameObject.GetComponentInChildren<TextMesh>();

        _colorParticleEffects = gameObject.GetComponentsInChildren<ParticleSystem>();
    }

    #region Updates

    void FixedUpdate()
    {
        if (_currentState == EnemyState.Inactive)
            return;

        if(_currentState != EnemyState.Staggered)
        {
            //This unfortunately flips the debug text as well
            if ((_target.position.x - transform.position.x > 0 && !_facingRight)
                || (_target.position.x - transform.position.x < 0 && _facingRight))
                Flip();
            if (_actionTimer > 0)
                _actionTimer -= Time.deltaTime;
            else
            {
                switch (EnemyBehavior)
                {
                    case Behavior.TrackPlayer:
                        if (InRange() && _currentState != EnemyState.Attacking)
                            Attack();
                        else if (_currentState != EnemyState.Attacking) 
                            MoveForward();
                        break;
                    case Behavior.Stationary:
                        if (InRange() && _currentState != EnemyState.Attacking)
                            Attack();
                        break;
                    default:
                        Debug.Log("Enemy has achieved nirvana and thinks beyond our mortal understanding");
                        break;
                }
            }
        }
        //Reset movement direction, update each new physics tick
        _xMov = 0;
        _yMov = 0;
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
            message += "AI: " + EnemyBehavior + "\n";
            message += "Current State: " + _currentState + "\n";

            message += "Color: (" + _currentColor.r + ", " + _currentColor.g + ", " + _currentColor.b + ")\n";
            message += "Color Status: " + _currentColorStatus + "\n";
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
        else if(_recoilTimer <= 0 && _currentState == EnemyState.Staggered)
        {
            _currentState = EnemyState.Waiting;
            _anim.SetBool("Recoil", false);
        }

        //Update status ailment time
        if (_colorTimer > 0)
            _colorTimer -= Time.deltaTime;
    }
    
    protected virtual void ResetColorStatus()
    {
        if(_colorTimer <= 0 && _currentColor != DefaultColor)
        {
            _colorTimer = 0;
            _currentColor = DefaultColor;
            _sprite.color = _currentColor;
            //Reset ailments
            _currentSpeed = Speed;
            _currentKnockbackForce = EnemyKnockbackForce;
            _currentColorStatus = ColorStatus.None;
            foreach (ParticleSystem ps in _colorParticleEffects)
            {
                if (ps.isPlaying)
                    ps.Stop();
            }
        }
    }

    public void SetActive(bool active)
    {
        _currentState = active ? EnemyState.Waiting : EnemyState.Inactive;
    }

    #endregion

    #region Enemy Health and Combat Status methods

    // When the enemy gets hit by something
    public virtual void EnemyDamaged(int damage, Color color, int direction,
                                     int additionalForce = 1)
    {
        //Only set the timer on first hit
        if(_colorTimer <= 0)
            _colorTimer = ColorCooldown;

        //Only mess with recoil when not stunned already
        if(_currentColorStatus != ColorStatus.Stun)    
            _recoilTimer = RecoilCooldown;

        _anim.SetBool("Recoil", true);
        _currentState = EnemyState.Staggered;
        _currentHealth -= damage;

        SetColor(color);

        _rb.AddForce(Vector2.right * direction * _currentKnockbackForce * additionalForce, 
                     ForceMode2D.Impulse);
    }

    //Combine colors and determine if status changed
    protected virtual void SetColor(Color color)
    {
        //Only allow color mixing when NOT under some ailment
        if (_currentColorStatus == ColorStatus.None)
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
            _currentColorStatus = (ColorStatus)(i);
            if (_currentColorStatus != ColorStatus.None)
                ApplyAilment();

            _sprite.color = _currentColor;
        }
    }

    //TODO: add sound effect if using
    protected virtual void ApplyAilment()
    {
        //When special color first applied, reset timer
        _colorTimer = ColorCooldown;
        if (!_colorParticleEffects[(int)_currentColorStatus].isPlaying)
            _colorParticleEffects[(int)_currentColorStatus].Play();
        switch (_currentColorStatus)
        {
            case ColorStatus.Stun:
                ApplyStun();
                return;
            case ColorStatus.WindRecoil:
                ApplyWindRecoil();
                return;
            case ColorStatus.DamageOverTime:
                StartCoroutine(ApplyDamageOverTime());
                return;
        }
    }

    //Stop enemy's movement
    protected virtual void ApplyStun()
    {
        _currentSpeed = 0;
        _recoilTimer = ColorCooldown;
        _anim.SetBool("Recoil", true);
        _currentState = EnemyState.Staggered;
    }

    //Increase enemy's knockback
    protected virtual void ApplyWindRecoil()
    {
        _currentKnockbackForce = WindKnockbackForce;
        _currentColor = Color.green;
    }

    //Reduce the enemy's health by 10% of currentHealth every second (Maxhealth instead?)
    protected virtual IEnumerator ApplyDamageOverTime()
    {
        while(_currentColorStatus == ColorStatus.DamageOverTime)
        {
            _currentHealth *= 0.90f;
            yield return new WaitForSeconds(1);
        }
    }

    //Trigger the attack animation if using and lock direction
    protected virtual void Attack()
    {
        //_attacking = true;
        _currentState = EnemyState.Attacking;
        _anim.SetTrigger("Attack");
    }

    //Called at animation's end and resets status
    protected virtual void EndAttack()
    {
        //_attacking = false;
        _currentState = EnemyState.Waiting;
        _actionTimer = ActionCooldown;
    }

    protected virtual void Die()
    {
        _currentState = EnemyState.Dying;
        _anim.SetTrigger("Death");  //TODO: May instead trigger death animation, and on last frame call Die()

        // Random health pack drop
        Random r = new Random();
        if (r.Next(0, 100) <= Percent)
        {
            GameObject healthDrop = Instantiate(HealthDrop);
            healthDrop.transform.position = transform.position;
        }

        Destroy(gameObject);
    }
    #endregion
    
    #region Movement & AI methods
    //Reorient enemy to face player
    protected virtual void Flip()
    {
        //Only flip when not stunned (looks better) && not attacking
        if (_currentColorStatus != ColorStatus.Stun && _currentState != EnemyState.Attacking)
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
        if (_currentState != EnemyState.Moving)
            _currentState = EnemyState.Moving;
        _xMov += _facingRight ? _currentSpeed : -_currentSpeed;
        //Base enemies only move left and right, so they won't be trying to move vertically to get away from each other
        //Override on more complex enemies
        _yMov = _rb.velocity.y;

        _xMov = Mathf.Clamp(_xMov, -_currentSpeed, _currentSpeed);
        //_yMov = Mathf.Clamp(_yMov, -_currentSpeed, _currentSpeed);
        _rb.velocity = new Vector2(_xMov, _yMov);
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
                if(TouchDamage > 0)
                    player.Knockback(collision.transform.position.x < transform.position.x);

                // Enemy hit sound
                _audioSource.clip = EnemySoundEffects[0];
                _audioSource.Play();
                break;
            case "Wall":
                // TODO: Make this more dynamic
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
                if (TouchDamage > 0)
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

    //Check other enemy locations
    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        switch(collision.tag)
        {
            case "Active Zone":
                float dist = Vector2.Distance(transform.position, collision.transform.position);
                if (dist < AvoidanceDistance)
                {
                    Vector2 dir = transform.position - collision.transform.position;
                    _xMov += Mathf.Lerp(dir.x, 0, dist / AvoidanceDistance);    //Stronger affect closer the enemy
                    _yMov += Mathf.Lerp(dir.y, 0, dist / AvoidanceDistance);
                }
                break;
        }
    }
    #endregion
}
