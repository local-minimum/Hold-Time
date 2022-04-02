using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ClockStatus { STOPPED, RUNNING, DESTROYED };
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

    [SerializeField, Range(0, 12)]
    float startHour = 3;

    [SerializeField, Range(0, 60)]
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
    bool clockAlive = true;

    private void Start()
    {
        degHours = -startHour / 12f * 360f;
        degMinutes = -startMinute / 60f * 360f;
        SetClock();
        OnClockTime?.Invoke(this, ClockStatus.RUNNING);
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

    static float angleToHourConst = 1 / (2 * Mathf.PI) * 12;
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

    static float angleToMinutesConst = 1 / (2 * Mathf.PI) * 60;
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

    private bool CheckInputCorrectTime()
    {
        float hours = InputHours;
        float minutes = InputMinutes;
        if (Mathf.Infinity == hours || Mathf.Infinity == minutes) return false;
        if (TimeDifference(hours, Hours, 12) > hourTolerance) return false;
        if (TimeDifference(minutes, Minutes, 60) > minuteTolerance) return false;
        return true;
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
}
