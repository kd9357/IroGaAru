using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorDisplay : MonoBehaviour {

    public AttackTrigger Weapon;
    public PlayerAttack PlayerAttack;
    public int max_colors;
    public WeaponColor active_color = WeaponColor.Red;
    public WeaponColor inactive_left_color = WeaponColor.Blue;
    public WeaponColor inactive_right_color = WeaponColor.Yellow;
    public Color[] colors;
    public Image active_image;
    public Image left_inactive_image;
    public Image right_inactive_image;


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
            if (MobileUI.Instance.GetSwitchColor())
            {
                CycleColors(1);
                MobileUI.Instance.SetSwitchColor(false);
            }
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
        active_image.color = colors[0];
        right_inactive_image.color = colors[1];
        left_inactive_image.color = colors[2];
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
        colors[0] = Weapon.GetColor(active_color);
        colors[1] = Weapon.GetColor(inactive_right_color);
        colors[2] = Weapon.GetColor(inactive_left_color);
        //Set static var here
        GameController.instance.EquippedColor = active_color;
    }

}
