using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentalHazard : MonoBehaviour {

    [Tooltip("How much damage the hazard deals on contact")]
    public int TouchDamage = 1;

    [Tooltip("Check if you want enemies to be affected")]
    public bool HurtEnemies;

    // Audio vars
    public AudioClip[] HazardSoundEffects;
    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    #region Collisions

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

                // Hit sound
                _audioSource.clip = HazardSoundEffects[0];
                _audioSource.Play();
                break;
            case "Enemy":
                if (HurtEnemies)
                {
                    Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        if (enemy.name == "Oni")
                        {
                            enemy.EnemyDamaged(TouchDamage, Color.white, 1, 10);
                        }
                        else
                        {
                            enemy.EnemyDamaged(TouchDamage, Color.white, 1);
                        }
                    }

                    // TODO: Play Enemy Hurt Sound!
                }
                break;
        }
    }

    // This is necessary if the player is pushing against the spike while invincible, 
    //  and their invincibility wears off
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

                _audioSource.clip = HazardSoundEffects[0];
                _audioSource.Play();
                break;
            case "Enemy":
                if (HurtEnemies)
                {
                    Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        // Don't hurt them if they're stuck on the spike; push them off
                        enemy.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 10,
                                                                   ForceMode2D.Impulse);
                    }

                    // TODO: Play Enemy Hurt Sound!
                }
                break;
        }
    }

    // Some enemies are triggers
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case "Enemy":
                if (HurtEnemies)
                {
                    int direction = collider.transform.position.x < transform.position.x ? -1 : 1;
                    Enemy enemy = collider.gameObject.GetComponent<Enemy>();
                    if (enemy != null)
                        enemy.EnemyDamaged(TouchDamage, Color.white, direction);

                    // TODO: Play Enemy Hurt Sound!
                }
                break;
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case "Enemy":
                if (HurtEnemies)
                {
                    Enemy enemy = collider.gameObject.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        // Don't hurt them if they're stuck on the spike; push them off
                        enemy.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 10,
                            ForceMode2D.Impulse);
                    }

                    // TODO: Play Enemy Hurt Sound!
                }
                break;
        }
    }

    #endregion
}
