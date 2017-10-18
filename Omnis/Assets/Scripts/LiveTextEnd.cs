using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Removed activate text at line functionality (may need to rename)
public class LiveTextEnd : MonoBehaviour {

    private TextManager _textManager;

    // Use this for initialization
    void Start () {
        _textManager = FindObjectOfType<TextManager>();
	}
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _textManager.DisableTextBox();
            Destroy(gameObject);
        }
    }

}
