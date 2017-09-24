using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightZone : MonoBehaviour {
    
    //If in the future different criteria other than destroy all used, must update
    [Tooltip("The list of enemies or objects to be destroyed before moving on")]
    public GameObject[] Challenges;
    private Camera _camera;
    private Transform _target;
    private Transform _player;

    //testing as public first
    public bool _unlock = false;

	// Use this for initialization
	void Start () {
        _camera = Camera.main;
        _target = gameObject.transform.GetChild(0).transform;
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }
	
	// Update is called once per frame
	void Update () {
        //TODO: find a way to restrict Player movement while locked on
        //suggestions: activate box colliders around camera frustum
        //             check if player position outside frustum -> clamp position

        _unlock = true;
        foreach(GameObject go in Challenges)
        {
            if(go != null)
            {
                _unlock = false;
                break;
            }
        }
        if (_unlock)
            _camera.GetComponent<CameraFollow>().SetTarget(_player, true);
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _camera.GetComponent<CameraFollow>().SetTarget(_target, false);
    }
}
