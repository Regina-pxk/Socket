using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketService
{
    class SocketServer
    {
        //和客户端通信的套接字
        static Socket client_socket = null;
        //集合：存储客户端信息
        static Dictionary<string, Socket> clientConnectionItems = new Dictionary<string, Socket> { };
        public static void Main(string[] args)
        {
            //和客户端通信的套接字，监听客户端发来的消息，三个参数：IP4寻找协议，流失连接，TCP协议
            client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //服务端发送信息需要一个IP地址和端口号
            IPAddress address = IPAddress.Parse("127.0.0.1");//传入本机IP地址
            //将IP地址和端口号绑定到网络节点point上
            IPEndPoint point = new IPEndPoint(address, 5000);//5000端口专门用来监听，5000为本机未占用的可用端口，可自定
            //监听绑定的网络节点
            client_socket.Bind(point);

            //将套接字的监听队列长度限制为20
            client_socket.Listen(20);

            //负责监听客户端的线程
            Thread client_thread = new Thread(watchconnecting);
            //将窗体线程设置为与后台同步，跟随主线程结束而结束
            client_thread.IsBackground = true;
            //启动线程
            client_thread.Start();
            Console.WriteLine("开启监听……");
            Console.WriteLine("点击输入任意数据回车退出程序……");
            Console.ReadKey();
            Console.WriteLine("退出监听，并关闭程序");

        }
        //监听客户端发来的请求
        static void watchconnecting()
        {
            Socket connection = null;
            //持续不断监听客户端发来的请求
            while (true)
            {
                try
                {
                    connection = client_socket.Accept();
                }
                catch (Exception ex)
                {
                    //套接字监听异常
                    Console.WriteLine(ex.Message);
                    break;
                }
                //获取客户端的Ip和端口号
                IPAddress clientIp = (connection.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;

                //让客户显示“连接成功”的信息
                string sendMsg = "连接服务端成功！\r\n" + "本地IP：" + clientIp + ",本地端口：" + clientPort.ToString();
                byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendMsg);
                connection.Send(arrSendMsg);

                //客户端网络结点号
                string remoteEndPoint = connection.RemoteEndPoint.ToString();
                //显示与客户端连接情况:
                Console.WriteLine("成功与" + remoteEndPoint + "客户端建立连接！\r\n");
                //添加客户端信息
                clientConnectionItems.Add(remoteEndPoint, connection);
                IPEndPoint netpoint = connection.RemoteEndPoint as IPEndPoint;
                //创建一个线程通信
                ParameterizedThreadStart pts = new ParameterizedThreadStart(recv);
                Thread thread = new Thread(pts);
                //设置后台进程，跟随主线程退出而退出
                thread.IsBackground = true;
                //启动线程
                thread.Start(connection);

            }
        }

        //接收客户端发来的消息，客户端套接字对象
        static void recv(object socketclientpara)
        {
            Socket socketServer = socketclientpara as Socket;
            while (true)
            {
                //创建一个内存缓存区，其大小为1024*1024字节，即1M
                byte[] arrServiceRecMsg = new byte[1024 * 1024];
                try
                {
                    int length = socketServer.Receive(arrServiceRecMsg);
                    //将机器接收的字节数组转换为字符串
                    string strRecMsg = Encoding.UTF8.GetString(arrServiceRecMsg, 0, length);
                    //将发送的字符串信息附加到文本框txtMsg上
                    Console.WriteLine("客户端：" +socketServer.RemoteEndPoint + ",time:" + DateTime.Now.ToString() + "\r\n" + strRecMsg + "\r\n\n");
                    //socketServer.Send(Encoding.UTF8.GetBytes("测试socketServer是否可以发送数据给client"));
                    socketServer.Send(Encoding.UTF8.GetBytes("通过server可以发送数据给client"));

                }
                catch (Exception ex)
                {
                    clientConnectionItems.Remove(socketServer.RemoteEndPoint.ToString());
                    Console.WriteLine("Client Count:" + clientConnectionItems.Count);

                    //提示套接字监听异常
                    Console.WriteLine("客户端" + socketServer.RemoteEndPoint + "已经中断连接" + "\r\n" + ex.Message + "\r\n" + ex.StackTrace +"\r\n");
                    //关闭之前accept出来的和客户端进行通讯的套接字
                    socketServer.Close();
                    break;
                }
            }
        }
    }
}
