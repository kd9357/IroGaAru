using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public bool EndGame = false;
    public WeaponColor EquippedColor;

    //private Player player;

    void Awake () {
		if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
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

        EndGame = true;

        // TODO: Make a real restart menu
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}