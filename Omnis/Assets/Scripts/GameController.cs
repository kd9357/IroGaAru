using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	
    public void GameOver()
    {
        EndGame = true;
    }
}