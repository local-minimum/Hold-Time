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
    float faceShakeMagnitude = 0.4f;

    float degHours = 0;
    float degMinutes = 0;
    float correctTime = 0;
    bool clockAlive = false;

    private void Start()
    {
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
        hoursHandle.rotation = Quaternion.Euler(0f, 0f, degHours);
        minutesHandle.rotation = Quaternion.Euler(0f, 0f, degMinutes);
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
    float InputHours { 
        get
        {
            Vector2 hours = Gamepad.current.leftStick.ReadValue();
            if (hours.magnitude < inputThreshold) return Mathf.Infinity;
            float hour = 3 - Mathf.Atan2(hours.y, hours.x) * angleToHourConst;
            if (hour < 0) return hour + 12;
            return hour;            
        }
    }

    static float angleToMinutesConst = 1f / (2f * Mathf.PI) * 60f;
    float InputMinutes
    {
        get
        {
            Vector2 minutes = Gamepad.current.rightStick.ReadValue();
            if (minutes.magnitude < inputThreshold) return Mathf.Infinity;
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
        if (TimeDifference(inputHours, hours, 12) <= hourTolerance && TimeDifference(inputMinutes, minutes, 60) <= minuteTolerance) return true;
        return (
            TimeDifference(inputMinutes * minuteToHour, hours, 12) <= hourTolerance 
            && TimeDifference(inputHours * hourToMinute, minutes, 60) <= minuteTolerance
        );
    }

    private void Update()
    {
        if (!clockAlive) return;
        SetClock(hoursSpeed * Time.deltaTime, minutesSpeed * Time.deltaTime);
        if (CheckInputCorrectTime())
        {
            face.transform.localPosition = new Vector3(Random.Range(-faceShakeMagnitude, faceShakeMagnitude), Random.Range(-faceShakeMagnitude, faceShakeMagnitude));
            if (correctTime == 0) OnClockTime?.Invoke(this, ClockStatus.STOPPED);
            correctTime += Time.deltaTime;
            if (correctTime >= challengeTime)
            {
                OnClockTime?.Invoke(this, ClockStatus.DESTROYED);
                clockAlive = false;
                Destroy(gameObject);
            }
        } else
        {
            if (correctTime > 0) OnClockTime?.Invoke(this, ClockStatus.RUNNING);
            correctTime = 0;
            face.transform.localPosition = Vector3.zero;
        }
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
