using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurpleBoss : Enemy {

    //Use this to know the player's location
    //May instead use some static variable?
    public GameObject Player;

    [Tooltip("Amount of time until enemy makes an action")]
    public float ActionCooldown;

    //Components
    private BoxCollider2D _slapBox;

    //Combat variables
    protected float _actionTimer;
    private int _phaseNum;
    private float _defaultMass;
    private Vector3 _defaultScale;
    private float _attackCooldown = 0.3f;
    private float _attackTimer;

    //Movement and Orientation
    private bool _facingRight;


    // Use this for initialization
    protected override void Start()
    {
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();

        _currentHealth = MaxHealth;
        _currentColor = DefaultColor;
        _currentSpeed = Speed;
        _currentKnockbackForce = EnemyKnockbackForce;
        _recoilTimer = 0;
        _actionTimer = ActionCooldown;  //Maybe set this zero until enemy is ready

        _phaseNum = 0;
        _defaultMass = _rb.mass;
        _defaultScale = transform.localScale;
        _attackTimer = 0;

        _slapBox = gameObject.GetComponent<BoxCollider2D>();
        _slapBox.enabled = false;

        //For testing purposes
        _textMesh = gameObject.GetComponentInChildren<TextMesh>();
    }

    #region Updates
    //Will want to use this kind of AI on generic enemies
    private void FixedUpdate()
    {
        //This unfortunately flips the debug text as well
        if ((Player.transform.position.x - transform.position.x > 0 && !_facingRight) 
            || (Player.transform.position.x - transform.position.x < 0 && _facingRight))
            Flip();

        //Update behavior times
        if (_recoilTimer <= 0)
        {
            if (_actionTimer > 0)
            {
                _actionTimer -= Time.deltaTime;
            }
            else if (_actionTimer < 0)
            {
                _actionTimer = -1;  //Ensure this statement continues to play each frame until action has ended
                //Move to player
                //Need to account for changing size
                if (Vector3.Distance(transform.position, Player.transform.position) > 1.5)
                {
                    if (_facingRight)
                        _rb.velocity = new Vector2(Speed, _rb.velocity.y);
                    else
                        _rb.velocity = new Vector2(-Speed, _rb.velocity.y);
                }
                //Once in range, attack
                else
                {
                    _actionTimer = 0;
                    _anim.SetTrigger("Attack");
                    //SlapAttack();
                }
            }
        }
    }

    void Update()
    {
        //Update stagger/knockback time
        if (_recoilTimer > 0)
            _recoilTimer -= Time.deltaTime;

        //Update attack times
        if (_attackTimer < 0)
        {
            _slapBox.enabled = false;
            _attackTimer = 0;
        }
        else
            _attackTimer -= Time.deltaTime;

        //Update status ailment time
        if (_colorTimer > 0)
        {
            _colorTimer -= Time.deltaTime;
        }
        else if (_colorTimer < 0)
        {
            _colorTimer = 0;
            _currentColor = DefaultColor;
            _sprite.color = _currentColor;
            _currentSpeed = Speed;
            _currentKnockbackForce = EnemyKnockbackForce;
            _currentStatus = ColorStatus.None;
        }

        //Update health status
        if (_currentHealth <= 0)
        {
            //For now just destroy object
            _anim.SetTrigger("Death");
            Destroy(gameObject);
        }

        if (DebugMode)
        {
            //Debug stuff
            string message = "";
            message += "HP: " + _currentHealth.ToString("F2") + "/" + MaxHealth + "\n";
            message += "AI: " + AI_Type + "\n";
            message += "Color: (" + _currentColor.r + ", " + _currentColor.g + ", " + _currentColor.b + ")\n";
            message += "Status: " + _currentStatus + "\n";
            message += "Color Timer: " + _colorTimer.ToString("F2");
            _textMesh.text = message;
        }
        else
        {
            _textMesh.text = "";
        }
    }
    #endregion

    #region Helper Methods

    private void SlapAttack()
    {
        //Enable hurtbox
        _slapBox.enabled = true;
        _attackTimer = _attackCooldown;
        _actionTimer = ActionCooldown;
    }

    public override void EnemyDamaged(int damage, Color color, int direction)
    {
        //Only set the timer on first hit
        if (_colorTimer == 0)
            _colorTimer = ColorCooldown;

        //Only allow color mixing when not under some ailment
        if (_currentStatus == ColorStatus.None)
        {
            _currentColor = (_currentColor + color) / 2;

            // Now determine other special effects
            Vector3 cc = new Vector3(_currentColor.r, _currentColor.g, _currentColor.b);
            Vector3 sc = Vector3.zero;

            //ignore i == 0, purple
            int i;
            float threshold = 0.34f;
            for (i = 1; i < SpecialColors.Count; i++)
            {
                sc.Set(SpecialColors[i].r, SpecialColors[i].g, SpecialColors[i].b);
                float distance = Vector3.Distance(sc, cc);
                if (distance < threshold)
                {
                    Debug.Log("Applying " + (ColorStatus)(i));
                    break;
                }

            }
            _currentStatus = (ColorStatus)(i);
            if (_currentStatus != ColorStatus.None)
                ApplyAilment();
        }

        _sprite.color = _currentColor;
        _rb.AddForce(Vector2.right * direction * _currentKnockbackForce, ForceMode2D.Impulse);
    }

    //Ignore purple effect
    protected override void ApplyAilment()
    {
        //When special color first applied, reset timer
        _colorTimer = ColorCooldown;
        switch (_currentStatus)
        {
            case ColorStatus.WindRecoil:
                _currentKnockbackForce *= _currentKnockbackForce;
                Debug.Log("Force: " + _currentKnockbackForce);
                //For now, knockback ^ 2 for boss
                //May change this to a one time hit
                //Don't mess with mass since we want the boss to be variably affected due to his increased mass
                _currentColor = Color.green;
                return;
            case ColorStatus.DamageOverTime:
                //Instead of damage, shrink to some minimum value
                StartCoroutine(ShrinkOverTime());
                return;
        }
    }

    IEnumerator ShrinkOverTime()
    {
        float timer = 0;
        float startMass = _rb.mass;
        Vector3 startScale = transform.localScale;

        while (_currentStatus == ColorStatus.DamageOverTime)
        {
            float proportionCompleted = timer / ColorCooldown;
            _rb.mass = Mathf.Lerp(startMass, _defaultMass, proportionCompleted);
            //Need to account for flip while shrinking
            if((startScale.x > 0 && _facingRight) || (startScale.x < 0 && !_facingRight))
                startScale.x *= -1;
            //Shrinking doesn't change location, so each time this calls the boss drops a bit
            //Basically it looks really shaky
            transform.localScale = Vector3.Lerp(startScale, _defaultScale, proportionCompleted);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    //When shocked, jump back to middle of arena and grow in size
    IEnumerator StartNextPhase()
    {
        //Should change color to purple?
        _anim.SetTrigger("Shocked");
        yield return new WaitForSeconds(1.5f);
        //Reset status
        _currentStatus = ColorStatus.None;
        _currentColor = DefaultColor;
        _sprite.color = _currentColor;
        //Need to jump back into arena
        //For now just move directly above
        transform.position = new Vector3(10, 40, 0);
        //scale size
        StartCoroutine(GrowOverTime());
    }

    IEnumerator GrowOverTime()
    {
        _anim.SetBool("Growing", true);
        float timer = 0;
        float endMass = _defaultMass * 10;
        Vector3 endScale = new Vector3(transform.localScale.x * 2, transform.localScale.y * 2, 1);
        //For now, just grow to twice size, 10 times mass over 5 seconds
        while (timer < 3f)
        {
            float proportionCompleted = timer / 5f;
            _rb.mass = Mathf.Lerp(_defaultMass, endMass, proportionCompleted);
            //Need to account for flip while shrinking
            if ((endScale.x > 0 && _facingRight) || (endScale.x < 0 && !_facingRight))
                endScale.x *= -1;
            transform.localScale = Vector3.Lerp(_defaultScale, endScale, proportionCompleted);
            timer += Time.deltaTime;
            yield return null;
        }
        _anim.SetBool("Growing", false);
    }

    void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        _defaultScale.x *= -1;

        //Delay Enemy's actions
        if(_actionTimer != 0)
            _actionTimer = ActionCooldown / 2;
    }
    #endregion

    #region Collisions
    // DON'T Hurt player on contact
    //Only care about water and knockback
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        //Check if enter water
        if(collision.gameObject.CompareTag("Hazard") && _recoilTimer <= 0)
        {
            _currentHealth--;
            _phaseNum++;
            _recoilTimer = RecoilCooldown;
            StartCoroutine(StartNextPhase());
        }
    }

    //Right now just override enemy's method
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        return;
    }

    //Slap connected
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with " + collision.tag);
        if(collision.CompareTag("Player"))
        {
            int direction = collision.transform.position.x < transform.position.x ? -1 : 1;
            collision.gameObject.GetComponent<Player>().Knockback(!_facingRight);
        }
    }
    #endregion


}
