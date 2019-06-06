using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WindowsFormsWebPOkerClient
{
    public partial class MainForm : Form
    {
        //public delegate void MyDelegate(string iText, int i); // для доступа к элементам из другого потока с передачей параметров
        //public void IzmeniLabel(string iText, int i)
        //{

        //    

        //}



        int gamerNumber = 0;
        List<Player> playerList;
        public enum TPokerAction { Check = 0, Rais = 1, Fold = 2 };
        TPokerAction PokerAction = 0;
        public MainForm()
        {
            InitializeComponent();

        }


        //const string login = "Ami";
        //const string name = "Ami";
        public  void Go(string name, string login, int port)
        {
            List<PictureBox> TableCard = new List<PictureBox>() 
            {
                card1,
                card2,
                card3,
                card4,
                card5
            };
            List<PictureBox> UserCard = new List<PictureBox>() 
            {
                UserCard10,
                UserCard9,
                UserCard8,
                UserCard7,
                UserCard6,
                UserCard5,
                UserCard4,
                UserCard3,
                UserCard2,
                UserCard1
            };
            List<PictureBox> UserPicth = new List<PictureBox>() 
            {
                pictureBox6,
                pictureBox5,
                pictureBox3,
                pictureBox4,
                pictureBox2
            };
            List<Label> namesLabel = new List<Label>() 
            {
                label3,
                label4,
                label6,
                label7,
                label5
            };

                //int quit = 0;
                //11006
                //конечная локальная точка
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[1];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

                //Сoздаем сокет Tcp/Ip
                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //Сокет подключение
                sender.Connect(ipEndPoint);

                User user = new User(login, name);
                //общаемся с сервером
                JsonHandle.SendObject(sender, user);

                //слушаем
                port = (int)JsonConvert.DeserializeObject<int>(JsonHandle.ReciveString(sender));

                LOgi.Text = LOgi.Text + "Connect to port:" + port.ToString();

                //close
                sender.Disconnect(true);
                //sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                //sender = null;

                //open
                Socket UserListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ipEndPoint = new IPEndPoint(ipAddr, port);

                //Сокет подключение
                UserListener.Bind(ipEndPoint);
                UserListener.Listen(1);
                Socket handler = UserListener.Accept();
                //Console.WriteLine("Ожидаем соединения через {0}", ipEndPoint);


                //раздача
                JsonConvert.DeserializeObject<bool>(JsonHandle.ReciveString(handler));
                playerList = (List<Player>)JsonConvert.DeserializeObject<List<Player>>(JsonHandle.ReciveString(handler));

                for (int i = 0; i < playerList.Count(); i++)
                {
                    //BeginInvoke(new MyDelegate(IzmeniLogi), );
                    LOgi.Text = LOgi.Text + " Add player: " + playerList[i].login;
                    //BeginInvoke(new MyDelegate(IzmeniUserPicth), "", i);
                    UserPicth[i].BackgroundImage = Image.FromFile("D:\\RepositHub\\webPoker\\WindowsFormsWebPOkerClient\\Resources\\active.png");
                    //BeginInvoke(new MyDelegate(IzmeniLabel), playerList[i].ToString(), i);
                    namesLabel[i].Text  = playerList[i].name;
                    if (playerList[i].login == login)
                    {
                        gamerNumber = i;
                        break;
                    }
                }

                LOgi.Text = LOgi.Text + "D:\\RepositHub\\webPoker\\WindowsFormsWebPOkerClient\\Resources\\" + playerList[gamerNumber].card1 + ".jpg" + "Карта1:" + playerList[gamerNumber].card1 + ", Карта2:" + playerList[gamerNumber].card2 + ", Ставка:" + playerList[gamerNumber].bet + ", Остаток:" + playerList[gamerNumber].money + "\n";
                UserCard[gamerNumber * 2].BackgroundImage = Image.FromFile("D:\\RepositHub\\webPoker\\WindowsFormsWebPOkerClient\\Resources\\"+ playerList[gamerNumber].card2 +".jpg");
                UserCard[gamerNumber * 2+1].BackgroundImage = Image.FromFile("D:\\RepositHub\\webPoker\\WindowsFormsWebPOkerClient\\Resources\\" + playerList[gamerNumber].card2 + ".jpg");
        }

        //private void SendAction(Socket handler)
        //{

        //    JsonHandle.SendObject(handler, PokerAction);
        //    if (PokerAction == TPokerAction.Rais)
        //    {
        //        Console.WriteLine("Введите ставку :");
        //        string buf = Console.ReadLine();
        //        int bet = int.Parse(buf);
        //        JsonHandle.SendObject(handler, bet);
        //    }
        //}
        //private bool RefreshMessage(Socket handler, List<PictureBox> TableCard)
        //{
        //    bool stop;
        //    while (JsonConvert.DeserializeObject<bool>(JsonHandle.ReciveString(handler))) //refresh
        //    {
        //        playerList = (List<Player>)JsonConvert.DeserializeObject<List<Player>>(JsonHandle.ReciveString(handler));
        //        BankLabel.Text = playerList[0].Allmoney.ToString();
        //        LOgi.Text = LOgi.Text + "Карта1:" + playerList[gamerNumber].card1 + ", Карта2:" + playerList[gamerNumber].card2 + ", Ставка:" + playerList[gamerNumber].bet + ", Остаток:" + playerList[gamerNumber].money + "  _Table_  " + playerList[gamerNumber].table + "\n";
        //        for (int i = 0; i < playerList[gamerNumber].table.Length; i = i + 2)
        //        {
        //            TableCard[i / 2].Image = Image.FromFile("D:\\RepositHub\\webPoker\\WindowsFormsWebPOkerClient\\Resources\\" + playerList[gamerNumber].table.Substring(i, 2) + ".png");
        //        }
        //    }
        //    stop = JsonConvert.DeserializeObject<bool>(JsonHandle.ReciveString(handler));
        //    return stop;
        //}
        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void начатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Go("ami", "ami", 11006);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
        
        }

        private void pictureBox15_Click(object sender, EventArgs e)
        {
        
        }

        private void pictureBox16_Click(object sender, EventArgs e)
        {
        
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
        
        }

        private void pbMyCard1_Click(object sender, EventArgs e)
        {
        
        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {
        
        }

        private void pictureBox13_Click(object sender, EventArgs e)
        {
        
        }

        private void pictureBox20_Click(object sender, EventArgs e)
        {
        
        }

        private void pictureBox19_Click(object sender, EventArgs e)
        {
        
        }

        private void pictureBox18_Click(object sender, EventArgs e)
        {
        
        }

        private void FoldButton_Click(object sender, EventArgs e)
        {

        }

        //private void afteraction(Socket handler, List<PictureBox> TableCard)
        //{
        //               //Console.WriteLine( );
        //            bool stop = RefreshMessage(handler, TableCard);
        //            while (!stop)
        //            {
        //                SendAction(handler);
        //                stop = RefreshMessage(handler, TableCard);
        //            }

        //            string winer = JsonConvert.DeserializeObject<string>(JsonHandle.ReciveString(handler));
        //            LOgi.Text = LOgi.Text + "Connect to port:" + winer;

        //            handler.Shutdown(SocketShutdown.Both);
        //            handler.Close();
        //            handler = null;

        //           // UserListener.Close();
        //           //UserListener = null;
        //            //Thread.Sleep(100);
        //}

        private void Check_CallButton_Click(object sender, EventArgs e)
        {
            //// КОД 1 - здесь код до запуска потока
            //Thread potok1 = new Thread(Go); // создание отдельного потока
            //potok1.Start(); // запуск потока
            //// КОД 2 - здесь код после запуска первого потока
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void SstartButt_Click(object sender, EventArgs e)
        {
            SstartButt.Visible = false;
            LoginBox.Visible = false;
            PortBox.Visible = false;
            NameBox.Visible = false;
            label8.Visible = false;
            label9.Visible = false;
            label10.Visible = false;
            Check_CallButton.Visible = true;
            FoldButton.Visible = true;
            RaiseButton.Visible = true;
            Go(NameBox.Text, LoginBox.Text, int.Parse(PortBox.Text));
        }
    }
}
