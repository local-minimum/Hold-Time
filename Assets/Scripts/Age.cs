using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public delegate void AgeEvent(int years);

public class Age : MonoBehaviour
{
    public static event AgeEvent OnNewAge;

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

    [SerializeField]
    Image yearProgressFrame;
    
    float destructionCost = 2;

    [SerializeField]
    Canvas canvas;

    [SerializeField]
    int startAge = 5;

    int years = 0;
    float dayOfYear = 356f;

    static Age _instance = null;

    bool levelDone = false;

    HashSet<Clock> activeClocks = new HashSet<Clock>();
    
    public static AgeInfo GetAge()
    {
        return new AgeInfo(_instance.years, _instance.dayOfYear);
    }

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
        LevelGoal.OnLevelDone += HandleLevelDone;
    }

    private void OnDisable()
    {
        Clock.OnClockTime -= HandleClockStatusChange;
        KillLayer.OnPlayerKilled -= HandlePlayerKilled;
        LevelGoal.OnLevelDone -= HandleLevelDone;
    }
    private void HandleLevelDone()
    {
        levelDone = true;
        canvas.enabled = false;
    }

    private void HandlePlayerKilled()
    {
        StartCoroutine(DelayIncreaseYear());
    }

    private IEnumerator<WaitForSeconds> DelayIncreaseYear()
    {
        yield return new WaitForSeconds(1f);
        dayOfYear += 356;
    }

    private void HandleClockStatusChange(Clock clock, ClockStatus status)
    {
        if (levelDone) return;

        if (status == ClockStatus.DESTROYED)
        {
            dayOfYear = dayOfYear + destructionCost;
            yearProgressImage.fillAmount = Mathf.Clamp01(dayOfYear / 356);
        } else if (status == ClockStatus.RUNNING)
        {
            activeClocks.Add(clock);
        } else
        {
            activeClocks.Remove(clock);
        }
    }

    private void Start()
    {
        canvas.gameObject.SetActive(true);
        years = startAge - 1;
    }

    private void Update()
    {
        if (levelDone) return;

        if (activeClocks.Count > 0)
        {
            dayOfYear += Time.deltaTime * daysPerSecond;
            yearProgressImage.fillAmount = Mathf.Clamp01(dayOfYear / 356);
        }
        if (dayOfYear >= 356f)
        {
            while (dayOfYear >= 356f)
            {
                dayOfYear -= 356;
                years++;
            }            
            yearProgressImage.fillAmount = Mathf.Clamp01(dayOfYear / 356);
            OnNewAge?.Invoke(years);
            StartCoroutine(ShowAge(years));
        }
    }       

    IEnumerator<WaitForSeconds> ShowAge(int showingYears)
    {
        Color color = Color.black;
        ageField.text = string.Format("~ Age {0} ~", showingYears);
        float start = Time.timeSinceLevelLoad;
        float delta = 0;
        yearProgressFrame.enabled = false;
        yearProgressImage.enabled = false;
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
        yearProgressFrame.enabled = true;
        yearProgressImage.enabled = true;

    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;   
        }
    }
}
