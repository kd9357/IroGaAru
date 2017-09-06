using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour {

    // Demo Vars -- will get rid of once we decide on platforming physics
    public bool Slide;
    public bool VariableJump;

    // Note: You can also smooth using Time.deltaTime
    public float Speed;
    public float Jump;

    private Rigidbody2D _rb;
    private PolygonCollider2D _footCollider;

    private bool _onGround;
    private bool _onWall;

	// Use this for initialization
	void Start ()
	{
	    _rb = this.GetComponent<Rigidbody2D>();
	    _footCollider = this.GetComponent<PolygonCollider2D>();

        _onGround = true;
	    _onWall = false;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
        float x_mov = Slide ? Input.GetAxis("Horizontal") * Speed : 
                              Input.GetAxisRaw("Horizontal") * Speed;
	    float y_mov = VariableJump ? Input.GetAxis("Jump") * Jump :
                                     Input.GetAxisRaw("Jump") * Jump;

	    y_mov = _onGround ? y_mov : _rb.velocity.y;
	    x_mov = _onGround || (!_onGround && !_onWall) ? x_mov : _rb.velocity.x;

	    _rb.velocity = new Vector2(x_mov, y_mov);
	}

    // Collisions wtih Player
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Wall")
        {
            _onWall = true;
        }

        if (collision.otherCollider is PolygonCollider2D)
        {
            _onGround = true;

//            DEBUGGING
//            Debug.Log("Collider: " + collision.collider.name);
//            Debug.Log("Other Collider: " + collision.otherCollider.name);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.otherCollider is PolygonCollider2D)
        {
            _onGround = false;
        }

        if (collision.collider.tag == "Wall")
        {
            _onWall = false;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.otherCollider is PolygonCollider2D)
        {
            _onGround = true;
        }

        if (collision.collider.tag == "Wall")
        {
            _onWall = true;
        }
    }
}
