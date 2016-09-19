using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using UnityEngine.SceneManagement;

public class PuzzleClient : MonoBehaviour
{
    public struct Message
    {
        public byte ModuleID;
        public byte Command;
        public byte[] Payload;

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("ID: {0:X2}, CMD: {1:X2}", ModuleID, Command);
            if (Payload != null && Payload.Length > 0)
            {
                b.Append(", Payload: ");
                for (int i = 0; i < Payload.Length; i++)
                {
                    if (i > 0)
                        b.Append(" ");
                    b.AppendFormat("{0:X2}", Payload[i]);
                }
            }
            return b.ToString();
        }
    }

    public string DeviceName;
    public byte ModuleID;

    protected string _hostname = null;
    protected int _port = 0;
    protected TcpClient _connection;
    protected NetworkStream _stream;
    protected BinaryWriter _outbound;
    protected bool _wasConnected = false;

    public GameObject CallbackTarget;
    public string OnConnectedCallback = "OnConnected";
    public string OnMessageReceivedCallback = "OnMessageReceived";

    protected byte[] _receiveBuffer;
    protected int _receiveIndex;

    protected const int kBufferSize = 60000;

    protected static IAsyncResult _connectionAttempt = null;
    protected bool _connectCalled = false;

    protected bool _connectedEver = false;

    public string Server
    {
        get
        {
            return _hostname + ":" + _port.ToString();
        }

        set
        {
            var parts = value.Split(':');
            _hostname = parts[0];

            int port = 0;
            if (parts.Length > 1)
            {
                if (!int.TryParse(parts[1], out port))
                    port = 0;
            }

            if (port > 0)
                _port = port;
            else
                _port = 9000;
        }
    }

    public bool IsConnected
    {
        get
        {
            if (_connection != null)
                return _connection.Connected;

            return false;
        }
    }

    public void Connect(string server = null)
    {
        _connectCalled = true;

        if (_connectionAttempt != null)
            return;

        Debug.Log(" I AM CONNCETING");
        if (IsConnected)
            Close();

        if (server != null)
            Server = server;

        _connection = new TcpClient();
        _connection.NoDelay = true;
        _connectionAttempt = _connection.BeginConnect(_hostname, _port, new System.AsyncCallback(connectCallback), this);
    }


    public bool LoadConfig(string filename)
    {
        StreamReader infile = new StreamReader(filename);
        string ip  = "";
        string port = "";
        while(!infile.EndOfStream)
        {
            string line = infile.ReadLine();
            int index = line.IndexOf('=');
            string key = line.Substring(0, index);
            string value = line.Substring(index + 1);
            if(key== "ip")
            {
                ip = value;
            }
            else if(key == "port")
            {
                port = value;
            }
            else if(key == "name")
            {
                this.DeviceName = value;
            }
        }
        Server = ip + ":" + port;
        Debug.Log(Server);

        return true;
    }

    protected static void connectCallback(IAsyncResult result)
    {
        _connectionAttempt = null;

        if (result.IsCompleted)
        {
            Debug.Log("CONNECTED");
            PuzzleClient me = (PuzzleClient)result.AsyncState;
            me._stream = me._connection.GetStream();
            me._outbound = new BinaryWriter(me._stream);
        }
        else
        {
            Debug.Log("Connection Failed!");
        }
    }

    protected byte[] composeNetworkHelloMessage()
    {
        string name = DeviceName;
        if (string.IsNullOrEmpty(name))
            name = "UNKNOWN";

        byte[] nameBytes = Encoding.UTF8.GetBytes(name);
        byte[] buffer = new byte[7 + nameBytes.Length];

        // TODO Get MAC address
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        
        foreach (var netInter in networkInterfaces)
        {
            if(netInter.OperationalStatus == OperationalStatus.Up)
            {
                var macAddr = netInter.GetPhysicalAddress().ToString().ToCharArray();
                for (int i = 0; i < 6; i++)
                    buffer[i] = (byte)(macAddr[i]);
                buffer[5] += 1;
                break;
            }
        }
        

        buffer[6] = (byte)'=';

        Buffer.BlockCopy(nameBytes, 0, buffer, 7, nameBytes.Length);

        return buffer;
    }

    protected void onConnect()
    {
        byte hello = 0xF0;
        if (_connectedEver)
            hello = 0xFB;

        SendGCMessage(0xFE, hello, composeNetworkHelloMessage());
        SendGCMessage(0x00, 0x03, new byte[] { 0x00 });
        SendGCMessage(0x00, 0x04, new byte[] { 0x00, ModuleID, 0xFE });
        SendGCMessage(0x00, 0x06, new byte[] { 0x01, 0x03, 0x80, 0x01 });

        if (CallbackTarget != null)
            CallbackTarget.SendMessage(OnConnectedCallback, true);

        _connectedEver = true;
    }

    protected string getDefaultServerIP()
    {
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            // Hack to discover NIC's preferred IP address for outbound connections. IP/port is totally irrelevant
            //  and won't even get called.
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            string me = endPoint.Address.ToString();
            var parts = me.Split('.');
            parts[3] = "2";
            return string.Join(".", parts);
        }
    }

    void OnDestroy()
    {
        Close();
    }

    public void Close()
    {
        Debug.Log("I AM CKLOSE");
        if (_connection != null)
            _connection.Close();
        _connection = null;

        if (_stream != null)
            _stream.Close();
        _stream = null;
    }

    void Start ()
    {
        _receiveBuffer = new byte[kBufferSize];

        if (_hostname == null)
            Server = getDefaultServerIP();
	}

    public bool SendGCMessage(byte moduleID, byte command)
    {
        return SendGCMessage(moduleID, command, (byte[])null);
    }

    protected int encodeNibble(byte[] buffer, int offset, byte nibble)
    {
        if (nibble < 0x0A)
            buffer[offset] = (byte)('0' + nibble);
        else
            buffer[offset] = (byte)('A' + (nibble - 0x0A));

        return offset + 1;
    }

    protected int encodeByte(byte[] buffer, int offset, byte data)
    {
        int result = encodeNibble(buffer, offset, (byte)(data >> 4));
        return encodeNibble(buffer, result, (byte)(data & 0x0F));
    }

    public bool SendGCMessage(Message msg)
    {
        return SendGCMessage(msg.ModuleID, msg.Command, msg.Payload);
    }

    public static string ByteArrayToString(byte[] bytes, int from, int count)
    {
        var sb = new StringBuilder("new byte[] { ");
        for (int i = from; i < count; i++)
        {
            sb.Append(bytes[i] + ", ");
        }
        sb.Append("}");

        return sb.ToString();
    }

    public bool SendGCMessage(byte moduleID, byte command, byte[] payload)
    {
       // Debug.Log((_stream == null).ToString());// + " " + IsConnected.ToString());
        if (_stream == null || !IsConnected)
            return false;
        
        int size = 2;
        int length = 0;
        if (payload != null)
        {
            length = payload.Length;
            size += length;
        }
        byte[] buffer = new byte[2 + (size * 2)];

        int offset = encodeByte(buffer, 0, moduleID);
        offset = encodeByte(buffer, offset, command);

        int index = 0;
        while (index < length)
        {
            offset = encodeByte(buffer, offset, payload[index]);
            ++index;
        }

        buffer[offset] = (byte)'\r';
        ++offset;
        buffer[offset] = (byte)'\n';
        ++offset;

        _outbound.Write(buffer, 0, offset);
        _outbound.Flush();

        return true;
    }

    public bool SendGCMessage(byte moduleID, byte command, string payload)
    {
        if (payload == null)
            return SendGCMessage(moduleID, command, (byte[])null);

        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        return SendGCMessage(moduleID, command, bytes);
    }

    protected void resetBufferTo(int index)
    {
        if (index > 0)
        {
            Buffer.BlockCopy(_receiveBuffer, index, _receiveBuffer, 0, _receiveIndex - index);
            _receiveIndex -= index;
        }
    }

    protected void stripTerminators()
    {
        int index = 0;
        while (_receiveBuffer[index] < ' ' && index < _receiveIndex)
            ++index;
        resetBufferTo(index);
    }

    protected List<byte> decodeBytes(int count)
    {
        List<byte> buffer = new List<byte>();

        bool literal = false;
        bool hiNibble = true;

        int index = 0;
        while (index < count)
        {
            if (literal)
            {
                if (_receiveBuffer[index] == 175)
                {
                    hiNibble = true;
                    literal = false;
                }
                else
                    buffer.Add(_receiveBuffer[index]);
            }
            else
            {
                int nibble = _receiveBuffer[index] - '0';
                if (nibble > 9)
                    nibble = 10 + (_receiveBuffer[index] - 'A');
                if (nibble > 15)
                    nibble = 10 + (_receiveBuffer[index] - 'a');
                if (nibble > 15)
                    nibble = 0;

                if (hiNibble)
                    buffer.Add((byte)(nibble << 4));
                else
                    buffer[buffer.Count - 1] = (byte)(buffer[buffer.Count - 1] + nibble);

                hiNibble = !hiNibble;
            }

            ++index;
        }

        return buffer;
    }

    protected void parseMessage(List<byte> buffer)
    {
        if (buffer.Count < 2)
            return;

        Message msg = new Message();
        msg.ModuleID = buffer[0];
        msg.Command = buffer[1];
        if (buffer.Count < 3)
            msg.Payload = new byte[0];
        else
            msg.Payload = buffer.GetRange(2, buffer.Count - 2).ToArray();

        if (CallbackTarget != null)
            CallbackTarget.SendMessage(OnMessageReceivedCallback, msg);
    }

    protected void seekMessage()
    {
        stripTerminators();

        int index = 0;
        while (_receiveBuffer[index] >= ' ' && index < _receiveIndex)
            ++index;

        if (index > 0 && index < _receiveIndex)
        {
            var buffer = decodeBytes(index);
            resetBufferTo(index);

            parseMessage(buffer);

            seekMessage();
        }
    }

    protected void receive()
    {
        if (_stream == null)
            return;

        int prevIndex = _receiveIndex;

        while (_stream.DataAvailable)
        {
            int read = _stream.Read(_receiveBuffer, _receiveIndex, kBufferSize - _receiveIndex);
            _receiveIndex += read;

            if (read == 0)
                break;
        }

        if (prevIndex != _receiveIndex)
            seekMessage();
    }

    protected bool testConnection()
    {
        if (_connection.Client.Poll(0, SelectMode.SelectRead))
        {
            byte[] buff = new byte[1];
            if (_connection.Client.Receive(buff, SocketFlags.Peek) == 0)
            {
                // Client disconnected
                return false;
            }
        }

        return true;
    }

    void Update()
    {
        if (!_wasConnected)
        {
            // We're not connected, so all we care about is whether we are now connected...
            if (_connection != null && _connection.Connected)
            {
                _wasConnected = true;
                onConnect();
            }

            return;
        }

        bool definitelyConnected = false;

        if (_stream.DataAvailable)
        {
            //Debug.Log((_stream == null).ToString());
            receive();
            definitelyConnected = true;
        }
        else
        {
            //Debug.Log((_stream == null).ToString());
        }

        bool definitelyDisconnected = !_connection.Connected;
        if (!definitelyConnected && !definitelyDisconnected)
            definitelyDisconnected = !testConnection();

        if (definitelyDisconnected)
        {
            Debug.Log("RECONNECT");
            _wasConnected = false;
            if (_connectCalled)
                Connect();
        }
    }
}
