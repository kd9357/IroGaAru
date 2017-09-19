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

    private static List<Color> SpecialColors = new List<Color>
    {
        new Color(.5f, 0f, .5f),        //Purple
        new Color(1f, .5f, 0f),         //Orange
        new Color(0.5f, 0.5f, 0.5f)     //Fake green
        
    };

    //For testing purposes
    [Tooltip("Check to display enemy stats")]
    public bool DebugMode = false;

    [Tooltip("Type in the movement behavior of the enemy")]
    public string AI_Type;
    public float MaxHealth = 5;
    [Tooltip("How much damage the enemy deals on contact")]
    public int TouchDamage = 1;
    public float Speed;
    public Color DefaultColor;

    [Tooltip("Force enemy experiences from player")]
    public float EnemyKnockbackForce;
    [Tooltip("Time before enemy can recover from recoil")]
    public float RecoilCooldown = 0.5f;
    [Tooltip("Time before enemy returns to default color")]
    public float ColorCooldown = 5f;

    // Audio vars
    public AudioClip[] EnemySoundEffects;

    // Components on enemy
    private Animator _anim;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    private AudioSource _audioSource;

    private float _currentHealth;
    private Color _currentColor;
    private float _currentSpeed;
    private float _currentKnockbackForce;
    private float _recoilTimer;
    private float _colorTimer;

    private ColorStatus _currentStatus = ColorStatus.None;

    private TextMesh _textMesh;

    // Use this for initialization
    void Start()
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

        //For testing purposes
        _textMesh = gameObject.GetComponentInChildren<TextMesh>();
    }

    #region Updates

    void FixedUpdate()
    {
        if (_recoilTimer <= 0)
        {
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
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Update stagger/knockback time
        if(_recoilTimer > 0)
            _recoilTimer-= Time.deltaTime;

        //Update status ailment time
        if (_colorTimer > 0)
        {
            _colorTimer -= Time.deltaTime;
        }
        else if (_colorTimer < 0)
        {
            _colorTimer = 0;
            _currentColor = DefaultColor;
            _sprite.color = _currentColor;
            _currentSpeed = Speed;
            _currentKnockbackForce = EnemyKnockbackForce;
            _currentStatus = ColorStatus.None;
        }

        //Update health status
        if (_currentHealth <= 0)
        {
            _anim.SetTrigger("Death");
            Destroy(gameObject);
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
    public void EnemyDamaged(int damage, Color color, int direction)
    {
        //Only set the timer on first hit
        if(_colorTimer != ColorCooldown)
            _colorTimer = ColorCooldown;
        _recoilTimer = RecoilCooldown;
        _currentHealth -= damage;

        //Only allow color mixing when not under some ailment
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
                {
                    Debug.Log("Applying " + (ColorStatus)(i));
                    break;
                }

            }
            _currentStatus = (ColorStatus)(i);
            if (_currentStatus != ColorStatus.None)
                ApplyAilment();
        }

        _sprite.color = _currentColor;
        _rb.AddForce(Vector2.right * direction * _currentKnockbackForce, ForceMode2D.Impulse);
    }

    protected void ApplyAilment()
    {
        //When special color first applied, reset timer
        _colorTimer = ColorCooldown;
        switch(_currentStatus)
        {
            case ColorStatus.Stun:
                _currentSpeed = 0;
                return;
            case ColorStatus.WindRecoil:
                _currentKnockbackForce *= 2;
                //For now, double knockback effect when green
                //May change this to a one time hit
                //Don't mess with mass since we want the boss to be variably affected due to his increased mass
                _currentColor = Color.green;
                return;
            case ColorStatus.DamageOverTime:
                StartCoroutine(DamageOverTime());
                return;
        }
    }

    //Reduce the enemy's health by 10% of currentHealth every second
    IEnumerator DamageOverTime()
    {
        while(_currentStatus == ColorStatus.DamageOverTime)
        {
            _currentHealth *= 0.90f;
            yield return new WaitForSeconds(1);
        }
    }

    #endregion

    #region Collisions
    // Hurt player on contact
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player.IsInvincible())
                return;

            player.PlayerDamaged(TouchDamage);
            player.Knockback(collision.transform.position.x < transform.position.x);

            // Enemy hit sound
            _audioSource.clip = EnemySoundEffects[0];
            _audioSource.Play();
        }

        // TODO: Make this more dynamic
        else if (collision.gameObject.CompareTag("Wall"))
            AI_Type = AI_Type == "Righty" ? "Lefty" : "Righty";
    }

    //This is necessary if the player is pushing against the enemy while invincible, and their invincibility wears off
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player.IsInvincible())
                return;

            player.PlayerDamaged(TouchDamage);
            player.Knockback(collision.transform.position.x < transform.position.x);

            // Enemy hit sound
            _audioSource.clip = EnemySoundEffects[0];
            _audioSource.Play();
        }
    }

#endregion
}
