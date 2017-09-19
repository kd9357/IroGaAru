using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    //testing var
    public bool UseAnimation = false;
    //Currently a box collider, eventually change this to the shape of the weapon during the swing
    //public Collider2D WeaponCollider;
    //public PolygonCollider2D LargeHitbox;   //testing
    //public PolygonCollider2D WeaponHitbox;  //testing
    public GameObject Weapon;

    public float AttackCooldown = 0.3f;

    //For placeholder testing purposes, delete later when proper animations are added
    private SpriteRenderer _weaponSprite;
    //testing
    private PolygonCollider2D[] _weaponColliders;
    private Animator _weaponAnim;

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
        //_trigg = WeaponCollider.gameObject.GetComponent<AttackTrigger>();
        _trigg = Weapon.GetComponent<AttackTrigger>();
        _player = GetComponent<Player>();
        _playerAudioSource = GetComponent<AudioSource>();

        //For testing
        //_weaponSprite = WeaponCollider.gameObject.GetComponent<SpriteRenderer>();
        _weaponSprite = Weapon.GetComponent<SpriteRenderer>();
        _weaponSprite.enabled = false;

        //WeaponCollider.enabled = false;
        _weaponColliders = Weapon.GetComponents<PolygonCollider2D>();
        foreach (PolygonCollider2D hb in _weaponColliders)
        {
            hb.enabled = false;
        }
        _weaponAnim = Weapon.GetComponent<Animator>();

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

            //TESTING
            //Use animation? activate large hitbox
            if (UseAnimation)
                _weaponColliders[0].enabled = true;
            else //Otherwise use close fitting hitbox
                _weaponColliders[1].enabled = true;

            //Original
            //WeaponCollider.enabled = true;

            // Play player attack sound effect
            _playerAudioSource.clip = _player.PlayerSoundEffects[0];
            _playerAudioSource.Play();

            //For testing
            _weaponSprite.enabled = true;
        }

        if(_attacking)
        {
            //There seems to be a problem using the timer
            //Doesn't line up with animation even if set to right value
            //May instead look if animation has finished playing
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
            }
            else
            {
                _attacking = false;
                //TESTING
                foreach (PolygonCollider2D hb in _weaponColliders)
                {
                    hb.enabled = false;
                }

                //Original
                //WeaponCollider.enabled = false;

                //For testing
                _weaponSprite.enabled = false;
            }
        }

        _anim.SetBool("Attacking", _attacking);

        //TESTING
        if(UseAnimation)
        {
            _weaponAnim.SetBool("AttackNoRotate", _attacking);
            _weaponAnim.SetBool("AttackWithRotate", false);
        }
        else
        {
            _weaponAnim.SetBool("AttackWithRotate", _attacking);
            _weaponAnim.SetBool("AttackNoRotate", false);
        }
	}

    public bool IsAttacking()
    {
        return _attacking;
    }
}
