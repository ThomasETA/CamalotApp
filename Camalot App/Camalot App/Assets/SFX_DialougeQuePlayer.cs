using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SFX_DialougeQuePlayer : MonoBehaviour {

    AudioSource myAudioSource;
    Animator myAnimator;

    //All audio clips, or, all audio clips for this charcter.
    public List<AudioClip> AllAudioClips = new List<AudioClip>();
    List<AudioClip> AudioClipQue = new List<AudioClip>();
    List<AudioClip> SparrowClipQueue = new List<AudioClip>();

    int queueCountLastFrame;
    int sparrowQueueCountLastFrame;
    int queueCount;
    int sparrowQueueCount;
    bool sendWhenDone = false;
    bool sendWhenSparrowDone = false;
    bool finalClipPlaying = false;
    bool finalSparrowClipPlaying = false;

	// Use this for initialization
	void Start ()
    {
        myAnimator = GetComponent<Animator>();
        myAudioSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(MonkeyGodManager.digThroughCarmenQueue == true)
        {
            for (int i = 0; i < MonkeyGodManager.carmenClipsToPlay.Count;)
            {
                AddClipToQueue(MonkeyGodManager.carmenClipsToPlay[i], true);
                MonkeyGodManager.carmenClipsToPlay.RemoveAt(0);
            }

            if (MonkeyGodManager.carmenClipsToPlay.Count == 0)
            {
                
                MonkeyGodManager.digThroughCarmenQueue = false;
            }
        }
        else if(MonkeyGodManager.digThroughQueue == true)
        {
            for (int i = 0; i < MonkeyGodManager.clipsToPlay.Count;)
            {
                AddClipToQueue(MonkeyGodManager.clipsToPlay[i], false);
                MonkeyGodManager.clipsToPlay.RemoveAt(0);
            }

            if(MonkeyGodManager.clipsToPlay.Count == 0)
            {
                MonkeyGodManager.digThroughQueue = false;
            }
        }

        queueCount = AudioClipQue.Count;
        sparrowQueueCount = SparrowClipQueue.Count;

        if (sendWhenDone == true)
        {
            if(myAudioSource.isPlaying == false)
            {
                MonkeyGodManager.playingPhoneClip = false;
                Debug.Log("we sendin");
                byte module = byte.Parse(GameObject.Find("GameControlConnector").GetComponent<PuzzleClient>().ModuleID.ToString("X"), System.Globalization.NumberStyles.HexNumber);
                byte command = 0x60;
                GameObject.Find("GameControlConnector").GetComponent<PuzzleClient>().SendGCMessage(module, command, "");
                sendWhenDone = false;
                finalClipPlaying = false;
            }
        }

        if (sendWhenSparrowDone == true)
        {
            if (myAudioSource.isPlaying == false)
            {
                Debug.Log("we sendin");
                byte module = byte.Parse(GameObject.Find("GameControlConnector").GetComponent<PuzzleClient>().ModuleID.ToString("X"), System.Globalization.NumberStyles.HexNumber);
                byte command = 0x60;
                GameObject.Find("GameControlConnector").GetComponent<PuzzleClient>().SendGCMessage(module, command, "");
                sendWhenSparrowDone = false;
                finalSparrowClipPlaying = false;
            }
        }

        bool skip = Input.GetKeyDown(KeyCode.Escape);

        if (skip == true || (myAudioSource.isPlaying == false && (queueCount > 0 || sparrowQueueCount > 0)))
        {
            if(sparrowQueueCount > 0)
            {
                MonkeyGodManager.playingPhoneClip = false;
                Debug.Log(SparrowClipQueue[0].name);
                myAudioSource.clip = SparrowClipQueue[0];
                SparrowClipQueue.RemoveAt(0);
                myAudioSource.Play();
            }
            else
            {
                MonkeyGodManager.playingPhoneClip = true;
                Debug.Log(AudioClipQue[0].name);
                myAudioSource.clip = AudioClipQue[0];
                AudioClipQue.RemoveAt(0);
                myAudioSource.Play();
            }
            

            skip = false;
        }

        if (sparrowQueueCount == 0 && sparrowQueueCountLastFrame != 0)
        {
            
            finalSparrowClipPlaying = true;     
        }

        if (queueCount == 0 && queueCountLastFrame != 0)
        {

            finalClipPlaying = true;
        }



        if (myAudioSource.isPlaying == true && finalClipPlaying == true)
        {
            sendWhenDone = true;
        }

        if (myAudioSource.isPlaying == true && finalSparrowClipPlaying == true)
        {
            sendWhenSparrowDone = true;
        }
        queueCountLastFrame = queueCount;
        sparrowQueueCountLastFrame = sparrowQueueCount;
        
	}

    public void AddClipToQue(string clipID_, int skipIfThisManyInBacklog_ = 100000000)
    {
        bool wasSomethingAdded = false;
        sendWhenDone = false;
        foreach (AudioClip a in AllAudioClips)
        {
            if(a.name.Substring(0, 3) == clipID_)
            {
                AudioClipQue.Add(a);
                wasSomethingAdded = true;
            }
        }

        if(!wasSomethingAdded)
        {
            Debug.LogError("Invalid clip index");
        }
    }

    public void AddClipToQueue(string clipID_, bool sparrow)
    {
        bool wasSomethingAdded = false;
        sendWhenDone = false;
        foreach (AudioClip a in AllAudioClips)
        {
            if(sparrow == false)
            {
                if (a.name.Substring(0, 3) == clipID_)
                {
                    AudioClipQue.Add(a);
                    wasSomethingAdded = true;
                }
            }     
            else
            {
                if (a.name.Substring(0, 3) == clipID_)
                {
                    SparrowClipQueue.Add(a);
                    wasSomethingAdded = true;
                }
            }    
        }

        if (!wasSomethingAdded)
        {
            Debug.LogError("Invalid clip index");
        }
    }

    public void ClearQueue(bool sparrow = false)
    {
        if(sparrow == false)
        {
            AudioClipQue.Clear();
        }
        else
        {
            SparrowClipQueue.Clear();
        }
    }

    void PausePlayback()
    {
        myAudioSource.Pause();
    }

    void UnpausePlayback()
    {
        myAudioSource.UnPause();
    }
}
