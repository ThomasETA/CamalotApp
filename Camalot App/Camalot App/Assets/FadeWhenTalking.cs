using UnityEngine;
using System.Collections;

public class FadeWhenTalking : MonoBehaviour
{
    public AudioSource Speaker;
    [Range(0,1)]
    public float MinVolume;
    [Range(0, 1)]
    float MaxVolume;
    public float FadeSpeed;
    float timer;

    AudioSource myAudio;

    bool atMin = false;
    bool atMax = false;

	// Use this for initialization
	void Start ()
    {
        myAudio = GetComponent<AudioSource>();
        MaxVolume = myAudio.volume;
        Debug.Log(MaxVolume);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Speaker.isPlaying == true && atMin == false)
        {
            if(myAudio.volume > MinVolume)
            {
                myAudio.volume = Mathf.Lerp(myAudio.volume, myAudio.volume - FadeSpeed, 1f);
            }
            else
            {
                atMin = true;
                atMax = false;
                myAudio.volume = MinVolume;
            }
        }

        if (Speaker.isPlaying == false && atMax == false)
        {
            if (myAudio.volume < MaxVolume)
            {
                myAudio.volume = Mathf.Lerp(myAudio.volume, myAudio.volume + FadeSpeed, 1f);
            }
            else
            {
                atMin = false;
                atMax = true;
                myAudio.volume = MaxVolume;
            }
        }

	
	}
}
