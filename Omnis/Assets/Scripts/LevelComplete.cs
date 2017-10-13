using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelComplete : MonoBehaviour {

    public void OnTriggerEnter2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case "Player":
                StartCoroutine(GameController.Instance.CompleteLevel());
                Destroy(collider.gameObject.GetComponent<Player>());
                break;
        }
    }
}
