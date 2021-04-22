using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPServer
{
    public partial class Form1 : Form
    {
        private Socket _socket;
        private string _ip = "127.0.0.1";
        private int _port = 9000;
        private byte[] receiveData = new byte[1024];
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IPAddress ip = IPAddress.Parse(_ip);
            IPEndPoint edp = new IPEndPoint(ip, _port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(edp);
            _socket.Listen(5);
            _socket.BeginAccept(AcceptCallBack, _socket);

        }
        private void AcceptCallBack(IAsyncResult ar)
        {
            Console.WriteLine("连接成功>>>{0}",ar.AsyncState);
            Socket sever1 = (Socket)ar.AsyncState;
            Socket client = sever1.EndAccept(ar);
            byte[] sendData = Encoding.Default.GetBytes("Hello client");
            client.Send(sendData);
            sever1.BeginAccept(new AsyncCallback(AcceptCallBack), sever1);//等待其他客户端连接
            //client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 100); 
            try
            {
                client.BeginReceive(receiveData, 0, receiveData.Length, SocketFlags.None, ReceiveCallBack, client);
            }
            catch (Exception e)
            {
                AddMsg(string.Format("[ERROR]>>>{0}", e.Message));
            }
        }
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                Socket client = ar.AsyncState as Socket;
                int cnt = client.EndReceive(ar);
                if (cnt > 0)
                {
                    string str = Encoding.ASCII.GetString(receiveData, 0, cnt);
                    string ip = client.RemoteEndPoint.ToString();
                    AddMsg(string.Format("ip[{0}]>>> {1}", ip, str));
                    string msg = string.Format("sever get msg ->{0}", str);
                    byte[] msgData = Encoding.Default.GetBytes(msg);
                    client.Send(msgData);
                    client.BeginReceive(receiveData, 0, receiveData.Length, SocketFlags.None, ReceiveCallBack, client);
                }
                else AddMsg(string.Format("[MSG]>>>ip[{0}] disconnected...",client.RemoteEndPoint));
            }
            catch (Exception e)
            {
                AddMsg(string.Format("[ERROR]>>>{0}", e.Message));
            }
        }
        private void AddMsg(string msg)
        {
            if (lbMsg.InvokeRequired)
            {
                // 当一个控件的InvokeRequired属性值为真时，说明有一个创建它以外的线程想访问它
                Action<string> actionDelegate = (x) => { lbMsg.Items.Add(x); };
                // 或者
                // Action<string> actionDelegate = delegate(string txt) { this.label2.Text = txt; };
                this.lbMsg.Invoke(actionDelegate, msg);
            }
            else lbMsg.Items.Add(msg);
        }
        private void UIDelegate(Delegate handle,object[] paras)
        {
            BeginInvoke(handle, paras);
        }
    }
}
