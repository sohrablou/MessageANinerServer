using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace SocketHandler
{

    public class SocketClient
    {
        private Socket client;
        private Thread connectionThread;
        static int bufferLength = 100000;
        protected byte[] recvBuffer = new byte[bufferLength];
        private string userBuffer = "";

        public string Ip_Address { get; set; }
        public int PortNumber { get; set; }
        public bool EnableKeeps { get; set; }
        public string Header { get; set; }
        public string Trailer { get; set; }
        public bool Connected { get; set; }
        public Queue<string> messageQueue { get; set; }

        public SocketClient(string serverAddress, int portNumber, bool enableKeeps, string header, string trailer)
        {
            messageQueue = new Queue<string>();
            Ip_Address = serverAddress;
            PortNumber = portNumber;
            EnableKeeps = enableKeeps;
            Header = header;
            Trailer = trailer;
            Connected = false;

            connectionThread = new Thread(new ThreadStart(GetHostConnection));
            connectionThread.IsBackground = true;
            connectionThread.Start();

        }
        
        private void GetHostConnection()
        {
            try
            {
                do
                {
                    if (!Connected)
                    {
                        if (client == null)
                        {
                            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            client.SendBufferSize = 1000000;
                        }

                        IAsyncResult returnValue;
                        returnValue = client.BeginConnect(Ip_Address, PortNumber, SocketConnected, null);
                        returnValue.AsyncWaitHandle.WaitOne();
                    }

                    Thread.Sleep(1000);
                } while (true);

            }
            catch (SocketException exSocket)
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void SocketConnected(IAsyncResult ar)
        {
            try
            {
                try
                {
                    client.EndConnect(ar);
                }
                catch (SocketException exSocket)
                {

                }
                catch (Exception ex)
                {

                }

                if (!client.Connected)
                {
                    Connected = false;
                }
                else
                {
                    client.NoDelay = true;
                    Connected = true;
                    client.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, ReceivedData, null);
                    //EnableKeepTimer(True) 
                }
            }
            catch (SocketException exSocket)
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void ReceivedData(IAsyncResult ar)
        {
            int numBytes = 0;

            try
            {
                if (client != null)
                {
                    lock (client)
                    {
                        numBytes = client.EndReceive(ar);
                    }
                }
            }
            catch (SocketException exSocket)
            {

            }
            catch (Exception ex)
            {

            }

            try
            {
                if (numBytes == 0)
                {
                    CloseSocket();                    //-- Connection has closed!
                    return;
                }

                //reset timer on new data
                //EnableKeepTimer(False)
                // EnableKeepTimer(True)

                //-- We have data!
                BuildMessage(recvBuffer, 0, numBytes);

                //-- Start Receiving Data Again!
                client.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, ReceivedData, null);

            }
            catch (Exception ex)
            {

            }
        }

        private void BuildMessage(byte[] Bytes, int offset, int count)
        {
            userBuffer += Encoding.Default.GetString(Bytes, offset, count);

             List<string> msgList = new List<string>();
             bool breakout = false;
             while (userBuffer.IndexOf(Trailer) >= 0 && !breakout)
             {
                switch (userBuffer.Substring(0, 5))
                {
                    case messageSpecs.PictureMessage.PictureMessageType:
                    case messageSpecs.FileMessage.FileMessageType:
                        if (userBuffer.Length >= int.Parse(userBuffer.Substring(messageSpecs.BaseMessage.startingSpot +35, 10)))
                        {
                            msgList.Add(userBuffer.Substring(0, int.Parse(userBuffer.Substring(messageSpecs.BaseMessage.startingSpot+35, 10))-1));
                            userBuffer = userBuffer.Remove(0, int.Parse(userBuffer.Substring(messageSpecs.BaseMessage.startingSpot+35, 10)));
                        }
                        else
                        {
                            breakout = true;
                        }
                        break;
                    default:
                        msgList.Add(userBuffer.Substring(0, userBuffer.IndexOf(Trailer)));
                        userBuffer = userBuffer.Remove(0, userBuffer.IndexOf(Trailer) + 1);
                        break;
                }
               
             }         

            foreach(string message in msgList)
            {
                lock (messageQueue)
                {
                    messageQueue.Enqueue(message);
                }
            }
        }

        public void CloseSocket(bool KillSocket = false)
        {
            try
            {
                if (KillSocket == false)
                {
                    if (client != null)
                        lock (client)
                        {
                            if (client.Connected)
                            {
                                client.Shutdown(SocketShutdown.Both);
                            }
                            client.Close(5);

                            //2015-03-24 added code to clear message queue and sleep thread to give socket enough time to close before reopening
                            clearQueueBuffer();
                            System.Threading.Thread.Sleep(2000);
                            client = null;
                        }
                }
                else
                {
                    //close socket and abort thread
                    if (client != null)
                    {
                        lock (client)
                        {
                            if (client.Connected)
                            {
                                client.Shutdown(SocketShutdown.Both);
                            }
                            client.Close(5);
                            clearQueueBuffer();
                            System.Threading.Thread.Sleep(2000);
                            client = null;
                        }
                    }
                    connectionThread.Abort();
                    connectionThread = null;
                }
                //EnableKeepTimer(False)
            }
            catch (SocketException exSocket)
            {

            }
            catch (Exception ex)
            {

            }
            finally
            {
                Connected = false;
            }
        }

        public void clearQueueBuffer()
        {
            lock (messageQueue)
            {
                messageQueue.Clear();
            }
        }

        public int Send(String outputMessage)
        {
            int ret = 0;
            try
            {
                if (client != null)
                {
                    byte[] bytes = Encoding.Default.GetBytes(outputMessage + Trailer);
                    lock (client)
                    {
                        ret = client.Send(bytes);
                    }
                }
            }
            catch
            {

            }
            return ret;
        }
    }

}
