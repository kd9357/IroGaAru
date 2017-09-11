using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour {

    // Demo Vars -- will get rid of once we decide on platforming physics
    public bool Slide;
    public bool StaticJump;

    // Movement parameters
    public float Speed;
    public float Jump;

    // Recoil parameters
    public float KnockbackForce = 20;
    public float KnockbackCooldown = 0.3f;

    private Rigidbody2D _rb;
    private PolygonCollider2D _footCollider;
    private Animator _anim;

    private bool _onGround;
    private bool _onWall;

    private bool _jumping;
    private bool _jumpCancel;

    private bool _facingRight = true;

    private bool _hit;
    private float _knockbackTimer;
    private bool _knockFromRight;

    // Use this for initialization
    void Start ()
	{
	    _rb = this.GetComponent<Rigidbody2D>();
	    _footCollider = this.GetComponent<PolygonCollider2D>();
        _anim = this.GetComponent<Animator>();

        _onGround = true;
	    _onWall = false;

        _jumpCancel = false;

	}

    // Fixed Update runs once per physics tick
    void FixedUpdate()
    {
        if(_knockbackTimer <= 0)
        { 
            float x_mov = Slide ? Input.GetAxis("Horizontal") * Speed :
                                  Input.GetAxisRaw("Horizontal") * Speed;
            float y_mov = _rb.velocity.y;

            x_mov = _onGround || (!_onGround && !_onWall) ? x_mov : _rb.velocity.x;

            if ((x_mov > 0 && !_facingRight) || (x_mov < 0 && _facingRight))
                Flip();

            if(_jumping)
            {
                Debug.Log("Jump initiated");
                y_mov = Jump;
                _jumping = false;
            }

            if(!StaticJump)
            {
                if (_jumpCancel && _rb.velocity.y > 0)
                {
                    y_mov = Mathf.Lerp(_rb.velocity.y, 0, 0.8f); //possibly use a timer to interpolate, although velocity is already changing over time
                }
            }

            _rb.velocity = new Vector2(x_mov, y_mov);
        }
        else
        {
            _knockbackTimer -= Time.deltaTime;
            if(_hit)
            {
                if(_knockFromRight)
                {
                    _rb.AddForce(new Vector2(-KnockbackForce, KnockbackForce), ForceMode2D.Impulse);
                }
                else
                {
                    _rb.AddForce(new Vector2(KnockbackForce, KnockbackForce), ForceMode2D.Impulse);
                }
                _hit = false;
            }
        }

        _anim.SetFloat("Horizontal Speed", _rb.velocity.x);
        _anim.SetFloat("Vertical Speed", _rb.velocity.y);
    }

    // Update runs once per frame
    private void Update()
    {
        //This is a little gacky but I swear it feels more responsive and idk why
        if (Input.GetButtonDown("Jump") && _onGround)
        {
            _jumping = true;
        }

        //_jumping = Input.GetButtonDown("Jump") && _onGround;

        _jumpCancel = Input.GetButtonUp("Jump");
    }

    void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void Knockback(bool fromRight)
    {
        _knockbackTimer = KnockbackCooldown;
        _knockFromRight = fromRight;
        _hit = true;
        _anim.SetTrigger("Recoil"); //Do i need to send in a timer as well?
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
