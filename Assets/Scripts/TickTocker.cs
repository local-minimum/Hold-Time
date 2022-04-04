using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickTocker : MonoBehaviour
{
    AudioSource speaker;

    // Start is called before the first frame update
    void Start()
    {
        speaker = GetComponent<AudioSource>();
        speaker.volume = 0;
    }

    public bool Tick
    {
        set
        {
            speaker.volume = value ? 0.3f : 0f;
        }
    }
}
