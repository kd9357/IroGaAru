using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTextColor : MonoBehaviour
{
    private Text _textComponent;

	// Use this for initialization
	void Start ()
	{
        _textComponent = gameObject.GetComponent<Text>();
	}

    public void SetColor(bool default_color)
    {
        _textComponent.color = default_color ? Color.black : Color.yellow;
    }
}
