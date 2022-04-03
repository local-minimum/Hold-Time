using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public delegate void LevelDoneEvent();

public class LevelGoal : MonoBehaviour
{   
    public static event LevelDoneEvent OnLevelDone;

    [SerializeField]
    Transform flagHolder;

    [SerializeField]
    string nextLevel = "TutorialScene";

    [SerializeField]
    string currentLevel = "TutorialScene";

    [SerializeField]
    string currentLevelName = "Tutorial";

    [SerializeField]
    TextMeshProUGUI levelNameText;

    [SerializeField]
    TextMeshProUGUI jumpsText;

    [SerializeField]
    TextMeshProUGUI retriesText;

    [SerializeField]
    TextMeshProUGUI clocksText;

    [SerializeField]
    TextMeshProUGUI ageText;

    [SerializeField]
    TextMeshProUGUI recordText;

    [SerializeField]
    Canvas levelDoneCanvas;

    Vector3 flagStartPosition;
    int jumps = 0;
    int retries = 0;

    private void OnEnable()
    {
        PlayerController.OnJump += HandleJump;
        KillLayer.OnPlayerKilled += HandleDeath;
        Clock.OnClockTime += HandleClock;
    }

    private void OnDisable()
    {
        PlayerController.OnJump -= HandleJump;
        KillLayer.OnPlayerKilled -= HandleDeath;
        Clock.OnClockTime -= HandleClock;
    }

    int clocks = 0;
    int clocksDestroyed = 0;

    private void HandleClock(Clock clock, ClockStatus status)
    {
        switch (status)
        {
            case ClockStatus.SPAWNED:
                clocks++;
                break;
            case ClockStatus.DESTROYED:
                clocksDestroyed++;
                break;
        }
    }

    private void HandleJump()
    {
        jumps++;
    }

    private void HandleDeath()
    {
        retries++;
    }

    bool alive = false;
    float wakeupTime;

    private void Start()
    {        
        levelDoneCanvas.gameObject.SetActive(false);
        levelNameText.text = currentLevelName;
        flagStartPosition = flagHolder.localPosition;
    }

    private string HighscoreLocation
    {
        get
        {
            return string.Format("highscore.{0}", currentLevel);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
        {
            OnLevelDone?.Invoke();
            jumpsText.text = jumps.ToString();
            retriesText.text = retries.ToString();
            clocksText.text = string.Format("{0} / {1}", clocksDestroyed, clocks);

            var ageInfo = Age.GetAge();
            ageText.text = ageInfo.ToString();

            int recordDays = PlayerPrefs.GetInt(HighscoreLocation, 101 * 365);
            if (ageInfo.Age < recordDays)
            {
                recordText.text = "New Record!";
                recordText.alignment = TextAlignmentOptions.BottomJustified;
                PlayerPrefs.SetInt(HighscoreLocation, ageInfo.Age);
            } else
            {
                var record = new AgeInfo(recordDays);
                if (record.years > 100)
                {
                    recordText.text = "Illegal highscore value!";
                } else
                {
                    recordText.text = string.Format("Record: {0}", record.ToString());
                }
                recordText.alignment = TextAlignmentOptions.BottomRight;
            }

            levelDoneCanvas.gameObject.SetActive(true);
            wakeupTime = Time.timeSinceLevelLoad;
            alive = true;
        }
    }

    public void HandleNext()
    {
        if (nextLevel.Length == 0)
        {
            SceneManager.LoadScene("Menu");
        } else
        {
            SceneManager.LoadScene(nextLevel);
        }
    }

    private void Update()
    {
        UpdateFlag();
        if (!alive || Time.timeSinceLevelLoad - wakeupTime < 0.5f) return;

        if (Gamepad.current.leftShoulder.IsPressed() || Gamepad.current.rightShoulder.IsPressed())
        {
            HandleNext();
        }
    }

    void UpdateFlag()
    {
        flagHolder.localPosition = flagStartPosition + Vector3.up * 0.5f * Mathf.Sin(Time.timeSinceLevelLoad * 0.5f);
        flagHolder.rotation = Quaternion.Euler(0, Mathf.Sin(Time.realtimeSinceStartup * 2f) * 30f, 0);
    }
}
