using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponColor
{
    red, yellow, blue
};

public class AttackTrigger : MonoBehaviour {

    public int Damage = 1;

    private WeaponColor _color;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Color col = GetColor(_color);
            collision.gameObject.GetComponent<Enemy>().Damage(Damage, col);
        }
    }

    #region Color

    public Color GetColor(WeaponColor color)
    {
        switch (color)
        {
            case WeaponColor.red:
            {
                return Color.red;
            }
            case WeaponColor.yellow:
            {
                return Color.yellow;
            }
            case WeaponColor.blue:
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
