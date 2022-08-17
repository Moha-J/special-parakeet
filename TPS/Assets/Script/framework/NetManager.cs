using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class NetManager
{

    static Socket socket;
    static bool isConnecting;
    static bool isClosing;
    static ByteArray readBuff;
    static Queue<ByteArray> writeQueue;
    static List<MsgBase> msgList = new List<MsgBase>();
    //消息列表长度
    static int msgCount = 0;
    //每一次Update处理的消息量
    readonly static int MAX_MESSAGE_FIRE = 10;

    public delegate void MsgListener(MsgBase msgBase);
    //消息监听列表
    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();
    //添加消息监听
    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        //添加
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        //新增
        else
        {
            msgListeners[msgName] = listener;
        }
    }
    //删除消息监听
    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
        }
    }
    //分发消息
    private static void FireMsg(string msgName, MsgBase msgBase)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }

    public static void Update()
    {
        MsgUpdate();
    }
    public static void MsgUpdate()
    {
        //初步判断，提升效率
        if (msgCount == 0)
        {
            return;
        }
        //重复处理消息
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            //获取第一条消息
            MsgBase msgBase = null;
            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            //分发消息
            if (msgBase != null)
            {
                FireMsg(msgBase.protoName, msgBase);
            }
            //没有消息了
            else
            {
                break;
            }
        }
    }
    public static void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
        {
            return;
        }
        //是否已连接
        if (isConnecting)
        {
            return;
        }
        readBuff = new ByteArray();
        //写入队列
        writeQueue = new Queue<ByteArray>();
        //是否正在连接
        isConnecting = false;
        //是否正在关闭
        isClosing = false;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(ip,port, ConnectCallback,socket);
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {

            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);

            isConnecting = false;
            //开始接收
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx,
                                            readBuff.remain, 0, ReceiveCallback, socket);
            Test.Login();
        }
        catch (SocketException ex)
        {
            isConnecting = false;
        }
    }
    //发送
    public static void Send(MsgBase msg)
    {
        //状态判断
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if (isClosing)
        {
            return;
        }
        //数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        //组装长度
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        //组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        //组装消息体
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        //写入队列
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;  //writeQueue的长度
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        //send
        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length,
                0, SendCallback, socket);
        }
    }

    //Send回调
    public static void SendCallback(IAsyncResult ar)
    {

        //获取state、EndSend的处理
        Socket socket = (Socket)ar.AsyncState;
        //状态判断
        if (socket == null || !socket.Connected)
        {
            return;
        }
        //EndSend
        int count = socket.EndSend(ar);
        //获取写入队列第一条数据            
        ByteArray ba;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }
        //完整发送
        ba.readIdx += count;
        if (ba.length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                ba = writeQueue.First();
            }
        }
        //继续发送
        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.length,
                0, SendCallback, socket);
        }
        //正在关闭
        else if (isClosing)
        {
            socket.Close();
        }
    }
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            //获取接收数据长度
            int count = socket.EndReceive(ar);
            readBuff.writeIdx += count;
            //处理二进制消息
            OnReceiveData();
            //继续接收数据
            if (readBuff.remain < 8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.length * 2);
            }
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx,
                    readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }

    public static void OnReceiveData()
    {
        
        //消息长度
        if (readBuff.length <= 2)
        {
            return;
        }
        //获取消息体长度
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (readBuff.length < bodyLength)
            return;
        readBuff.readIdx += 2;
        //解析协议名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
        if (protoName == "")
        {
            return;
        }
        readBuff.readIdx += nameCount;
        //解析协议体
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        //添加到消息队列
        lock (msgList)
        {
            msgList.Add(msgBase);
            msgCount++;
        }
        //继续读取消息
        if (readBuff.length > 2)
        {
            OnReceiveData();
        }
    }
}

