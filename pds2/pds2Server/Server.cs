using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using pds2.Shared.Messages;
using System.IO;
using pds2.Shared;

namespace pds2.ServerSide
{
    public class Server : IConnection
    {
        private volatile ArrayList clients = ArrayList.Synchronized(
            new ArrayList());
        private BlockingCollection<TextMessage> msgQueue = new BlockingCollection<TextMessage>();
        private BlockingCollection<ImageMessage> videoQueue = new BlockingCollection<ImageMessage>();
        private BlockingCollection<ClipboardMessage> clipQueue = new BlockingCollection<ClipboardMessage>();

        private Thread _accepterConn;
        private Thread _videoDispatcher;
        private Thread _chatDispatcher;
        private Thread _clipboardDispatcher;
        private int _listenPort = 2626;

        private IPAddress _localend = IPAddress.Parse("127.0.0.1");
        private string _password = "password";
        private IOException _disconnectReason;

        public int ListenPort
        {
            get
            {
                return _listenPort;
            }
            set
            {
                if (_connect)
                    throw new ArgumentException("It is not allowed to change settings while the pool is running");
                _listenPort = value;
            }
        }
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (_connect)
                    throw new ArgumentException("It is not allowed to change settings while the pool is running");
                _password = value;
            }
        }
        public IPAddress Localend
        {
            get
            {
                return _localend;
            }
            set
            {
                if (_connect)
                    throw new ArgumentException("It is not allowed to change settings while the pool is running");
                _localend = value;
            }
        }
        private WorkerPool _wp;
        public void Stream()
        {
            if (_wp.IsConnect)
               _wp.RequestStop();
            else
                _wp.RequestStart();            
        }
        public Server(IMainWindow mwc)
            : base(mwc)
        {
            mwc.sendMessage += this.DispatchMsg;
            _wp = new WorkerPool(this, videoQueue);
            mwc.shareVideo += Stream;
            mcw.shareClipboard += DispatchClipboard;
            _username = "Server";
        }
        public override void Connect()
        {
            if (_connect)
                throw new ArgumentException("The pool is already connected");
            lock (this)
            {
                _connect = true;
                _accepterConn = new Thread(_receiveConnection);
                _accepterConn.Start();
                _chatDispatcher = new Thread(_dispatchChat);
                _chatDispatcher.Start();
                _clipboardDispatcher = new Thread(_dispatchClipboard);
                _clipboardDispatcher.Start();
                _chatDispatcher = new Thread(_dispatchTextMsg);
                _chatDispatcher.Start();
                _videoDispatcher = new Thread(_dispatchVideo);
                _videoDispatcher.Start();
                if (connectionStateEvent != null)
                    connectionStateEvent(true);
            }

        }
        public void DispatchMsg(TextMessage msg)
        {
            msgQueue.Add(msg);
        }
        public void DispatchMsg(String msg)
        {
            TextMessage
                 msg2 = new TextMessage();
            msg2.messageType = MessageType.TEXT;
            msg2.message = msg;
            msg2.username = _username;
            msgQueue.Add(msg2);
        }
        public override void Disconnect()
        {
            if (!_connect)
                throw new ArgumentException("The pool is not connected");
            lock (this)
            {
                if (connectionStateEvent != null)
                    connectionStateEvent(false);
                //shutdown all threads
                _connect = false;
                killThread(_accepterConn);
                killThread(_chatDispatcher);
                killThread(_clipboardDispatcher);
                killThread(_chatDispatcher);
                killThread(_videoDispatcher);
                if (listen != null)
                    listen.Stop();
                //reset state
                foreach (ClientConnection client in (ArrayList)clients.Clone())
                {
                    client.Disconnect();
                    clients.Remove(client);
                }


            }


        }
        private void killThread(Thread t)
        {
            try
            {
                t.Abort();
            }
            catch (Exception) { }
        }
        private void _dispatchTextMsg()
        {

            while (_connect)
            {
                try
                {
                    TextMessage msg = msgQueue.Take();
                    ReceivedMessage(msg);
                    foreach (ClientConnection cli in clients)
                    {
                        cli.sendChat(msg);

                    }
                }
                catch (ObjectDisposedException)
                {
                    return; //the queue has been closed
                }
            }
        }
        private void _dispatchVideo()
        {
            while (_connect)
            {
                try
                {
                    ImageMessage msg = videoQueue.Take();
                    foreach (ClientConnection cli in clients)
                    {
                        cli.sendVideo(msg);
                    }
                }
                catch (ObjectDisposedException)
                {
                    return; //the queue has been closed
                }
            }
        }
        private void _dispatchChat()
        {
            while (_connect)
            {
                try
                {
                    TextMessage msg = msgQueue.Take();
                    ReceivedMessage(msg);
                    if (msg.messageType.Equals(MessageType.USER_LEAVE))
                    {
                        foreach (ClientConnection cli in (ArrayList)clients.Clone())
                            if (cli.Username.Equals(msg.username))
                            {
                                clients.Remove(cli);
                            }
                    }
                    foreach (ClientConnection cli in (ArrayList)clients.Clone())
                    {

                        try
                        {
                            cli.sendChat(msg);
                        }
                        catch (Exception)
                        {

                            try
                            {
                                TextMessage l = new TextMessage();
                                l.messageType = MessageType.USER_LEAVE;
                                l.username = cli.Username;
                                DispatchMsg(l);
                                clients.Remove(cli);
                                cli.Disconnect();
                            }
                            catch (Exception) { }
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    return; //the queue has been closed
                }
            }
        }
        private void _dispatchClipboard()
        {
            while (_connect)
            {
                try
                {
                    ClipboardMessage msg = clipQueue.Take();
                    if (!msg.username.Equals(_username))
                        receivedClipboard(msg);
                    foreach (ClientConnection cli in clients)
                    {
                        cli.sendClipboard(msg);
                    }
                }
                catch (ObjectDisposedException)
                {
                    return; //the queue has been closed
                }
            }
        }
        TcpListener listen = null;
        private void _receiveConnection()
        {


            try
            {
                listen = new TcpListener(_localend, _listenPort);
                listen.Start();
                while (_connect)
                {

                    TcpClient ncli = listen.AcceptTcpClient();
                    ArrayList name = new ArrayList();
                    foreach (ClientConnection cli in clients)
                        name.Add(cli.Username);
                    ClientConnection c = new ClientConnection(this, ncli, _password, name);
                    clients.Add(c);
                    TextMessage ms = new TextMessage();
                    ms.messageType = MessageType.USER_JOIN;
                    ms.username = c.Username;
                    ms.message = "Nuovo utente connesso";
                    DispatchMsg(ms);
                }
            }
            catch (ClientConnectionFail ae)
            {
                TextMessage ms = new TextMessage();
                ms.messageType = MessageType.ADMIN;
                ms.message = ae.Message;
                DispatchMsg(ms);
            }
            catch (IOException e)
            {
                _disconnectReason = e;
                return;
            }
            catch (Exception es)
            {
                if (_connect)
                    throw es;
                return;
            }
            finally
            {
                if (listen != null)
                {
                    listen.Stop();
                    listen = null;

                }
                if (_connect)
                    Disconnect();
            }

        }



        public void DispatchClipboard(ClipboardMessage msg)
        {

            clipQueue.Add(msg);
        }
        public void DispatchVideo(ImageMessage msg)
        {
            videoQueue.Add(msg);
        }

        public override event ConnectioState connectionStateEvent;
        public override event TextMessageDelegate receivedMessage;
        private void ReceivedMessage(TextMessage msg)
        {
            if (receivedMessage != null)
                receivedMessage(msg);
        }

        public override event ImageMessageDelegate receivedVideo;

        public override event ClipboardMessageDelegate receivedClipboard;



        
    }
}
