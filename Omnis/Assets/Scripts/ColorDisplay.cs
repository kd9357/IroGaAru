// TeamTwo

/*
 * Include Files
 */

using UnityEngine;
using UnityEngine.UI;
using CnControls;

public class ColorDisplay : MonoBehaviour {

    /* 
     * Public Member Variables
     */

    public AttackTrigger Weapon;
    public Player Player;

    public WeaponColor RedColor = WeaponColor.Red;
    public WeaponColor YellowColor = WeaponColor.Yellow;
    public WeaponColor BlueColor = WeaponColor.Blue;

    public Image RedImage;
    public Image YellowImage;
    public Image BlueImage;
    public Image Ring;

    /*
     * Public Method Declarations
     */

    void Start()
    {
        if(GameController.Instance.EquippedColor == RedColor)
        {
            CycleColors(0);
        }
        if (GameController.Instance.EquippedColor == YellowColor)
        {
            CycleColors(1);
        }
        if (GameController.Instance.EquippedColor == BlueColor)
        {
            CycleColors(2);
        }
    }

    void Update()
    {
#if (UNITY_ANDROID || UNITY_IPHONE)
        if (CnInputManager.GetButtonDown("Red"))
        {
            CycleColors(0);
        }
        if (CnInputManager.GetButtonDown("Yellow"))
        {
            CycleColors(1);
        }
        if (CnInputManager.GetButtonDown("Blue"))
        {
            CycleColors(2);
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

    public void CycleColors(int direction)
    {
        switch (direction)
        {
            case 0:     Ring.rectTransform.localPosition = new Vector3(-40,-42,0);
                        RedImage.rectTransform.localScale = new Vector3(1, 1, 0);
                        YellowImage.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        BlueImage.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        GameController.Instance.EquippedColor = RedColor;
                        break;
            case 1:     Ring.rectTransform.localPosition = new Vector3(0, -42, 0);
                        RedImage.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        YellowImage.rectTransform.localScale = new Vector3(1, 1, 0);
                        BlueImage.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        GameController.Instance.EquippedColor = YellowColor;
                        break;
            case 2:     Ring.rectTransform.localPosition = new Vector3(40, -42, 0);
                        RedImage.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        YellowImage.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0);
                        BlueImage.rectTransform.localScale = new Vector3(1, 1, 0);
                        GameController.Instance.EquippedColor = BlueColor;
                        break;
        }
    }

}
