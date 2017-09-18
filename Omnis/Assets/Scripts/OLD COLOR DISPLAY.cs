using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class color_display : MonoBehaviour {

    public AttackTrigger Weapon;
    public PlayerAttack PlayerAttack;

    public WeaponColor active_color = WeaponColor.Red;
    public WeaponColor inactive_left_color = WeaponColor.Blue;
    public WeaponColor inactive_right_color = WeaponColor.Yellow;
    public int max_colors = 3;
    public int start_x = 32;
    public int start_y = 160;
    public int active_adjust_x = 5;
    public int active_adjust_y = 5;
    public int inactive_adjust_x = 15;
    public int inactive_adjust_y = 1;
    public Color[] colors;

    public Texture active_ring;
    public Texture side_ring;
    public Texture2D selected_color;
    public Texture2D left_color;
    public Texture2D right_color;

    private WeaponColor previous_color;
    private Texture2D shift_texture;
    private Texture2D shift_texture_left;
    private Texture2D shift_texture_right;

    void Start()
    {
        //Should really do this programatically
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
#if (UNITY_ANDROID || UNITY_IPHONE)

            CycleColors(MobileUI.Instance.GetSwitchColor() ? 1 : 0);
#else
            // Not easy to use mousewheel
//            float direction = Input.GetAxisRaw("Mouse ScrollWheel");
//            direction = direction > 0 ? 1 :
//                direction < 0 ? -1 : 0;
//            CycleColors((int)direction);

            if (Input.GetButtonUp("Left Bumper"))
            {
                CycleColors(-1);
            }
            if (Input.GetButtonUp("Right Bumper"))
            {
                CycleColors(1);
            }
#endif
        }
    }

    void OnGUI()
    {
        if(previous_color != active_color)
        { SetColor(); }
        previous_color = active_color;
        GUI.DrawTexture(new Rect(start_x + active_adjust_x, start_y + active_adjust_y, 32, 32), selected_color);
        GUI.DrawTexture(new Rect(start_x - inactive_adjust_x, start_y + inactive_adjust_y, 10, 10), left_color);
        GUI.DrawTexture(new Rect(start_x - inactive_adjust_x + 50 , start_y + inactive_adjust_y, 10, 10), right_color);
        //50 pixels is the distance between left and right circles.

        GUI.DrawTexture(new Rect(start_x, start_y, 39, 39), active_ring);
        GUI.DrawTexture(new Rect(start_x - inactive_adjust_x, start_y, 62, 12), side_ring);
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


    public void CycleColors(int direction)
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
