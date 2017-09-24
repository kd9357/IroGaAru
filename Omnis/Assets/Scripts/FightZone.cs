using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Modified from https://www.youtube.com/watch?v=Vrld13ypX_I

public class FightZone : MonoBehaviour {

    public enum SpawnState { SPAWNING, WAITING, COUNTING };

    [System.Serializable]
    public class Wave
    {
        [Tooltip("The identifier for this wave")]
        public string Name;
        [Tooltip("The challenge to be spawned in this wave")]
        public GameObject Challenge;
        [Tooltip("The number of the above challenges to be spawned in this wave")]
        public int Count;
        [Tooltip("The rate at which a new challenge will be spawned, until Count has been reached")]
        public float SpawnRate;
    }

    [Tooltip("Each unique round that occurs after initial challenges destroyed")]
    public Wave[] Waves;
    [Tooltip("The amount of time, after a wave has been completed, before the next wave begins")]
    public float TimeBetweenWaves = 3f;

    //If in the future different criteria other than destroy all is used, must update
    [Tooltip("The initial challenges that must be destroyed before the next wave triggers (If empty starts wave 0 immediately)")]
    public List<GameObject> ChallengeList;
    [Tooltip("The list of spawn points enemies will randomly spawn out of (If empty enemies will spawn from fightzone position)")]
    public Transform[] SpawnPoints;

    private Transform _target;
    private Transform _player;

    private int _nextWave = -1;
    private float _waveCountdown;
    private SpawnState _state = SpawnState.WAITING;

    // Use this for initialization
    void Start () {
        _target = transform.Find("Camera Center");
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update() {
        if (_state == SpawnState.WAITING)
        {
            //Check if enemies are dead
            if (WaveCompleted())
                StartNewWave();
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

    bool WaveCompleted()
    {
        foreach (GameObject challenge in ChallengeList)
        {
            if (challenge != null)
                return false;
        }
        return true;
    }

    //Begin wave, spawning a challenge at a defined SpawnRate
    IEnumerator SpawnWave(Wave wave)
    {
        _state = SpawnState.SPAWNING;
        for(int i = 0; i < wave.Count; i++)
        {
            SpawnChallenge(wave.Challenge);
            //Add to challenge list
            yield return new WaitForSeconds(1f / wave.SpawnRate);
        }
        _state = SpawnState.WAITING;
        yield break;
    }

    //Instantiate a challenge game object either at a spawn point or default position
    void SpawnChallenge(GameObject challenge)
    {
        //Default to center of trigger fight zone
        Transform spawnPos = transform;
        //If we have spawn points set up, randomly choose one of them
        if(SpawnPoints.Length > 0)
            spawnPos = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        ChallengeList.Add(Instantiate(challenge, spawnPos.position, spawnPos.rotation));
    }

    //All waves completed, unlock camera and destroy self
    void UnlockZone()
    {
        Camera.main.GetComponent<CameraFollow>().SetTarget(_player, true);
        Destroy(gameObject);
    }

    #region Collisions

    //When player enters zone, lock camera and begin wave process
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Camera.main.GetComponent<CameraFollow>().SetTarget(_target, false);
            if(_waveCountdown <= 0)
                _waveCountdown = TimeBetweenWaves;
        }
    }

    #endregion
}
