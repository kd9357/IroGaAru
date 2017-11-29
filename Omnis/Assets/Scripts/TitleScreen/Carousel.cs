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

public class Carousel : MonoBehaviour
{ 
    /*
     * Public Member Variables
     */

    public Texture[] Art;
    [Tooltip("In seconds")]
    public int TransitionTime;

    /*
     * Private Member Variables
     */

    private RawImage _background;
    private int _index;

    /*
     * Public Method Declarations
     */

    void Start()
    {
        _background = gameObject.GetComponent<RawImage>();
        _index = 0;

        _background.texture = Art[_index];

        StartCoroutine("CycleImages");
    }

    /*
     * Private Method Declarations
     */

    private IEnumerator CycleImages()
    {
        yield return new WaitForSeconds(TransitionTime);

        _background.CrossFadeAlpha(0f, 1f, false);
        yield return new WaitForSeconds(1f);

        _index = ++_index % Art.Length;
        _background.texture = Art[_index];

        _background.CrossFadeAlpha(2f, 1f, false);
        yield return new WaitForSeconds(1f);

        StartCoroutine("CycleImages");
    }
}
