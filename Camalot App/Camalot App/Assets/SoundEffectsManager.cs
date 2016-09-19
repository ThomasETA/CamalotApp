using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundEffectsManager : MonoBehaviour
{
    public List<AudioClip> SFXList = new List<AudioClip>();
    AudioSource myAudio;
    bool clipStarted = false;
    public byte Command = 0;

	// Use this for initialization
	void Start ()
    {
        myAudio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(clipStarted && myAudio.isPlaying == false)
        {
            clipStarted = false;
            byte module = byte.Parse(GameObject.Find("GameControlConnector").GetComponent<PuzzleClient>().ModuleID.ToString("X"), System.Globalization.NumberStyles.HexNumber);
            byte command = byte.Parse(Command.ToString("X"), System.Globalization.NumberStyles.HexNumber);
            GameObject.Find("GameControlConnector").GetComponent<PuzzleClient>().SendGCMessage(module, command, "");
        }
	}

    public void PlaySFX(string name)
    {
        switch (name)
        {
            case "Gun":
                myAudio.clip = SFXList[0];
                break;

            case "Siren":
                myAudio.clip = SFXList[1];
                break;

            case "Steps":
                myAudio.clip = SFXList[2];
                break;

            case "Rain":
                myAudio.clip = SFXList[3];
                break;

            case "Glass":
                myAudio.clip = SFXList[4];
                break;
                
            case "Thunder":
                myAudio.clip = SFXList[5];
                break;
                
            case "Safe":
                myAudio.clip = SFXList[6];
                break;

            case "Drawer":
                myAudio.clip = SFXList[7];
                break;
                
            default:
                break;
        }
        
        myAudio.Play();
        clipStarted = true;
    }
}
