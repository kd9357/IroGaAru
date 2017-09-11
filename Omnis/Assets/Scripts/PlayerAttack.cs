using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    //Currently a box collider, eventually change this to the shape of the weapon during the swing
    public Collider2D WeaponCollider;
    public float AttackCooldown = 0.3f;

    //For placeholder testing purposes, delete later when proper animations are added
    private SpriteRenderer _weaponSprite;

    private Animator _anim;
    private AttackTrigger _trigg;

    private bool _attacking = false;
    private float _timer;
    private WeaponColor _color;

    private void Awake()
    {
        _anim = gameObject.GetComponent<Animator>();
        _trigg = WeaponCollider.gameObject.GetComponent<AttackTrigger>();

        //For testing
        _weaponSprite = WeaponCollider.gameObject.GetComponent<SpriteRenderer>();

        WeaponCollider.enabled = false;
        _timer = 0;

        //For testing
        _weaponSprite.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        //Check to see which weapon is equipped
        if(_color != GameController.instance.EquippedColor)
        {
            _color = GameController.instance.EquippedColor;
            _trigg.SetColor(_color);
        }
        

		if(Input.GetButtonDown("Attack") && !_attacking)
        {
            _attacking = true;
            _timer = AttackCooldown;
            WeaponCollider.enabled = true;

            //For testing
            _weaponSprite.enabled = true;
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
                _weaponSprite.enabled = false;
            }
        }

        _anim.SetBool("Attacking", _attacking);
	}
}
