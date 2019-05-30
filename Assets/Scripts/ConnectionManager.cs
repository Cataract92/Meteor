using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Assets.Scripts
{
    public class ConnectionManager : MonoBehaviour
    {
        public string ServerIp;
        public int Port;

        private Vector3 _worldPosition = new Vector3();
        private TcpClient _serverClient;

        struct WelcomeMessage
        {
            public string position;
            public string type;

        }

        public void Connect()
        {
            _serverClient.Connect( ServerIp,Port );
            SendMsg( "{ \"type\" : \"hello\", \"name\" : \"AR\" }" );
            while (true)
            {
                if (_serverClient.GetStream().CanRead)
                {
                    byte[] bytes = new byte[_serverClient.ReceiveBufferSize];
                    _serverClient.GetStream().Read(bytes, 0, _serverClient.ReceiveBufferSize);
                    var msg = Encoding.UTF8.GetString( bytes );
                    WelcomeMessage json = JsonUtility.FromJson<WelcomeMessage>(msg);

                    Debug.Log( "Connected" );

                    var pos = json.position.Split( ',' );
                    _worldPosition.x = float.Parse( pos[0] );
                    _worldPosition.y = float.Parse(pos[1]);
                    _worldPosition.z = float.Parse(pos[2]);
                    break;
                }

                Thread.Sleep(100);
            }


        }

        // Start is called before the first frame update
        void Start()
        {
            _serverClient = new TcpClient();
            Connect();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void SendMsg(string message)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(message + "\r\n");
            _serverClient.GetStream().Write(bytes, 0, bytes.Length);
            _serverClient.GetStream().Flush();
        }
        public void ReadServerMessage()
        {
            while (true)
            {
                if (_serverClient.GetStream().CanRead)
                {
                    byte[] bytes = new byte[_serverClient.ReceiveBufferSize];
                    _serverClient.GetStream().Read(bytes, 0, _serverClient.ReceiveBufferSize);
                    foreach (var s in Encoding.UTF8.GetString(bytes).Replace("\0", String.Empty).Split(new[] { '\r', '\n' }))
                    {
                        
                    }

                }

                Thread.Sleep(100);
            }
        }

    }
}
