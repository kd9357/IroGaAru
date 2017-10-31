// TeamTwo

/*
 * Include Files
 */

using UnityEngine;
using UnityEngine.UI;

/*
 * Typedefs
 */

public enum GaugeColor
{
    Red,
    Yellow,
    Blue
}

public class GaugeManager : MonoBehaviour
{
    /*
     * Public Member Varialbes
     */

    public static GaugeManager Instance;

    public Slider RedSlider;
    public Slider YellowSlider;
    public Slider BlueSlider;

    [Tooltip("For any attack, how much should the gauge deplete? [0, 1]")]
    public float Depletion = .2f;
    [Tooltip("For any attack, how much should the gauge regenerate? [0, 1]")]
    public float Regeneration = .001f;    
    [Tooltip("How much faster should 0 to full gauge regenerate? [0, 1]")]
    public float RegenerationRate = 3f;

    private const float MAX_VAL = 1f;

    private bool _redDisabled;
    private bool _yellowDisabled;
    private bool _blueDisabled;

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

        if (RedSlider == null || YellowSlider == null || BlueSlider == null)
        {
            Debug.LogError("Gauges are incorrectly set!");
            return;
        }
    }

    void Start()
    {
        _redDisabled = false;
        _yellowDisabled = false;
        _blueDisabled = false;

        RedSlider.value = MAX_VAL;
        YellowSlider.value = MAX_VAL;
        BlueSlider.value = MAX_VAL;
    }

    void Update()
    {
        RegenerateGauges();
    }

    public void DepleteGauge(GaugeColor g)
    {
        switch (g)
        {
            case GaugeColor.Red:
                if (DepleteSlider(RedSlider))
                    _redDisabled = true;
                break;
            case GaugeColor.Yellow:
                if (DepleteSlider(YellowSlider))
                    _yellowDisabled = true;
                break;
            case GaugeColor.Blue:
                if (DepleteSlider(BlueSlider))
                    _blueDisabled = true;
                break;
            default:
                Debug.LogError(g + " is not a valid gauge color!");
                break;
        }
    }

    public void RegenerateGauges()
    {
        // TODO: Play sound/animation to show gauge is filled
        if (RegenerateSlider(RedSlider))
            _redDisabled = false;
        if (RegenerateSlider(YellowSlider))
            _yellowDisabled = false;
        if (RegenerateSlider(BlueSlider))
            _blueDisabled = false;
    }

    #region Accessors

    public bool IsRedDisabled()
    {
        return _redDisabled;
    }

    public bool IsYellowDisabled()
    {
        return _yellowDisabled;
    }

    public bool IsBlueDisabled()
    {
        return _blueDisabled;
    }

    #endregion

    /*
     * Private Method Declarations
     */

    // Depletes respective slider and returns if it should be disabled
    private bool DepleteSlider(Slider s)
    {
        s.value = Mathf.Max(0, s.value - Depletion);
        if (s.value <= 0f)
            return true;
        return false;
    }

    // Regenerates respective slider and returns if it was fully filled
    private bool RegenerateSlider(Slider s)
    {
        s.value = Mathf.Min(MAX_VAL, s.value + Regeneration * RegenerationRate);
        if (s.value >= MAX_VAL)
            return true;
        return false;
    }
}