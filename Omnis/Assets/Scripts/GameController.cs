using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public string NextScene = "Start_Screen";
    public bool EndGame = false;
    public WeaponColor EquippedColor;
    [Tooltip("The minimum number of enemies to be defeated before the environment is fully colored")]
    public int MinimumEnemies;
    public Material GrayscaleMaterial;

    private GameObject gameoverCanvas;
    private Image gameoverPanel;
    private Text gameoverText;

    private GameObject pauseCanvas; //May just reuse gameoverCanvas instead

    private GameObject levelCompleteCanvas;
    private Image levelCompletePanel;
    private List<Text> levelCompleteTexts;

    private Player _player;         //To pause the player's inputs
    private AudioSource _audioSourceManager;
    private AudioSource _gameOverAudio;

    private bool _paused;

    // Critical Vars
    private int enemyCount;
    private int attackCount;
    private int enemyHitCount;
    private System.Object enemyLock;
    private System.Object attackLock;
    private System.Object hitLock;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        _paused = false;

#if (UNITY_ANDROID || UNITY_IPHONE)
        Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif
    }

    void Start()
    {
        SetupGameOverCanvas();
        SetupLevelCompleteCanvas();
        SetupPauseCanvas();

        // NOTE: If parent has audiosource, will be counted
        var audiosources = gameObject.GetComponentsInChildren<AudioSource>();
        _gameOverAudio = audiosources[0];
        _audioSourceManager = audiosources[1];   //Assuming music is first audio source in children

        // TODO: Need to change for start screen, since there is no player
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        //Color Transition setup
        GrayscaleMaterial.SetFloat("_AmountColored", 0);

        // Initialize critical section vars
        enemyCount = 0;
        attackCount = 0;
        enemyHitCount = 0;
        enemyLock = new System.Object();
        attackLock = new System.Object();
        hitLock = new System.Object();
    }

    void Update()
    {
        if (EndGame && gameoverPanel.canvasRenderer.GetAlpha() >= .9f)
        {
            _audioSourceManager.volume -= _audioSourceManager.volume > 0f ? Time.deltaTime * 2 : 0f;

            if (Input.anyKey)
            {
                LoadScene(SceneManager.GetActiveScene().name);
            }  
        }
        
        if(Input.GetButtonDown("Cancel"))
        {
            //Eventually setup a pause screen
            TogglePause();
        }
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
        gameoverCanvas = canvas.gameObject;

        var panel = gameoverCanvas.transform.Find("Panel");
        if (panel == null)
        {
            Debug.LogError("Cannot find game over panel");
            return;
        }

        var text = panel.transform.Find("Text");
        if (text == null)
        {
            Debug.LogError("Cannot find game over panel");
            return;
        }

        gameoverPanel = panel.gameObject.GetComponent<Image>();
        gameoverText = text.gameObject.GetComponent<Text>();

        gameoverPanel.canvasRenderer.SetAlpha(0f);
        gameoverText.canvasRenderer.SetAlpha(0f);

        gameoverCanvas.SetActive(false);

    }

    private void SetupLevelCompleteCanvas()
    {
        var canvas = transform.Find("Level Complete Canvas");
        if (canvas == null)
        {
            Debug.LogError("Cannot find level complete canvas");
            return;
        }
        levelCompleteCanvas = canvas.gameObject;

        var panel = levelCompleteCanvas.transform.Find("Panel");
        if (panel == null)
        {
            Debug.LogError("Cannot find level complete panel");
            return;
        }
        levelCompletePanel = panel.gameObject.GetComponent<Image>();

        levelCompleteTexts = new List<Text>();
        Text[] texts = panel.transform.GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            t.canvasRenderer.SetAlpha(0f);
            levelCompleteTexts.Add(t);
        }

        levelCompletePanel.canvasRenderer.SetAlpha(0f);
        levelCompleteCanvas.SetActive(false);
    }

    private void SetupPauseCanvas()
    {
        var canvas = transform.Find("Pause Canvas");
        if (canvas == null)
        {
            Debug.LogError("Cannot find pause canvas");
            return;
        }
        pauseCanvas = canvas.gameObject;
        pauseCanvas.SetActive(false);
    }

    #endregion

    #region Critical Sections

    public void IncrementEnemiesDefeated()
    {
        lock (enemyLock)
        {
            ++enemyCount;
            if (enemyCount <= MinimumEnemies)
            {
                GrayscaleMaterial.SetFloat("_AmountColored", (float)enemyCount / MinimumEnemies);
            }
        }
    }

    public void IncrementAttacksMade()
    {
        lock (attackLock)
        {
            ++attackCount;
        }
    }

    public void IncrementAttacksConnected()
    {
        lock (hitLock)
        {
            ++enemyHitCount;
        }
    }

    #endregion

    #region Pause
    //Used for dialogue at the moment
    public void PauseGame(bool pause)
    {
        _paused = pause;
        Time.timeScale = _paused ? 0 : 1;
    }

    //Used just for pressing esc at the moment
    private void TogglePause()
    {
        _paused = !_paused;
        Time.timeScale = _paused ? 0 : 1;
        _audioSourceManager.mute = _paused;
        //TODO: Disable/Enable player input here
        _player.FreezeMovement(_paused);
        pauseCanvas.SetActive(_paused);
    }
    #endregion

    #region Scene Management

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        //When loading a new scene or restarting on death, make sure to reset colors to gray
        GrayscaleMaterial.SetFloat("_AmountColored", 0);
    }

    public IEnumerator CompleteLevel()
    {
        // Set level complete results
        foreach (Text t in levelCompleteTexts)
        {
            switch (t.gameObject.name)
            {
                case "Enemies Number":
                    t.text = enemyCount.ToString();
                    break;
                case "Swings Number":
                    t.text = attackCount.ToString();
                    break;
                case "Accuracy Number":
                    float accuracy = 100f * enemyHitCount / attackCount;
                    accuracy = float.IsNaN(accuracy) ? 0 : accuracy;
                    t.text = String.Format("{0:0.00}", accuracy);
                    break;
            }
        }

        // Show level complete results
        levelCompleteCanvas.SetActive(true);
        levelCompletePanel.CrossFadeAlpha(2f, 1f, false);
        levelCompleteTexts[0].CrossFadeAlpha(2f, 1f, false);

        yield return new WaitForSeconds(1f);

        for (int k = 1; k < levelCompleteTexts.Count; ++k)
        {
            levelCompleteTexts[k].CrossFadeAlpha(2f, 1.5f, false);
        }

        yield return new WaitForSeconds(5f);

        levelCompletePanel.CrossFadeColor(Color.black, 1f, false, false, true);
        foreach (Text t in levelCompleteTexts)
        {
            t.CrossFadeAlpha(0, .5f, false);
        }

        yield return new WaitForSeconds(2);

        LoadScene(NextScene);
    }

    public void GameOver()
    {
        _gameOverAudio.volume = 1f;
        _gameOverAudio.Play();

        gameoverCanvas.SetActive(true);
        gameoverPanel.CrossFadeAlpha(2f, 1.5f, false);
        gameoverText.CrossFadeAlpha(2f, 1.5f, false);

        EndGame = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion
}