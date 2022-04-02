using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Age : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI ageField;

    [SerializeField, Range(0f, 100f)]
    float daysPerSecond = 5f;

    [SerializeField, Range(0f, 5f)]
    float textVisibilityTime = 2f;

    [SerializeField, Range(0f, 1f)]
    float textEasingTime = 0.5f;

    [SerializeField]
    AnimationCurve textEasing;

    [SerializeField]
    Image yearProgressImage;

    [SerializeField, Range(0, 356)]
    float destructionReset = 40;

    int years = 0;
    float dayOfYear = 356f;

    static Age _instance = null;

    HashSet<Clock> activeClocks = new HashSet<Clock>();
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        } else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        Clock.OnClockTime += HandleClockStatusChange;
        KillLayer.OnPlayerKilled += HandlePlayerKilled;
    }

    private void OnDisable()
    {
        Clock.OnClockTime -= HandleClockStatusChange;
        KillLayer.OnPlayerKilled -= HandlePlayerKilled;
    }

    private void HandlePlayerKilled()
    {
        years++;
        StartCoroutine(ShowAge(years));
    }

    private void HandleClockStatusChange(Clock clock, ClockStatus status)
    {
        if (status == ClockStatus.DESTROYED)
        {
            dayOfYear = Mathf.Clamp(dayOfYear - destructionReset, 0, 356);
            yearProgressImage.fillAmount = Mathf.Clamp01(dayOfYear / 356);
        } else if (status == ClockStatus.RUNNING)
        {
            activeClocks.Add(clock);
        } else
        {
            activeClocks.Remove(clock);
        }
    }

    private void Update()
    {
        if (activeClocks.Count > 0)
        {
            dayOfYear += Time.deltaTime * daysPerSecond;
            yearProgressImage.fillAmount = Mathf.Clamp01(dayOfYear / 356);
        }
        if (dayOfYear >= 356f)
        {
            dayOfYear = 0;
            yearProgressImage.fillAmount = Mathf.Clamp01(dayOfYear / 356);
            years++;
            StartCoroutine(ShowAge(years));
        }
    }       

    IEnumerator<WaitForSeconds> ShowAge(int showingYears)
    {
        Color color = Color.black;
        ageField.text = string.Format("~ Age {0} ~", showingYears);
        float start = Time.timeSinceLevelLoad;
        float delta = 0;
        while (delta < textEasingTime)
        {
            delta = Time.timeSinceLevelLoad - start;
            color.a = Mathf.Clamp01(textEasing.Evaluate(delta / textEasingTime));
            ageField.color = color;
            if (showingYears != years) yield break;
            yield return new WaitForSeconds(0.02f);
        }
        color.a = 1;
        ageField.color = color;
        yield return new WaitForSeconds(textVisibilityTime);
        if (showingYears != years) yield break;
        start = Time.timeSinceLevelLoad;
        delta = 0;
        while (delta < textEasingTime)
        {
            delta = Time.timeSinceLevelLoad - start;
            color.a = 1 - Mathf.Clamp01(textEasing.Evaluate(delta / textEasingTime));
            ageField.color = color;
            if (showingYears != years) yield break;
            yield return new WaitForSeconds(0.02f);

        }
        color.a = 0;
        ageField.color = color;
    }
}
