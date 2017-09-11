using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthManagement : MonoBehaviour {

    public bool GodMode = false;
    public int MaxHealth = 10;
    public static int CurrentHealth;

    public float InvincibilityCooldown = 3f;

    private float _timer;
    private bool _invincible;

    void Start()
    {
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
            //For now just destroy game object (will want to move this to game over function)
            Destroy(gameObject);
        }
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else if(!GodMode)
        {
            _invincible = false;
        }
    }

    public void Damage(int damage)
    {
        CurrentHealth -= damage;
        _timer = InvincibilityCooldown;
        //Knockback animation here or from enemy call?
        //var playerMovement = gameObject.GetComponent<Player>();

        _invincible = true;
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
