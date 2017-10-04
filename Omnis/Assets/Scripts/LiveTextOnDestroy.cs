using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Removed activate text at line functionality (may need to rename)
public class LiveTextOnDestroy : MonoBehaviour {

    private TextManager _textManager;

    // Use this for initialization
    void Start () {
        _textManager = FindObjectOfType<TextManager>();
	}
    private void OnDestroy()
    {
        _textManager.DisableTextBox();
    }

}
