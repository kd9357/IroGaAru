using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTransition : MonoBehaviour {

    [Tooltip("The list of objects to be affected by color transition")]
    public GameObject ColorableEnvironment;
    [Tooltip("The minimum number of enemies to be defeated until 100 percent colored")]
    public int MinimumEnemies;

    private SpriteRenderer[] _sprites;
    private int _currentEnemyCount = 0;
    private float h, s, v;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
