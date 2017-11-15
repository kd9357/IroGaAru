// TeamTwo

/*
 * Include Files
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
 * Typedefs
 */

public class TutorialDialogue : MonoBehaviour
{
    /*
     * Public Member Variables
     */

    // Components to fade in/out
    public Image[] Panels;
    public Text NameText;
    public Text DialogueText;

    public string[] Dialogue;

    /*
     * Private Member Variables
     */

    private static GameObject _tutorialDialogue;
    private static Player _player;

    private bool _started;
    private int _lineNumber;
    private bool _typing;
    private bool _continueTyping;

    /*
     * Public (Unity) Method Declarations
     */

    void Start()
    {
        _started = false;
        _lineNumber = 0;
        _typing = false;
        _continueTyping = false;

        _tutorialDialogue = GameObject.Find("Tutorial Dialogue");
        if (_tutorialDialogue == null)
        {
            Debug.LogError("Could not find Tutorial Dialogue GO!");
            return;
        }
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        _tutorialDialogue.SetActive(false);
    }

    void Update()
    {
        if (_started)
        {
            if (Input.anyKeyDown)
                ContinueDialogue();
        }
    }

    /*
     * Public Method Declarations
     */

    public void ContinueDialogue()
    {
        if (_typing)
        {
            ShowFullText();
        }
        else
        {
            ++_lineNumber;
            StartCoroutine("TypingText");
        }
    }

    /*
     * Private Method Declarations
     */

    private IEnumerator FadeDialogueIn()
    {
        _player.FreezeMovement(true);

        NameText.canvasRenderer.SetAlpha(0f);
        foreach (Image t in Panels)
            t.canvasRenderer.SetAlpha(0f);

        _tutorialDialogue.SetActive(true);

        Panels[0].CrossFadeAlpha(.8f, 1f, false);
        for (int k = 1; k < Panels.Length; ++k)
            Panels[k].CrossFadeAlpha(2f, 1f, false);
        NameText.CrossFadeAlpha(2f, 1f, false);

        yield return new WaitForSeconds(1f);

        _started = true;
        _lineNumber = 0;
        StartCoroutine("TypingText");
    }

    private IEnumerator FadeDialogueOut()
    {
        StopCoroutine("TypingText");

        DialogueText.CrossFadeAlpha(0f, 1f, false);
        NameText.CrossFadeAlpha(0f, 1f, false);
        foreach (var cr in Panels)
            cr.CrossFadeAlpha(0f, 1f, false);

        yield return new WaitForSeconds(1f);

        DialogueText.text = string.Empty;
        DialogueText.canvasRenderer.SetAlpha(1f);

        _tutorialDialogue.SetActive(false);

        // Only trigger dialogue once?
        gameObject.SetActive(false);
        _player.FreezeMovement(false);
    }

    private IEnumerator TypingText()
    {
        if (_lineNumber >= Dialogue.Length)
        {
            StartCoroutine("FadeDialogueOut");
            yield return null;
        }
        else
        {
            string line = Dialogue[_lineNumber];
            DialogueText.text = string.Empty;

            _typing = true;
            _continueTyping = true;
            foreach (char c in line)
            {
                // Flag gets changed in ShowFullText 
                if (_continueTyping)
                    DialogueText.text += c;
                yield return new WaitForSeconds(.02f);
            }

            _typing = false;
        }
    }

    private void ShowFullText()
    {
        _continueTyping = false;
        StopCoroutine("TypingText");

        string line = Dialogue[_lineNumber];
        DialogueText.text = line;
        _typing = false;
    }
}