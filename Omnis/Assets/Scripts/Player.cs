using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public float Speed;
    public float Jump;

    private Rigidbody2D _rb;

	// Use this for initialization
	void Start ()
	{
	    _rb = this.GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    float x_mov = Input.GetAxis("Horizontal") * Speed;
	    float y_mov = Input.GetKeyUp(KeyCode.Space) ? Jump : 0;

	    _rb.velocity = new Vector2(x_mov, y_mov);
	}
}
