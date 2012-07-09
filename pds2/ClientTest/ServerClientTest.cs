using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using pds2.Shared.Messages;
using System.Threading;
using pds2.ServerSide;
using pds2.ClientSide;
using System.Net;
using pds2.Shared;

namespace pds2.Test
{
    [TestClass]
    public class ServerClientTest
    {
        [TestMethod]
        public void Connect()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            test t = new test(ev);
            Server server = new Server(t);
            server.Password = "password";
            server.ListenPort = 2626;
            server.Localend = IPAddress.Parse("127.0.0.1");
            server.Connect();
            
             
            Client client = new Client(t);
            client.configure("tizio", "password", "127.0.0.1", 2626);
            client.Connect();
            TextMessage ms = new TextMessage();
            ms.messageType = MessageType.TEXT;
            ms.message = "prova";
            server.DispatchMsg(ms);
            
            Assert.IsTrue(server.IsConnect);
            Assert.IsTrue(client.IsConnect);
            ev.WaitOne();
            Assert.AreEqual(ms.message, t.recvmsg);
            client.Disconnect();
            server.Disconnect();


        }
       
        private class test : IMainWindow
        {
            public test(AutoResetEvent ev)
            {
                this.ev = ev;
            }
            public string recvmsg;
            event ClipboardMessageDelegate shareClipboard;
            public void addChatLogText(TextMessage msg)
            {

                recvmsg = msg.message;
                ev.Set();
            }

            public void addChatLogText(string msg, System.Drawing.Color col)
            {
                recvmsg = msg;
                ev.Set();
            }

           public event StringMessage sendMessage;       

          
           private AutoResetEvent ev;



            


           event ClipboardMessageDelegate IMainWindow.shareClipboard
           {
               add { throw new NotImplementedException(); }
               remove { throw new NotImplementedException(); }
           }

           public event Action shareVideo;
        }
    }
}
