using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour {

    public int Damage = 1;

    private WeaponColor _color;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (!collision.isTrigger && collision.CompareTag("Enemy"))
        if (collision.CompareTag("Enemy"))
        {
            Color col = GameController.instance.GetColor(_color);
            collision.gameObject.GetComponent<Enemy>().Damage(Damage, col);
        }
    }

    public void SetColor(WeaponColor color)
    {
        _color = color;
        GetComponent<SpriteRenderer>().color = GameController.instance.GetColor(color);
    }

}
