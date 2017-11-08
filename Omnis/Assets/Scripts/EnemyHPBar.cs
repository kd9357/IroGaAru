using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour {

    public GameObject EnemyBar;
    public GameObject EnemyNameText;

    private Enemy TargetEnemy;
    private Slider s;
    private Text t;

	void Start () {
        s = EnemyBar.GetComponent<Slider>();
        t = EnemyNameText.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        TargetEnemy = GameController.Instance.LastEnemy; // Probably find a way to not do this on update.
        if (TargetEnemy != null && TargetEnemy.EnemyHPPercent() > 0)
        {
            EnemyBar.SetActive(true);
            s.value = TargetEnemy.EnemyHPPercent();
            t.text = TargetEnemy.GetName();
        }
        else
        {
            EnemyBar.SetActive(false);
        }
	}
}
