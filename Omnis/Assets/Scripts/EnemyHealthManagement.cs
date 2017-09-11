using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthManagement : MonoBehaviour
{

    public int MaxHealth = 10;
    public int TouchDamage = 3;

    private Animator _anim;

    private int _currentHealth;

    //Awake vs start? could initialize animation of enemy,.then deactivate until proximity of player
    //Then in start, set its attributes
    void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {
        _currentHealth = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentHealth <= 0)
        {
            //set death animation
            _anim.SetTrigger("Death");
            Destroy(gameObject);
        }
    }

    public void Damage(int damage)
    {
        _currentHealth -= damage;
        //Set hit animation + stagger?
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
