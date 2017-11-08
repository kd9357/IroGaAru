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
    public Image RedFill;
    public Image YellowFill;
    public Image BlueFill;
    public AudioClip[] AudioClips;

    [Tooltip("For any attack, how much should the gauge deplete? [0, 1]")]
    public float Depletion = .2f;
    [Tooltip("For any attack, how much should the gauge regenerate? [0, 1]")]
    public float Regeneration = .001f;    
    [Tooltip("How much faster should 0 to full gauge regenerate? [0, 1]")]
    public float RegenerationRate = 3f;

    private const float MAX_GAUGE_VAL = 1f;
    private const float FLASH_DURATION = 1.5f;
    private const float FLASH_MULTIPLIER = 8f;

    private AudioSource _audioSource;

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

        _audioSource = gameObject.GetComponent<AudioSource>();

        RedFill.canvasRenderer.SetAlpha(1f);
        YellowFill.canvasRenderer.SetAlpha(1f);
        BlueFill.canvasRenderer.SetAlpha(1f);

        RedSlider.value = MAX_GAUGE_VAL;
        YellowSlider.value = MAX_GAUGE_VAL;
        BlueSlider.value = MAX_GAUGE_VAL;
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
                {
                    _redDisabled = true;
                    DimGauge(RedFill);
                }
                break;
            case GaugeColor.Yellow:
                if (DepleteSlider(YellowSlider))
                {
                    _yellowDisabled = true;
                    DimGauge(YellowFill);
                }
                break;
            case GaugeColor.Blue:
                if (DepleteSlider(BlueSlider))
                {
                    _blueDisabled = true;
                    DimGauge(BlueFill);
                }
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
        {
            if (_redDisabled)
            {
                _redDisabled = false;
                StartCoroutine(FlashGauge(RedFill));
            }
        }
        if (RegenerateSlider(YellowSlider))
        {
            if (_yellowDisabled)
            {
                _yellowDisabled = false;
                StartCoroutine(FlashGauge(YellowFill));
            }
        }
        if (RegenerateSlider(BlueSlider))
        {
            if (_blueDisabled)
            {
                _blueDisabled = false;
                StartCoroutine(FlashGauge(BlueFill));
            }
        }
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
        s.value = Mathf.Min(MAX_GAUGE_VAL, s.value + Regeneration * RegenerationRate *
            ComboManager.Instance.RegenMultiplier());
        if (s.value >= MAX_GAUGE_VAL)
            return true;
        return false;
    }

    private void DimGauge(Image fill)
    {
        fill.canvasRenderer.SetAlpha(.25f);
        _audioSource.clip = AudioClips[1];
        _audioSource.Play();
    }

    private IEnumerator FlashGauge(Image fill)
    {
        _audioSource.clip = AudioClips[0];
        _audioSource.Play();

        fill.canvasRenderer.SetAlpha(1f);
        float start = Time.time;
        while (Time.time - start < FLASH_DURATION)
        {
            fill.canvasRenderer.SetAlpha(Mathf.PingPong(Time.time * FLASH_MULTIPLIER, 1f));
            yield return new WaitForEndOfFrame();
        }

        // Ping Pong may not end in alpha of 1
        fill.canvasRenderer.SetAlpha(1f);
    }
}