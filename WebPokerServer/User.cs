using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPokerServer
{
    class User
    {
        public string login = "";
        public string name = "";
    }

    public class Player
    {
        public string login;
        public string name;
        public string card1;
        public string card2;
        public int money;
        public int bank;
        public bool fold = false;
        public int bet;
        public string table;
    }
}
