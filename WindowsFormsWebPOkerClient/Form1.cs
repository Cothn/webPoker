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
        public delegate void MyDelegate(string iText, int i); // для доступа к элементам из другого потока с передачей параметров
        public void IzmeniTabel(string iText, int i)
        {

                List<PictureBox> TableCard = new List<PictureBox>() 
                {
                card1,
                card2,
                card3,
                card4,
                card5
                };

                TableCard[i].BackgroundImage = Image.FromFile("D:\\RepositHub\\webPoker\\WindowsFormsWebPOkerClient\\Resources\\"+iText+".jpg");

        }
        public void IzmeniLOgi(string iText, int i)
        {
            LOgi.Items.Add(iText + "\n");
        }


        Socket handler;
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
        public  Socket Go(string name, string login, int port)
        {
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

                LOgi.Items.Add( "Connect to port:" + port.ToString());

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
                    LOgi.Items.Add(" Add player: " + playerList[i].login);
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

               // LOgi.Text = LOgi.Text + "D:\\RepositHub\\webPoker\\WindowsFormsWebPOkerClient\\Resources\\" + playerList[gamerNumber].card1 + ".jpg" + "Карта1:" + playerList[gamerNumber].card1 + ", Карта2:" + playerList[gamerNumber].card2 + ", Ставка:" + playerList[gamerNumber].bet + ", Остаток:" + playerList[gamerNumber].money + "\n";
                UserCard[gamerNumber * 2].BackgroundImage = Image.FromFile("D:\\RepositHub\\webPoker\\WindowsFormsWebPOkerClient\\Resources\\"+ playerList[gamerNumber].card1 +".jpg");
                UserCard[gamerNumber * 2+1].BackgroundImage = Image.FromFile("D:\\RepositHub\\webPoker\\WindowsFormsWebPOkerClient\\Resources\\" + playerList[gamerNumber].card2 + ".jpg");


                return handler;
        }

        private void SendAction(TPokerAction PokerAction)
        {

            JsonHandle.SendObject(handler, PokerAction);
            if (PokerAction == TPokerAction.Rais)
            {

                int bet = trackBarRaise.Value;
                //Thread.Sleep(200);
                JsonHandle.SendObject(handler, bet);
            }
        }
        private void RefreshMessage()
        {

            while (JsonConvert.DeserializeObject<bool>(JsonHandle.ReciveString(handler))) //refresh
            {
                playerList = (List<Player>)JsonConvert.DeserializeObject<List<Player>>(JsonHandle.ReciveString(handler));
                //BankLabel.Text = playerList[0].Allmoney.ToString();

                string Text ="Карта1:" + playerList[gamerNumber].card1 + ", Карта2:" + playerList[gamerNumber].card2 + ", банк:" + playerList[gamerNumber].Allmoney + ", Ставка:" + playerList[gamerNumber].bet + ", Остаток:" + playerList[gamerNumber].money + "  _Table_  " + playerList[gamerNumber].table + "\n";
                BeginInvoke(new MyDelegate(IzmeniLOgi), Text, 0);
               Text = "Карта1:" + playerList[gamerNumber].card1 + ", Карта2:" + playerList[gamerNumber].card2 + ", банк:" + playerList[gamerNumber].Allmoney + ", Ставка:" + playerList[gamerNumber].bet + ", Остаток:" + playerList[gamerNumber].money + "  _Table_  " + playerList[gamerNumber].table + "\n";
               
                for (int i = 0; i < playerList[gamerNumber].table.Length; i = i + 2)
                {
                    BeginInvoke(new MyDelegate(IzmeniTabel), playerList[gamerNumber].table.Substring(i, 2), i/2); 

                    //TableCard[i / 2].Image = Image.FromFile("D:\\RepositHub\\webPoker\\WindowsFormsWebPOkerClient\\Resources\\" +  + ".png");
                }
            }

        }

        private void начатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Go("ami", "ami", 11006);
        }


        private void EndGame(string winer)
        {
            LOgi.Items.Add( "Winer:" + winer);
            string WinerLogin = winer.Substring(winer.IndexOf('_')+1, winer.IndexOf('-') -winer.IndexOf('_')-1);
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].login == WinerLogin)
                {
                    BankLabel.Text = WinerLogin;
                    break;
                }
            }

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            handler = null;

            SstartButt.Visible = true;
            LoginBox.Visible = true;
            PortBox.Visible = true;
            NameBox.Visible = true;
            label8.Visible = true;
            label9.Visible = true;
            label10.Visible = true;
            Check_CallButton.Visible = false;
            FoldButton.Visible = false;
            RaiseButton.Visible = false;

            // UserListener.Close();
            //UserListener = null;
            //Thread.Sleep(100);
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
            handler = Go(NameBox.Text, LoginBox.Text, int.Parse(PortBox.Text));

            RefreshMessage();
            //// КОД 1 - здесь код до запуска потока
            //Thread potok1 = new Thread(RefreshMessage); // создание отдельного потока
            //potok1.Start(); // запуск потока
            //// КОД 2 - здесь код после запуска первого потока
        }

        private void Check_CallButton_Click(object sender, EventArgs e)
        {
            string str = JsonHandle.ReciveString(handler);
            if (str == "False" || str == "false")
            {
                SendAction(TPokerAction.Check);
                Thread potok1 = new Thread(RefreshMessage); // создание отдельного потока
                potok1.Start(); // запуск потока
            }
            else
            {
                EndGame(str);
            }
        }

        private void FoldButton_Click(object sender, EventArgs e)
        {
            string str = JsonHandle.ReciveString(handler);
            if (str == "False" || str == "false")
            {
                SendAction(TPokerAction.Fold);
                Thread potok1 = new Thread(RefreshMessage); // создание отдельного потока
                potok1.Start(); // запуск потока
            }
            else
            {
                EndGame(str);
            }
        }

        private void RaiseButton_Click(object sender, EventArgs e)
        {
            string str = JsonHandle.ReciveString(handler);
            if (str == "False" || str == "false")
            {
                SendAction(TPokerAction.Rais);
                Thread potok1 = new Thread(RefreshMessage); // создание отдельного потока
                potok1.Start(); // запуск потока
            }
            else 
            {
                EndGame(str);
            }
        }
    }
}
