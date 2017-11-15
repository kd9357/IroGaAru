using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firebird : Enemy {

    [Tooltip("Projectile to be launched by Firebird")]
    public GameObject Projectile;

    protected override void Attack()
    {
    
        base.Attack();
        Vector3 direction = _facingRight ? Vector2.right : Vector2.left;
        //Spawn projectile
        Vector3 spawnPosition = transform.position + Vector3.up * 0.75f;
        GameObject projectile = Instantiate(Projectile, spawnPosition, transform.rotation);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript == null)
            Debug.LogError("Prefab does not have projectile script");

        projectileScript.Launch(direction);
    }
}
