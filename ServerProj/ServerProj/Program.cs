﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ServerProj
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server("127.0.0.1", 4444);
            server.Start();
            server.Stop();
        }
    }
}
