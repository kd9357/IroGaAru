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