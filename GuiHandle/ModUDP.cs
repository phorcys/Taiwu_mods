using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace GuiHandle
{
    public class ModUDP : MonoBehaviour
    {
        public static ModUDP instance;
        public static List<Socket> sockets = new List<Socket>();
        private void Awake()
        {
            instance = this;
        }

        public static string message;
        private void Start()
        {
            Thread thread = new Thread(StartUDP);
            thread.Start();
        }

        private void StartUDP()
        {
            ModReceivePage modReceivePage = new ModReceivePage();
            modReceivePage.OnServerInitialized();
        }

        public void MyUpdate()
        {
            Main.Logger.Log(message);
        }

        private void OnDestroy()
        {
            foreach (var item in sockets)
            {
                item.Close();
            }
        }
    }




    // 接收操作数据
    public class ModReceivePage
    {
        public static ModReceivePage instance;

        public static float left_x;
        public static float left_y;
        public static float right_x;
        public static float right_y;
        public static short boolvalue;
        public static byte num;

        public ModReceivePage()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
        }

        public void OnServerInitialized()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ModUDP.sockets.Add(sock);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 9158);
            sock.Bind(iep);
            while (true)
            {
                EndPoint ep = (EndPoint)iep;
                byte[] data = new byte[11];
                int recv = sock.ReceiveFrom(data, ref ep);

                int idx = 0;
                byte num = data[idx++];
                if ((num < 75 && ModReceivePage.num > 150) || ModReceivePage.num < num)
                {
                    ModReceivePage.num = num;
                    short leftx = BitConverter.ToInt16(data, 1);
                    short lefty = BitConverter.ToInt16(data, 3);
                    short rightx = BitConverter.ToInt16(data, 5);
                    short righty = BitConverter.ToInt16(data, 7);
                    boolvalue = BitConverter.ToInt16(data, 9);
                    left_x = (float)leftx / 10000;
                    left_y = (float)lefty / 10000;
                    right_x = (float)rightx / 10000;
                    right_y = (float)righty / 10000;


                    ModUDP.message = data[0] + "\t" + data[1] + "\t" + data[2] + "\t" + data[3] + "\t" + data[4] + "\t" + data[5] + "\t" + data[6] + "\t" + data[7] + "\t" + data[8] + "\t" + data[9] + "\t" + data[10];
                }
                Thread.Sleep(0);
            }
        }
    }






    // 广播IP地址
    public class ModPublicizeIp
    {
        private Socket sock;
        private IPEndPoint iep1;
        private byte[] data;
        public void Start()
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ModUDP.sockets.Add(sock);
            //255.255.255.255
            iep1 = new IPEndPoint(IPAddress.Broadcast, 9159);
            string hostname = Dns.GetHostName();
            data = Encoding.ASCII.GetBytes(hostname);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            Thread t = new Thread(BroadcastMessage);
            t.Start();
        }
        private void BroadcastMessage()
        {
            while (true)
            {
                sock.SendTo(data, iep1);
                Thread.Sleep(2000);
            }
        }

        public void Close()
        {
            sock.Close();
        }
    }
}