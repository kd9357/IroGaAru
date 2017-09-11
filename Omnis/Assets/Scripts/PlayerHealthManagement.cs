using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthManagement : MonoBehaviour {

    public bool GodMode = false;
    public int MaxHealth = 8;
    public static int CurrentHealth;    //Maybe put this in gamecontroller?

    public float InvincibilityCooldown = 3f;

    private SpriteRenderer _sprite;
    private float _timer;
    private bool _invincible;

    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        CurrentHealth = MaxHealth;
        _timer = 0;
        _invincible = GodMode;
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentHealth <= 0)
        {
            //Initiate Game Over
            //GameController.instance.GameOver();

            //For now just destroy game object (will want to move this to game over function)
            Destroy(gameObject);
        }
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else if(!GodMode)
        {
            _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, 1f);
            _invincible = false;
        }
    }

    public void Damage(int damage)
    {
        CurrentHealth -= damage;
        _timer = InvincibilityCooldown;
        //Knockback animation here or from enemy call?
        //Need to figure out direction then.
        //var playerMovement = gameObject.GetComponent<Player>();

        _invincible = true;

        //For now, just half the transparency when hit + invincible
        _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, 0.5f);
    }

    public void RestoreHealth(int health)
    {
        CurrentHealth = CurrentHealth + health > MaxHealth ? MaxHealth : CurrentHealth + health;
    }

    public bool isInvincinble()
    {
        return _invincible;
    }


}
