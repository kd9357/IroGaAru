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
            collision.gameObject.GetComponent<EnemyHealthManagement>().Damage(Damage, _color);
        }
    }

    public void SetColor(WeaponColor color)
    {
        _color = color;
        GetComponent<SpriteRenderer>().color = GameController.instance.GetColor(color);
    }

}
