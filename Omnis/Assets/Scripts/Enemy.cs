using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public int MaxHealth = 1;
    public int TouchDamage = 1;

    public float Speed;

    public float KnockbackForce;

    public Color DefaultColor;

    public float ComboCooldown = 0.5f;

    private Animator _anim;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;

    private int _currentHealth;
    //private EnemyColor _color;

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
            _currentColor = DefaultColor;
            _sprite.color = _currentColor;
        }
        if (_currentHealth <= 0)
        {
            //set death animation
            _anim.SetTrigger("Death");
            Destroy(gameObject);
        }
    }

    public void Damage(int damage, Color color)
    {
        _timer = ComboCooldown;
        _currentHealth -= damage;
        _currentColor = (_currentColor + color) / 2;
        _sprite.color = _currentColor;

        //Set hit animation + stagger?
        _rb.AddForce(new Vector2(KnockbackForce, 0), ForceMode2D.Impulse);

    }

    // Hurt player on contact
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var playerHealth = collision.gameObject.GetComponent<PlayerHealthManagement>();
            if (playerHealth.isInvincinble())
                return;
            playerHealth.Damage(TouchDamage);

            var playerMovement = collision.gameObject.GetComponent<Player>();
            playerMovement.Knockback(collision.transform.position.x < transform.position.x);
        }
    }

    //This is necessary if the player is pushing against the enemy while invincible, and their invincibility wears off
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var playerHealth = collision.gameObject.GetComponent<PlayerHealthManagement>();
            if (playerHealth.isInvincinble())
                return;
            playerHealth.Damage(TouchDamage);

            var playerMovement = collision.gameObject.GetComponent<Player>();
            playerMovement.Knockback(collision.transform.position.x < transform.position.x);
        }
    }
}
