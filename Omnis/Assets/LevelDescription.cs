using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelDescription : MonoBehaviour {
    public GameObject LevelName;
    public GameObject Description;
    public GameObject Colored;
    public GameObject Difficulty;
    public GameObject Panel;
    public GameObject Blocker;
    public LevelSelect levelSelect;
    public int selectedLevel;

    public string DojoName = "Dojo";
    public string DojoSceneName = "persia_tutorial";
    public string DojoDescription = "The dojo where Sensei resides.";
    public int DojoDifficulty = 0;

    public string ForestName = "Forest";
    public string ForestSceneName = "forest_level";
    public string ForestDescription = "A forest with towering trees.";
    public int ForestDifficulty = 1;

    public string OceanName = "Ocean";
    public string OceanSceneName = "water_level";
    public string OceanDescription = "A fishing village next to the ocean.";
    public int OceanDifficulty = 2;

    public string VolcanoName = "Volcano";
    public string VolcanoSceneName = "fire_level";
    public string VolcanoDescription = "A palace within a volcano.";
    public int VolcanoDifficulty = 2;

    public string DifficultyPrefix = "Difficulty: ";
    public string ColoredSuffix = "% colored";

    public string[] difficulties = { "Easy", "Normal", "Hard" };

    private Text NameTxt;
    private Text DescTxt;
    private Text ColorTxt;
    private Text DiffTxt;
    // Use this for initialization
    void Start () {
        NameTxt = LevelName.GetComponent<Text>();
        DescTxt = Description.GetComponent<Text>();
        DiffTxt = Difficulty.GetComponent<Text>();
        ColorTxt = Colored.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update () {
		
	}
    public void changeText(int level)
    {
        switch(level)
        {
            case 0:
                NameTxt.text = DojoName;
                DescTxt.text = DojoDescription;
                DiffTxt.text = DifficultyPrefix + difficulties[DojoDifficulty];
                ColorTxt.text = (int) (levelSelect.DojoProgress * 100) + ColoredSuffix;
                selectedLevel = 0;
                break;
            case 1:
                NameTxt.text = ForestName;
                DescTxt.text = ForestDescription;
                DiffTxt.text = DifficultyPrefix + difficulties[ForestDifficulty];
                ColorTxt.text = (int)(levelSelect.GreenProgress * 100) + ColoredSuffix;
                selectedLevel = 1;
                break;
            case 2:
                NameTxt.text = OceanName;
                DescTxt.text = OceanDescription;
                DiffTxt.text = DifficultyPrefix + difficulties[OceanDifficulty];
                ColorTxt.text = (int)(levelSelect.PurpleProgress * 100) + ColoredSuffix;
                selectedLevel = 2;
                break;
            case 3:
                NameTxt.text = VolcanoName;
                DescTxt.text = VolcanoDescription;
                DiffTxt.text = DifficultyPrefix + difficulties[VolcanoDifficulty];
                ColorTxt.text = (int)(levelSelect.OrangeProgress * 100) + ColoredSuffix;
                selectedLevel = 3;
                break;

        }
    }
    public void selectLevel()
    {
        switch (selectedLevel)
        {
            case 0: GameController.Instance.LoadScene(DojoSceneName); break;
            case 1: GameController.Instance.LoadScene(ForestSceneName); break;
            case 2: GameController.Instance.LoadScene(OceanSceneName); break;
            case 3: GameController.Instance.LoadScene(VolcanoSceneName); break;
        }
        return;
    }
    public void hidePanel()
    {
        Panel.SetActive(false);
        Blocker.SetActive(false);
    }
    public void showPanel()
    {
        Panel.SetActive(true);
        Blocker.SetActive(true);
    }
}
