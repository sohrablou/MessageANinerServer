using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace SocketHandler
{
    class SocketData
    {
        private Socket _userSocket;
        private string userData = "";

        public int port
        {
            get
            {
                if (_userSocket != null)
                {
                    return ((System.Net.IPEndPoint)_userSocket.LocalEndPoint).Port;
                }
                return 0;
            }
        }

        public long UID { get; set; }

        public System.Net.IPEndPoint endPoint
        {
            get
            {
                if (_userSocket != null)
                {
                    return (System.Net.IPEndPoint)_userSocket.LocalEndPoint;
                }
                return null;
            }
        }

        public SocketData(Socket userSocket)
        {
            _userSocket = userSocket;
        }

        public void send(byte[] sendingBytes)
        {
            lock (_userSocket)
            {
                _userSocket.Send(sendingBytes);
            }
        }

        public void send(string sendingString)
        {
            send(Encoding.Default.GetBytes(sendingString + (Char)3));
        }

        public void addToBuffer(string messageToAdd)
        {
            userData = userData + messageToAdd;
        }

        public string[] getMessages(string splitChar)
        {
            List<string> msgList = new List<string>();
            while (userData.IndexOf(splitChar) >= 0)
            {
                switch (userData.Substring(0,5))
                {
                    case messageSpecs.PictureMessage.PictureMessageType:
                    case messageSpecs.FileMessage.FileMessageType:
                        if (userData.Length >= int.Parse(userData.Substring(messageSpecs.BaseMessage.startingSpot + 35, 10)))
                        {
                            msgList.Add(userData.Substring(0,int.Parse(userData.Substring(messageSpecs.BaseMessage.startingSpot+35,10))-1));
                            userData = userData.Remove(0, int.Parse(userData.Substring(messageSpecs.BaseMessage.startingSpot+35, 10)));
                        }
                        else
                        {
                            return msgList.ToArray();
                        }
                        break;
                    default:
                          msgList.Add(userData.Substring(0, userData.IndexOf(splitChar)));
                          userData = userData.Remove(0, userData.IndexOf(splitChar) + 1);
                        break;
                }
               
            }         
            return msgList.ToArray();
        }

        public void startReceive(SocketAsyncEventArgs e)
        {
            _userSocket.ReceiveAsync(e);
        }

        public void shutdownSocket()
        {
            if (_userSocket.Connected)
            {
                _userSocket.Shutdown(SocketShutdown.Both);
            }
            _userSocket.Close();
        }
    }
}
