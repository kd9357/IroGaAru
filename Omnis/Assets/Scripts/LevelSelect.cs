using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelect : MonoBehaviour {

    // Use this for initialization
    public float DojoProgress;
    public float OrangeProgress;
    public float PurpleProgress;
    public float GreenProgress;
    public GameObject DojoPartial;
    public GameObject OrangePartial;
    public GameObject PurplePartial;
    public GameObject GreenPartial;
    public GameObject OrangeFull;
    public GameObject PurpleFull;
    public GameObject GreenFull;
    public GameObject Full;
    void Start () {
        if(LevelManager.Instance)
        {
            DojoProgress = LevelManager.Instance.tutorialScore;
            OrangeProgress = LevelManager.Instance.volcanoScore;
            PurpleProgress = LevelManager.Instance.oceanScore;
            GreenProgress = LevelManager.Instance.forestScore;
        }

        if (DojoProgress > 0.0f)
        {
            DojoPartial.SetActive(true);
        }
        if (OrangeProgress > 0.0f)
        {
            if (OrangeProgress >= 1.0f) { OrangeFull.SetActive(true); OrangePartial.SetActive(false); } else { OrangeFull.SetActive(false); OrangePartial.SetActive(true); }
        }
        if (PurpleProgress > 0.0f)
        {
            if (PurpleProgress >= 1.0f) { PurpleFull.SetActive(true); PurplePartial.SetActive(false); } else { PurpleFull.SetActive(false); PurplePartial.SetActive(true); }
        }
        if (GreenProgress > 0.0f)
        {
            if (GreenProgress >= 1.0f) { GreenFull.SetActive(true); GreenPartial.SetActive(false); } else { GreenFull.SetActive(false); GreenPartial.SetActive(true); }
        }
        if (DojoProgress >= 1.0f && OrangeProgress >= 1.0f && PurpleProgress >= 1.0f && GreenProgress >= 1.0f)
        {
            Full.SetActive(true);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
