using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
namespace SocketTester
{
    public partial class Form1 : Form
    {
        public SocketHandler.SocketServer mySockets = new SocketHandler.SocketServer(0, 1000);
        public SocketHandler.SocketClient clientSocket;
         
        public Timer readTimer = new Timer { Interval = 1000 };
        public Form1()
        {
            InitializeComponent();
            mySockets.Init();
            mySockets.Start(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 20000));
            readTimer.Tick += new EventHandler(readMessages);
            readTimer.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clientSocket = new SocketHandler.SocketClient("127.0.0.1", 20000, false, ((char)3).ToString(),((char)3).ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            messageSpecs.UserCreationMessage myCreationMessage = new messageSpecs.UserCreationMessage();
            myCreationMessage.EmailAddress = "ceidsnes@uncc.edu";
            myCreationMessage.UserName = "coeids";
            myCreationMessage.FirstName = "Cody";
            myCreationMessage.LastName = "Eidsness";
            //messageSpecs.UserCreationMessage myCreationMessage = new messageSpecs.UserCreationMessage();
            //myCreationMessage.EmailAddress = "ceidsnes@uncc.edu";
            //myCreationMessage.UserName = "SSDIprojectteam1";
            //myCreationMessage.FirstName = "SSDI";
            //myCreationMessage.LastName = "Development";
            clientSocket.Send(myCreationMessage.getMessageString());
        }

        public void readMessages(object sender, EventArgs e)
        {
            if (clientSocket != null)
            {
                if (clientSocket.Connected)
                {
                    lblConnected.Text = "CONNECTED";
                    lblConnected.BackColor = Color.Green;
                }
                else{
                    lblConnected.Text = "NOT CONNECTED";
                    lblConnected.BackColor = Color.Red;
                }
                while (clientSocket.messageQueue.Count > 0)
                {
                    string message = clientSocket.messageQueue.Dequeue();
                    msgBox.Items.Insert(0, message);
                    switch (message.Substring(0, 5))
                    {
                        case messageSpecs.PictureMessage.PictureMessageType:
                            messageSpecs.PictureMessage recMessage = new messageSpecs.PictureMessage(message);
                            pictureBox2.Image = recMessage.PictureMsg;
                            break;
                        case messageSpecs.FileMessage.FileMessageType:
                            messageSpecs.FileMessage u = new messageSpecs.FileMessage(message);
                            
                            break;
                    }
                }
            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            messageSpecs.LogonMessage myLogonMessage = new messageSpecs.LogonMessage();
            myLogonMessage.UserName = "coeids";
            myLogonMessage.PassWord = "pword";
            clientSocket.Send(myLogonMessage.getMessageString());
        }

        private void btnLookup_Click(object sender, EventArgs e)
        {
            messageSpecs.ContactLookupMessage x = new messageSpecs.ContactLookupMessage();
            x.LookupName = "coei";
            clientSocket.Send(x.getMessageString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            messageSpecs.AddContactMessage x = new messageSpecs.AddContactMessage();
            x.sender = "10006";
            x.receiver = "2";
            clientSocket.Send(x.getMessageString());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            messageSpecs.ChangeUserSettingsMessage message = new messageSpecs.ChangeUserSettingsMessage();
            message.EmailAddress = "ceidsnes@uncc.edu";
            message.UserID = "10006";
            message.FirstName = "C";
            message.LastName = "eids";
            message.PassWord = "pword";
            clientSocket.Send(message.getMessageString());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            messageSpecs.PictureMessage x = new messageSpecs.PictureMessage();
            x.sender = "10006";
            x.receiver = "10006";
            x.PictureMsg = pictureBox1.Image;
            clientSocket.Send(x.getMessageString());



            //messageSpecs.PictureMessage y = new messageSpecs.PictureMessage(x.getMessageString());
            //pictureBox2.Image = y.PictureMsg;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string y = "";
            string connectionString = "Data Source=.;Initial Catalog=MessageANiner;User ID=sa;Password=SSDIproject1;Connect Timeout=15";

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand command = new SqlCommand("select top 1 message from MessageTable order by timestamp desc", sqlConnection))
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        byte[] bytes = (byte[])command.ExecuteScalar();
                        y = Encoding.Default.GetString(bytes);
                    }
                }
                catch
                {

                }
                finally
                {
                    sqlConnection.Close();
                }

            }

            messageSpecs.PictureMessage h = new messageSpecs.PictureMessage(y);
            pictureBox2.Image = h.PictureMsg;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            messageSpecs.FileMessage x = new messageSpecs.FileMessage();
            x.FileName = "C:\\Users\\cody\\Documents\\Hw1.txt";
            
            clientSocket.Send(x.getMessageString());
   
        }

        private void button9_Click(object sender, EventArgs e)
        {
            messageSpecs.getPastMessages x = new messageSpecs.getPastMessages();
            x.lastMsgTime = DateTime.Now;
            messageSpecs.getPastMessages y = new messageSpecs.getPastMessages(x.getMessageString());
        }

        private void button10_Click(object sender, EventArgs e)
        {
            messageSpecs.CreateGroupMessage x = new messageSpecs.CreateGroupMessage();
            x.memberList.Add("1");
            x.memberList.Add("2");
            messageSpecs.CreateGroupMessage y = new messageSpecs.CreateGroupMessage(x.getMessageString());
        }

        private void button11_Click(object sender, EventArgs e)
        {
            messageSpecs.ContactResponseMessage x = new messageSpecs.ContactResponseMessage();
            x.sender = "10006";
            x.receiver = "10007";
            x.Reply = messageSpecs.ContactResponseMessage.ContactResponse.Accept;
            clientSocket.Send(x.getMessageString());
        }

        private void button12_Click(object sender, EventArgs e)
        {
            messageSpecs.CreateGroupMessage x = new messageSpecs.CreateGroupMessage();
            x.GroupName = "Test Group";
            x.memberList.Add(2);
            x.memberList.Add(10006);
            x.memberList.Add(10007);
            clientSocket.Send(x.getMessageString());
        }
    }
}
