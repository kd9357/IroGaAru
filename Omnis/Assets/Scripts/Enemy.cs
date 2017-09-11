using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public string AI_Type;
    public int MaxHealth = 1;
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

    private int _currentHealth;

    private Color _currentColor;
    private float _percentage;

    private float _timer;

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
        if(_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else if(_timer < 0)
        {
            _timer = 0;
            _recoil = false;
            _currentColor = DefaultColor;
            _sprite.color = _currentColor;
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
        _timer = ComboCooldown;
        _recoil = true;

        _currentHealth -= damage;
        _currentColor = (_currentColor + color) / 2;
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
