using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fountain : InteractableEnvironment {

    [Range(0, 20f)]
    public float StunRadius = 10f;

    protected override void ApplyStun()
    {
        base.ApplyStun();
        var layerMask = (1 << LayerMask.NameToLayer("Enemy")) | (1 << LayerMask.NameToLayer("Transient Enemy"));
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, layerMask);
        foreach (Collider2D e in enemies)
        {
            var enemyScript = e.GetComponent<Enemy>();
            if (enemyScript != null)
                enemyScript.SetColor(new Color(0.5f, 0f, 0.5f), true);
        }
    }

}
