using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiveTextManager : MonoBehaviour {

    //Should find these in start, not public
    public GameObject TextBox;
    public Text TheText;

    //Should also make these private down the line
    public TextAsset TextFile;
    public string[] TextLines;
    public float maxTime = 3.0f;

    private int _currentLine;
    private bool _isActive;
    private float _time;
    

	// Use this for initialization
	void Start () {
        if (TextFile != null)
        {
            TextLines = (TextFile.text.Split('\n'));
        }
        _currentLine = 0;
        TextBox.SetActive(false);
        _isActive = false;
	}
	
	// Update is called once per frame
	void Update () {
        if(_isActive)
        {
            _time += Time.deltaTime;
            TheText.text = TextLines[_currentLine];
            if (_time >= maxTime)
            {
                _time = 0;
                _currentLine += 1;
                if (_currentLine >= TextLines.Length)
                    _currentLine = 0;
            }
        }
    }

    public void EnableTextBox()
    {
        TextBox.SetActive(true);
        _isActive = true;
    }

    public void DisableTextBox()
    {
        TextBox.SetActive(false);
        _isActive = false;
    }

    public void ReloadScript(TextAsset newText)
    {
        if(newText == null)
        {
            Debug.Log("Text not provided");
            return;
        }
        _currentLine = 0;
        _time = 0;
        TextLines = (newText.text.Split('\n'));
    }
}
