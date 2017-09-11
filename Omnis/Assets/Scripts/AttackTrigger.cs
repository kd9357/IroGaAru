using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour {

    public int Damage = 5;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (!collision.isTrigger && collision.CompareTag("Enemy"))
        if (collision.CompareTag("Enemy"))
        {
            collision.SendMessageUpwards("Damage", Damage);
        }
    }

}
