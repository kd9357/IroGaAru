using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveZone : MonoBehaviour {

    private Enemy _parent;

	// Use this for initialization
	void Start () {
        _parent = gameObject.GetComponentInParent<Enemy>();
	}

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
            _parent.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
            _parent.SetActive(false);    
    }
}
