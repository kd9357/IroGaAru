using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Modified from https://www.youtube.com/watch?v=Vrld13ypX_I

public class FightZone : MonoBehaviour {

    public enum SpawnState { SPAWNING, WAITING, COUNTING, INACTIVE };

    [System.Serializable]
    public class Wave
    {
        [Tooltip("The identifier for this wave")]
        public string Name;
        [Tooltip("The list of challenges to be spawned in this wave")]
        public SpawnDetails[] NewChallenges;
        [Tooltip("The amount of time before the next NewChallenge is spawned")]
        public float Delay = 1f;
    }

    [System.Serializable]
    public class SpawnDetails
    {
        [Tooltip("Drag a prefab for the folder here")]
        public GameObject SpawnObject;
        [Tooltip("Drag an existing spawn point here")]
        public Transform SpawnPoint;
    }

    [Tooltip("Each unique round that occurs after initial challenges destroyed")]
    public Wave[] Waves;
    [Tooltip("The amount of time, after a wave has been completed, before the next wave begins")]
    public float TimeBetweenWaves = 3f;

    //If in the future different criteria other than destroy all is used, must update
    [Tooltip("The initial challenges that must be destroyed before the next wave triggers (If empty starts wave 0 immediately)")]
    public List<GameObject> ChallengeList;

    private Transform _target;
    private Transform _player;

    private int _nextWave = -1;
    private float _waveCountdown;
    private SpawnState _state = SpawnState.INACTIVE;

    // Use this for initialization
    void Start () {
        _target = transform.Find("Camera Center");
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update() {
        if (_state == SpawnState.INACTIVE)
            return;
        if (_state == SpawnState.WAITING)
        {
            //Check if enemies are dead
            if (WaveCompleted())
            {
                StartNewWave();
                return;
            }
            else
                return;
        }

        if(_waveCountdown <= 0)
        {
            if(_state != SpawnState.SPAWNING)
                StartCoroutine(SpawnWave(Waves[_nextWave]));
        }
        else
            _waveCountdown -= Time.deltaTime;
    }

    //Check if all waves completed, otherwise clear list out and increment wave number
    void StartNewWave()
    {
        if(_nextWave + 1 > Waves.Length - 1)
            UnlockZone();
        else
        {
            _state = SpawnState.COUNTING;
            ChallengeList.Clear();
            _waveCountdown = TimeBetweenWaves;
            _nextWave++;
        }
    }

    //Check the list of challenges, if any are not null (still active) false
    bool WaveCompleted()
    {
        foreach (GameObject challenge in ChallengeList)
        {
            if (challenge != null)
                return false;
        }
        return true;
    }

    //Begin wave, spawning its NewChallenges
    IEnumerator SpawnWave(Wave wave)
    {
        _state = SpawnState.SPAWNING;
        for(int i = 0; i < wave.NewChallenges.Length; i++)
        {
            SpawnChallenge(wave.NewChallenges[i]);
            yield return new WaitForSeconds(wave.Delay);
        }
        _state = SpawnState.WAITING;
        yield break;
    }

    //Create the object given the SpawnDetails, defaulting to the center of the fightzone transform
    void SpawnChallenge(SpawnDetails nextObject)
    {
        //Default to center of trigger fight zone
        Transform spawnPosition = nextObject.SpawnPoint != null ? nextObject.SpawnPoint : transform;
        ChallengeList.Add(Instantiate(nextObject.SpawnObject, spawnPosition.position, spawnPosition.rotation));
    }

    //All waves completed, unlock camera and destroy self
    void UnlockZone()
    {
        Camera.main.GetComponent<CameraFollow>().SetTarget(_player, true);
        Destroy(gameObject);
    }

    //When player enters zone, lock camera and begin wave process
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Camera.main.GetComponent<CameraFollow>().SetTarget(_target, false);
            _state = SpawnState.WAITING;
            if(_waveCountdown <= 0)
                _waveCountdown = TimeBetweenWaves;
        }
    }

}
