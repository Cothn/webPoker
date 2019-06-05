using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebPokerConsoleClient
{
    class User
    {
        public string login = "";
        public string password = "";

        public User(string log, string pass)
        {
            login = log;
            password = pass;
        }
    }
}
