using System;
using System.Threading;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace JTS_Final_Server
{
    class Program
    {
        static int ESPPostListenPort = 2333;
        static int ESPGetListenPort = 2334;
        static int setListenPort = 2335;
        static int getListenPort = 2336;
        static Dictionary<string, jsonData> boxes = new Dictionary<string, jsonData>();
        static targetJsonData deviceTarget;
        static void Main(string[] args)
        {
            TcpListener ESPPostListener = new TcpListener(IPAddress.Any, ESPPostListenPort);
            TcpListener ESPGetListener = new TcpListener(IPAddress.Any, ESPGetListenPort);
            TcpListener SetListener = new TcpListener(IPAddress.Any, setListenPort);
            TcpListener GetListener = new TcpListener(IPAddress.Any, getListenPort);
            ESPPostListener.Start();
            ESPGetListener.Start();
            SetListener.Start();
            GetListener.Start();
            Thread ESPPostListen = new Thread(() => {
                while (true)
                {
                    TcpClient ESPPostClient = ESPPostListener.AcceptTcpClient();
                    Thread handleDataThread = new Thread(new ParameterizedThreadStart(receiveData));
                    handleDataThread.Start(ESPPostClient);
                    Thread.Sleep(100);
                }
            });
            ESPPostListen.Start();
            Thread ESPGetListen = new Thread(() => {
                while (true)
                {
                    TcpClient ESPGetClient = ESPGetListener.AcceptTcpClient();
                    Thread sendDataThread = new Thread(new ParameterizedThreadStart(sendESPData));
                    sendDataThread.Start(ESPGetClient);
                    Thread.Sleep(100);
                }
            });
            ESPGetListen.Start();
            Thread SetClientListen = new Thread(() => {
                while (true)
                {
                    TcpClient SetClient = SetListener.AcceptTcpClient();
                    Thread setDataThread = new Thread(new ParameterizedThreadStart(setData));
                    setDataThread.Start(SetClient);
                    Thread.Sleep(100);
                }
            });
            SetClientListen.Start();
            Thread GetClientListen = new Thread(() => {
                while (true)
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
                boxesData temp = new boxesData();
                temp.boxes = boxes;
                string data = JsonSerializer.Serialize<boxesData>(temp);
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
            //Debug.WriteLine(JsonSerializer.Deserialize<jsonData>(data).boxNum);
            jsonData deviceStatus=new jsonData();
            if (data != "")
            {
                deviceStatus = JsonSerializer.Deserialize<jsonData>(data);
                deviceStatus.startTime = DateTime.Now;
                boxes[deviceStatus.boxNum] = deviceStatus;
            }
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
            string data = sr.ReadToEnd();
            if (data != "") deviceTarget = JsonSerializer.Deserialize<targetJsonData>(data);
            Console.WriteLine($"set:{data}");
            ns.Dispose();
            tcpClient.Close();
            tcpClient.Dispose();
        }

    }
    public class jsonData
    {
        public string boxNum { get; set; }
        public string status { get; set; }
        public int accessCode { get; set; }
        public DateTime startTime{ get; set; }
    }
    public class targetJsonData
    {

    }
    public class boxesData
    {
        public Dictionary<string, jsonData> boxes { get; set; }
    }
}
