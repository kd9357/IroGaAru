// TeamTwo

/*
 * Include Files
 */
 
using UnityEngine;

/*
 * Typedefs
 */

public class DialogueTextTrigger : MonoBehaviour
{
    /*
     * Public Member Variables
     */

    public TutorialDialogue TutorialDialogue;
    public string[] Dialogue;

    /*
     * Private Member Variables
     */

    private bool _triggered;
    private Object _lock;

    /*
     * Public (Unity) Method Declarations
     */

    void Start()
    {
        _lock = new Object();
        _triggered = false;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        TriggerDialogue(collider);
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        TriggerDialogue(collider);
    }

    /*
     * Private Method Declarations
     */

    private void TriggerDialogue(Collider2D collider)
    {
        // Make sure dialogue only triggered once
        if (collider.tag == "Player")
        {
            lock (_lock)
            {
                if (!_triggered)
                {
                    TutorialDialogue.gameObject.SetActive(true);
                    TutorialDialogue.Dialogue = Dialogue;
                    TutorialDialogue.StartCoroutine("FadeDialogueIn");
                    _triggered = true;
                }
            }
        }
    }
}