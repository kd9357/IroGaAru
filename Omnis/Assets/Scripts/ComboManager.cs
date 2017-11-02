// TeamTwo

/*
 * Include Files
 */

using System;
using NUnit.Framework;
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
        Assert.GreaterOrEqual(newCount, 0);
        UpdateComboText(newCount);

        float newMultiplier = _regenMultiplier + newCount * PercentIncreasePerHit;
        _regenMultiplier = Mathf.Min(newMultiplier, MAX_MULTIPLIER);

        // DEBUGGING
        Debug.Log("Regen Multiplier:\t" + _regenMultiplier);
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

        // TODO: Play Animation
    }
}