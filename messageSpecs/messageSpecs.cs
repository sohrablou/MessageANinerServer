using System;

namespace messageSpecs
{
    public class BaseMessage
    {
        public static int messageTypeLength
        {
            get
            {
                return 5;
            }
        }
        public static int receiverLength
        {
            get
            {
                return 10;
            }
        }
        public static int senderLength
        {
            get
            {
                return 10;
            }
        }
        public static int startingSpot
        {
            get
            {
                return messageTypeLength + receiverLength + senderLength;
            }
        }

        private string _messageType = "NtSet";
        public string messageType
        {
            get
            {
                return padding(_messageType, messageTypeLength, padType.text);
            }
            set
            {
                _messageType = value;
            }
        }

        private string _receiver = "";
        public string receiver
        {
            get
            {
                return padding(_receiver, receiverLength, padType.numeric);
            }
            set
            {
                _receiver = value;
            }
        }

        private string _sender = "";
        public string sender
        {
            get
            {
                return padding(_sender, senderLength, padType.numeric);
            }
            set
            {
                _sender = value;
            }
        }

        protected enum padType
        {
            text,
            numeric
        }

        protected string padding(string stringToPad, int padWidth, padType paddingType)
        {
            if (paddingType == padType.numeric)
            {
                return stringToPad.PadLeft(padWidth, '0');
            }
            else
            {
                return stringToPad.PadRight(padWidth, ' ');
            }
        }

        public BaseMessage()
        {
            messageType = "NTSET";
            receiver = "0";
            sender = "0";
        }

        public BaseMessage(string message)
        {
            messageType = message.Substring(0, messageTypeLength);
            receiver = message.Substring(messageTypeLength, receiverLength);
            sender = message.Substring(messageTypeLength + receiverLength, senderLength);
        }

    }

    public class LogonMessage : BaseMessage
    {
        public const string LoginMessageType = "LOGIN";
        #region "Properties"
        private string _EmailAddress;
        public string EmailAddress
        {
            get
            {
                return padding(_EmailAddress, 50, padType.text);
            }
            set
            {
                _EmailAddress = value;
            }
        }

        private string _userName;
        public string UserName
        {
            get
            {
                return padding(_userName, 20, padType.text);
            }
            set
            {
                _userName = value;
            }
        }

        private string _password;
        public string PassWord
        {
            get
            {
                return padding(_password, 20, padType.text);
            }
            set
            {
                _password = value;
            }
        }

        private string _returnMessage;
        public string ReturnMessage
        {
            get
            {
                return padding(_returnMessage, 50, padType.text);
            }
            set
            {
                _returnMessage = value;
            }
        }

        public bool Verified { get; set; }

        public bool PasswordReset { get; set; }

        private string _userID;
        public string UserID
        {
            get
            {
                return padding(_userID, receiverLength, padType.numeric);
            }
            set
            {
                _userID = value;
            }
        }

        private string _firstName;
        public string FirstName
        {
            get
            {
                return padding(_firstName, 20, padType.text);
            }
            set
            {
                _firstName = value;
            }
        }

        private string _lastName;
        public string LastName
        {
            get
            {
                return padding(_lastName, 20, padType.text);
            }
            set
            {
                _lastName = value;
            }
        }
        #endregion

        public LogonMessage(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                UserName = message.Substring(messageTypeLength, 20);
                PassWord = message.Substring(messageTypeLength + 20, 20);
                EmailAddress = message.Substring(messageTypeLength + 40, 50);
                ReturnMessage = message.Substring(messageTypeLength + 90, 50);
                if (message.Substring(messageTypeLength + 140, 5) == "True ")
                {
                    Verified = true;
                }
                else
                {
                    Verified = false;
                }
                if (message.Substring(messageTypeLength + 145, 5) == "True ")
                {
                    PasswordReset = true;
                }
                else
                {
                    PasswordReset = false;
                }
                FirstName = message.Substring(messageTypeLength + 150, 20);
                LastName = message.Substring(messageTypeLength + 170, 20);
                UserID = message.Substring(messageTypeLength + 190, receiverLength);
            }
            catch
            {

            }
        }

        public LogonMessage()
        {
            messageType = LoginMessageType;
            UserName = "";
            PassWord = "";
            EmailAddress = "";
            UserID = "";
            ReturnMessage = "";
            FirstName = "";
            LastName = "";
            PasswordReset = false;
            Verified = false;
        }

        public string getMessageString()
        {
            string returnString = "";
            returnString += messageType;
            returnString += UserName;
            returnString += PassWord;
            returnString += EmailAddress;
            returnString += ReturnMessage;
            returnString += padding(Verified.ToString(), 5, padType.text);
            returnString += padding(PasswordReset.ToString(), 5, padType.text);
            returnString += FirstName;
            returnString += LastName;
            returnString += UserID;
            return returnString;
        }
    }

    public class UserCreationMessage : BaseMessage
    {
        public const string CreateUserMessageType = "CTUSR";
        #region "Properties"
        private string _EmailAddress;
        public string EmailAddress
        {
            get
            {
                return padding(_EmailAddress, 50, padType.text);
            }
            set
            {
                _EmailAddress = value;
            }
        }

        private string _userName;
        public string UserName
        {
            get
            {
                return padding(_userName, 20, padType.text);
            }
            set
            {
                _userName = value;
            }
        }

        private string _password;
        public string PassWord
        {
            get
            {
                return padding(_password, 20, padType.text);
            }
            set
            {
                _password = value;
            }
        }

        private string _returnMessage;
        public string ReturnMessage
        {
            get
            {
                return padding(_returnMessage, 50, padType.text);
            }
            set
            {
                _returnMessage = value;
            }
        }

        public bool Verified { get; set; }

        private string _firstName;
        public string FirstName
        {
            get
            {
                return padding(_firstName, 20, padType.text);
            }
            set
            {
                _firstName = value;
            }
        }

        private string _lastName;
        public string LastName
        {
            get
            {
                return padding(_lastName, 20, padType.text);
            }
            set
            {
                _lastName = value;
            }
        }
        #endregion

        public UserCreationMessage(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                UserName = message.Substring(messageTypeLength, 20);
                PassWord = message.Substring(messageTypeLength + 20, 20);
                EmailAddress = message.Substring(messageTypeLength + 40, 50);
                FirstName = message.Substring(messageTypeLength + 90, 20);
                LastName = message.Substring(messageTypeLength + 110, 20);
                ReturnMessage = message.Substring(messageTypeLength + 130, 50);
                if (message.Substring(messageTypeLength + 180, 5) == "True ")
                {
                    Verified = true;
                }
                else
                {
                    Verified = false;
                }
            }
            catch
            {

            }
        }

        public UserCreationMessage()
        {
            messageType = CreateUserMessageType;
            UserName = "";
            PassWord = "";
            EmailAddress = "";
            FirstName = "";
            LastName = "";
            ReturnMessage = "";
            Verified = false;
        }

        public string getMessageString()
        {
            string returnString = "";
            returnString += messageType;
            returnString += UserName;
            returnString += PassWord;
            returnString += EmailAddress;
            returnString += FirstName;
            returnString += LastName;
            returnString += ReturnMessage;
            returnString += padding(Verified.ToString(), 5, padType.text);
            return returnString;
        }
    }

    public class FriendStatusMessage : BaseMessage
    {
        public const string FriendStatusMessageType = "FSTAT";
        public enum LoggedOnStatus
        {
            Offline = 0,
            Online = 1,
            Friend = 2,
            Group = 3
        }
        #region "Properties"
        private LoggedOnStatus _UserStatus;
        public LoggedOnStatus UserStatus
        {
            get
            {
                return _UserStatus;
            }
            set
            {
                _UserStatus = value;
            }
        }

        private string _firstName;
        public string FirstName
        {
            get
            {
                return padding(_firstName, 20, padType.text);
            }
            set
            {
                _firstName = value;
            }
        }

        private string _lastName;
        public string LastName
        {
            get
            {
                return padding(_lastName, 20, padType.text);
            }
            set
            {
                _lastName = value;
            }
        }
        #endregion

        public FriendStatusMessage(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                receiver = message.Substring(messageTypeLength, receiverLength);
                sender = message.Substring(messageTypeLength + receiverLength, senderLength);
                FirstName = message.Substring(startingSpot, 20);
                LastName = message.Substring(startingSpot + 20, 20);
                UserStatus = (LoggedOnStatus)Enum.ToObject(typeof(LoggedOnStatus), int.Parse(message.Substring(startingSpot + 40, 1)));
            }
            catch
            {

            }
        }

        public FriendStatusMessage()
        {
            messageType = FriendStatusMessageType;
            receiver = "0";
            sender = "0";
            FirstName = "";
            LastName = "";
            UserStatus = LoggedOnStatus.Online;
        }

        public string getMessageString()
        {
            string returnString = "";
            returnString += messageType;
            returnString += receiver;
            returnString += sender;
            returnString += FirstName;
            returnString += LastName;
            returnString += padding(((int)UserStatus).ToString(), 1, padType.numeric);
            return returnString;
        }
    }

    public class ContactLookupMessage : BaseMessage
    {
        public const string ContactLookupMessageType = "CLOOK";
        #region "Properties"

        private string _LookupName;
        public string LookupName
        {
            get
            {
                return padding(_LookupName, 20, padType.text);
            }
            set
            {
                _LookupName = value;
            }
        }

        public System.Data.DataTable ReturnTable { get; set; }
        #endregion

        public ContactLookupMessage(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                LookupName = message.Substring(messageTypeLength, 20);
                string dataTableInString = message.Substring(messageTypeLength + 20, message.Length - (messageTypeLength + 20));
                string[] rows = dataTableInString.Split(';');
                System.Data.DataTable myTable = new System.Data.DataTable();
                myTable.Columns.Add("UID", typeof(long));
                myTable.Columns.Add("FirstName", typeof(string));
                myTable.Columns.Add("LastName", typeof(string));
                myTable.Columns.Add("UserName", typeof(string));
                myTable.Columns.Add("EmailAddress", typeof(string));
                if (dataTableInString != "None")
                {
                    foreach (string row in rows)
                    {
                        myTable.Rows.Add(row.Split(','));
                    }
                }

                ReturnTable = myTable;
            }
            catch
            {

            }
        }

        public ContactLookupMessage()
        {
            messageType = ContactLookupMessageType;
            LookupName = "";
            System.Data.DataTable myTable = new System.Data.DataTable();
            myTable.Columns.Add("UID", typeof(long));
            myTable.Columns.Add("FirstName", typeof(string));
            myTable.Columns.Add("LastName", typeof(string));
            myTable.Columns.Add("UserName", typeof(string));
            myTable.Columns.Add("EmailAddress", typeof(string));
            ReturnTable = myTable;
        }

        public string getMessageString()
        {
            string returnString = "";
            returnString += messageType;
            returnString += LookupName;

            string TableInString = "";

            foreach (System.Data.DataRow x in ReturnTable.Rows)
            {
                string rowString = "";
                foreach (object y in x.ItemArray)
                {
                    rowString += "," + y.ToString();
                }
                rowString = rowString.Remove(0, 1);
                TableInString += ";" + rowString;
            }
            if (TableInString.Length > 0)
            {
                TableInString = TableInString.Remove(0, 1);
            }
            else
            {
                TableInString = "None";
            }

            returnString += TableInString;
            return returnString;
        }
    }

    public class AddContactMessage : BaseMessage
    {
        public const string AddContactMessageType = "ADDFR";

        private string _returnMessage;
        public string ReturnMessage
        {
            get
            {
                return padding(_returnMessage, 50, padType.text);
            }
            set
            {
                _returnMessage = value;
            }
        }

        public AddContactMessage(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                receiver = message.Substring(messageTypeLength, receiverLength);
                sender = message.Substring(messageTypeLength + receiverLength, senderLength);
                ReturnMessage = message.Substring(startingSpot, 50);
            }
            catch
            {

            }
        }

        public AddContactMessage()
        {
            messageType = AddContactMessageType;
            receiver = "0";
            sender = "0";
            ReturnMessage = "";
        }

        public string getMessageString()
        {
            string returnString = "";
            returnString += messageType;
            returnString += receiver;
            returnString += sender;
            returnString += ReturnMessage;
            return returnString;
        }
    }

    public class ChangeUserSettingsMessage : BaseMessage
    {
        public const string ChangeUserSettingsMessageType = "CHSET";

        #region "Properties"
        private string _EmailAddress;
        public string EmailAddress
        {
            get
            {
                return padding(_EmailAddress, 50, padType.text);
            }
            set
            {
                _EmailAddress = value;
            }
        }

        private string _password;
        public string PassWord
        {
            get
            {
                return padding(_password, 20, padType.text);
            }
            set
            {
                _password = value;
            }
        }

        private string _returnMessage;
        public string ReturnMessage
        {
            get
            {
                return padding(_returnMessage, 50, padType.text);
            }
            set
            {
                _returnMessage = value;
            }
        }

        public bool Verified { get; set; }

        private string _firstName;
        public string FirstName
        {
            get
            {
                return padding(_firstName, 20, padType.text);
            }
            set
            {
                _firstName = value;
            }
        }

        private string _lastName;
        public string LastName
        {
            get
            {
                return padding(_lastName, 20, padType.text);
            }
            set
            {
                _lastName = value;
            }
        }

        private string _userID;
        public string UserID
        {
            get
            {
                return padding(_userID, receiverLength, padType.numeric);
            }
            set
            {
                _userID = value;
            }
        }
        #endregion

        public ChangeUserSettingsMessage(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                PassWord = message.Substring(messageTypeLength, 20);
                EmailAddress = message.Substring(messageTypeLength + 20, 50);
                FirstName = message.Substring(messageTypeLength + 70, 20);
                LastName = message.Substring(messageTypeLength + 90, 20);
                ReturnMessage = message.Substring(messageTypeLength + 110, 50);
                if (message.Substring(messageTypeLength + 160, 5) == "True ")
                {
                    Verified = true;
                }
                else
                {
                    Verified = false;
                }
                UserID = message.Substring(messageTypeLength + 165, receiverLength);
            }
            catch
            {

            }
        }

        public ChangeUserSettingsMessage()
        {
            messageType = ChangeUserSettingsMessageType;
            PassWord = "";
            EmailAddress = "";
            FirstName = "";
            LastName = "";
            ReturnMessage = "";
            UserID = "";
            Verified = false;
        }

        public string getMessageString()
        {
            string returnString = "";
            returnString += messageType;
            returnString += PassWord;
            returnString += EmailAddress;
            returnString += FirstName;
            returnString += LastName;
            returnString += ReturnMessage;
            returnString += padding(Verified.ToString(), 5, padType.text);
            returnString += UserID;
            return returnString;
        }
    }

    public class TextMessage : BaseMessage
    {
        public const string TextMessageType = "TXTMG";

        public string TextToSend {get;set;}
        public bool insertAtTop { get; set; }
        public DateTime messageTimestamp { get; set; }

        public TextMessage(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                receiver = message.Substring(messageTypeLength, receiverLength);
                sender = message.Substring(messageTypeLength + receiverLength, senderLength);
                if (message.Substring(startingSpot, 5) == "True ")
                {
                    insertAtTop = true;
                }
                else
                {
                    insertAtTop = false;
                }

                messageTimestamp = Convert.ToDateTime(message.Substring(startingSpot + 5, 30));
                TextToSend = message.Substring(startingSpot + 35, message.Length - startingSpot-35);
            }
            catch
            {

            }
        }

        public TextMessage()
        {
            messageType = TextMessageType;
            receiver = "0";
            sender = "0";
            TextToSend = "";
            insertAtTop = false;
            messageTimestamp = DateTime.Now;
        }

        public string getMessageString()
        {
            string returnString = "";
            returnString += messageType;
            returnString += receiver;
            returnString += sender;
            returnString += padding(insertAtTop.ToString(), 5, padType.text);
            returnString += padding(messageTimestamp.ToString("R"), 30, padType.text);
            returnString += TextToSend;
            return returnString;
        }

    }

    public class PictureMessage : BaseMessage
    {
        public const string PictureMessageType = "PCTMG";
        public System.Drawing.Image PictureMsg { get; set; }
        public string _MsgLength = "";
        public string MsgLength
        {
            get
            {
                return padding(_MsgLength, 10, padType.numeric);
            }
            set
            {
                _MsgLength = value;
            }
        }
        public bool insertAtTop { get; set; }
        public DateTime messageTimestamp { get; set; }

        public PictureMessage(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                receiver = message.Substring(messageTypeLength, receiverLength);
                sender = message.Substring(messageTypeLength + receiverLength, senderLength);
                if (message.Substring(startingSpot, 5) == "True ")
                {
                    insertAtTop = true;
                }
                else
                {
                    insertAtTop = false;
                }

                messageTimestamp = Convert.ToDateTime(message.Substring(startingSpot + 5, 30));
                MsgLength = message.Substring(startingSpot+35, 10);
                if (message.Length == startingSpot+45)
                {
                    PictureMsg = null;
                }
                else
                {
                    string picture = message.Substring(startingSpot + 45, message.Length - 45-startingSpot);
                    var x = picture.Length;
                    PictureMsg = System.Drawing.Image.FromStream(new System.IO.MemoryStream(System.Text.Encoding.Default.GetBytes(picture)));
                }
             
            }
            catch
            {

            }
        }

        public PictureMessage()
        {
            messageType = PictureMessageType;
            receiver = "0";
            sender = "0";
            MsgLength = "";
            insertAtTop = false;
            messageTimestamp = DateTime.Now;
            PictureMsg = null;
        }

        public string getMessageString()
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            PictureMsg.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] bytes = ms.ToArray();
            string pictureString = System.Text.Encoding.Default.GetString(bytes);
            MsgLength = (pictureString.Length + messageTypeLength + receiverLength + senderLength + 46).ToString();

            string returnString = "";
            returnString += messageType;
            returnString += receiver;
            returnString += sender;
            returnString += padding(insertAtTop.ToString(), 5, padType.text);
            returnString += padding(messageTimestamp.ToString("R"), 30, padType.text);
            returnString += MsgLength;  
            returnString += pictureString;
            return returnString;
        }

    }

    public class UserTyping : BaseMessage
    {
        public const string UserTypingMessageType = "FTYPE";

        public UserTyping(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                receiver = message.Substring(messageTypeLength, receiverLength);
                sender = message.Substring(messageTypeLength + receiverLength, senderLength);
            }
            catch
            {

            }
        }

        public UserTyping()
        {
            messageType = UserTypingMessageType;
            receiver = "0";
            sender = "0";
        }

        public string getMessageString()
        {
            string returnString = "";
            returnString += messageType;
            returnString += receiver;
            returnString += sender;
            return returnString;
        }
    }

    public class FileMessage : BaseMessage
    {
        public const string FileMessageType = "FLMSG";

        public string FileName { get; set; }
        private string _Extension = "";
        public string Extension
        {
            get
            {
                return padding(_Extension, 20, padType.text);
            }
            set
            {
                _Extension = value;
            }
        }

        public string _MsgLength = "";
        public string MsgLength
        {
            get
            {
                return padding(_MsgLength, 10, padType.numeric);
            }
            set
            {
                _MsgLength = value;
            }
        }
        public bool insertAtTop { get; set; }
        public DateTime messageTimestamp { get; set; }

        public FileMessage(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                receiver = message.Substring(messageTypeLength, receiverLength);
                sender = message.Substring(messageTypeLength + receiverLength, senderLength);
                if (message.Substring(startingSpot, 5) == "True ")
                {
                    insertAtTop = true;
                }
                else
                {
                    insertAtTop = false;
                }
                messageTimestamp = Convert.ToDateTime(message.Substring(startingSpot + 5, 30));
                MsgLength = message.Substring(startingSpot+35, 10);
                Extension = message.Substring(startingSpot + 45, 20);
                if (message.Length == startingSpot+65)
                {
                    FileName = null;
                }
                else
                {
                    string picture = message.Substring(startingSpot + 65, message.Length - 65-startingSpot);
                    var x = picture.Length;
                    byte[] fileBytes = System.Text.Encoding.Default.GetBytes(picture);
                    string NewfileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") +  Extension;
                    FileName = NewfileName;
                    using (System.IO.Stream file = System.IO.File.OpenWrite(NewfileName))
                    {
                        file.Write(fileBytes, 0, fileBytes.Length);
                    }
                }
             
            }
            catch
            {

            }
        }

        public FileMessage()
        {
            messageType = FileMessageType;
            receiver = "0";
            sender = "0";
            MsgLength = "";
            FileName = "";
            insertAtTop = false;
            messageTimestamp = DateTime.Now;
        }

        public string getMessageString()
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.FileStream stream = System.IO.File.OpenRead(FileName);
            byte[] fileBytes = new byte[stream.Length];
            stream.Read(fileBytes, 0, fileBytes.Length);
            stream.Close();
            string pictureString = System.Text.Encoding.Default.GetString(fileBytes);

            MsgLength = (pictureString.Length + messageTypeLength + receiverLength + senderLength + 66).ToString();

            string returnString = "";
            returnString += messageType;
            returnString += receiver;
            returnString += sender;
            returnString += padding(insertAtTop.ToString(), 5, padType.text);
            returnString += padding(messageTimestamp.ToString("R"), 30, padType.text);
            returnString += MsgLength;
            returnString += Extension;
            returnString += pictureString;
            return returnString;
        }
    }

    public class AcknowledgementMessage : BaseMessage
    {
        public const string AcknowledgementMessageType = "AKMSG";
        
        public AcknowledgementMessage()
        {
            messageType = AcknowledgementMessageType;
            receiver = "0";
            sender = "0";
        }

        public AcknowledgementMessage(string message)
        {
            messageType = message.Substring(0, messageTypeLength);
            receiver = message.Substring(messageTypeLength, receiverLength);
            sender = message.Substring(messageTypeLength + receiverLength, senderLength);
        }

    }

    public class getPastMessages : BaseMessage
    {
        public const string getPastMessagesType = "GTMSG";
        public DateTime lastMsgTime { get; set; }

        public getPastMessages()
        {
            messageType = getPastMessagesType;
            receiver = "0";
            sender = "0";
            lastMsgTime = DateTime.Now;
        }

        public getPastMessages(string message)
        {
            messageType = message.Substring(0, messageTypeLength);
            receiver = message.Substring(messageTypeLength, receiverLength);
            sender = message.Substring(messageTypeLength + receiverLength, senderLength);
            lastMsgTime = DateTime.Parse(message.Substring(startingSpot, 20));
        }

        public string getMessageString()
        {
            string returnString = "";
            returnString += messageType;
            returnString += receiver;
            returnString += sender;
            returnString += lastMsgTime.ToString().PadRight(20,' ');
            return returnString;
        }
    }

    public class CreateGroupMessage : BaseMessage
    {
        public const string CreateGroupMessageType = "CTGRP";
        public System.Collections.ArrayList memberList { get; set; }
        private string _GroupName = "";
        public string GroupName
        {
            get
            {
                return padding(_GroupName, 20, padType.text);
            }
            set
            {
                _GroupName = value;
            }
        }

        public CreateGroupMessage()
        {
            messageType = CreateGroupMessageType;
            receiver = "0";
            sender = "0";
            memberList = new System.Collections.ArrayList();
        }

        public CreateGroupMessage(string message)
        {
            memberList = new System.Collections.ArrayList();
            messageType = message.Substring(0, messageTypeLength);
            receiver = message.Substring(messageTypeLength, receiverLength);
            sender = message.Substring(messageTypeLength + receiverLength, senderLength);
            GroupName = message.Substring(startingSpot, message.Length - startingSpot);
            string[] leftOver = message.Substring(startingSpot+20, message.Length - startingSpot - 20).Split(',');
            foreach (string user in leftOver)
            {
                memberList.Add(user);
            }
        }

        public string getMessageString()
        {

            string userList = "";
            foreach (object user in memberList)
            {
                userList += ("," + user.ToString());
            }
            userList = userList.Remove(0, 1);

            string returnString = "";
            returnString += messageType;
            returnString += receiver;
            returnString += sender;
            returnString += GroupName;
            returnString += userList;
            return returnString;
        }
    }

    public class ContactResponseMessage : BaseMessage
    {
        public const string ContactResponseMessageType = "CTCRS";
        public enum ContactResponse
        {
            Decline = 0,
            Accept = 1         
        }
        #region "Properties"
        private ContactResponse _Reply;
        public ContactResponse Reply
        {
            get
            {
                return _Reply;
            }
            set
            {
                _Reply = value;
            }
        }
        #endregion

        public ContactResponseMessage(string message)
        {
            try
            {
                messageType = message.Substring(0, messageTypeLength);
                receiver = message.Substring(messageTypeLength, receiverLength);
                sender = message.Substring(messageTypeLength + receiverLength, senderLength);
                Reply = (ContactResponse)Enum.ToObject(typeof(ContactResponse), int.Parse(message.Substring(startingSpot, 1)));
            }
            catch
            {

            }
        }

        public ContactResponseMessage()
        {
            messageType = ContactResponseMessageType;
            receiver = "0";
            sender = "0";
            Reply = ContactResponse.Accept;
        }

        public string getMessageString()
        {
            string returnString = "";
            returnString += messageType;
            returnString += receiver;
            returnString += sender;
            returnString += padding(((int)Reply).ToString(), 1, padType.numeric);
            return returnString;
        }
    }

}


