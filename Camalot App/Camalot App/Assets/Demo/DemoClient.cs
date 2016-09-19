using UnityEngine;
using System.Collections;

public class DemoClient : MonoBehaviour
{
    protected PuzzleClient _client;
    protected Material _mat;

	void Start ()
    {

        _client = GetComponent<PuzzleClient>();
        //_client.LoadConfig("Assets/config");
        _client.Server = "10.3.1.2:9010";
        _client.DeviceName = "Ambience";
        
        if (_client == null)
            Debug.Log("No PuzzleClient found!");
        else
        {
            _client.Connect(_client.Server);
            Debug.Log("Connecting to " + _client.Server);
        }
        _mat = GetComponent<Renderer>().material;
        _mat.color = Color.red;
	}

    protected bool _toggle = false;
    void Update()
    {
        if (_client != null && !_client.IsConnected)
        {
            _toggle = !_toggle;
            _mat.color = _toggle ? Color.blue : Color.red;
        }

    }

    void OnConnected()
    {
        _mat.color = Color.green;
    }

    void OnMessageReceived(PuzzleClient.Message msg)
    {
        Debug.Log(msg.ToString());
    }
}
