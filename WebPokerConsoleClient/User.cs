using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebPokerConsoleClient
{
    class User
    {
        public string login = "";
        public string name = "";

        public User(string log, string nameUser)
        {
            login = log;
            name = nameUser;
        }
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
