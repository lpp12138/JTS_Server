using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace JTS_2021_ServerApp
{
    public class UdpClass
    {
        public string UnProcessedData;
        private UdpClient udpClient;
        private IPEndPoint iPEndPoint;
        UdpClass()
        {

        }

        UdpClass(IPEndPoint iPEnd)
        {
            this.iPEndPoint = iPEnd;
            Thread thread = new Thread(Receive);

        }
        public void Receive()
        {
            if(iPEndPoint!=null)
            {
                byte[] rawData = udpClient.Receive(ref iPEndPoint);
            }
        }
        public void Receive(IPEndPoint iPEnd)
        {
            this.iPEndPoint = iPEnd;
            byte[] rawData= udpClient.Receive(ref iPEndPoint);
        }
        public void BroadCastUdpData(string data,int port)
        {
            IPEndPoint BC = new IPEndPoint(IPAddress.Broadcast,port);
            UdpClient UDPsender = new UdpClient();
            byte[] rawData = Encoding.ASCII.GetBytes(data);
            UDPsender.Send(rawData, rawData.Length, BC);
            UDPsender.Dispose();
            UDPsender = null;
        }

        public void SendUdpData(string data,IPEndPoint iPEndPoint)
        {
            UdpClient UDPSender = new UdpClient();
            byte[] rawData = Encoding.ASCII.GetBytes(data);
            UDPSender.Send(rawData,rawData.Length,iPEndPoint);
            UDPSender.Dispose();
            UDPSender = null;
        }
    }
}
