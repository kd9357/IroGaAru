using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightZone : MonoBehaviour {
    
    //TODO: just use a separate spawner script

    //If in the future different criteria other than destroy all used, must update
    [Tooltip("The list of enemies or objects to be destroyed before moving on")]
    public List<GameObject> Challenges;
    [Tooltip("The list enemy or object that will spawn after a wave")]
    public GameObject ReserveChallenge;
    [Tooltip("The total number of waves of enemies to be destroyed")]
    public int Waves = 1;

    private Camera _camera;
    private Transform _target;
    private Transform _player;
    private Transform _spawnPoint;

    private int _currentWave = 0;

	// Use this for initialization
	void Start () {
        _camera = Camera.main;
        _target = transform.Find("Camera Center");
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _spawnPoint = transform.Find("Spawn Point");
    }
	
	// Update is called once per frame
	void Update () {
        if (WaveCompleted())
        {
            //Move on to next wave
            if(_currentWave < Waves)
            {
                //Clear out list
                Challenges.Clear();
                //Spawn more enemies
                Challenges.Add(Instantiate(ReserveChallenge, _spawnPoint.position, Quaternion.identity));
                Debug.Log("Added challenge, count: " + Challenges.Count);
                _currentWave++;
            }
            else
                UnlockZone();
        }
            
	}

    void UnlockZone()
    {
        _camera.GetComponent<CameraFollow>().SetTarget(_player, true);
        Destroy(gameObject);
    }

    bool WaveCompleted()
    {
        foreach(GameObject go in Challenges)
        {
            if (go != null)
                return false;
        }
        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _camera.GetComponent<CameraFollow>().SetTarget(_target, false);
    }
}
