using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using S2.Munin.Plugin;

namespace S2.Munin.Service
{
    public class Listener
    {
        private static Listener instance = new Listener();
        public static Listener Instance { get { return instance; } }

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
            // set SO_REUSEADDR on socket
            this.tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            // start listener
            this.tcpListener.Start();
            this.running = true;

            Thread thread = new Thread(ListenLoop);
            thread.Start();
        }

        void ListenLoop()
        {
            while (this.running)
            {
                try
                {
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    MuninConnection connection = new MuninConnection(client);
                    connection.Start();
                }
                catch (Exception e)
                {
                    Logger.Error("Connection Error", e);
                }
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
