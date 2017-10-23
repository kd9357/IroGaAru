using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player")
        {
            GameController.Instance.UpdateLastCheckpoint(gameObject.transform.position);
            // TODO: Play sounds, animation
        }
    }
}
