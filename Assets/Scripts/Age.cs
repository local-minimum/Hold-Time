using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public struct AgeInfo
{
    public int years;
    public int days;

    public AgeInfo(int years, float days)
    {
        this.years = years;
        this.days = Mathf.FloorToInt(days);
    }

    public AgeInfo(int days)
    {
        years = days / 365;
        this.days = days - years * 365;
    }

    override public string ToString()
    {
        var months = new int[] {
            31, // Jan
            28, // Feb
            31, // Mar
            30, // Apr
            31, // May
            30, // Jun
            30, // Jul
            31, // Aug
            30, // Sep
            31, // Oct
            30, // Nov
            31, // Dec
        };
        var d = days;
        for (int i = 0; i < months.Length; i++)
        {
            if (d < months[i])
            {
                if (i == 0)
                {
                    return string.Format("{0}Y {1}D", years, days);
                }
                return string.Format("{0}Y {1}M", years, i);
            }
            d -= months[i];
        }
        return string.Format("{0} years", years + 1);
    }

    public int Age {
        get
        {
            return years * 365 + days; 
        }
    }
}

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
        years++;
        StartCoroutine(ShowAge(years));
    }

    private void HandleClockStatusChange(Clock clock, ClockStatus status)
    {
        if (levelDone) return;

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

    private void Start()
    {
        canvas.gameObject.SetActive(true);
        years = startAge;
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

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
