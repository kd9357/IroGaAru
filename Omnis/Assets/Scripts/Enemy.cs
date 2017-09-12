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
        new Color(.5f, 0f, .5f, 1f),
        new Color(1f, .5f, 0f, 1f)
    };

    public string AI_Type;
    public float MaxHealth = 1;
    public int TouchDamage = 1;
    public float Speed;
    public Color DefaultColor;

    [Tooltip("Force enemy experiences from player")]
    public float EnemyKnockbackForce;
    public float ComboCooldown = 0.5f;

    private Animator _anim;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;

    private bool _recoil;

    public float _currentHealth;

    private Color _currentColor;
    private float _percentage;

    public float ColorTimer;

    private ColorStatus _currentStatus = ColorStatus.None;
    private string _cachedAI_Type;


    // Use this for initialization
    void Start()
    {
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _currentHealth = MaxHealth;
        _currentColor = DefaultColor;
        _recoil = false;
    }

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
        if (ColorTimer > 0)
        {
            ColorTimer -= Time.deltaTime;
            if (_currentStatus == ColorStatus.DamageOverTime)
            {
                _currentHealth *= 0.99f; //Every frame reduce enemy health to 90 percent of current health
            }
            else if (_currentStatus == ColorStatus.Stun)
            {
                AI_Type = "Stand";
            }
        }
        else if (ColorTimer < 0)
        {
            ColorTimer = 0;
            _recoil = false;
            _currentColor = DefaultColor;
            _sprite.color = _currentColor;
            if (_currentStatus == ColorStatus.Stun)
                AI_Type = _cachedAI_Type;
            _currentStatus = ColorStatus.None;

        }
        if (_currentHealth <= 0)
        {
            //set death animation
            _anim.SetTrigger("Death");
            Destroy(gameObject);

            // TODO: Set game over
        }
    }

    public void Damage(int damage, Color color, int direction)
    {
        ColorTimer = ComboCooldown;
        _recoil = true;

        _currentHealth -= damage;

        // GAME JAM HACK: Needs to be refined for game -- right now, can't make green
        // just by averaging
        _currentColor = (_currentColor + color) / 2;

        // Determine if any special effects
        // Purple
        Vector3 sc = new Vector3(SpecialColors[0].r, SpecialColors[0].g, SpecialColors[0].b);
        Vector3 cc = new Vector3(_currentColor.r, _currentColor.g, _currentColor.b);

        float distance = Vector3.Distance(sc, cc);
        if (distance < .4f)
        {
            Debug.Log("Similar to Purple");
            _currentStatus = ColorStatus.Stun;
            _cachedAI_Type = AI_Type;
            ColorTimer += 1;
        }
        else
        {
            // Orange
            sc = new Vector3(SpecialColors[1].r, SpecialColors[1].g, SpecialColors[1].b);
            distance = Vector3.Distance(sc, cc);
            if (distance < .2f)
            {
                Debug.Log("Similar to Orange");
                _currentStatus = ColorStatus.DamageOverTime;
            }
            else
            {
                _currentStatus = ColorStatus.None;
            }
        }



        _sprite.color = _currentColor;

        //Set hit animation + stagger?
        _rb.AddForce(Vector2.right * direction * EnemyKnockbackForce * _rb.mass, ForceMode2D.Impulse);

    }

    // Hurt player on contact
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player.IsInvincible())
                return;

            player.Damage(TouchDamage);
            player.Knockback(collision.transform.position.x < transform.position.x);
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

            player.Damage(TouchDamage);
            player.Knockback(collision.transform.position.x < transform.position.x);
        }
    }
}
