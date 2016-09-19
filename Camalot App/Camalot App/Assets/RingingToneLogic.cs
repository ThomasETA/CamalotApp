using UnityEngine;
using System.Collections;

public class RingingToneLogic : MonoBehaviour
{
    public AudioClip Ring;
    public AudioClip DialTone;
    AudioSource myAudio;

	// Use this for initialization
	void Start ()
    {
        myAudio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void ToggleTone(bool play)
    {
        if(play == true)
        {
            if (myAudio.isPlaying == false)
            {
                myAudio.clip = DialTone;
                myAudio.Play();
            }
        }
        else if(myAudio.isPlaying == true)
        {
            if(myAudio.clip == DialTone)
            {
                myAudio.Stop();
            }           
        }
    }

    public void ToggleRing(bool play)
    {
        if (play == true)
        {
            if (myAudio.isPlaying == false)
            {
                myAudio.clip = Ring;
                myAudio.Play();
            }
        }
        else if (myAudio.isPlaying == true)
        {
            if (myAudio.clip == Ring)
            {
                myAudio.Stop();
            }
        }
    }
}
