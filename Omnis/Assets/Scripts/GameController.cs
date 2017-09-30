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

    private const string START_SCREEN = "Start_Screen";

    private GameObject gameoverCanvas;
    private Image gameoverPanel;
    private Text gameoverText;

    private bool _paused;
    private GameObject pauseCanvas; //May just reuse gameoverCanvas instead
    private Player _player;         //To pause the player's inputs
    private AudioSource _audio;

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

        //for pause screen
        canvas = transform.Find("Pause Canvas");
        if (canvas == null)
        {
            Debug.LogError("Cannot find pause canvas");
            return;
        }
        pauseCanvas = canvas.gameObject;

        pauseCanvas.SetActive(false);

        var audiosources = gameObject.GetComponentsInChildren<AudioSource>();
        _audio = audiosources[1];   //Assuming music is first audio source in children

        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    void Update()
    {
        if (EndGame && gameoverPanel.canvasRenderer.GetAlpha() >= .9f)
        {
            if (Input.anyKey)
            {
                LoadScene(START_SCREEN);
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
        _audio.mute = _paused;
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
        // Only sound available is gameover sound
        GetComponent<AudioSource>().Play();

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