using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hp_display : MonoBehaviour {
    //public int hp;
    //public int max_hp;
    public Player player;
	public Texture[] hp_array;
	public Texture hp_back;

	private int start_x;
	private int start_y;

	void Start () {
		start_x = 32;
		start_y = 16;

        Texture[] hp_array = new Texture[player.MaxHealth];
	}
	
	// OnGUI called to draw GUI objects.
	void OnGUI () {
		GUI.DrawTexture(new Rect(start_x, start_y, 21, 77), hp_back); //Draw background of the bar.
	    int player_health = player.GetCurrentHealth();
        for (int i = 0; i < player_health; i++) { 
			GUI.DrawTexture(new Rect(start_x,start_y + (7-i)*9, 21, 14), hp_array[i]); //Draw current HP.
		}
	}
}
