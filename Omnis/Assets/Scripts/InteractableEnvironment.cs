using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableEnvironment : MonoBehaviour {

    protected static List<Color> SpecialColors = new List<Color>
    {
        new Color(.5f, 0f, .5f),        //Purple
        new Color(1f, .5f, 0f),         //Orange
        new Color(0.5f, 0.5f, 0.5f)     //Fake green
        
    };

    #region Public Attributes

    [Tooltip("The amount of health this object has (relevant only for DoT for now)")]
    public int MaxHealth = 1;
    [Tooltip("The color the object will reset to")]
    public Color DefaultColor = Color.white;
    [Tooltip("Time before object returns to default color")]
    public float ColorCooldown = 5f;
    [Tooltip("Force object experiences when hit by player")]
    public float KnockbackForce = 0;
    //Color effects
    [Tooltip("How much damage this object receives each second while Orange")]
    public int BurnDamage = 0;
    [Tooltip("How strong the enemy is knockbacked while Green")]
    public float WindKnockbackForce = 0;

    #endregion

    #region Protected Attributes

    //protected SpriteRenderer _sprite;
    protected SpriteRenderer[] _sprites;
    protected Rigidbody2D _rb;
    protected int _currentHealth;
    protected Color _currentColor;
    protected float _colorTimer;
    protected ColorStatus _currentColorStatus;
    protected float _currentKnockbackForce;
    protected Vector3 _initialPos;
    protected Quaternion _initialRot;

    //Assuming 0: Purple, 1: Orange, 2: Green
    protected ParticleSystem[] _colorParticleEffects;
    #endregion

    // Use this for initialization
    protected virtual void Start () {
        _currentHealth = MaxHealth;
        _sprites = GetComponentsInChildren<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();

        _currentColor = DefaultColor;
        _colorTimer = 0;
        _currentColorStatus = ColorStatus.None;
        _currentKnockbackForce = KnockbackForce;
        _initialPos = transform.position;
        _initialRot = transform.rotation;

        _colorParticleEffects = GetComponentsInChildren<ParticleSystem>();
	}
    
    // Update is called once per frame
    protected virtual void Update () {
        UpdateTimers();
        ResetColorStatus();
        if (_currentHealth <= 0)
            Die();
	}

    #region Helper Methods

    #region Bookkeeping

    protected virtual void UpdateTimers()
    {
        if (_colorTimer > 0)
            _colorTimer -= Time.deltaTime;
    }

    protected virtual void ResetColorStatus()
    {
        if(_colorTimer <= 0 && _currentColor != DefaultColor)
        {
            _colorTimer = 0;
            _currentColor = DefaultColor;
            //_sprite.color = _currentColor;
            foreach (SpriteRenderer sr in _sprites)
            {
                sr.color = _currentColor;
            }
            //Reset ailments
            _currentKnockbackForce = KnockbackForce;
            _currentColorStatus = ColorStatus.None;
            foreach (ParticleSystem ps in _colorParticleEffects)
            {
                if (ps.isPlaying)
                    ps.Stop();
            }
        }
    }

    public virtual void ResetObject()
    {
        transform.position = _initialPos;
        transform.rotation = _initialRot;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Color Management

    public virtual void EnvironmentDamaged(Color color, int direction)
    {
        //Only set the timer on first hit
        if (_colorTimer <= 0)
            _colorTimer = ColorCooldown;

        SetColor(color);

        _rb.AddForce(Vector2.right * direction * _currentKnockbackForce,
                     ForceMode2D.Impulse);
    }

    protected virtual void SetColor(Color color)
    {
        //Only allow color mixing when NOT under some ailment
        if (_currentColorStatus == ColorStatus.None)
        {
            _currentColor = (_currentColor + color) / 2;

            // Now determine other special effects
            Vector3 cc = new Vector3(_currentColor.r, _currentColor.g, _currentColor.b);
            Vector3 sc = Vector3.zero;

            int i;
            float threshold = 0.4f; //For purple
            for (i = 0; i < SpecialColors.Count; i++)
            {
                sc.Set(SpecialColors[i].r, SpecialColors[i].g, SpecialColors[i].b);
                float distance = Vector3.Distance(sc, cc);
                if (i != 0)
                    threshold = 0.34f; //gack, change threshold for orange and green
                if (distance < threshold)
                    break;
            }
            _currentColorStatus = (ColorStatus)(i);
            if (_currentColorStatus != ColorStatus.None)
                ApplyAilment();

            //_sprite.color = _currentColor;
            foreach (SpriteRenderer sr in _sprites)
            {
                sr.color = _currentColor;
            }
        }
    }

    //TODO: add sound effect if using
    protected virtual void ApplyAilment()
    {
        //When special color first applied, reset timer
        _colorTimer = ColorCooldown;
        if (!_colorParticleEffects[(int)_currentColorStatus].isPlaying)
            _colorParticleEffects[(int)_currentColorStatus].Play();
        switch (_currentColorStatus)
        {
            case ColorStatus.Stun:
                ApplyStun();
                return;
            case ColorStatus.WindRecoil:
                ApplyWindRecoil();
                return;
            case ColorStatus.DamageOverTime:
                StartCoroutine(ApplyDamageOverTime());
                return;
        }
    }

    //Stop enemy's movement
    protected virtual void ApplyStun()
    {
        return;
    }

    //Increase enemy's knockback
    protected virtual void ApplyWindRecoil()
    {
        _currentKnockbackForce = WindKnockbackForce;
        _currentColor = Color.green;
    }

    //Reduce the enemy's health by 10% of currentHealth every second (Maxhealth instead?)
    protected virtual IEnumerator ApplyDamageOverTime()
    {
        while (_currentColorStatus == ColorStatus.DamageOverTime)
        {
            if (BurnDamage > 0)
            {
                foreach (SpriteRenderer sr in _sprites)
                {
                    sr.color = (sr.color + Color.black) / 2;
                }
            }
            _currentHealth -= BurnDamage;
            yield return new WaitForSeconds(0.5f);
        }
    }

    #endregion

    #endregion

}
