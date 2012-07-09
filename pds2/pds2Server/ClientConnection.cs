using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using pds2.Shared.Messages;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using pds2.Shared;
using System.Threading;
using System.Collections;

namespace pds2.ServerSide
{
    class ClientConnectionFail : Exception
    {
        public ClientConnectionFail(string msg)
            : base(msg)
        {

        }
    }
    class ClientConnection
    {
        private TcpClient _textTcp;
        private TcpClient _videoTcp;
        private TcpClient _clipTcp;
        private Thread _textReceiver;
        private Thread _clipReceiver;
        private Server _father;
        private string _username;
        public string Username { get { return _username; } }
     

        internal ClientConnection(Server father,TcpClient textTcp, String password,ArrayList name)
        {
           
            this._father = father;
            this._textTcp = textTcp;
            ChallengeMessage auth = new ChallengeMessage();
            auth.salt = new Random().Next().ToString();
            auth.sendMe(textTcp.GetStream()); //mando il sale
            ResponseChallengeMessage respo = ResponseChallengeMessage.recvMe(textTcp.GetStream());
            
            if (name.Contains(respo.username))
            {
                //connessione fallita
                ConfigurationMessage fail = new ConfigurationMessage("Nome utente già utilizzato");
                fail.sendMe(textTcp.GetStream());
                throw new ClientConnectionFail("Un client ha fornito un nome utente già utilizzato");
            }
            this._username = respo.username;
            if (Pds2Util.createPswMD5(password, auth.salt).Equals(respo.pswMd5))
            {
                //creo le connessioni per i socket clipboard e video
                IPAddress localadd = ((IPEndPoint)textTcp.Client.LocalEndPoint).Address;
                TcpListener lvideo = new TcpListener(localadd, 0);
                lvideo.Start();
                IAsyncResult videores = lvideo.BeginAcceptTcpClient(null, null);
                TcpListener lclip = new TcpListener(localadd, 0);
                lclip.Start();
                IAsyncResult clipres = lclip.BeginAcceptTcpClient(null, null);
                int porta_video = ((IPEndPoint)lvideo.LocalEndpoint).Port;
                int clip_video = ((IPEndPoint)lclip.LocalEndpoint).Port;
                new ConfigurationMessage(porta_video, clip_video, "Benvenuto")
                    .sendMe(textTcp.GetStream());
                _clipTcp = lclip.EndAcceptTcpClient(clipres);
                _videoTcp = lvideo.EndAcceptTcpClient(videores);
                //now the client is connected
                 _textReceiver=new Thread(_receiveText);
                 _textReceiver.IsBackground = true;
                 _textReceiver.Start();
                _clipReceiver=new Thread(_receiveClipboard);
                _clipReceiver.IsBackground = true;
                _clipReceiver.Start();
            }
            else
            {
                //connessione fallita
                ConfigurationMessage fail = new ConfigurationMessage("password sbagliata");
                fail.sendMe(textTcp.GetStream());
                throw new ClientConnectionFail("Un client ha fornito una password sbagliata");
            }
        }

        private volatile bool _connect = true;
        private IOException _disconnectReason;
        internal void Disconnect()
        {
            try
            {
                _connect = false;
                try
                {
                    new TextMessage(MessageType.DISCONNECT, "pool Disconnect",
                        "Il pool ha interrotto la comunicazione").sendMe(_textTcp.GetStream());
                }
                catch (Exception) { }
                _textReceiver.Abort();
                _clipReceiver.Abort();
            }
            finally
            {

                _videoTcp.Close();
                _clipTcp.Close();
                _textTcp.Close();
            }
        }
        internal void sendVideo(ImageMessage msg)
        {
            msg.sendMe(_videoTcp.GetStream());
        }
        internal void send(object msg)
        {
            if (msg is TextMessage)
            {
                sendChat((TextMessage)msg);
            }else  if (msg is ImageMessage)
            {
                    sendVideo((ImageMessage)msg);
            }
            else if (msg is ClipboardMessage)
            {
                sendClipboard((ClipboardMessage)msg);
            }
            else{
                throw new ArgumentException("Unknown message type");
            }

        }
        internal void sendAsync(object msg, AsyncCallback callback, Object state)
        {
            if (msg is TextMessage)
            {
                sendChatAsync((TextMessage)msg, callback, state);
            }
            else if (msg is ImageMessage)
            {
                sendVideoAsync((ImageMessage)msg, callback, state);
            }
            else if (msg is ClipboardMessage)
            {
                sendClipboardAsync((ClipboardMessage)msg, callback, state);
            }
            else
            {
                throw new ArgumentException("Unknown message type");
            }

        }
        internal void sendChat(TextMessage msg)
        {
            msg.sendMe(_textTcp.GetStream());
        }
        internal void sendClipboard(ClipboardMessage msg)
        {
            if (!(msg.username != null && msg.username.Equals(_username)))
            msg.sendMe(_clipTcp.GetStream());
        }
        internal void sendVideoAsync(ImageMessage msg, AsyncCallback callback, Object state)
        {
            msg.sendMeAsync(_videoTcp.GetStream(), callback, state);
        }
        internal void sendChatAsync(TextMessage msg, AsyncCallback callback, Object state)
        {
            msg.sendMeAsync(_textTcp.GetStream(), callback, state);
        }
        internal void sendClipboardAsync(ClipboardMessage msg, AsyncCallback callback, Object state)
        {
            if (!(msg.username != null && msg.username.Equals(_username)))
            msg.sendMeAsync(_clipTcp.GetStream(), callback, state);
        }

        private void _receiveText()
        {
           
            try
            {
                while (_connect)
                {
                    TextMessage msg = TextMessage.recvMe(_textTcp.GetStream());
                    _father.DispatchMsg(msg);     
                }
            }
            catch (IOException e)
            {
                _disconnectReason = e;
                return;
            }
            finally
            {
                _textTcp.Close();
                if(_connect)
                Disconnect();
            }
        }
        private void _receiveClipboard()
        {
            try
            {
                while (_connect)
                {
                    ClipboardMessage msg = ClipboardMessage.recvMe(_clipTcp.GetStream());
                    msg.username = _username;
                    _father.DispatchClipboard(msg);
                }
            }
            catch (IOException e)
            {
                _disconnectReason = e;
                return;
            }
            finally
            {
                _clipTcp.Close();
                if(_connect)
                    Disconnect();
            }
        }
    }
}
