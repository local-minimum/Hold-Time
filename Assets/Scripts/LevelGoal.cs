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

    [SerializeField]
    Butterfly butterflyPrefab;

    Vector3 flagStartPosition;
    int jumps = 0;
    int retries = 0;

    private void OnEnable()
    {
        PlayerController.OnJump += HandleJump;
        KillLayer.OnPlayerKilled += HandleDeath;
        Clock.OnClockTime += HandleClock;
        Age.OnNewAge += HandleNewAge;
    }


    private void OnDisable()
    {
        PlayerController.OnJump -= HandleJump;
        KillLayer.OnPlayerKilled -= HandleDeath;
        Clock.OnClockTime -= HandleClock;
        Age.OnNewAge -= HandleNewAge;
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

    bool listenForJump = false;
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

    private void MakeStats(AgeInfo age, string recordText, TextAlignmentOptions textAlignment) 
    {
        jumpsText.text = jumps.ToString();
        retriesText.text = retries.ToString();
        clocksText.text = string.Format("{0} / {1}", clocksDestroyed, clocks);       
        ageText.text = age.ToString();

        this.recordText.text = recordText;
        this.recordText.alignment = textAlignment;

        levelDoneCanvas.gameObject.SetActive(true);
        wakeupTime = Time.timeSinceLevelLoad;
        listenForJump = true;
    }

    bool continueWithNext = true;

    PlayerController goalPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<PlayerController>();
        if (!listenForJump && player != null)
        {
            goalPlayer = player;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() == goalPlayer)
        {
            goalPlayer = null;
        }
    }

    void Goal()
    {
        OnLevelDone?.Invoke();
        var ageInfo = Age.GetAge();
        string recordText = "Illegal highscore value!";
        TextAlignmentOptions alignment = TextAlignmentOptions.BottomJustified;
        int recordDays = PlayerPrefs.GetInt(HighscoreLocation, 101 * 365);

        if (ageInfo.Age < recordDays)
        {
            recordText = "New Record!";
            PlayerPrefs.SetInt(HighscoreLocation, ageInfo.Age);
        }
        else
        {
            var record = new AgeInfo(recordDays);
            if (record.years < 100)
            {
                recordText = string.Format("Record: {0}", record.ToString());
            }
            alignment = TextAlignmentOptions.BottomRight;
        }
        MakeStats(ageInfo, recordText, alignment);
        continueWithNext = true;
    }

    private void HandleNewAge(int years)
    {
        if (years < 100) return;
        Vector3 spawn = Camera.main.transform.position + Vector3.forward * 4f;
        int bflies = Random.Range(10, 20);
        for (int i = 0; i< bflies; i++)
        {
            var b = Instantiate(butterflyPrefab);
            b.transform.position = spawn;
        }
        OnLevelDone?.Invoke();
        var ageInfo = Age.GetAge();
        MakeStats(ageInfo, "Too old to continue", TextAlignmentOptions.BottomJustified);
        continueWithNext = false;
    }

    public void HandleNext()
    {
        if (!continueWithNext)
        {
            SceneManager.LoadScene(currentLevel);
        } else if (nextLevel.Length == 0)
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
        if (!listenForJump)
        {
            if (goalPlayer != null && goalPlayer.Alive && goalPlayer.Grounded)
            {
                Goal();
            }

            return;
        }
            
        if (Time.timeSinceLevelLoad - wakeupTime < 0.5f) return;

        if (SimpleUnifiedInput.Jump != JumpingState.NotJumping)
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
