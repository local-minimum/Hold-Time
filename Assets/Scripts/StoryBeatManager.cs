using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryBeatManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI textField;

    [SerializeField]
    GameObject background;

    Queue<string> queue = new Queue<string>();


    bool showingMessage = false;

    private void OnEnable()
    {
        StoryBeat.OnStory += HandleStory;
        PlayerController.OnJump += HandleJump;
    }

    private void OnDisable()
    {
        StoryBeat.OnStory -= HandleStory;
        PlayerController.OnJump -= HandleJump;
    }

    bool resetting = false;
    bool resetRestarts = false;
    float resetStart = 0;

    private void HandleJump()
    {
        resetRestarts = true;
    }

    private void HandleStory(string message)
    {
        if (showingMessage || resetting)
        {
            queue.Enqueue(message);
        } else
        {
            StartCoroutine(ShowMessage(message));
        }
    }    

    IEnumerator<WaitForSeconds> ShowMessage(string message)
    {
        showingMessage = true;
        textField.text = message;
        background.SetActive(true);
        yield return new WaitForSeconds(message.Length * 0.1f);
        textField.text = "";
        background.SetActive(false);
        showingMessage = false;
    }

    private void Start()
    {
        background.SetActive(false);
    }

    bool ShowResetDuration(float timeToReset = 3)
    {
        var remaining = timeToReset - (Time.timeSinceLevelLoad - resetStart);
        if (remaining < 0)
        {
            background.SetActive(false);
            textField.text = "";
            return false;
        }
        background.SetActive(true);
        textField.text = string.Format(resetRestarts ? "Restart Level in {0}" : "Quit in {0}", Mathf.Round(remaining));
        return true;
    }

    private void Update()
    {
        var reset = SimpleUnifiedInput.Reset;
        if (!resetting && reset)
        {            
            resetting = true;
            resetStart = Time.timeSinceLevelLoad;
            if (!string.IsNullOrEmpty(textField.text))
            {
                queue.Enqueue(textField.text);
            }
            ShowResetDuration();
        } else if (resetting && reset)
        {
            if (!ShowResetDuration())
            {
                if (resetRestarts)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                } else
                {
                    SceneManager.LoadScene("Menu");
                }                
            }
        } else if (resetting)
        {
            resetting = false;
            textField.text = "";
            background.SetActive(false);
        } else if (queue.Count > 0)
        {
            StartCoroutine(ShowMessage(queue.Dequeue()));
        }       
    }
}
