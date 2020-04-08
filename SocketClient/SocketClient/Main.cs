using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketClient
{
    public partial class Main : Form
    {
        //创建一个客户端套接字和一个负责监听服务端请求的线程
        Thread threadClient = null;
        Socket socketClient = null;

        public Main()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            //关闭对文本框的非法线程操作检查
            TextBox.CheckForIllegalCrossThreadCalls = false;
            this.button1.Enabled = false;
            this.button1.Visible = false;
            this.textBox2.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //调用ClientSendMessage方法，将文本框中输入的信息发送到服务端
            ClientSendMsg(this.textBox2.Text.Trim());
            this.textBox2.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.button2.Enabled = false;
            //定义一个套接字监听
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //获取文本框中的IP地址
            IPAddress address = IPAddress.Parse("192.168.1.112");//192.168.1.112
            //将获取的Ip地址和端口绑定在网络节点上
            IPEndPoint point = new IPEndPoint(address, 12200);

            try
            {
                //客户端套接字连接到网络节点上，用的是Connect
                socketClient.Connect(point);
                this.button1.Enabled = true;
                this.button1.Visible = true;
                this.textBox2.Visible = true;
                this.textBox1.AppendText("连接成功！IP:" + address.ToString() + "\r\n");

            }
            catch (Exception ex)
            {
                Debug.WriteLine("连接失败\r\n" + "失败原因：" + ex.Message);
                this.textBox1.AppendText("连接失败\r\n" + "失败原因：" + ex.Message);

                return;
            }
            threadClient = new Thread(recv);
            threadClient.IsBackground = true;
            threadClient.Start();

        }
        //接收服务端发来的消息
        void recv()
        {
            int x = 0;
            //持续监听服务端发来的消息
            while (true)
            {
                try
                {
                    //定义一个1M的内存缓冲区，用于临时存储接收到的消息
                    byte[] arrRecvmsg = new byte[1024 * 1024 * 500];
                    //将客户端套接字接收到的数据存入到内存缓冲区中，并获取长度
                    int length = socketClient.Receive(arrRecvmsg);
                    //将套接字获取到的字符数组转换为可以看懂的字符串
                    string strRevMsg = Encoding.UTF8.GetString(arrRecvmsg, 0, length);
                    this.textBox1.AppendText("获取信息为：" + strRevMsg + "\r\n");
                    if (x == 1)
                    {
                        this.textBox1.AppendText("服务器：" + DateTime.Now.ToString() + "\r\n" + strRevMsg + "\r\n\n");
                        Debug.WriteLine("服务器：" + DateTime.Now.ToString() + "\r\n" + strRevMsg + "\r\n\n");
                    }
                    else
                    {
                        this.textBox1.AppendText( strRevMsg + "\r\n");
                        Debug.WriteLine(strRevMsg + "\r\n");
                        x = 1;
                    }

                }
                catch (Exception ex)
                {
                    this.textBox1.AppendText("远程服务器已经中断连接！" + "\r\n\n" + "原因：" + ex.Message + "\r\n\n");
                    Debug.WriteLine("远程服务器已经中断连接！" + "\r\n\n" + "原因：" + ex.Message + "\r\n\n");
                    break;
                }
            }
        }
        //发送字符信息到服务端
        void ClientSendMsg(string sendMsg)
        {
            try
            {
                //将输入的内容字符串转换为机器可以识别的字节数组
                byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
                //调用客户端套接字发送字节数组
                socketClient.Send(arrClientSendMsg);
                //将发送的信息追加到聊天内容文本框中
                Debug.WriteLine("Hello……" + ":" + DateTime.Now.ToString() + "\r\n" + sendMsg + "\r\n\n");
                this.textBox1.AppendText("Hello……" + ":" + DateTime.Now.ToString() + "\r\n" + sendMsg + "\r\n\n");


            }
            catch (Exception ex)
            {
                Debug.WriteLine("远程服务器已经中断连接" + "\r\n\n");
                Debug.WriteLine("远程服务器已经中断连接" + "\r\n\n");
            }
        }
    }
}
