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

public class ComboManager : MonoBehaviour
{
    /*
     * Public Member Variables
     */

    public static ComboManager Instance;

    public float PercentIncreasePerHit = .001f;

    /*
     * Private Member Variables
     */

    private const float MAX_MULTIPLIER = 2f;
    private const float SHAKE_DURATION = .8f;
    private const float SHAKE_RANGE = 20f;

    private int _comboCount;
    private float _regenMultiplier;

    // Components
    private Text _comboText;

    /*
     * Public Method Declarations
     */

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            gameObject.SetActive(false);
            return;
        }

    }

    void Start()
    {
        _comboCount = 0;
        _regenMultiplier = 1f;

        _comboText = gameObject.GetComponent<Text>();
        _comboText.enabled = false;
    }

    public void IncrementComboCount()
    {
        ++_comboCount;
        UpdateComboCount(_comboCount);
    }

    public void ResetComboCount()
    {
        _comboCount = 0;
        _regenMultiplier = 1f;
        UpdateComboText(_comboCount);
    }

    #region Accessors

    public float RegenMultiplier()
    {
        return _regenMultiplier;
    }

    #endregion

    /*
     * Private Method Declarations
     */

    private void UpdateComboCount(int newCount)
    {
        UpdateComboText(newCount);

        float newMultiplier = _regenMultiplier + newCount * PercentIncreasePerHit;
        _regenMultiplier = Mathf.Min(newMultiplier, MAX_MULTIPLIER);

        // DEBUGGING
        // Debug.Log("Regen Multiplier:\t" + _regenMultiplier);
    }

    private void UpdateComboText(int newCount)
    {
        _comboText.text = "x " + newCount;
        if (newCount == 0)
        {
            _comboText.enabled = false;
        }
        else if (!_comboText.enabled)
        {
            _comboText.enabled = true;
        }

        if (newCount == 1 || newCount % 10 == 0)
            StartCoroutine(ShakeComboText());
    }

    private IEnumerator ShakeComboText()
    {
        float start = Time.time;
        Quaternion origRot = _comboText.transform.rotation;
        while (Time.time - start < SHAKE_DURATION)
        {
            // Shake Text
            float z = Random.value * SHAKE_RANGE - SHAKE_RANGE / 2;
            _comboText.transform.eulerAngles = new Vector3(origRot.x, origRot.y, origRot.z + z);

            // Pingpong Color
            _comboText.color = Color.Lerp(Color.black, Color.yellow, 
                Mathf.PingPong(Time.time * 6f, 1f));
            yield return new WaitForEndOfFrame();
        }

        _comboText.transform.rotation = origRot;
        _comboText.color = Color.black;
    }
}