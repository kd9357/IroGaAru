using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Better to have one enum color instead of two?
public enum WeaponColor
{
    red, yellow, blue
};

public enum EnemyColor
{
    red, yellow, blue, orange, green, purple
};

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
	
	// Update is called once per frame
	void Update () {
 
	}

    public void GameOver()
    {
        EndGame = true;
    }

    public Color GetColor(WeaponColor color)
    {
        switch (color)
        {
            case WeaponColor.red:
                {
                    return new Color(1f, 0, 0);
                }
            case WeaponColor.yellow:
                {
                    return new Color(1f, 1f, 0);
                }
            case WeaponColor.blue:
                {
                    return new Color(0, 0, 1f);
                }
            default:
                {
                    return new Color(0, 0, 0);
                }
        }
    }
}