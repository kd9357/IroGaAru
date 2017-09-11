using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class color_display : MonoBehaviour {

    //It may be better to handle weapon switching in GameController instead

    public AttackTrigger Weapon;
    public PlayerAttack PlayerAttack;

    public WeaponColor active_color = WeaponColor.red;
    public WeaponColor inactive_left_color = WeaponColor.blue;
    public WeaponColor inactive_right_color = WeaponColor.yellow;
    public int max_colors = 3;
    public int start_x;
    public int start_y;
    public int adjust_x;
    public int adjust_y;
    public Color[] colors;

    public Texture active_ring;
    public Texture side_ring;
    public Texture2D selected_color;
    public Texture2D left_color;
    public Texture2D right_color;
    private Texture2D shift_texture;
    private Texture2D shift_texture_left;
    private Texture2D shift_texture_right;

    void Start()
    {
        //I don't know why but can't default these values
        start_x = 32;
        start_y = 160;
        adjust_x = 5;
        adjust_y = 5;

        //Should really do this programatacially
        colors = new Color[max_colors];
        colors[0] = Weapon.GetColor(active_color);
        colors[1] = Weapon.GetColor(inactive_right_color);
        colors[2] = Weapon.GetColor(inactive_left_color);
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerAttack.IsAttacking())
        {
            float direction = Input.GetAxisRaw("Mouse ScrollWheel");
            direction = direction > 0 ? 1 :
                direction < 0 ? -1 : 0;
            CycleColors((int)direction);
        }
    }

    void OnGUI()
    {
        SetColor();
        GUI.DrawTexture(new Rect(start_x + adjust_x, start_y + adjust_y, 32, 32), selected_color);
        GUI.DrawTexture(new Rect(start_x - 15 + 1, start_y + 1, 10, 10), left_color);
        GUI.DrawTexture(new Rect(start_x + 35 + 1, start_y + 1, 10, 10), right_color);
        GUI.DrawTexture(new Rect(start_x, start_y, 39, 39), active_ring);
        GUI.DrawTexture(new Rect(start_x - 15, start_y, 62, 12), side_ring);
    }

    void SetColor()
    {
        shift_texture = selected_color;
        Color[] new_textures = shift_texture.GetPixels(0, 0, shift_texture.width, shift_texture.height);
        for (int i = 0; i < new_textures.Length; i++)
        {
            new_textures[i].r = colors[(int)active_color].r;
            new_textures[i].g = colors[(int)active_color].g;
            new_textures[i].b = colors[(int)active_color].b;
        }
        shift_texture.SetPixels(0, 0, shift_texture.width, shift_texture.height, new_textures);
        shift_texture.Apply();
        selected_color = shift_texture;


        shift_texture = left_color;
        new_textures = shift_texture.GetPixels(0, 0, shift_texture.width, shift_texture.height);
        for (int i = 0; i < new_textures.Length; i++)
        {
            new_textures[i].r = colors[(int)inactive_left_color].r;
            new_textures[i].g = colors[(int)inactive_left_color].g;
            new_textures[i].b = colors[(int)inactive_left_color].b;
        }
        shift_texture.SetPixels(0, 0, shift_texture.width, shift_texture.height, new_textures);
        shift_texture.Apply();
        left_color = shift_texture;

        shift_texture = right_color;
        new_textures = shift_texture.GetPixels(0, 0, shift_texture.width, shift_texture.height);
        for (int i = 0; i < new_textures.Length; i++)
        {
            new_textures[i].r = colors[(int)inactive_right_color].r;
            new_textures[i].g = colors[(int)inactive_right_color].g;
            new_textures[i].b = colors[(int)inactive_right_color].b;
        }
        shift_texture.SetPixels(0, 0, shift_texture.width, shift_texture.height, new_textures);
        shift_texture.Apply();
        right_color = shift_texture;
    }


    void CycleColors(int direction)
    {
        if (direction == 0)
            return;

        int temp = (int)active_color;
        int left, right, current;

        if (direction < 0)
        {
            right = temp;
            current = temp - 1 < 0 ? max_colors - 1 : temp - 1;
            left = current - 1 < 0 ? max_colors - 1 : current - 1;
            ActivateColor((WeaponColor)current, (WeaponColor)left, (WeaponColor)right);
        }
        else if (direction > 0)
        {
            left = temp;
            current = temp + 1 >= max_colors ? 0 : temp + 1;
            right = current + 1 >= max_colors ? 0 : current + 1;

            ActivateColor((WeaponColor)current, (WeaponColor)left, (WeaponColor)right);
        }
    }

    void ActivateColor(WeaponColor current, WeaponColor left, WeaponColor right)
    {
        active_color = current;
        inactive_left_color = left;
        inactive_right_color = right;

        //Set static var here
        GameController.instance.EquippedColor = active_color;
    }

}
