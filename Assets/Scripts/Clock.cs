using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ClockStatus { SPAWNED, STOPPED, RUNNING, DESTROYED };
public delegate void ClockTimeEvent(Clock clock, ClockStatus status);

public class Clock : MonoBehaviour
{
    public static event ClockTimeEvent OnClockTime;

    [SerializeField]
    Transform hoursHandle;

    [SerializeField]
    Transform minutesHandle;

    [SerializeField]
    Transform face;
    
    float startHour = 3;
    float startMinute = 42;

    [SerializeField, Range(-100, 100)]
    float minutesSpeed = 0f;

    [SerializeField, Range(-100, 100)]
    float hoursSpeed = 0f;

    [SerializeField, Range(0, 6)]
    float hourTolerance = 1f;

    [SerializeField, Range(0, 30)]
    float minuteTolerance = 5f;

    [SerializeField, Range(0, 3)]
    float challengeTime = 1f;

    [SerializeField, Range(0, 1)]
    float inputThreshold = 0.3f;

    [SerializeField, Range(0, .1f)]
    float shakeMagnitude = 0.02f;

    float degZeroHours;
    float degZeroMinutes;
    Vector3 faceStartPosition;
    Vector3 hoursStartPosition;
    Vector3 minutesStartPosition;

    float degHours = 0;
    float degMinutes = 0;
    float correctTime = 0;
    bool clockAlive = false;

    private void Start()
    {
        degZeroHours = hoursHandle.rotation.eulerAngles.z;
        degZeroMinutes = minutesHandle.rotation.eulerAngles.z;

        faceStartPosition = face.localPosition;
        hoursStartPosition = hoursHandle.localPosition;
        minutesStartPosition = minutesHandle.localPosition;

        startHour = Random.Range(0, 11);
        startMinute = Random.Range(0, 59);

        degHours = -startHour / 12f * 360f;
        degMinutes = -startMinute / 60f * 360f;        

        SetClock();
        StartCoroutine(ClockWakupInfo());
    }

    private IEnumerator<WaitForSeconds> ClockWakupInfo()
    {
        yield return new WaitForSeconds(1f);
        OnClockTime?.Invoke(this, ClockStatus.SPAWNED);
    }

    private void SetClock(float addHoursAngle = 0, float addMinutesAngle = 0)
    {
        degHours -= addHoursAngle;
        degMinutes -= addMinutesAngle;
        degHours %= 360f;
        degMinutes %= 360f;
        hoursHandle.rotation = Quaternion.Euler(0f, 0f, degZeroHours + degHours);
        minutesHandle.rotation = Quaternion.Euler(0f, 0f, degZeroMinutes + degMinutes);
    }

    static float degreeToHour = 12f / 360f;
    float Hours
    {
        get
        {
            float hour = 12 - degHours * degreeToHour;
            if (hour > 12) return hour - 12f;
            return hour;
        }
    }

    static float degreeToMinute = 60f / 360f;
    float Minutes
    {
        get
        {
            float minute = 60 - degMinutes * degreeToMinute;
            if (minute > 60) return minute - 60f;
            return minute;
        }
    }

    static float angleToHourConst = 1f / (2f * Mathf.PI) * 12f;
    bool hoursFromKeyboard;
    float InputHours { 
        get
        {
            
            Vector2 hours = SimpleUnifiedInput.VirtualPrimaryStick(inputThreshold, out hoursFromKeyboard);
            if (hours.sqrMagnitude == 0) return Mathf.Infinity;
            float hour = 3 - Mathf.Atan2(hours.y, hours.x) * angleToHourConst;
            if (hour < 0) return hour + 12;
            return hour;            
        }
    }

    static float angleToMinutesConst = 1f / (2f * Mathf.PI) * 60f;
    bool minutesFromKeyboard;
    float InputMinutes
    {
        get
        {
            Vector2 minutes = SimpleUnifiedInput.VirtualSecondaryStick(inputThreshold, out minutesFromKeyboard);
            if (minutes.sqrMagnitude == 0) return Mathf.Infinity;
            float minute = 15 - Mathf.Atan2(minutes.y, minutes.x) * angleToMinutesConst;
            if (minute < 0) return minute + 60;
            return minute;
        }
    }

    static float TimeDifference(float a, float b, float max)
    {
        float diff = Mathf.Abs(b - a);
        if (diff > max / 2)
        {
            return max - diff;
        }
        return diff;
    }

    static float hourToMinute = 60f / 12f;
    static float minuteToHour = 12f / 60f;
    private bool CheckInputCorrectTime()
    {
        float inputHours = InputHours;
        float inputMinutes = InputMinutes;
        float hours = Hours;
        float minutes = Minutes;
        if (Mathf.Infinity == inputHours || Mathf.Infinity == inputMinutes) return false;
        if (
            TimeDifference(inputHours, hours, 12) <= (hoursFromKeyboard ? 1 : hourTolerance)
            && TimeDifference(inputMinutes, minutes, 60) <= (minutesFromKeyboard ? 4 : minuteTolerance)
        ) return true;
        return (
            TimeDifference(inputMinutes * minuteToHour, hours, 12) <= (minutesFromKeyboard ? 1 : hourTolerance)
            && TimeDifference(inputHours * hourToMinute, minutes, 60) <= (hoursFromKeyboard ? 4 : minuteTolerance)
        );
    }

    Vector3 Shake
    {
        get
        {
            return new Vector3(Random.Range(-shakeMagnitude, shakeMagnitude), Random.Range(-shakeMagnitude, shakeMagnitude));
        }
    }

    private void Update()
    {
        if (!clockAlive) return;
        SetClock(hoursSpeed * Time.deltaTime, minutesSpeed * Time.deltaTime);
        if (CheckInputCorrectTime())
        {
            face.localPosition = faceStartPosition + Shake;
            minutesHandle.localPosition = minutesStartPosition + Shake;
            hoursHandle.localPosition = hoursStartPosition + Shake;

            if (correctTime == 0) OnClockTime?.Invoke(this, ClockStatus.STOPPED);
            correctTime += Time.deltaTime;
            if (correctTime >= challengeTime)
            {
                OnClockTime?.Invoke(this, ClockStatus.DESTROYED);
                clockAlive = false;
                StartCoroutine(Explode());
            }
        } else
        {
            if (correctTime > 0) OnClockTime?.Invoke(this, ClockStatus.RUNNING);
            correctTime = 0;
            face.localPosition = faceStartPosition;
            minutesHandle.localPosition = minutesStartPosition;
            hoursHandle.localPosition = hoursStartPosition;
        }
    }

    IEnumerator<WaitForSeconds> Explode()
    {
        var rb = hoursHandle.gameObject.AddComponent<Rigidbody2D>();
        rb.velocity = Shake.normalized * Random.Range(1, 5);
        rb = minutesHandle.gameObject.AddComponent<Rigidbody2D>();
        rb.velocity = Shake.normalized * Random.Range(1, 5);
        rb = face.gameObject.AddComponent<Rigidbody2D>();
        rb.velocity = Shake.normalized * Random.Range(1, 5);
        yield return new WaitForSeconds(2f);
        hoursHandle.gameObject.SetActive(false);
        minutesHandle.gameObject.SetActive(false);
        face.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            clockAlive = true;
            if (CheckInputCorrectTime())
            {
                OnClockTime?.Invoke(this, ClockStatus.STOPPED);
            } else
            {
                OnClockTime?.Invoke(this, ClockStatus.RUNNING);
            }
            clockAlive = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            clockAlive = false;
            OnClockTime?.Invoke(this, ClockStatus.STOPPED);
            clockAlive = false;
        }
    }
}
