using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonkeyGodManager : MonoBehaviour
{
    public GameObject Mouth;
    public AudioSource BGM;
    public RingingToneLogic DialTone;
    public AudioSource DialSounds;
    public SoundEffectsManager CeilingSounds;
    public SoundEffectsManager WindowSounds;
    public SoundEffectsManager CenterSounds;

    public static List<string> clipsToPlay = new List<string>();
    public static List<string> carmenClipsToPlay = new List<string>();
    public static bool digThroughQueue = false;
    public static bool digThroughCarmenQueue = false;
    public static bool playingPhoneClip = false;

     
    void Awake()
    {
        if (!Application.isEditor)
        {
            Application.runInBackground = true;
            Screen.SetResolution(800, 600, false, 0);
            //Cursor.visible = false;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.anyKeyDown)
        //{
        //    WindowSounds.PlaySFX("Steps");

        //}

        //Debug.Log(playingPhoneClip);
    }

    public void OnConnected()
    {

    }

    public void OnMessageReceived(PuzzleClient.Message msg)
    {
        Debug.Log("GOT A MESSAGE");
        string clipToAdd = "";

        string textToAdd = "";

        for (int pos = 0; pos < msg.Payload.Length; ++pos)
        {
            textToAdd += (char)msg.Payload[pos];
        }

        Debug.Log(textToAdd);

        switch (msg.Command)
        {
            /***************************************************************
             Command: 0x50
             Function: Stops the audio clip queue
             Payload Type: none

             Description: Clears the Phone Audio Queue and if there is 
             someone on the phone currently speaking it stops their audio.
            **************************************************************/
            case 0x50:
                digThroughQueue = false;
                clipsToPlay.Clear();
                Mouth.GetComponent<SFX_DialougeQuePlayer>().ClearQueue(false);
                if(playingPhoneClip == true)
                {
                    Mouth.GetComponent<AudioSource>().Stop();
                    Mouth.GetComponent<AudioSource>().clip = null;
                }
                
                break;

            /***************************************************************
             Command: 0x51
             Function: Add voice clip to Sparrow Clip Queue
             Payload Type: Byte Array (already parsed into String: textToAdd)
             Value of Payload: 
             -Three number clip ID (E.G. "001" 0r "011")

             Description: Has Sparrow play the requested clip by adding
             it to the queue.
            **************************************************************/
            case 0x51:                  
                for(int i = 0; i < msg.Payload.Length; ++i)
                {
                    if ((char)msg.Payload[i] == ',')
                    {
                        carmenClipsToPlay.Add(clipToAdd);
                        clipToAdd = "";
                        
                        if(i != msg.Payload.Length - 1)
                        {
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    clipToAdd += (char)msg.Payload[i];                                
                }

                if(clipToAdd != "")
                {                  
                    carmenClipsToPlay.Add(clipToAdd);
                    clipToAdd = "";
                    digThroughCarmenQueue = true;
                }                               
                break;

            /***************************************************************
             Command: 0x52
             Function: Tells DialTone object to play either Dial Tone or 
             Ring sound
             Payload Type: Byte
             Value of Payload: 
             -00: DialTone
             -01: Ring sound

             Description: Play Dialtone when phone is picked up, and Ring 
             when someone is called.
            **************************************************************/
            case 0x52:
                if(msg.Payload[0] == 00)
                {
                    DialTone.ToggleTone(true);
                }
                else if(msg.Payload[0] == 01)
                {
                    DialTone.ToggleRing(true);
                }
                break;

            /***************************************************************
             Command: 0x53
             Function: Tells DialTone object to stop any sound it 
             is playing.
             Payload Type: Byte
             Value of Payload: 
             -00: DialTone
             -01: Ring sound
             -Can send nothing to silence both.
             Description: Stops the Dial Tone or Ring sound
            **************************************************************/
            case 0x53:
                if(msg.Payload.Length > 0)
                {
                    if (msg.Payload[0] == 00)
                    {
                        DialTone.ToggleTone(false);
                    }
                    else if (msg.Payload[0] == 01)
                    {
                        DialTone.ToggleRing(false);
                    }
                }                
                else if(msg.Payload.Length == 0)
                {
                    DialTone.ToggleTone(false);
                    DialTone.ToggleRing(false);
                }
                break;

            /***************************************************************
             Command: 0x54
             Function: BGM Volume Settings
             Payload Type: Two Bytes 
             Value of Payload: 
             -First Byte: Value of tens place (00 - 09)
             -Second Byte: Value of ones place (00 - 09)

             Description: The bytes from the payload determine the value 
             of the Volume based on a percent.
             (E.G. if the first byte sent is 05 and the second is 03 
             then the volume will be set at 53%)
            **************************************************************/
            case 0x54:
                float percent = msg.Payload[0] * 10;
                percent += msg.Payload[1];
                percent /= 100;

                if(percent > 1)
                {
                    percent = 1;
                }
                else if(percent < 0)
                {
                    percent = 0;
                }

                BGM.volume = percent;
                break;

            /***************************************************************
             Command: 0x55
             Function: Plays Dial Sounds and Hang up noise
             Payload Type: Bytes
             Value of Payload: 
             -00 to 09: 0 through 9 buttons
             -0A: Star key
             -0B: Pound Key
             -0C: Hang Up Sound

             Description: Plays the requested sound from the DialSounds
             Audio Source
            **************************************************************/
            case 0x55:
                DialSounds.clip = DialSounds.GetComponent<DialSoundsLogic>().DialSoundsList[msg.Payload[0]];
                DialSounds.Play();
                break;


            /***************************************************************
             Command: 0x56
             Function: Plays a soundeffect through window speaker
             Payload Type: Byte Array (already parsed into String: textToAdd)
             Value of Payload: 
             -Gun: Plays Gunshot
             -Glass: Plays glass breaking
             -Steps: Plays Footsteps
             -Siren: Plays Siren
             -Rain: Plays storm
             -Safe: Plays safe opening sound
             -Drawer: Plays Drawer opening sound

             Description: This will send a sound effect to the window 
             speaker only!
            **************************************************************/
            case 0x56:
                WindowSounds.PlaySFX(textToAdd);
                break;

            /***************************************************************
             Command: 0x57
             Function: Plays a soundeffect through ceiling Speaker
             Payload Type: Byte Array (already parsed into String: textToAdd)
             Value of Payload: 
             -Gun: Plays Gunshot
             -Glass: Plays glass breaking
             -Steps: Plays Footsteps
             -Siren: Plays Siren
             -Rain: Plays storm
             -Safe: Plays safe opening sound
             -Drawer: Plays Drawer opening sound

             Description: This will send a sound effect to the ceiling
             speaker only!
            **************************************************************/
            case 0x57:
                CeilingSounds.PlaySFX(textToAdd);
                break;

            /***************************************************************
             Command: 0x58
             Function: Plays a soundeffect through Both Speaker
             Payload Type: String
             Value of Payload: 
             -Gun: Plays Gunshot
             -Glass: Plays glass breaking
             -Steps: Plays Footsteps
             -Siren: Plays Siren
             -Rain: Plays storm
             -Safe: Plays safe opening sound
             -Drawer: Plays Drawer opening sound

             Description: This will send a sound effect to the both
             speaker only!
            **************************************************************/
            case 0x58:
                CenterSounds.PlaySFX(textToAdd);
                break;

            /***************************************************************
             Command: 0x59
             Function: Add voice clip to Phone Clip Queue
             Payload Type: Byte Array (already parsed into String: textToAdd)
             Value of Payload: 
             -Three number clip ID (E.G. "001" 0r "011")

             Description: Has the phone played the requested clip by adding
             it to the queue.
            **************************************************************/
            case 0x59:
                for (int i = 0; i < msg.Payload.Length; ++i)
                {
                    if ((char)msg.Payload[i] == ',')
                    {
                        clipsToPlay.Add(clipToAdd);
                        clipToAdd = "";

                        if (i != msg.Payload.Length - 1)
                        {
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    clipToAdd += (char)msg.Payload[i];
                }

                if (clipToAdd != "")
                {
                    clipsToPlay.Add(clipToAdd);
                    clipToAdd = "";
                    digThroughQueue = true;
                }
                break;

            default:
                break;


        }
    }
}
