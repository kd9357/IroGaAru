﻿// TeamTwo

/*
 * Include Files
 */

using System;
using System.Collections;
using System.Collections.Generic;
using CnControls;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * Typedefs
 */

public class GameController : MonoBehaviour
{
    /*
     * Public Member Varialbes
     */

    public static GameController Instance;

    public string NextScene = "Start_Screen";
    public bool EndGame = false;
    public WeaponColor EquippedColor;
    [Tooltip("The minimum number of enemies to be defeated before the environment is fully colored")]
    public int MinimumEnemies;
    public Material GrayscaleMaterial;

    /*
     * Private Member Variables
     */

    // Game Over Canvas
    private GameObject _gameoverCanvas;
    private Image _gameoverPanel;
    private Text _gameoverText;

    // Pause Canvas
    private GameObject _pauseCanvas; //May just reuse gameoverCanvas instead

    // Level Complete Canvas
    private GameObject _levelCompleteCanvas;
    private Image _levelCompletePanel;
    private List<Text> _levelCompleteTexts;

    // Load Level Canvas
    private GameObject _loadLevelCanvas;
    private Image _loadLevelPanel;
    private Text _loadLevelText;

    private Player _player;         //To pause the player's inputs
    private AudioSource _audioSourceManager;
    private AudioSource _gameOverAudio;

    private Vector3 _lastCheckpointPos;
    private GameObject[] _enemyObjects;
    private GameObject[] _itemObjects;
    private GameObject[] _interactableObjects;
    private GameObject[] _fightZoneObjects;


    private bool _paused;
    private bool _loadScene;

    // Critical Vars
    private int _enemyCount;
    private int _attackCount;
    private int _enemyHitCount;
    private object _enemyLock;
    private object _attackLock;
    private object _hitLock;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            gameObject.SetActive(false);
            return;
        }

        // DON'T TOUCH
//        DontDestroyOnLoad(gameObject);

#if (UNITY_ANDROID || UNITY_IPHONE)
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        GameObject mUI = GameObject.Find("Mobile UI");
        if (mUI != null)
        {
            mUI.SetActive(true);
        }
#else
        var mUI = GameObject.Find("Mobile UI");
        if (mUI != null)
            mUI.SetActive(false);
#endif
    }

    private void Start()
    {
        SetupGameOverCanvas();
        SetupLevelCompleteCanvas();
        SetupPauseCanvas();
        SetupLoadLevelCanvas();

        // NOTE: If parent has audiosource, will be counted
        var audiosources = gameObject.GetComponentsInChildren<AudioSource>();
        _gameOverAudio = audiosources[0];
        _audioSourceManager = audiosources[1];   //Assuming music is first audio source in children

        // TODO: Need to change for start screen, since there is no player
        //  kd: evidently start gets called each time a new scene is loaded
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            _player = playerGO.GetComponent<Player>();
        else
            Debug.LogError("There is no Player in the scene!");

        //Color Transition setup
        GrayscaleMaterial.SetFloat("_AmountColored", 0);

        _lastCheckpointPos = _player.transform.position;
        _enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        _itemObjects = GameObject.FindGameObjectsWithTag("Item");
        _interactableObjects = GameObject.FindGameObjectsWithTag("Interactable Environment");
        _fightZoneObjects = GameObject.FindGameObjectsWithTag("Fight Zone");


        _paused = false;
        _loadScene = false;

        // Initialize critical section vars
        _enemyCount = 0;
        _attackCount = 0;
        _enemyHitCount = 0;
        _enemyLock = new object();
        _attackLock = new object();
        _hitLock = new object();
    }

    #region Canvas Setups

    private void SetupGameOverCanvas()
    {
        var canvas = transform.Find("Game Over Canvas");
        if (canvas == null)
        {
            Debug.LogError("Cannot find game over canvas");
            return;
        }
        _gameoverCanvas = canvas.gameObject;

        var panel = _gameoverCanvas.transform.Find("Panel");
        if (panel == null)
        {
            Debug.LogError("Cannot find game over panel");
            return;
        }

        var text = panel.transform.Find("Text");
        if (text == null)
        {
            Debug.LogError("Cannot find game over text");
            return;
        }

        _gameoverPanel = panel.gameObject.GetComponent<Image>();
        _gameoverText = text.gameObject.GetComponent<Text>();

        _gameoverPanel.canvasRenderer.SetAlpha(0f);
        _gameoverText.canvasRenderer.SetAlpha(0f);

        _gameoverCanvas.SetActive(false);

    }

    private void SetupLevelCompleteCanvas()
    {
        var canvas = transform.Find("Level Complete Canvas");
        if (canvas == null)
        {
            Debug.LogError("Cannot find level complete canvas");
            return;
        }
        _levelCompleteCanvas = canvas.gameObject;

        var panel = _levelCompleteCanvas.transform.Find("Panel");
        if (panel == null)
        {
            Debug.LogError("Cannot find level complete panel");
            return;
        }
        _levelCompletePanel = panel.gameObject.GetComponent<Image>();

        _levelCompleteTexts = new List<Text>();
        var texts = panel.transform.GetComponentsInChildren<Text>();
        foreach (var t in texts)
        {
            t.canvasRenderer.SetAlpha(0f);
            _levelCompleteTexts.Add(t);
        }

        _levelCompletePanel.canvasRenderer.SetAlpha(0f);
        _levelCompleteCanvas.SetActive(false);
    }

    private void SetupPauseCanvas()
    {
        var canvas = transform.Find("Pause Canvas");
        if (canvas == null)
        {
            Debug.LogError("Cannot find pause canvas");
            return;
        }
        _pauseCanvas = canvas.gameObject;
        _pauseCanvas.SetActive(false);
    }

    private void SetupLoadLevelCanvas()
    {
        var canvas = transform.Find("Load Level Canvas");
        if (canvas == null)
        {
            Debug.LogError("Cannot find load level canvas");
            return;
        }
        _loadLevelCanvas = canvas.gameObject;

        var panel = _loadLevelCanvas.transform.Find("Panel");
        if (panel == null)
        {
            Debug.LogError("Cannot find load level panel");
            return;
        }

        var text = panel.transform.Find("Text");
        if (text == null)
        {
            Debug.LogError("Cannot find load level text");
            return;
        }

        _loadLevelPanel = panel.gameObject.GetComponent<Image>();
        _loadLevelText = text.gameObject.GetComponent<Text>();

        _loadLevelPanel.canvasRenderer.SetAlpha(0f);
        _loadLevelText.canvasRenderer.SetAlpha(0f);

        _loadLevelCanvas.SetActive(false);

    }

    #endregion

    private void Update()
    {
        // Visually indicate that level is actively loading
        if (_loadScene)
            _loadLevelText.canvasRenderer.SetAlpha(Mathf.PingPong(Time.time, 1f));

        if (EndGame && _gameoverPanel.canvasRenderer.GetAlpha() >= .9f)
        {
            _audioSourceManager.volume -= _audioSourceManager.volume > 0f ? Time.deltaTime * 2 : 0f;

            if (Input.anyKey)
            {
                EndGame = false;
                LoadCheckpoint();
            }
        }
#if UNITY_ANDROID || UNITY_IPHONE
        if (CnInputManager.GetButtonDown("Cancel"))
        {
            //Eventually setup a pause screen
            TogglePause();
        }
#else
        if (Input.GetButtonDown("Cancel"))
            TogglePause();
#endif
    }

    #region Critical Sections

    public void IncrementEnemiesDefeated()
    {
        lock (_enemyLock)
        {
            ++_enemyCount;
            if (_enemyCount <= MinimumEnemies)
                GrayscaleMaterial.SetFloat("_AmountColored", (float)_enemyCount / MinimumEnemies);
        }
    }

    public void IncrementAttacksMade()
    {
        lock (_attackLock)
        {
            ++_attackCount;
        }
    }

    public void IncrementAttacksConnected()
    {
        lock (_hitLock)
        {
            ++_enemyHitCount;
        }
    }

    #endregion

    public void UpdateLastCheckpoint(Vector3 pos)
    {
        _lastCheckpointPos = new Vector3(pos.x, pos.y + 1f, pos.z);
    }

    #region Pause
    //Used for dialogue at the moment
    public void PauseGame(bool pause)
    {
        _paused = pause;
        Time.timeScale = _paused ? 0 : 1;
    }

    //Used just for pressing esc at the moment
    public void TogglePause()
    {
        if (SceneManager.GetActiveScene().name != "Start_Screen")
        {
            _paused = !_paused;
            Time.timeScale = _paused ? 0 : 1;
            _audioSourceManager.mute = _paused;
            if (_player != null)
                _player.FreezeMovement(_paused);
            _pauseCanvas.SetActive(_paused);
        }
    }
    #endregion

    #region Scene Management

    public void LoadScene(string sceneName)
    {
        _loadScene = true;
        GrayscaleMaterial.SetFloat("_AmountColored", 0);

        _loadLevelCanvas.SetActive(true);
        _loadLevelPanel.CrossFadeAlpha(2f, 1.5f, false);

        StartCoroutine(LoadLevelInBackground(sceneName));
    }

    private IEnumerator LoadLevelInBackground(string sceneName)
    {
        // NOTE: This is artifical because the game loads so quickly
        yield return new WaitForSeconds(3);

        var loadAsync = SceneManager.LoadSceneAsync(sceneName);
        yield return loadAsync;

        _loadScene = false;
    }

    public void ReturnToStart()
    {
        TogglePause();  //Turn off pause screen before returning to start

        _loadLevelCanvas.SetActive(true);
        _loadLevelPanel.CrossFadeAlpha(2f, 1.5f, false);

        StartCoroutine(LoadLevelInBackground("Start_Screen"));

    }
    public IEnumerator CompleteLevel()
    {
        // Set level complete results
        foreach (var t in _levelCompleteTexts)
            switch (t.gameObject.name)
            {
                case "Enemies Number":
                    t.text = _enemyCount.ToString();
                    break;
                case "Swings Number":
                    t.text = _attackCount.ToString();
                    break;
                case "Accuracy Number":
                    var accuracy = 100f * _enemyHitCount / _attackCount;
                    accuracy = float.IsNaN(accuracy) ? 0 : accuracy;
                    t.text = string.Format("{0:0.00}", accuracy);
                    break;
            }

        // Show level complete results
        _levelCompleteCanvas.SetActive(true);
        _levelCompletePanel.CrossFadeAlpha(2f, 1f, false);
        _levelCompleteTexts[0].CrossFadeAlpha(2f, 1f, false);

        yield return new WaitForSeconds(1f);

        for (var k = 1; k < _levelCompleteTexts.Count; ++k)
            _levelCompleteTexts[k].CrossFadeAlpha(2f, 1.5f, false);

        yield return new WaitForSeconds(3f);

        _levelCompletePanel.CrossFadeColor(Color.black, 1f, false, false, true);
        foreach (var t in _levelCompleteTexts)
            t.CrossFadeAlpha(0, .5f, false);

        yield return new WaitForSeconds(2);

        LoadScene(NextScene);
    }

    public void GameOver()
    {
        _gameOverAudio.volume = 1f;
        _gameOverAudio.Play();

        _gameoverCanvas.SetActive(true);

        _gameoverPanel.canvasRenderer.SetAlpha(0f);
        _gameoverText.canvasRenderer.SetAlpha(0f);
        _gameoverPanel.CrossFadeAlpha(2f, 1.5f, false);
        _gameoverText.CrossFadeAlpha(2f, 1.5f, false);

        EndGame = true;   
    }

    private void LoadCheckpoint()
    {
        // Reset enemies
        foreach (var e in _enemyObjects)
        {
            if (e.GetComponent<Enemy>())
                e.GetComponent<Enemy>().Clear();
            else if (e.GetComponent<FlyingEnemy>())
                e.GetComponent<FlyingEnemy>().Clear();
            else if (e.GetComponent<Kappa>())
                e.GetComponent<Kappa>().Clear();
            else if (e.GetComponent<Spearfish>())
                e.GetComponent<Spearfish>().Clear();
        }

        // Reset items
        foreach (var i in _itemObjects)
            if (i.GetComponent<Item>())
                i.GetComponent<Item>().ResetItem();

        // Reset interactable objects
        foreach (var i in _interactableObjects)
        {
            if (i != null && i.GetComponent<InteractableEnvironment>()) //This doesn't reset burned trees!
                i.GetComponent<InteractableEnvironment>().ResetObject();
        }

        // Reset fight zones and camera
        Camera.main.GetComponent<CameraFollow>().SetTarget(_player.gameObject.transform, true);
        foreach (var fz in _fightZoneObjects)
            if (fz.GetComponent<FightZone>())
                fz.GetComponent<FightZone>().UnlockZone();

        StartCoroutine(FadeOutGameOver());
    }

    private IEnumerator FadeOutGameOver()
    {
        _player.gameObject.SetActive(true);
        _player.TeleportToPosition(_lastCheckpointPos);

        _gameoverText.CrossFadeAlpha(0f, .5f, false);
        _gameoverPanel.CrossFadeAlpha(0f, 2f, false);
        while (_gameoverPanel.canvasRenderer.GetAlpha() >= .01f)
            yield return null;

        _gameoverCanvas.SetActive(false);

        _audioSourceManager.volume = 1f;
        _audioSourceManager.Play();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion
}