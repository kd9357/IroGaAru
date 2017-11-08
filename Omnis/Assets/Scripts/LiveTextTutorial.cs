using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This should only be used for the tutorial!

public class LiveTextTutorial : MonoBehaviour {

    [Tooltip("Tutorial Text files to display")]
    public TextAsset[] TextToDisplay;
    [Tooltip("Mobile text to display")]
    public TextAsset[] MobileTextToDisplay;
    [Tooltip("Target dummy enemy for tutorial")]
    public Enemy TargetEnemy;

    private Transform _player;
    private LiveTextManager _textManager;
    private int _tutorialPhase = 0;
    private bool _active;

    // Use this for initialization
    void Start () {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _textManager = FindObjectOfType<LiveTextManager>();
        _active = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!_active)
            return;

        //check tutorialphase, and reload when necessary

        if (TargetEnemy != null)
        {
            Color enemyColor = TargetEnemy.CurrentColor();
            ColorStatus enemyStatus = TargetEnemy.CurrentColorStatus();
            if(enemyColor == Color.white && _tutorialPhase != 0)
            {
                _tutorialPhase = 0;
                ReloadScript();
            }
            else if(enemyColor != Color.white && enemyStatus == ColorStatus.None &&  _tutorialPhase != 1)
            {
                _tutorialPhase = 1;
                ReloadScript();
            }

            if (enemyStatus == ColorStatus.DamageOverTime && _tutorialPhase != 2)
            {
                _tutorialPhase = 2;
                ReloadScript();
            }
            if (enemyStatus == ColorStatus.Stun && _tutorialPhase != 3)
            {
                _tutorialPhase = 3;
                ReloadScript();
            }
            if (enemyStatus == ColorStatus.WindRecoil && _tutorialPhase != 4)
            {
                _tutorialPhase = 4;
                ReloadScript();
            }

            //Finish
            if(!TargetEnemy.gameObject.activeInHierarchy)
            {
                _tutorialPhase = 5;
                ReloadScript();
            }
        }
	}

    private void ReloadScript()
    {
        if(_tutorialPhase >= TextToDisplay.Length)
        {
            return;
        }
#if (UNITY_ANDROID || UNITY_IPHONE)
        if(_tutorialPhase >= MobileTextToDisplay.Length)//use standard dialogue
        {
            _textManager.ReloadScript(TextToDisplay[_tutorialPhase]);
        }
        else//use mobile dialogue
        {
            _textManager.ReloadScript(MobileTextToDisplay[_tutorialPhase]);
        }
#else
        _textManager.ReloadScript(TextToDisplay[_tutorialPhase]);
#endif
        _textManager.EnableTextBox();
        _active = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ReloadScript();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            _textManager.DisableTextBox();
            _active = false;
        }
    }
}
