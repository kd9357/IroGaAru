using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hp_display : MonoBehaviour {
	public int hp;
	public int max_hp;
	public Texture[] hp_array;
	public Texture hp_back;

	private int start_x;
	private int start_y;

	void Start () {
		hp = 8;
		max_hp = 8;
		start_x = 32;
		start_y = 16;
		Texture[] hp_array = new Texture[max_hp];
	}
	
	// OnGUI called to draw GUI objects.
	void OnGUI () {
		GUI.DrawTexture(new Rect(start_x,start_y,21,77),hp_back); //Draw background of the bar.
		for (int i = 0; i < hp; i++) {
			GUI.DrawTexture(new Rect(start_x,start_y + (7-i)*9, 21, 14), hp_array[i]); //Draw current HP.
		}
	}
}
