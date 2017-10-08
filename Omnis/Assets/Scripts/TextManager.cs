using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour {

    //Should find these in start, not public
    public GameObject TextBox;
    public Text TheText;

    //Should also make these private down the line
    public TextAsset TextFile;
    public string[] TextLines;

    private int _currentLine;
    private bool _isActive;

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
            TheText.text = TextLines[_currentLine];
            if (Input.GetButtonDown("Submit"))
            {
                _currentLine += 1;
                if (_currentLine >= TextLines.Length)
                    DisableTextBox();
            }
        }
    }

    public void EnableTextBox()
    {
        TextBox.SetActive(true);
        _isActive = true;
        GameController.Instance.PauseGame(true);
    }

    public void DisableTextBox()
    {
        TextBox.SetActive(false);
        _isActive = false;
        GameController.Instance.PauseGame(false);
    }

    public void ReloadScript(TextAsset newText)
    {
        if(newText == null)
        {
            Debug.Log("Text not provided");
            return;
        }
        _currentLine = 0;
        TextLines = (newText.text.Split('\n'));
    }
}
