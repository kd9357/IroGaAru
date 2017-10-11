using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorDisplay : MonoBehaviour {

    public AttackTrigger Weapon;
    public Player Player;
    //public PlayerAttack PlayerAttack;
    public WeaponColor red_color = WeaponColor.Red;
    public WeaponColor yellow_color = WeaponColor.Yellow;
    public WeaponColor blue_color = WeaponColor.Blue;
    public Image red_image;
    public Image yellow_image;
    public Image blue_image;
    public Image ring;

    private int _direction = 0;

    void Start()
    {
        if(GameController.instance.EquippedColor == red_color)
        {
            CycleColors(0);
        }
        if (GameController.instance.EquippedColor == yellow_color)
        {
            CycleColors(1);
        }
        if (GameController.instance.EquippedColor == blue_color)
        {
            CycleColors(2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (!PlayerAttack.IsAttacking())
        if(!Player.IsAttacking())
        {
#if (UNITY_ANDROID || UNITY_IPHONE)
            //Currently just cycles in one direction
            if (MobileUI.Instance.GetSwitchColor())
            {
                _direction++;
                if(_direction > 2)
                    _direction = 0;
                CycleColors(_direction);
                MobileUI.Instance.SetSwitchColor(false);
            }
#else

            if (Input.GetButtonDown("Red"))
            {
                CycleColors(0);
            }
            if (Input.GetButtonDown("Yellow"))
            {
                CycleColors(1);
            }
            if (Input.GetButtonDown("Blue"))
            {
                CycleColors(2);
            }
#endif
        }
    }

    void OnGUI()
    {

    }


    public void CycleColors(int direction)
    {
        switch (direction)
        {
            case 0:     ring.rectTransform.localPosition = new Vector3(-40,-42,0);
                        red_image.rectTransform.localScale = new Vector3(1, 1, 0);
                        yellow_image.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        blue_image.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        GameController.instance.EquippedColor = red_color;
                        break;
            case 1:     ring.rectTransform.localPosition = new Vector3(0, -42, 0);
                        red_image.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        yellow_image.rectTransform.localScale = new Vector3(1, 1, 0);
                        blue_image.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        GameController.instance.EquippedColor = yellow_color;
                        break;
            case 2:     ring.rectTransform.localPosition = new Vector3(40, -42, 0);
                        red_image.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        yellow_image.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        blue_image.rectTransform.localScale = new Vector3(1, 1, 0);
                        GameController.instance.EquippedColor = blue_color;
                        break;
        }
    }

}
