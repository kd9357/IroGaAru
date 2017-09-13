using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorStatus
{
    Stun,
    DamageOverTime,
    None
}

public class Enemy : MonoBehaviour
{

    private static List<Color> SpecialColors = new List<Color>
    {
        new Color(.5f, 0f, .5f),    //Purple
        new Color(1f, .5f, 0f)    //Orange
    };

    [Tooltip("Type in the movement behavior of the enemy")]
    public string AI_Type;
    public float MaxHealth = 5;
    [Tooltip("How much damage the enemy deals on contact")]
    public int TouchDamage = 1;
    public float Speed;
    public Color DefaultColor;

    [Tooltip("Force enemy experiences from player")]
    public float EnemyKnockbackForce;
    [Tooltip("Time before color ailment expires (stun extends this)")]
    public float ComboCooldown = 0.5f;

    // Audio vars
    public AudioClip[] EnemySoundEffects;

    // Components on enemy
    private Animator _anim;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    private AudioSource _audioSource;

    private bool _recoil;

    private float _currentHealth;
    private Color _currentColor;

    private float _colorTimer;

    private ColorStatus _currentStatus = ColorStatus.None;
    private string _cachedAI_Type;


    // Use this for initialization
    void Start()
    {
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();

        _currentHealth = MaxHealth;
        _currentColor = DefaultColor;
        _recoil = false;
    }

#region Updates

    void FixedUpdate()
    {
        if (!_recoil)
        {
            switch (AI_Type)
            {
                case "Lefty":
                    _rb.velocity = new Vector2(-Speed, 0);
                    break;
                case "Righty":
                    _rb.velocity = new Vector2(Speed, 0);
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
        if (_colorTimer > 0)
        {
            _colorTimer -= Time.deltaTime;
            if (_currentStatus == ColorStatus.DamageOverTime)
            {
                _currentHealth *= 0.99f; //Every frame reduce enemy health by 1% of current health
            }
            else if (_currentStatus == ColorStatus.Stun)
            {
                AI_Type = "Stand";
            }
        }
        else if (_colorTimer < 0)
        {
            _colorTimer = 0;
            _recoil = false;
            _currentColor = DefaultColor;
            _sprite.color = _currentColor;
            if (_currentStatus == ColorStatus.Stun)
                AI_Type = _cachedAI_Type;
            _currentStatus = ColorStatus.None;

        }
        if (_currentHealth <= 0)
        {
            _anim.SetTrigger("Death");
            Destroy(gameObject);
        }
    }

#endregion

    public void EnemyDamaged(int damage, Color color, int direction)
    {
        _colorTimer = ComboCooldown;
        _recoil = true;

        _currentHealth -= damage;

        // GAME JAM HACK: Needs to be refined for game -- right now, can't make green
        // just by averaging
        _currentColor = (_currentColor + color) / 2;

        // Determine if any special effects

        //Our current model of determining the proximity of a color is incredibly limited
        //The most accurate model is using CIELAB coordinates and Delta E, but is an expensive calculation
        //There's a cheaper YUV comparison but I couldn't get it working quickly
        //HSV comparison of hues is reasonable, but requires just as much fine tweaking as RGB

        //The problems mostly stem from the fact that we're using RBY over RGB and CMY colors
        //Cyan = (0, 1, 1), Magenta = (1, 0, 1), Yellow = (1, 1, 0), 
        //so it would be trivial to average towards those colors and find a percentage of them

        Vector3 cc = new Vector3(_currentColor.r, _currentColor.g, _currentColor.b);
        int i;
        float threshold = 0.4f;    
        for(i = 0; i < SpecialColors.Count; i++)
        {
            Vector3 sc = new Vector3(SpecialColors[i].r, SpecialColors[i].g, SpecialColors[i].b);
            float distance = Vector3.Distance(sc, cc);
            if (i != 0) 
                threshold = 0.34f; //gack
            if (distance < threshold)
            {
                Debug.Log("Applying " + (ColorStatus)(i));
                if (_currentStatus == ColorStatus.Stun)  // even more gack
                {
                    _cachedAI_Type = AI_Type;
                    _colorTimer += 1;
                }
                break;
            }

        }
        _currentStatus = (ColorStatus)(i);

        if(_currentStatus == ColorStatus.None)
            Debug.Log("Status reset to NONE");

        _sprite.color = _currentColor;
        _rb.AddForce(Vector2.right * direction * EnemyKnockbackForce * _rb.mass, ForceMode2D.Impulse);

    }

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
