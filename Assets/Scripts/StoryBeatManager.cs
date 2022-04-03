using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    }

    private void OnDisable()
    {
        StoryBeat.OnStory -= HandleStory;
    }
    private void HandleStory(string message)
    {
        if (showingMessage)
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

    private void Update()
    {
        if (queue.Count > 0)
        {
            StartCoroutine(ShowMessage(queue.Dequeue()));
        }       
    }
}
