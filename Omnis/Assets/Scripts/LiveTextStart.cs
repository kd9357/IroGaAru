// TeamTwo

/*
 * Include Files
 */

using UnityEngine;

/* 
 * Typedefs
 */

public class LiveTextStart : MonoBehaviour {

    /*
     * Public Member Variables
     */

    public TextAsset DialogText;

    [Tooltip("Check to only activate textbox on first trigger")]
    public bool DisableWhenActivated;

    [Tooltip("Enable if text should be sensitive to time rather than input")] 
    public bool TimeSensitive = true;
    public float DisplayTime = 3.0f;

    /* 
     * Private Member Variables
     */

    private static TextManager _textManager;

    /*
     * Private Method Declarations
     */

    private void Awake () 
    {
        _textManager = FindObjectOfType<TextManager>();
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _textManager.LoadTextFile(DialogText);
            _textManager.EnableTextBox(TimeSensitive, DisplayTime);

            if (DisableWhenActivated)
            {
                gameObject.SetActive(false);   
            }
        }
    }

}
