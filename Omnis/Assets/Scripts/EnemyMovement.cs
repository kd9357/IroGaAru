using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {

    private Rigidbody2D _rb;

    //Awake vs start? could initialize animation of enemy,.then deactivate until proximity of player
    //Then in start, set its attributes

    // Use this for initialization
    void Start () {
        _rb = GetComponent<Rigidbody2D>();	
	}
	
	// Update is called once per frame
	void Update () {

	}

}
