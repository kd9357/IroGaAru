using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    //Currently a box collider, eventually change this to the shape of the weapon during the swing
    public Collider2D WeaponCollider;
    public float AttackCooldown = 0.3f;

    //For placeholder testing purposes, delete later when proper animations are added
    public SpriteRenderer WeaponSprite;

    private Animator _anim;
    private bool _attacking = false;
    private float _timer;

    private void Awake()
    {
        _anim = gameObject.GetComponent<Animator>();
        WeaponCollider.enabled = false;
        _timer = 0;

        //For testing
        WeaponSprite.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Attack") && !_attacking)
        {
            _attacking = true;
            _timer = AttackCooldown;
            WeaponCollider.enabled = true;

            //For testing
            WeaponSprite.enabled = true;
        }

        if(_attacking)
        {
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
            }
            else
            {
                _attacking = false;
                WeaponCollider.enabled = false;

                //For testing
                WeaponSprite.enabled = false;
            }
        }

        _anim.SetBool("Attacking", _attacking);
	}
}
