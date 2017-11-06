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

    private static int _enemyID;

    private WeaponColor _color;
    private Player _player;

    void Start()
    {
        _enemyID = -1;
        _player = GetComponentInParent<Player>();    
    }

    // Player attacks enemy
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.tag)
        {
            case "Enemy":
                int direction = _player.FacingRight() ? 1 : -1;
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null && _enemyID != collision.transform.root.GetInstanceID())
                {
                    _enemyID = collision.transform.root.GetInstanceID();
                    if (!enemy.EnemyDamaged(Damage, GameController.Instance.GetColor(_color), direction))
                    {
                        //Enemy was protected, knockback player (or some alternative animation)
                        _player.Knockback(direction > 0);
                    }
                }
                break;
            case "Interactable Environment":
                //direction = collision.transform.position.x < transform.position.x ? -1 : 1;
                direction = _player.FacingRight() ? 1 : -1;
                InteractableEnvironment ie = collision.gameObject.GetComponentInParent<InteractableEnvironment>();
                if (ie != null)
                    ie.EnvironmentDamaged(GameController.Instance.GetColor(_color), direction);
                break;
            case "Projectile":
                Projectile p = collision.gameObject.GetComponent<Projectile>();
                if(p != null)
                {
                    p.ProjectileDamaged(_color);
                }
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Enemy":
                _enemyID = -1;
                break;
        }
    }

    #region Color

    //public Color GetColor(WeaponColor color)
    //{
    //    switch (color)
    //    {
    //        case WeaponColor.Red:
    //        {
    //            return Color.red;
    //        }
    //        case WeaponColor.Yellow:
    //        {
    //            return Color.yellow;
    //        }
    //        case WeaponColor.Blue:
    //        {
    //            return Color.blue;
    //        }
    //        default:
    //        {
    //            Debug.LogError("Invalid color: " + color);
    //            return Color.white;
    //        }
    //    }
    //}

    public void SetColor(WeaponColor color)
    {
        _color = color;
        GetComponent<SpriteRenderer>().color = GameController.Instance.GetColor(color);
    }

    #endregion
}
