// TeamTwo

/*
 * Include Files
 */

using UnityEngine;

/* 
 * Typedefs
 */

public class LiveTextEnd : MonoBehaviour
{

    /* 
     * Private Member Variables
     */

    private static TextManager _textManager;

    /*
     * Private Method Declarations
     */

    private void Awake()
    {
        _textManager = FindObjectOfType<TextManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _textManager.DisableTextBox();
            gameObject.SetActive(false);
        }
    }

}
