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
    private Player _player;
    private AudioSource _playerAudioSource;

    private bool _attacking = false;
    private float _timer;
    private WeaponColor _color;

    private void Start()
    {
        _anim = gameObject.GetComponent<Animator>();
        _trigg = WeaponCollider.gameObject.GetComponent<AttackTrigger>();
        _player = GetComponent<Player>();
        _playerAudioSource = GetComponent<AudioSource>();

        //For testing
        _weaponSprite = WeaponCollider.gameObject.GetComponent<SpriteRenderer>();
        _weaponSprite.enabled = false;

        WeaponCollider.enabled = false;
        _timer = 0;
    }
	
	// Update is called once per frame
	void Update () {
        //Check to see which weapon is equipped
        if(_color != GameController.instance.EquippedColor)
        {
            _color = GameController.instance.EquippedColor;
            _trigg.SetColor(_color);
        }

	    bool doAttack = false;
#if (UNITY_ANDROID || UNITY_IPHONE)
        doAttack = MobileUI.Instance.GetAttack() && !_attacking;
#else
	    doAttack = Input.GetButtonDown("Fire1") && !_attacking;
#endif
        
		if(doAttack)
        {
            _attacking = true;
            _timer = AttackCooldown;
            WeaponCollider.enabled = true;

            // Play player attack sound effect
            _playerAudioSource.clip = _player.PlayerSoundEffects[0];
            _playerAudioSource.Play();

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

        //Most likely we will not be handling weapon animations separately, but for now:
        WeaponCollider.gameObject.GetComponent<Animator>().SetBool("Attacking", _attacking);
	}

    public bool IsAttacking()
    {
        return _attacking;
    }
}
