using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponColor
{
    Red, 
    Yellow, 
    Blue
}

public class AttackTrigger : MonoBehaviour {

    public int Damage = 1;

    private WeaponColor _color;
    private Player _player;

    void Start()
    {
        _player = GetComponentInParent<Player>();    
    }

    // Player attacks enemy
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.tag)
        {
            case "Enemy":
                //int direction = collision.transform.position.x < transform.parent.position.x ? -1 : 1;
                int direction = _player.FacingRight() ? 1 : -1;
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                    enemy.EnemyDamaged(Damage, GetColor(_color), direction);
                break;
            case "Interactable Environment":
                //direction = collision.transform.position.x < transform.position.x ? -1 : 1;
                direction = _player.FacingRight() ? 1 : -1;
                InteractableEnvironment ie = collision.gameObject.GetComponentInParent<InteractableEnvironment>();
                if (ie != null)
                    ie.EnvironmentDamaged(GetColor(_color), direction);
                break;

        }
    }

    #region Color

    public Color GetColor(WeaponColor color)
    {
        switch (color)
        {
            case WeaponColor.Red:
            {
                return Color.red;
            }
            case WeaponColor.Yellow:
            {
                return Color.yellow;
            }
            case WeaponColor.Blue:
            {
                return Color.blue;
            }
            default:
            {
                Debug.LogError("Invalid color: " + color);
                return Color.white;
            }
        }
    }

    public void SetColor(WeaponColor color)
    {
        _color = color;
        GetComponent<SpriteRenderer>().color = GetColor(color);
    }

    #endregion
}
