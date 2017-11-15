using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {
    private bool _active = false;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player" && !_active)
        {
            _active = true;
            GameController.Instance.UpdateLastCheckpoint(gameObject.transform.position);
            GetComponent<Animator>().SetBool("Active", _active);
            // TODO: Play sounds
        }
    }
}
