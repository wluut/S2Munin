using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace S2.Munin.Service
{
    public class Listener
    {
        private static Listener instance = new Listener();
        public static Listener Instance { get { return instance;}}

        private TcpListener tcpListener;
        private bool running;

        public void SetupSocket(string address, int port)
        {

            if (string.IsNullOrEmpty(address))
            {
                this.tcpListener = new TcpListener(IPAddress.Any, port);
            }
            else
            {
                IPAddress localAddr = IPAddress.Parse(address);
                this.tcpListener = new TcpListener(localAddr, port);
            }
            this.tcpListener.Start();
            this.running = true;

            Thread thread = new Thread(ListenLoop);
            thread.Start();
        }

        void ListenLoop()
        {
            while (this.running)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();

                MuninConnection connection = new MuninConnection(client);
                connection.Start();
            }
            this.tcpListener.Stop();
            this.tcpListener = null;
        }

        public void StopSocket()
        {
            this.running = false;
        }
    }
}
