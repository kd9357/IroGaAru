using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaBehavior : MonoBehaviour {

    [Tooltip("The lava death object to be moved (should be in children)")]
    public GameObject Lava;
    [Tooltip("The speed at which the lava climbs")]
    public float Speed = 0.01f;
    [Tooltip("The maximum distance the lava may go up to")]
    public float MaxDistance;

    private bool _active;
    private Vector3 _initialPos;
    private Quaternion _initialRot;
    private float _distanceTraveled = 0;

    private Transform _target;
    private Transform _player;
    private CameraFollow _camScript;

    // Use this for initialization
    void Start () {
        _initialPos = Lava.transform.position;
        _initialRot = Lava.transform.rotation;
        _target = transform.Find("Camera Center");
        if (_target == null)
        {
            Debug.LogError("Camera lock for fight zone not found");
            return;
        }
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        if (_player == null)
        {
            Debug.LogError("Player does not exist");
            return;
        }
        _camScript = Camera.main.GetComponent<CameraFollow>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!_active)
            return;
        if (_distanceTraveled < MaxDistance)
        {
            Lava.transform.position += Vector3.up * Speed;
            _distanceTraveled += Speed;
        }
        else
        {
            Unlock();
        }
	}

    public void Unlock()
    {
        _camScript.SetTarget(_player, true);
        _camScript.EnableWalls(false);
        _active = false;
    }
    public void ResetObject()
    {
        _active = false;
        Lava.transform.position = _initialPos;
        Lava.transform.rotation = _initialRot;
        _distanceTraveled = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !_active)
        {
            _active = true;
            _camScript.SetTarget(_target, false);
            _camScript.EnableWalls(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Unlock();
        }
    }
}
