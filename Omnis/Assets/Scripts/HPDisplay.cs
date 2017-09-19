using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPDisplay : MonoBehaviour {
    //public int hp;
    //public int max_hp;
    public Player player;
    public Image[] hp_array;
    public int danger_number;

    void Start () {
        GUIElement[] hp_array = new GUIElement[player.MaxHealth];
	}
	
	// OnGUI called to draw GUI objects.
	void OnGUI () {
	    int player_health = player.GetCurrentHealth();
        for (int i = 0; i < player.MaxHealth; i++) {
            if (i < player_health)
            {
                hp_array[i].enabled = true;
                if (player_health <= danger_number)
                {
                    hp_array[i].color = new Color(1,0,0);
                }
                else
                {
                    hp_array[i].color = new Color(1,1,1);
                }
            }
            else
            {
                hp_array[i].enabled = false;
            }
            
		}
	}
}
