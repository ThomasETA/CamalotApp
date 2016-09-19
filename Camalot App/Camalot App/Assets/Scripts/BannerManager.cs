using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BannerManager : MonoBehaviour
{
    public GameObject display1;
    public GameObject display2;
    public GameObject display3;
    public GameObject display4;

    public Sprite king;
    public Sprite knight;
    public Sprite lady;
    public Sprite castle;

    public Sprite justice;
    public Sprite strength;
    public Sprite judgement;
    public Sprite temperance;

    public GameObject hint1;
    public GameObject hint2;
    public GameObject hint3;
    public GameObject hint4;

    public GameObject victory1;
    public GameObject victory2;
    public GameObject victory3;
    public GameObject victory4;

    public GameObject defeat1;
    public GameObject defeat2;
    public GameObject defeat3;
    public GameObject defeat4;

    private AudioSource audio;

    public AudioClip hintSwoosh;
    public AudioClip puzzleSolved;

    // Use this for initialization
    void Start()
    {
        Cursor.visible = false;
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMessageReceived(PuzzleClient.Message msg)
    {
        Debug.Log("MESSAGE RECEIVED");

        switch (msg.Command)
        {
            /***************************************************************
             Command: 0x60
             Function: Change Display 1 (KING / JUSTICE)
             Payload Type: Byte
             Value of Payload:
             -00: Default
             -01: Tarot Card

             Description: Changes Display 1 to its alternate image.
            **************************************************************/
            case 0x60:
                if(msg.Payload[0] == 00)
                {
                    display1.GetComponent<SpriteRenderer>().sprite = king;
                }
                if(msg.Payload[0] == 01)
                {
                    display1.GetComponent<SpriteRenderer>().sprite = justice;
                }
                break;

            /***************************************************************
            Command: 0x61
            Function: Change Display 2 (KNIGHT / STRENGTH)
            Payload Type: Byte
            Value of Payload:
            -00: Default
            -01: Tarot Card

            Description: Changes Display 2 to its alternate image.
           **************************************************************/
           
            case 0x61:
                if (msg.Payload[0] == 00)
                {
                    display2.GetComponent<SpriteRenderer>().sprite = knight;
                }
                if (msg.Payload[0] == 01)
                {
                    display2.GetComponent<SpriteRenderer>().sprite = strength;
                }
                break;

            /***************************************************************
             Command: 0x62
             Function: Change Display 3 (LADY / JUDGEMENT)
             Payload Type: Byte
             Value of Payload:
             -00: Default
             -01: Tarot Card

             Description: Changes Display 3 to its alternate image.
            **************************************************************/
            
            case 0x62:
                if (msg.Payload[0] == 00)
                {
                    display3.GetComponent<SpriteRenderer>().sprite = lady;
                }
                if (msg.Payload[0] == 01)
                {
                    display3.GetComponent<SpriteRenderer>().sprite = judgement;
                }
                break;

            /***************************************************************
            Command: 0x63
            Function: Change Display 2 (KNIGHT / STRENGTH)
            Payload Type: Byte
            Value of Payload:
            -00: Default
            -01: Tarot Card

            Description: Changes Display 2 to its alternate image.
           **************************************************************/
            case 0x63:
                if (msg.Payload[0] == 00)
                {
                    display4.GetComponent<SpriteRenderer>().sprite = castle;
                }
                if (msg.Payload[0] == 01)
                {
                    display4.GetComponent<SpriteRenderer>().sprite = temperance;
                }
                break;

            /***************************************************************
            Command: 0x64
            Function: Toggle Display 1 Hint
            Payload Type: Byte
            Value of Payload:
            -00: Show
            -01: Hide

            Description: Toggles between showing and hiding the Display 1
                         hint.
           **************************************************************/
            case 0x64:
                if (msg.Payload[0] == 00)
                {
                    hint1.SetActive(true);
                }
                if (msg.Payload[0] == 01)
                {
                    hint1.SetActive(false);
                }
                audio.clip = hintSwoosh;
                if(audio.clip != null)
                {
                    audio.Play();
                }
                else
                {
                    Debug.Log("Hint audio not playing. Check that it is defined.");
                }
                break;

            /***************************************************************
            Command: 0x65
            Function: Toggle Display 2 Hint
            Payload Type: Byte
            Value of Payload:
            -00: Show
            -01: Hide

            Description: Toggles between showing and hiding the Display 2
                         hint.
           **************************************************************/
            case 0x65:
                if (msg.Payload[0] == 00)
                {
                    hint2.SetActive(true);
                }
                if (msg.Payload[0] == 01)
                {
                    hint2.SetActive(false);
                }
                audio.clip = hintSwoosh;
                if(audio.clip != null)
                {
                    audio.Play();
                }
                else
                {
                    Debug.Log("Hint audio not playing. Check that it is defined.");
                }
                break;

            /***************************************************************
            Command: 0x66
            Function: Toggle Display 3 Hint
            Payload Type: Byte
            Value of Payload:
            -00: Show
            -01: Hide

            Description: Toggles between showing and hiding the Display 3
                         hint.
           **************************************************************/
            case 0x66:
                if (msg.Payload[0] == 00)
                {
                    hint3.SetActive(true);
                }
                if (msg.Payload[0] == 01)
                {
                    hint3.SetActive(false);
                }
                audio.clip = hintSwoosh;
                if(audio.clip != null)
                {
                    audio.Play();
                }
                else
                {
                    Debug.Log("Hint audio not playing. Check that it is defined.");
                }
                break;

            /***************************************************************
            Command: 0x67
            Function: Toggle Display 4 Hint
            Payload Type: Byte
            Value of Payload:
            -00: Show
            -01: Hide

            Description: Toggles between showing and hiding the Display 4
                         hint.
           **************************************************************/
            case 0x67:
                if (msg.Payload[0] == 00)
                {
                    hint4.SetActive(true);
                }
                if (msg.Payload[0] == 01)
                {
                    hint4.SetActive(false);
                }
                audio.clip = hintSwoosh;
                if(audio.clip != null)
                {
                    audio.Play();
                }
                else
                {
                    Debug.Log("Hint audio not playing. Check that it is defined.");
                }
                break;

            /***************************************************************
            Command: 0x68
            Function: Toggle victory banners
            Payload Type: Byte
            Value of Payload:
            -00: Show
            -01: Hide

            Description: Changes the displays to the victory banner.
           **************************************************************/
            case 0x68:
                if (msg.Payload[0] == 00)
                {
                    victory1.SetActive(true);
                    victory2.SetActive(true);
                    victory3.SetActive(true);
                    victory4.SetActive(true);
                }
                if (msg.Payload[0] == 01)
                {
                    victory1.SetActive(false);
                    victory2.SetActive(false);
                    victory3.SetActive(false);
                    victory4.SetActive(false);
                }
                break;

            /***************************************************************
            Command: 0x69
            Function: Toggle defeat banners
            Payload Type: Byte
            Value of Payload:
            -00: Show
            -01: Hide

            Description: Changes the displays to the defeat banner.
           **************************************************************/
            case 0x69:
                if (msg.Payload[0] == 00)
                {
                    defeat1.SetActive(true);
                    defeat2.SetActive(true);
                    defeat3.SetActive(true);
                    defeat4.SetActive(true);
                }
                if (msg.Payload[0] == 01)
                {
                    defeat1.SetActive(false);
                    defeat2.SetActive(false);
                    defeat3.SetActive(false);
                    defeat4.SetActive(false);
                }
                break;

            /***************************************************************
            Command: 0x70
            Function: Play "Puzzle Solved!" audio cue
            Payload Type: none

            Description: Lets players know that they've solved a puzzle
                         correctly.
           **************************************************************/
            case 0x70:
                audio.clip = puzzleSolved;
                if(audio.clip != null)
                {
                    audio.Play();
                }
                else
                {
                    Debug.Log("Puzzle Solved audio not playing. Check that it is defined.");
                }
                break;
        }
    }
}
