using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    //Public Vars
    public float Speed;
    public float Lifetime;
    public int Damage;
    [Tooltip("The damage projectile gives to enemies when reflected")]
    public int DamageMultipler;
    
    //Private components
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;

    //Private vars
    private float _lifeTimer;
    private Vector2 _direction = Vector2.zero;
    private Color _currentColor;
    private int[] _colorCounter;
    private bool _mixed;

    private const int _playerLayer = 12;

	// Use this for initialization
	void Start () {
        _lifeTimer = 0;
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _currentColor = Color.white;
        _sprite.color = _currentColor;
        _colorCounter = new int[3];
        
    }
	
	// Update is called once per frame
	void Update () {
        if (_lifeTimer > Lifetime)
            Die();
        _lifeTimer += Time.deltaTime;
        _rb.velocity = _direction * Speed;
    }

    public void Launch(Vector2 dir)
    {
        _direction = dir;
    }
    
    public void ProjectileDamaged(WeaponColor color)
    {
        if (_mixed)
            return;
        //May manually set a good color for fireball or something
        _currentColor = (_currentColor + GameController.Instance.GetColor(color)) / 2;
        switch (color)
        {
            case WeaponColor.Red:
                _colorCounter[(int)WeaponColor.Red]++;
                break;
            case WeaponColor.Yellow:
                _colorCounter[(int)WeaponColor.Yellow]++;
                break;
            case WeaponColor.Blue:
                _colorCounter[(int)WeaponColor.Blue]++;
                break;
        }
        _currentColor = CheckStatus();
        _sprite.color = _currentColor;
    }

    Color CheckStatus()
    {
        if (_colorCounter[0] >= 1 && _colorCounter[1] >= 1)
        {
            _mixed = true;
            return new Color(1f, .5f, 0f);  //Return orange
        }
        if (_colorCounter[1] >= 1 && _colorCounter[2] >= 1)
        {
            _mixed = true;
            ApplyWindRecoil();
            return Color.green;
        }
        if (_colorCounter[0] >= 1 && _colorCounter[2] >= 1)
        {
            _mixed = true;
            return new Color(.5f, 0f, .5f); //Return purple
        }
        return _currentColor;
    }

    void ApplyWindRecoil()
    {
        //Reflect back
        _direction = -_direction;
        Speed *= 2;
        gameObject.layer = _playerLayer;
        _lifeTimer = 0;
    }

    void Die()
    {
        //Dissapate and kill self
        Destroy(gameObject);
    }
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            case "Player":
                var player = collision.gameObject.GetComponent<Player>();
                if (player == null)
                    Debug.LogError("Player script doesn't exist!");
                if (player.IsInvincible())
                    break;

                player.PlayerDamaged(Damage);
                if (Damage > 0)
                    player.Knockback(collision.transform.position.x < transform.position.x);

                // Projectile hit sound
                //_audioSource.clip = ProjectileSoundEffects[0];
                //_audioSource.Play();

                break;
            case "Enemy":
                int direction = _direction.x > 0 ? 1 : _direction.x < 0 ? -1 : 0;
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    GameController.Instance.LastEnemy = enemy;
                    enemy.EnemyDamaged(Damage * DamageMultipler, Color.white, direction);
                }
                break;
            default:
                break;
        }
        Die();
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            case "Player":
                var player = collision.gameObject.GetComponent<Player>();
                if (player == null)
                    Debug.LogError("Player script doesn't exist!");
                if (player.IsInvincible())
                    break;

                player.PlayerDamaged(Damage);
                if (Damage > 0)
                    player.Knockback(collision.transform.position.x < transform.position.x);

                // Projectile hit sound
                //_audioSource.clip = ProjectileSoundEffects[0];
                //_audioSource.Play();

                break;
            case "Enemy":
                Debug.Log("Hit enemy");
                int direction = _direction.x > 0 ? 1 : _direction.x < 0 ? -1 : 0;
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    GameController.Instance.LastEnemy = enemy;
                    enemy.EnemyDamaged(Damage * 5, Color.white, direction);
                }
                break;
            default:
                break;
        }
        Die();
    }
}
