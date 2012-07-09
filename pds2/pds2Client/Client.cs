using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Drawing;
using System.Net.Sockets;
using pds2.Shared.Messages;
using pds2.Shared;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows;

namespace pds2.ClientSide
{

    public class Client : IConnection
    {

        public override event ConnectioState connectionStateEvent;

        public override event TextMessageDelegate receivedMessage;
       
        private volatile BlockingCollection<TextMessage> msgTosend;       
        private Thread chatSenderThread, chatReceiveThread, clipboardThread, videoThread;
        private string  pass, srip;
        private int srport;       
        public Client(IMainWindow mcw):base(mcw)
        {
            mcw.sendMessage += this.sendMessage;
            mcw.shareClipboard += _sendClipboard;
        }
        public void configure(string username, string pass, string srip, int port)
        {
            if (IsConnect)
                throw new ArgumentException("Impossibile modificare le impostazioni mentre si è connessi");
            this._username = username;
            this.pass = pass;
            this.srip = srip;
            this.srport = port;
            _configured = true;
        }
        private TcpClient _textSocket;
        private TcpClient _clipSocket;
        private TcpClient _videoSocket;
        private volatile Exception _disconnectReason;
        public override void Connect()
        {
            try
            {
                _connect = true;
                _textSocket = new TcpClient(srip, srport);

                //inizia l'autenticazione
                //ricevo il sale
                ChallengeMessage sale = ChallengeMessage.recvMe(_textSocket.GetStream());
                ResponseChallengeMessage resp = new ResponseChallengeMessage();
                resp.username = _username;
                resp.pswMd5 = Pds2Util.createPswMD5(this.pass, sale.salt);
                resp.sendMe(_textSocket.GetStream());
                ConfigurationMessage conf = ConfigurationMessage.recvMe(_textSocket.GetStream());
                if (!conf.success)
                    throw new ArgumentException(conf.message);

                _clipSocket = new TcpClient(srip, conf.clip_port);
                _videoSocket = new TcpClient(srip, conf.video_port);
               
            }
            catch (Exception e)
            {

                connectionStateEvent(false);
                _connect = false;
                throw new ArgumentException("Impossibile connettersi al server. \n" + e.Message);
            }
            msgTosend = new BlockingCollection<TextMessage>();
            chatSenderThread = new Thread(this._deleverMsg);
            chatReceiveThread = new Thread(_receiveMsg);
            chatReceiveThread.Start();
            chatSenderThread.Start();
          //  chatSenderThread.IsBackground = true;
         //   chatReceiveThread.IsBackground = true;
            clipboardThread = new
                Thread(_listenClipboard);
            clipboardThread.Start();
            videoThread = new Thread(_receiveVideo);
           // videoThread.IsBackground = true;
            videoThread.Start();

            if (connectionStateEvent != null)
                connectionStateEvent(true);

        }
        public override void Disconnect()
        {
            if (!_connect)
                throw new ArgumentException("Il server non è connesso");
            TextMessage leav = new TextMessage();
            leav.messageType = MessageType.USER_LEAVE;
            leav.username = _username;
              if (_disconnectReason != null)
            {
                leav.message = _disconnectReason.Message;
                ReceivedMessage(leav); //scrivo nella chat del client che mi disconnetto
            }
            leav.message = "bye bye";
            try
            {
                sendMessage(leav);
            }
            catch (Exception) { }
            _connect = false;
            mcw.sendMessage -= this.sendMessage;
            msgTosend.Dispose();
            chatSenderThread.Abort();
            clipboardThread.Abort();
            chatReceiveThread.Abort();

            if (connectionStateEvent != null)
                connectionStateEvent(false);
        }
        private void sendMessage(String msg)
        {
            TextMessage m = new TextMessage();
            m.message = msg;
            m.username = _username;
            m.messageType = MessageType.TEXT;
            sendMessage(m);
        }
        private void sendMessage(TextMessage msg)
        {
            if (!_connect)
                throw new ArgumentException("ClientSide non connesso");
            if (msg == null)
                throw new ArgumentException("Impossibile inviare messaggi nulli");
            msgTosend.Add(msg);
        }
        private void _deleverMsg()
        {

            try
            {

                while (_connect)
                {
                    TextMessage msg = msgTosend.Take();
                    msg.sendMe(_textSocket.GetStream());
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                _disconnectReason = e;
                return;
            }
            finally
            {
                _textSocket.Close();
                if (_connect)
                {
                    Disconnect();
                }
            }
        }

        private void _receiveMsg()
        {

            try
            {
                NetworkStream ns = _textSocket.GetStream();

                while (_connect)
                {
                    TextMessage msg = TextMessage.recvMe(ns);
                    ReceivedMessage(msg);
                    switch (msg.messageType)
                    {
                        case MessageType.ADMIN:
                             
                            break;
                        case MessageType.TEXT:
                           
                            break;
                        case MessageType.USER_JOIN:
                             
                            break;
                        case MessageType.USER_LEAVE:
                            //TODO remove the user
                            break;
                        case MessageType.DISCONNECT:
                           //TODO disconnect
                            Disconnect();
                            return;
                    }

                }
            }
            catch (Exception e )
            {
                    _disconnectReason = e;
                    return;
            } finally
            {
                if(_connect)
                    Disconnect();
                _textSocket.Close();

            }

        }

        private void _sendClipboard(ClipboardMessage ms)
        {


             ms.sendMe(_clipSocket.GetStream());
                     
              
        }
        private void _listenClipboard()
        {
            try
            {
                NetworkStream ns = _clipSocket.GetStream();

                while (_connect)
                {
                    ClipboardMessage msg = ClipboardMessage.recvMe(ns);
                    if (receivedClipboard != null)
                        receivedClipboard(msg);
                }
            }
            catch (Exception e)
            {
                _disconnectReason = e;
                return;
            }
            finally
            {
                if (_connect)
                    Disconnect();
                _textSocket.Close();

            }
        }
        private void _receiveVideo()
        {
            try
            {
                NetworkStream ns = _videoSocket.GetStream();

                while (_connect)
                {
                    ImageMessage msg = ImageMessage.recvMe(ns);
                    if(receivedVideo!=null)
                        receivedVideo(msg);
                }
            }
            catch (Exception e)
            {
                _disconnectReason = e;
                return;
            }
            finally
            {
                if (_connect)
                    Disconnect();
                _textSocket.Close();

            }
        }
        private void ReceivedMessage(TextMessage msg)
        {
            if (receivedMessage != null)
                receivedMessage(msg);
        }




        public override event ImageMessageDelegate receivedVideo;

        public override event ClipboardMessageDelegate receivedClipboard;

        //send clipboard

        


    }
}
