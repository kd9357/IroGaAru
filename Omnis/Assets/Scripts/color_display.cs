using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class color_display : MonoBehaviour {

	// Use this for initialization
	public int active_color;
	public int inactive_left_color;
	public int inactive_right_color;
	public int start_x;
	public int start_y;
	public int adjust_x;
	public int adjust_y;
	public Color[] colors;
	public int max_colors;
	public Texture active_ring;
	public Texture side_ring;
	public Texture2D selected_color;
	public Texture2D left_color;
	public Texture2D right_color;
	private Texture2D shift_texture;
	private Texture2D shift_texture_left;
	private Texture2D shift_texture_right;
	void Start () {
		active_color = 0;
		inactive_left_color = 2;
		inactive_right_color = 1;
		max_colors = 3;
		start_x = 32;
		start_y = 160;
		adjust_x = 5;
		adjust_y = 5;
		colors = new Color[max_colors];
		colors[0] = new Color(1F,0F,0F);
		colors[1] = new Color(1F,1F,0F);
		colors[2] = new Color(0F,0F,1F);
	}
	
	// Update is called once per frame
	void Update() {
		if (Input.GetButtonUp ("Fire1")) {
			CycleLeft ();
		}
		if (Input.GetButtonUp ("Fire2")) {
			CycleRight ();
		}
	}

	void OnGUI () {
		SetColor ();
		GUI.DrawTexture (new Rect(start_x+adjust_x, start_y+adjust_y, 32, 32), selected_color);
		GUI.DrawTexture (new Rect(start_x-15+1, start_y+1, 10, 10), left_color);
		GUI.DrawTexture (new Rect(start_x+35+1, start_y+1, 10, 10), right_color);
		GUI.DrawTexture (new Rect(start_x, start_y, 39,39), active_ring);
		GUI.DrawTexture (new Rect (start_x - 15, start_y, 62, 12), side_ring);
	}
	void SetColor (){
		shift_texture = selected_color;
		Color[] new_textures = shift_texture.GetPixels(0,0,shift_texture.width, shift_texture.height);
		for (int i = 0; i < new_textures.Length; i++) {
			new_textures [i].r = colors [active_color].r;
			new_textures [i].g = colors [active_color].g;
			new_textures [i].b = colors [active_color].b;
		}
		shift_texture.SetPixels (0, 0, shift_texture.width, shift_texture.height, new_textures);
		shift_texture.Apply();
		selected_color = shift_texture;


		shift_texture = left_color;
		new_textures = shift_texture.GetPixels(0,0,shift_texture.width, shift_texture.height);
		for (int i = 0; i < new_textures.Length; i++) {
			new_textures [i].r = colors [inactive_left_color].r;
			new_textures [i].g = colors [inactive_left_color].g;
			new_textures [i].b = colors [inactive_left_color].b;
		}
		shift_texture.SetPixels (0, 0, shift_texture.width, shift_texture.height, new_textures);
		shift_texture.Apply();
		left_color = shift_texture;

		shift_texture = right_color;
		new_textures = shift_texture.GetPixels(0,0,shift_texture.width, shift_texture.height);
		for (int i = 0; i < new_textures.Length; i++) {
			new_textures [i].r = colors [inactive_right_color].r;
			new_textures [i].g = colors [inactive_right_color].g;
			new_textures [i].b = colors [inactive_right_color].b;
		}
		shift_texture.SetPixels (0, 0, shift_texture.width, shift_texture.height, new_textures);
		shift_texture.Apply();
		right_color = shift_texture;
	}

	void CycleLeft(){
		int i = active_color;
		int left, right, current;
		right = active_color;
		if(i-1 < 0){
			current = max_colors - 1;
		}
		else{
			current = i - 1;
		}

		if (current - 1 < 0) {
			left = max_colors - 1;
		} 
		else {
			left = current - 1;
		}

		active_color = current;
		inactive_left_color = left;
		inactive_right_color = right;
	}

	void CycleRight(){
		int i = active_color;
		int left, right, current;
		left = active_color;
		if(i+1 >= max_colors){
			current = 0;
		}
		else{
			current = i + 1;
		}

		if (current + 1 >= max_colors) {
			right = 0;
		} 
		else {
			right = current + 1;
		}

		active_color = current;
		inactive_left_color = left;
		inactive_right_color = right;
	}
}