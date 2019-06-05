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
}
