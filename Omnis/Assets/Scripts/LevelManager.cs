using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public float forestScore = 0;
    public float oceanScore = 0;
    public float volcanoScore = 0;
    public float tutorialScore = 0;
    // Use this for initialization
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            gameObject.SetActive(false);
            return;
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
