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
        public byte card1;
        public byte card2;
        public byte blind;
        public int money;
        public bool fold = false;
        public int bet;
    }
}
