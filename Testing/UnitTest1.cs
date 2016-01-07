using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketHandler;
using messageSpecs;

namespace Testing
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void UserCreationTest()
        {
            UserCreationMessage x = new UserCreationMessage();
            UserCreationMessage y = new UserCreationMessage(x.getMessageString());
            Assert.AreEqual(x.FirstName,y.FirstName);
            Assert.AreEqual(x.PassWord, y.PassWord);
            Assert.AreEqual(x.LastName, y.LastName);
            Assert.AreEqual(x.UserName, y.UserName);
            Assert.AreEqual(x.ReturnMessage, y.ReturnMessage);
        }

        [TestMethod]
        public void LogonMessageTest()
        {
            LogonMessage x = new LogonMessage();
            LogonMessage y = new LogonMessage(x.getMessageString());
            Assert.AreEqual(x.FirstName, y.FirstName);
            Assert.AreEqual(x.PassWord, y.PassWord);
            Assert.AreEqual(x.LastName, y.LastName);
            Assert.AreEqual(x.UserName, y.UserName);
            Assert.AreEqual(x.ReturnMessage, y.ReturnMessage);
        }

        [TestMethod]
        public void ChangePasswordTest()
        {
            ChangeUserSettingsMessage x = new ChangeUserSettingsMessage();
            ChangeUserSettingsMessage y = new ChangeUserSettingsMessage(x.getMessageString());
            Assert.AreEqual(x.FirstName, y.FirstName);
            Assert.AreEqual(x.PassWord, y.PassWord);
            Assert.AreEqual(x.LastName, y.LastName);
            Assert.AreEqual(x.UserID, y.UserID);
            Assert.AreEqual(x.ReturnMessage, y.ReturnMessage);
        }

        [TestMethod]
        public void addContactTest()
        {
            AddContactMessage x = new AddContactMessage();
            AddContactMessage y = new AddContactMessage(x.getMessageString());
            Assert.AreEqual(x.sender, y.sender);
            Assert.AreEqual(x.receiver, y.receiver);
            Assert.AreEqual(x.ReturnMessage, y.ReturnMessage);
        }

        [TestMethod]
        public void contactLookupTest()
        {
            ContactLookupMessage x = new ContactLookupMessage();
            ContactLookupMessage y = new ContactLookupMessage(x.getMessageString());
            Assert.AreEqual(x.LookupName, y.LookupName);
            Assert.AreEqual(x.sender, y.sender);
            Assert.AreEqual(x.receiver, y.receiver);
        }

        [TestMethod]
        public void FriendStatusTest()
        {
            FriendStatusMessage x = new FriendStatusMessage();
            FriendStatusMessage y = new FriendStatusMessage(x.getMessageString());
            Assert.AreEqual(x.UserStatus, y.UserStatus);
            Assert.AreEqual(x.sender, y.sender);
            Assert.AreEqual(x.receiver, y.receiver);
        }

        [TestMethod]
        public void CreateSocket()
        {
             SocketHandler.SocketServer mySockets = new SocketHandler.SocketServer(0, 1000);
             SocketHandler.SocketClient clientSocket = null;
             mySockets.Init();
             mySockets.Start(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 20000));
             clientSocket = new SocketHandler.SocketClient("127.0.0.1", 20000, false, ((char)2).ToString(), ((char)3).ToString());

             System.Threading.Thread.Sleep(1000);
             Assert.AreEqual(true, clientSocket.Connected);
        }

        [TestMethod]
        public void CreateUser()
        {
            SocketHandler.SocketServer mySockets = new SocketHandler.SocketServer(0, 1000);
            SocketHandler.SocketClient clientSocket = null;
            mySockets.Init();
            mySockets.Start(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 20000));
            clientSocket = new SocketHandler.SocketClient("127.0.0.1", 20000, false, ((char)2).ToString(), ((char)3).ToString());

            messageSpecs.UserCreationMessage myCreationMessage = new messageSpecs.UserCreationMessage();
            myCreationMessage.EmailAddress = "ceidsnes@uncc.edu";
            myCreationMessage.UserName = "coeids";
            myCreationMessage.FirstName = "Cody";
            myCreationMessage.LastName = "Eidsness";
            clientSocket.Send(myCreationMessage.getMessageString());

            for(int i = 1;i<=5;i++)
            {
                System.Threading.Thread.Sleep(1000);
                while (clientSocket.messageQueue.Count > 0)
                {
                    if (clientSocket.messageQueue.Dequeue().Substring(0, 5) == UserCreationMessage.CreateUserMessageType)
                    {
                        return;
                    }
                }
            }
            Assert.Fail("Never got login message back");
        }

        [TestMethod]
        public void LookupUser()
        {
            SocketHandler.SocketServer mySockets = new SocketHandler.SocketServer(0, 1000);
            SocketHandler.SocketClient clientSocket = null;
            mySockets.Init();
            mySockets.Start(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 20000));
            clientSocket = new SocketHandler.SocketClient("127.0.0.1", 20000, false, ((char)2).ToString(), ((char)3).ToString());

            messageSpecs.ContactLookupMessage x = new messageSpecs.ContactLookupMessage();
            x.LookupName = "coei";
            clientSocket.Send(x.getMessageString());

            for (int i = 1; i <= 5; i++)
            {
                System.Threading.Thread.Sleep(2000);
                while (clientSocket.messageQueue.Count > 0)
                {
                    if (clientSocket.messageQueue.Dequeue().Substring(0, 5) == ContactLookupMessage.ContactLookupMessageType)
                        return;
                }
            }
            Assert.Fail("Never got login message back");
        }

        [TestMethod]
        public void AddContact()
        {
            SocketHandler.SocketServer mySockets = new SocketHandler.SocketServer(0, 1000);
            SocketHandler.SocketClient clientSocket = null;
            mySockets.Init();
            mySockets.Start(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 20000));
            clientSocket = new SocketHandler.SocketClient("127.0.0.1", 20000, false, ((char)2).ToString(), ((char)3).ToString());

            messageSpecs.AddContactMessage x = new messageSpecs.AddContactMessage();
            x.sender = "10006";
            x.receiver = "2";
            clientSocket.Send(x.getMessageString());

            for (int i = 1; i <= 5; i++)
            {
                System.Threading.Thread.Sleep(1000);
                while (clientSocket.messageQueue.Count > 0)
                {
                    if (clientSocket.messageQueue.Dequeue().Substring(0, 5) == AddContactMessage.AddContactMessageType)
                        return;
                }
            }
            Assert.Fail("Never got login message back");
        }

        [TestMethod]
        public void ChangeSettings()
        {
            SocketHandler.SocketServer mySockets = new SocketHandler.SocketServer(0, 1000);
            SocketHandler.SocketClient clientSocket = null;
            mySockets.Init();
            mySockets.Start(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 20000));
            clientSocket = new SocketHandler.SocketClient("127.0.0.1", 20000, false, ((char)2).ToString(), ((char)3).ToString());

            messageSpecs.ChangeUserSettingsMessage message = new messageSpecs.ChangeUserSettingsMessage();
            message.EmailAddress = "ceidsnes@uncc.edu";
            message.UserID = "10006";
            message.FirstName = "Cody";
            message.LastName = "eidsness";
            message.PassWord = "pword";
            clientSocket.Send(message.getMessageString());

            for (int i = 1; i <= 5; i++)
            {
                System.Threading.Thread.Sleep(1000);
                while (clientSocket.messageQueue.Count > 0)
                {
                    if (clientSocket.messageQueue.Dequeue().Substring(0, 5) == ChangeUserSettingsMessage.ChangeUserSettingsMessageType)
                        return;
                }
            }
            Assert.Fail("Never got login message back");
        }
    }
}
