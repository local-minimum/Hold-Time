using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void StoryEvent(string message);
public enum StoryRepeatMode { Cycle, RepeatLast, Oneshot };

public class StoryBeat : MonoBehaviour
{
    public static event StoryEvent OnStory;

    [SerializeField]
    StoryRepeatMode RepeatMode = StoryRepeatMode.Oneshot;

    [SerializeField]
    string[] Messages;

    [SerializeField]
    string StoryId;

    string StoryLocation
    {
        get
        {
            return string.Format("Story.{0}", StoryId);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
        {            
            var nextIdx = PlayerPrefs.GetInt(StoryLocation, 0);
            if (nextIdx >= Messages.Length)
            {
                switch (RepeatMode)
                {
                    case StoryRepeatMode.Cycle:
                        nextIdx = 0;
                        break;
                    case StoryRepeatMode.RepeatLast:
                        nextIdx = Messages.Length - 1;
                        break;
                }
            }
            if (nextIdx < Messages.Length)
            {
                PlayerPrefs.SetInt(StoryLocation, nextIdx + 1);
                OnStory?.Invoke(Messages[nextIdx]);
            }
        }
    }
}
