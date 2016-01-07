using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketHandler
{

    public class SocketServer
    {

        //public readonly System.Net.Sockets.Socket embeddedSocket { get; }
        public string ipAddress;
        public int portNumber;
        private int m_numConnections;   // the maximum number of connections the sample is designed to handle simultaneously 
        private int m_receiveBufferSize = int.MaxValue;// buffer size to use for each socket I/O operation 
        const int opsToPreAlloc = 2;    // read, write (don't alloc buffer space for accepts)
        Socket listenSocket;            // the socket used to listen for incoming connection requests
        // pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations
        SocketAsyncEventArgsPool m_readWritePool;
        SortedList<long, SocketAsyncEventArgs> clientList = new SortedList<long, SocketAsyncEventArgs>();
        SortedList<long, System.Collections.ArrayList> groupLookup = new SortedList<long, System.Collections.ArrayList>();
        public string connectionString = "Data Source=.;Initial Catalog=MessageANiner;User ID=sa;Password=SSDIproject1;Connect Timeout=15";
        MessageHandler myMessageHandler;
        static int bufferLength = 100000;


        public SocketServer(int numConnections, int receiveBufferSize)
        {
            myMessageHandler = new MessageHandler(connectionString);
            m_numConnections = numConnections;
            m_receiveBufferSize = receiveBufferSize;
            // allocate buffers such that the maximum number of sockets can have one outstanding read and 
            //write posted to the socket simultaneously  
            // m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToPreAlloc,
            //  receiveBufferSize);

            m_readWritePool = new SocketAsyncEventArgsPool(numConnections);
            //   m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);

            SqlDataReader dr = null;
            try
            {
                SQLManager.SQLManager mySql = new SQLManager.SQLManager(connectionString);
                dr = mySql.getDataReader("EXEC getGroups");
                while (dr.Read())
                {
                    System.Collections.ArrayList userList = new System.Collections.ArrayList();
                    string users = (string)dr["Users"];
                    string[] userArr = users.Split((",".ToCharArray()));
                    for (int i = 0; i<userArr.Length-1; i++)
                    {
                        userList.Add(long.Parse(userArr[i]));
                    }
                    groupLookup.Add((long)dr["GPID"], userList);
                }
            }
            catch
            {

            }
            finally
            {
                if (dr != null)
                    dr.Close();
            }
        }

        // Initializes the server by preallocating reusable buffers and 
        // context objects.  These objects do not need to be preallocated 
        // or reused, but it is done this way to illustrate how the API can 
        // easily be used to create reusable objects to increase server performance.
        //
        public void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds 
            // against memory fragmentation
            // m_bufferManager.InitBuffer();

            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs readWriteEventArg;

            for (int i = 0; i < m_numConnections; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.DisconnectReuseSocket = true;


                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                readWriteEventArg.SetBuffer(new byte[bufferLength], 0, bufferLength);
                // add SocketAsyncEventArg to the pool
                m_readWritePool.Push(readWriteEventArg);
            }

        }

        // Starts the server such that it is listening for 
        // incoming connection requests.    
        //
        // <param name="localEndPoint">The endpoint which the server will listening 
        // for connection requests on</param>
        public void Start(IPEndPoint localEndPoint)
        {
            // create the socket which listens for incoming connections
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));
            listenSocket.NoDelay = true;
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listenSocket.Bind(localEndPoint);
            // start the server with a listen backlog of 100 connections
            listenSocket.Listen(1);

            // post accepts on the listening socket
            StartAccept(null);
        }


        // Begins an operation to accept a connection request from the client 
        //
        // <param name="acceptEventArg">The context object to use when issuing 
        // the accept operation on the server's listening socket</param>
        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
                acceptEventArg.DisconnectReuseSocket = true;
            }
            else
            {
                // socket must be cleared since the context object is being reused
                acceptEventArg.AcceptSocket = null;
            }

            // m_maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        // This method is the callback method associated with Socket.AcceptAsync 
        // operations and is invoked when an accept operation is complete
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            // Interlocked.Increment(ref m_numConnectedSockets);
            // Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
            //m_numConnectedSockets);

            // Get the socket for the accepted client connection and put it into the 
            //ReadEventArg object user token
            SocketAsyncEventArgs readEventArgs = m_readWritePool.Pop();
            if (readEventArgs == null)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                readEventArgs = new SocketAsyncEventArgs();
                e.AcceptSocket.ReceiveBufferSize = bufferLength;
                readEventArgs.DisconnectReuseSocket = true;
                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                //m_bufferManager.SetBuffer();
                readEventArgs.SetBuffer(new byte[bufferLength], 0, bufferLength);
            }
            readEventArgs.UserToken = new SocketData(e.AcceptSocket);
            readEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            // ((AsyncUserToken)readEventArgs.UserToken).Socket = e.AcceptSocket;

            // As soon as the client is connected, post a receive to the connection
            bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }

            // Accept the next connection request
            StartAccept(e);
        }

        // This method is called whenever a receive or send operation is completed on a socket 
        //
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        public void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }


        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                // Console.WriteLine("The server has read a total of {0} bytes", m_totalBytesRead);

                //echo the data received back to the client

                try
                {
                    string receivedString = Encoding.Default.GetString(e.Buffer);
                    ((SocketData)e.UserToken).addToBuffer(receivedString.Substring(0, e.BytesTransferred));
                    using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                    {

                        string[] arr = ((SocketData)e.UserToken).getMessages(((char)3).ToString());


                        for (int i = 0; i < arr.Length; i++)
                        {
                            string item = arr[i];

                            try
                            {
                                sqlConnection.Open();
                                using (SqlCommand command = new SqlCommand("InsertMessageLog", sqlConnection))
                                {
                                    command.Parameters.Add(new SqlParameter("@Message", Encoding.Default.GetBytes(item)));
                                    command.CommandType = System.Data.CommandType.StoredProcedure;
                                    command.ExecuteNonQuery();
                                    var y = command.CommandText.Length;
                                }
                            }
                            catch
                            {

                            }
                            finally
                            {
                                sqlConnection.Close();
                                handleMessage(item, e);
                            }

                        }

                    }
                }
                catch (Exception ex)
                {
                    int x = 2;
                }

                e.SetBuffer(e.Buffer, e.Offset, e.Buffer.Length);
                ((SocketData)e.UserToken).startReceive(e);
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                // read the next block of data send from the client
                e.SetBuffer(0, e.Buffer.Length);
                bool willRaiseEvent = ((Socket)e.UserToken).ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            //AsyncUserToken token = e.UserToken as AsyncUserToken;

            // close the socket associated with the client
            try
            {
                e.Completed -= IO_Completed;

                if (((SocketData)e.UserToken).UID != 0)
                {
                    myMessageHandler.sendFriendLogonMessages(((SocketData)e.UserToken).UID, clientList, messageSpecs.FriendStatusMessage.LoggedOnStatus.Offline);
                    if (clientList.ContainsKey(((SocketData)e.UserToken).UID))
                    {
                        clientList.Remove(((SocketData)e.UserToken).UID);
                    }

                }

                ((SocketData)e.UserToken).shutdownSocket();
                e.UserToken = null;

                m_readWritePool.Push(e);
            }
            // throws if client process has already closed
            catch (Exception) { }


            // decrement the counter keeping track of the total number of clients connected to the server
            // m_maxNumberAcceptedClients.Release();
            //Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", m_numConnectedSockets);

            // Free the SocketAsyncEventArg so they can be reused by another client
        }

        public void handleMessage(string message, SocketAsyncEventArgs e)
        {
            string returnMsg = "";
            string type = message.Substring(0, 5);

            switch (type)
            {
                case messageSpecs.UserCreationMessage.CreateUserMessageType:
                    returnMsg = myMessageHandler.handleUserCreation(message);
                    break;
                case messageSpecs.LogonMessage.LoginMessageType:
                    messageSpecs.LogonMessage myLoginMessage = myMessageHandler.handleLogin(message);
                    if (myLoginMessage.Verified)
                    {
                        if (!clientList.ContainsKey(long.Parse(myLoginMessage.UserID)))
                        {
                            clientList.Add(long.Parse(myLoginMessage.UserID), e);
                            ((SocketHandler.SocketData)e.UserToken).UID = long.Parse(myLoginMessage.UserID);
                            myMessageHandler.sendFriendLogonMessages(int.Parse(myLoginMessage.UserID), clientList, messageSpecs.FriendStatusMessage.LoggedOnStatus.Online);
                        }
                        else
                        {
                            myLoginMessage.Verified = false;
                            myLoginMessage.ReturnMessage = "Sorry, your account is already logged in somewhere else.";
                        }
                    } 
                    returnMsg = myLoginMessage.getMessageString();
                    break;
                case messageSpecs.ContactLookupMessage.ContactLookupMessageType:
                    returnMsg = myMessageHandler.handleContactLookup(message);
                    break;
                case messageSpecs.AddContactMessage.AddContactMessageType:
                    returnMsg = myMessageHandler.handleContactAdding(message, clientList);
                    break;
                case messageSpecs.ChangeUserSettingsMessage.ChangeUserSettingsMessageType:
                    returnMsg = myMessageHandler.handleUserUpdate(message);
                    break;
                case messageSpecs.ContactResponseMessage.ContactResponseMessageType:
                    myMessageHandler.handleContactResponseMessage(message,clientList);
                    break;
                case messageSpecs.CreateGroupMessage.CreateGroupMessageType:
                    myMessageHandler.handleGroupCreation(message, clientList,groupLookup);
                    break;
                case messageSpecs.PictureMessage.PictureMessageType:
                case messageSpecs.TextMessage.TextMessageType:
                case messageSpecs.UserTyping.UserTypingMessageType:
                case messageSpecs.FileMessage.FileMessageType:
                    returnMsg = message;

                    messageSpecs.BaseMessage myMessage = new messageSpecs.BaseMessage(message);

                    if (groupLookup.ContainsKey(long.Parse(myMessage.receiver)))
                    {
                        foreach (long receiver in groupLookup[long.Parse(myMessage.receiver)])
                        {
                            if (clientList.ContainsKey(receiver))
                            {
                                ((SocketHandler.SocketData)clientList[receiver].UserToken).send(message);
                            }
                        }
                    }
                    else if (clientList.ContainsKey(long.Parse(myMessage.receiver)))
                    {
                        ((SocketHandler.SocketData)clientList[long.Parse(myMessage.receiver)].UserToken).send(message);
                    }

                    break;
            }

            if (returnMsg != "")
            {
                ((SocketData)e.UserToken).send(returnMsg);
            }
        }
    }
}

class MessageHandler
{
    public String connectionString { get; set; }
    private SQLManager.SQLManager mySql;

    public MessageHandler(string connectionstring)
    {
        connectionString = connectionstring;
        mySql = new SQLManager.SQLManager(connectionstring);
    }

    #region "Login Handlers"
    public messageSpecs.LogonMessage handleLogin(string message)
    {
        SqlDataReader dr = null;
        messageSpecs.LogonMessage loginMsg = new messageSpecs.LogonMessage(message);
        loginMsg.ReturnMessage = "Failure logging in, Please try again later";
        try
        {

            string pass = encrypt(loginMsg.PassWord);
            dr = mySql.getDataReader(string.Format("EXEC logInUser @UserName = '{0}',@Password = '{1}'",
                loginMsg.UserName,
                pass
            ));
            while (dr.Read())
            {
                loginMsg.Verified = (bool)dr["Verified"];
                loginMsg.FirstName = (string)dr["FirstName"];
                loginMsg.LastName = (string)dr["LastName"];
                loginMsg.UserID = (string)dr["UserID"];
                loginMsg.ReturnMessage = (string)dr["ReturnMessage"];
                loginMsg.PasswordReset = (bool)dr["ResetFlag"];
            }
        }
        catch
        {

        }
        finally
        {
            if (dr != null)
            {
                dr.Close();
            }

        }
        return loginMsg;
    }

    public void sendFriendLogonMessages(long loggingInUID, SortedList<long, SocketAsyncEventArgs> clientList, messageSpecs.FriendStatusMessage.LoggedOnStatus status)
    {
        SqlDataReader dr = null;
        try
        {
            dr = mySql.getDataReader(string.Format("EXEC getFriends @UID = '{0}'", loggingInUID));

            while (dr.Read())
            {
                if ((int)dr["Response"] == 0)
                {
                    messageSpecs.FriendStatusMessage friendMessage = new messageSpecs.FriendStatusMessage();
                    friendMessage.UserStatus = messageSpecs.FriendStatusMessage.LoggedOnStatus.Friend;
                    friendMessage.sender = ((long)dr["Friend"]).ToString();
                    friendMessage.receiver = loggingInUID.ToString();
                    sendFriendStatMessage(friendMessage, clientList[loggingInUID]);
                }
                else if ((int)dr["Response"] == 2)
                {
                    messageSpecs.FriendStatusMessage friendMessage = new messageSpecs.FriendStatusMessage();
                    friendMessage.UserStatus = messageSpecs.FriendStatusMessage.LoggedOnStatus.Group;
                    friendMessage.sender = ((long)dr["Friend"]).ToString();
                    friendMessage.receiver = loggingInUID.ToString();
                    sendFriendStatMessage(friendMessage, clientList[loggingInUID]);
                }
                else if (clientList.ContainsKey((long)dr["Friend"]))
                {
                    messageSpecs.FriendStatusMessage friendMessage = new messageSpecs.FriendStatusMessage();
                    friendMessage.UserStatus = status;
                    friendMessage.sender = loggingInUID.ToString();
                    friendMessage.receiver = ((long)dr["Friend"]).ToString();
                    sendFriendStatMessage(friendMessage, clientList[(long)dr["Friend"]]);

                    friendMessage.sender = ((long)dr["Friend"]).ToString();
                    friendMessage.receiver = loggingInUID.ToString();
                    sendFriendStatMessage(friendMessage, clientList[loggingInUID]);
                }
                else
                {
                    messageSpecs.FriendStatusMessage friendMessage = new messageSpecs.FriendStatusMessage();
                    friendMessage.UserStatus = messageSpecs.FriendStatusMessage.LoggedOnStatus.Offline;
                    friendMessage.sender = ((long)dr["Friend"]).ToString();
                    friendMessage.receiver = loggingInUID.ToString();
                    sendFriendStatMessage(friendMessage, clientList[loggingInUID]);
                }

            }
        }
        catch
        {

        }
        finally
        {
            if (dr != null)
                dr.Close();
        }
    }
    #endregion

    public string handleUserCreation(string message)
    {
        SqlDataReader dr = null;
        messageSpecs.UserCreationMessage userCreationMsg = new messageSpecs.UserCreationMessage(message);
        userCreationMsg.ReturnMessage = "Failure creating account, Please try again later";
        try
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string pass = new string(Enumerable.Repeat(chars, 10)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            userCreationMsg.PassWord = pass;

            string encryptpass = encrypt(userCreationMsg.PassWord);

            dr = mySql.getDataReader(string.Format("EXEC createUser @EmailAddress = '{0}' ,@UserName = '{1}',@FirstName = '{2}',@LastName = '{3}',@PassWord = '{4}',@EncryptPassword = '{5}'",
                userCreationMsg.EmailAddress,
                userCreationMsg.UserName,
                userCreationMsg.FirstName,
                userCreationMsg.LastName,
                userCreationMsg.PassWord,
                encryptpass));

            while (dr.Read())
            {
                userCreationMsg.Verified = (bool)dr[0];
                userCreationMsg.ReturnMessage = (string)dr[1];
            }
        }
        catch
        {

        }
        finally
        {
            if (dr != null)
            {
                dr.Close();
            }

        }
        return userCreationMsg.getMessageString();
    }

    public string handleContactLookup(string message)
    {
        string returnString = "";
        SqlDataReader dr = null;
        try
        {
            messageSpecs.ContactLookupMessage contactMessage = new messageSpecs.ContactLookupMessage(message);
            dr = mySql.getDataReader(string.Format("EXEC contactLookup '{0}'", contactMessage.LookupName.Trim()));
            while (dr.Read())
            {
                contactMessage.ReturnTable.Rows.Add((long)dr["UserID"], dr["FirstName"], dr["LastName"], dr["UserName"], dr["EmailAddress"]);
            }

            returnString = contactMessage.getMessageString();
        }
        catch
        {

        }
        finally
        {
            if (dr != null)
                dr.Close();
        }

        return returnString;
    }

    public string handleContactAdding(string message, SortedList<long, SocketAsyncEventArgs> clientList)
    {
        string returnString = "";
        SqlDataReader dr = null;
        try
        {
            messageSpecs.AddContactMessage contactMessage = new messageSpecs.AddContactMessage(message);

            if (contactMessage.sender == contactMessage.receiver)
            {
                contactMessage.ReturnMessage = "You cannot add yourself as a friend!";
                return contactMessage.getMessageString();
            }


            contactMessage.ReturnMessage = "Failed trying to add user";
            dr = mySql.getDataReader(string.Format("EXEC addContact '{0}','{1}'", contactMessage.sender, contactMessage.receiver));
            while (dr.Read())
            {
                if ((bool)dr["Completed"])
                {
                    contactMessage.ReturnMessage = "Friend request was sent";
                    if (clientList.ContainsKey(long.Parse(contactMessage.receiver)))
                    {
                        messageSpecs.FriendStatusMessage fStatus = new messageSpecs.FriendStatusMessage();
                        fStatus.receiver = contactMessage.receiver;
                        fStatus.sender = contactMessage.sender;
                        fStatus.UserStatus = messageSpecs.FriendStatusMessage.LoggedOnStatus.Friend;
                        sendFriendStatMessage(fStatus, clientList[long.Parse(contactMessage.receiver)]);
                    }
                }
                else
                {
                    contactMessage.ReturnMessage = "User already is a friend";
                }

            }
            returnString = contactMessage.getMessageString();
        }
        catch
        {

        }
        finally
        {
            if (dr != null)
                dr.Close();
        }

        return returnString;
    }

    public string handleUserUpdate(string message)
    {
        string returnMessage = "";

        messageSpecs.ChangeUserSettingsMessage userMessage = new messageSpecs.ChangeUserSettingsMessage(message);
        SqlDataReader dr = null;
        try
        {
            dr = mySql.getDataReader(string.Format("EXEC updateUser '{0}','{1}','{2}','{3}','{4}'",
                userMessage.EmailAddress,
                userMessage.FirstName,
                userMessage.LastName,
                encrypt(userMessage.PassWord),
                userMessage.UserID
                ));

            userMessage.Verified = true;
        }
        catch
        {

        }
        finally
        {
            if (dr != null)
                dr.Close();
        }
        returnMessage = userMessage.getMessageString();
        return returnMessage;
    }

    public void sendFriendStatMessage(messageSpecs.FriendStatusMessage message, SocketAsyncEventArgs clientToSend)
    {
        SqlDataReader dr = null;
        try
        {
            dr = mySql.getDataReader(string.Format("SELECT firstName,LastName FROM userTable where UserID = '{0}'", message.sender));
            while (dr.Read())
            {
                message.FirstName = (string)dr["firstName"];
                message.LastName = (string)dr["lastName"];
            }
            ((SocketHandler.SocketData)clientToSend.UserToken).send(message.getMessageString());
        }
        catch
        {

        }
        finally
        {
            if (dr != null)
                dr.Close();
        }
    }

    public void handleGroupCreation(string message,SortedList<long, SocketAsyncEventArgs> clientList,SortedList<long, System.Collections.ArrayList> groupLookup)
    {
        messageSpecs.CreateGroupMessage grpMsg = new messageSpecs.CreateGroupMessage(message);
        SqlDataReader dr = null;
        try
        {
            long grpID = 0;
            dr = mySql.getDataReader(string.Format("EXEC createGroup '{0}'",grpMsg.GroupName));
            
            while (dr.Read())
            {
                grpID = (long)dr[0];
            }

            if (grpID == 0)
                return;
            System.Collections.ArrayList userList = new System.Collections.ArrayList();

            messageSpecs.FriendStatusMessage fMessage = new messageSpecs.FriendStatusMessage();
            fMessage.sender = grpID.ToString();
            fMessage.UserStatus = messageSpecs.FriendStatusMessage.LoggedOnStatus.Group;
            foreach (string UID in grpMsg.memberList)
            {
                userList.Add(long.Parse(UID));
                mySql.doExecuteNonQuery(string.Format("INSERT INTO GroupTable VALUES('{0}','{1}')", grpID, UID));

                if (clientList.ContainsKey(long.Parse(UID)))
                {
                    fMessage.receiver = UID;
                    sendFriendStatMessage(fMessage, clientList[long.Parse(UID)]);
                }
            }
            groupLookup.Add(grpID, userList);
        }
        catch
        {

        }
    }

    public void handleContactResponseMessage(string message, SortedList<long, SocketAsyncEventArgs> clientList)
    {
        messageSpecs.ContactResponseMessage responseMessage = new messageSpecs.ContactResponseMessage(message);
        mySql.doExecuteNonQuery(string.Format("EXEC UpdateContactResponse @Requested = '{0}', @Requestee = '{1}', @Response = '{2}'",responseMessage.sender,responseMessage.receiver,((int)responseMessage.Reply).ToString()));
        if (responseMessage.Reply == messageSpecs.ContactResponseMessage.ContactResponse.Accept)
        {
            try
            {
                messageSpecs.FriendStatusMessage friendsMessage = new messageSpecs.FriendStatusMessage();
                if (clientList.ContainsKey(long.Parse(responseMessage.receiver)))
                {
                    friendsMessage.sender = responseMessage.sender;
                    friendsMessage.receiver = responseMessage.receiver;
                    friendsMessage.UserStatus = messageSpecs.FriendStatusMessage.LoggedOnStatus.Online;
                    sendFriendStatMessage(friendsMessage, clientList[long.Parse(responseMessage.receiver)]);

                    friendsMessage.sender = responseMessage.receiver;
                    friendsMessage.receiver = responseMessage.sender;
                    sendFriendStatMessage(friendsMessage, clientList[long.Parse(responseMessage.sender)]);
                }
                else
                {
                    friendsMessage.sender = responseMessage.receiver;
                    friendsMessage.receiver = responseMessage.sender;
                    friendsMessage.UserStatus = messageSpecs.FriendStatusMessage.LoggedOnStatus.Offline;
                    sendFriendStatMessage(friendsMessage, clientList[long.Parse(responseMessage.sender)]);
                }
            }
            catch { }
            
        }
    }

    #region "EncryptDecrypt"



    private static string encrypt(string plainText)
    {
        string PasswordHash = "m^P@5SW0rD";
        string SaltKey = "5A!1tK3y";
        string VIKey = "@1B2c3D4e5F6g7H8";
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] keyBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
        var symmetricKey = new System.Security.Cryptography.RijndaelManaged() { Mode = System.Security.Cryptography.CipherMode.CBC, Padding = System.Security.Cryptography.PaddingMode.Zeros };
        var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

        byte[] cipherTextBytes;

        using (var memoryStream = new System.IO.MemoryStream())
        {
            using (var cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
            {
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                cipherTextBytes = memoryStream.ToArray();
                cryptoStream.Close();
            }
            memoryStream.Close();
        }
        return Convert.ToBase64String(cipherTextBytes);
    }

    private static string decrypt(string encryptedText)
    {
        string PasswordHash = "m^P@5SW0rD";
        string SaltKey = "5A!1tK3y";
        string VIKey = "@1B2c3D4e5F6g7H8";
        byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
        byte[] keyBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
        var symmetricKey = new System.Security.Cryptography.RijndaelManaged() { Mode = System.Security.Cryptography.CipherMode.CBC, Padding = System.Security.Cryptography.PaddingMode.None };

        var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
        var memoryStream = new System.IO.MemoryStream(cipherTextBytes);
        var cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, decryptor, System.Security.Cryptography.CryptoStreamMode.Read);
        byte[] plainTextBytes = new byte[cipherTextBytes.Length];

        int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
        memoryStream.Close();
        cryptoStream.Close();
        return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
    }


    #endregion
}

// Represents a collection of reusable SocketAsyncEventArgs objects.  
class SocketAsyncEventArgsPool
{
    Stack<SocketAsyncEventArgs> m_pool;

    // Initializes the object pool to the specified size
    //
    // The "capacity" parameter is the maximum number of 
    // SocketAsyncEventArgs objects the pool can hold
    public SocketAsyncEventArgsPool(int capacity)
    {
        m_pool = new Stack<SocketAsyncEventArgs>(capacity);
    }

    // Add a SocketAsyncEventArg instance to the pool
    //
    //The "item" parameter is the SocketAsyncEventArgs instance 
    // to add to the pool
    public void Push(SocketAsyncEventArgs item)
    {
        if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
        lock (m_pool)
        {
            m_pool.Push(item);
        }
    }

    // Removes a SocketAsyncEventArgs instance from the pool
    // and returns the object removed from the pool
    public SocketAsyncEventArgs Pop()
    {
        lock (m_pool)
        {
            if (m_pool.Count == 0)
                return null;

            return m_pool.Pop();
        }
    }

    // The number of SocketAsyncEventArgs instances in the pool
    public int Count
    {
        get { return m_pool.Count; }
    }

}

