using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public bool EndGame = false;
    public WeaponColor EquippedColor;

    private GameObject gameoverCanvas;
    private Image gameoverPanel;
    private Text gameoverText;

    private GameObject pauseCanvas; //May just reuse gameoverCanvas instead
    private Player _player;         //To pause the player's inputs
    private AudioSource _audioSourceManager;
    private AudioSource _gameOverAudio;

    private bool _paused;

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
        // Game Over Setup
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

        // Pause Screen Setup
        canvas = transform.Find("Pause Canvas");
        if (canvas == null)
        {
            Debug.LogError("Cannot find pause canvas");
            return;
        }
        pauseCanvas = canvas.gameObject;

        pauseCanvas.SetActive(false);

        // NOTE: If parent has audiosource, will be counted
        var audiosources = gameObject.GetComponentsInChildren<AudioSource>();
        _gameOverAudio = audiosources[0];
        _audioSourceManager = audiosources[1];   //Assuming music is first audio source in children

        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
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

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
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
}