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

    private int previous_hp;

    void Start () {
        GUIElement[] hp_array = new GUIElement[player.MaxHealth];
        previous_hp = 8;
    }
	
	// OnGUI called to draw GUI objects.
	void OnGUI () {
	    int player_health = player.GetCurrentHealth();
        if(player_health < previous_hp)
        {
            
            for (int i = previous_hp; i > player_health; i--)
            {
                
                DamageBlocks a = hp_array[i-1].GetComponent<DamageBlocks>();
                a.Fall();
            }
        }
        else if (player_health > previous_hp)
        {
            for (int i = previous_hp; i < player_health; i++)
            {
                print(i);
                DamageBlocks a = hp_array[i].GetComponent<DamageBlocks>();
                a.Reset();
                hp_array[i].transform.gameObject.SetActive(true);
            }
        }
        previous_hp = player_health;
        for (int i = 0; i < player.MaxHealth; i++) {
            if (i < player_health)
            {
                if (player_health <= danger_number)
                {
                    hp_array[i].color = new Color(1,0,0);
                }
                else
                {
                    hp_array[i].color = new Color(1,1,1);
                }
            }
            
		}
    }

}
