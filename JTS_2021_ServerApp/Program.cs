using System;
using System.Threading;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace JTS_2021_ServerApp
{
    class Program
    {
        static int ESPPostListenPort=2333;
        static int ESPGetListenPort = 2334;
        static int setListenPort = 2335;
        static int getListenPort = 2336;
        static jsonData deviceStatus;
        static targetJsonData deviceTarget;
        static void Main(string[] args)
        {
            TcpListener ESPPostListener = new TcpListener(IPAddress.Any, ESPPostListenPort);
            TcpListener ESPGetListener = new TcpListener(IPAddress.Any, ESPGetListenPort);
            TcpListener SetListener = new TcpListener(IPAddress.Any, setListenPort);
            TcpListener GetListener = new TcpListener(IPAddress.Any,getListenPort);
            ESPPostListener.Start();
            ESPGetListener.Start();
            SetListener.Start();
            GetListener.Start();
            Thread ESPPostListen = new Thread(()=> {
                while(true)
                {
                    TcpClient ESPPostClient = ESPPostListener.AcceptTcpClient();
                    Thread handleDataThread = new Thread(new ParameterizedThreadStart(receiveData));
                    handleDataThread.Start(ESPPostClient);
                    Thread.Sleep(100);
                }
            });
            ESPPostListen.Start();
            Thread ESPGetListen = new Thread(()=> {
                while(true)
                {
                    TcpClient ESPGetClient = ESPGetListener.AcceptTcpClient();
                    Thread sendDataThread = new Thread(new ParameterizedThreadStart(sendESPData));
                    sendDataThread.Start(ESPGetClient);
                    Thread.Sleep(100);
                }
            });
            ESPGetListen.Start();
            Thread SetClientListen = new Thread(()=> {
                while(true)
                {
                    TcpClient SetClient = SetListener.AcceptTcpClient();
                    Thread setDataThread = new Thread(new ParameterizedThreadStart(setData));
                    setDataThread.Start(SetClient);
                    Thread.Sleep(100);
                }
            });
            SetClientListen.Start();
            Thread GetClientListen = new Thread(()=> { 
                while(true)
                {
                    TcpClient GetClient = GetListener.AcceptTcpClient();
                    Thread sendDeviceDataThread = new Thread(new ParameterizedThreadStart(sendDeviceData));
                    sendDeviceDataThread.Start(GetClient);
                    Thread.Sleep(100);
                }
            });
            GetClientListen.Start();
            while (true)
            {
                Thread.Sleep(100);
            }
        }

        static void sendDeviceData(object obj)
        {
            TcpClient tcpClient = obj as TcpClient;
            NetworkStream ns = tcpClient.GetStream();
            if (ns.CanWrite)
            {
                string data = JsonSerializer.Serialize<jsonData>(deviceStatus);
                Byte[] sendBytes = Encoding.UTF8.GetBytes(data);
                Console.WriteLine($"sent:{data}");
                ns.Write(sendBytes, 0, sendBytes.Length);
            }
            ns.Dispose();
            tcpClient.Close();
            tcpClient.Dispose();
        }
        static void receiveData(object obj)
        {
            TcpClient tcpClient = obj as TcpClient;
            NetworkStream ns = tcpClient.GetStream();
            StreamReader sr = new StreamReader(ns);
            string data = sr.ReadToEnd();
            Console.WriteLine($"received:{data}");
            if (data != "") deviceStatus = JsonSerializer.Deserialize<jsonData>(data);
            Console.WriteLine(deviceStatus.temperature);
            ns.Dispose();
            tcpClient.Close();
            tcpClient.Dispose();
        }

        static void sendESPData(object obj)
        {
            TcpClient tcpClient = obj as TcpClient;
            NetworkStream ns = tcpClient.GetStream();
            if (ns.CanWrite)
            {
                string data = JsonSerializer.Serialize<targetJsonData>(deviceTarget);
                Byte[] sendBytes = Encoding.UTF8.GetBytes(data);
                Console.WriteLine($"sent:{data}");
                ns.Write(sendBytes, 0, sendBytes.Length);
            }
            ns.Dispose();
            tcpClient.Close();
            tcpClient.Dispose();
        }

        static void setData(object obj)
        {
            TcpClient tcpClient = obj as TcpClient;
            NetworkStream ns = tcpClient.GetStream();
            StreamReader sr = new StreamReader(ns);
            string data= sr.ReadToEnd();
            if(data!="") deviceTarget = JsonSerializer.Deserialize<targetJsonData>(data);
            Console.WriteLine($"set:{data}");
            ns.Dispose();
            tcpClient.Close();
            tcpClient.Dispose();
        }
        
    }
    public class jsonData
    {
        public int temperature { get; set; }
        public int humidity { get; set; }
        public int brightness { get; set; }
        public int lightValue { get; set; }
        public int alert { get; set; }
        public int window { get; set; }
        public int sunshade { get; set; }
        public int mode { get; set; }
        public int beep { get; set; }
    }
    public class targetJsonData
    {
        public int setLightValue { get; set; }
        public int setWindow { get; set; }
        public int setSunshade { get; set; }
        public int setMode { get; set; }
        public int setBeep { get; set; }
    }
}
