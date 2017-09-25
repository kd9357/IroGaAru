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

    public bool _paused;
    private GameObject pauseCanvas; //May just reuse gameoverCanvas instead
    private Image pausePanel;
    private Text pauseText;

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

        panel = gameoverCanvas.transform.Find("Pause Panel");
        if (panel == null)
        {
            Debug.LogError("Cannot find pause panel");
            return;
        }
        text = panel.transform.Find("Pause Text");
        if (text == null)
        {
            Debug.Log("Cannot find pause text");
            return;
        }
        pausePanel = panel.gameObject.GetComponent<Image>();
        pauseText = text.gameObject.GetComponent<Text>();

        pauseCanvas.SetActive(false);


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

    //In the event we need to pause outside of controller (ie dialogue), have these public methods
    public void PauseGame()
    {
        _paused = true;
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        _paused = false;
        Time.timeScale = 1;
    }

    //Used just for pressing esc at the moment
    private void TogglePause()
    {
        _paused = !_paused;
        Time.timeScale = _paused ? 0 : 1;
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