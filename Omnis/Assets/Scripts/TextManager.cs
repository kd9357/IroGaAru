// TeamTwo

/* 
 * Include Files
 */

using UnityEngine;
using UnityEngine.UI;

/*
 * Typedefs
 */

public class TextManager : MonoBehaviour
{
    /*
     * Public Member Variables
     */

    public GameObject TextBox;

    /*
     * Private Member Variables
     */

    private Text _text;
    private TextAsset _dialogFile;

    private bool _timeSensitive;
    private float _maxDisplayTime;
    private float _time;

    private string[] _lines;
    private int _currentLine;

    /* 
     * Public Method Declarations
     */

    void Start()
    {
        _dialogFile = null;
        _text = TextBox.transform.GetComponentInChildren<Text>();
        TextBox.SetActive(false);
    }

    public void LoadTextFile(TextAsset dialogFile)
    {
        _dialogFile = dialogFile;
        _currentLine = 0;
        _time = 0;

        if (dialogFile != null)
        {
            _lines = _dialogFile.text.Split('\n');
        }
        else
        {
            Debug.LogError("Textfile is null; cannot load text. Aborting Text Manager");
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (TextBox.activeInHierarchy)
        {
            _text.text = _lines[_currentLine];

            if (_timeSensitive)
            {
                _time += Time.deltaTime;
                if (_time >= _maxDisplayTime)
                {
                    _time = 0;
                    _currentLine += 1;
                    if (_currentLine >= _lines.Length)
                    {
                        _currentLine = 0;
                    }
                }
            }
            else
            {
                if (Input.GetButtonDown("Submit"))
                {
                    _currentLine += 1;
                    if (_currentLine >= _lines.Length)
                    {
                        _currentLine = 0;
                        DisableTextBox();
                    }
                }
            }
        }
    }

    public void EnableTextBox(bool timeSensitive = false, float maxTime = 0f)
    {
        _timeSensitive = timeSensitive;
        _maxDisplayTime = maxTime <= 0f ? _maxDisplayTime : maxTime;

        TextBox.SetActive(true);

        if (!_timeSensitive)
        {
            GameController.Instance.PauseGame(true);
        }

    }

    public void DisableTextBox()
    {
        TextBox.SetActive(false);

        if (!_timeSensitive)
        {
            GameController.Instance.PauseGame(false);
        }
    }
}
