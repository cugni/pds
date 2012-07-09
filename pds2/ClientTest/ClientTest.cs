using pds2.ClientSide;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using pds2.Shared.Messages;
using pds2.Shared;
using System.Diagnostics;


namespace pds2.Test
{
    
    
    /// <summary>
    ///Classe di test per pds2.Test.
    ///Creata per contenere tutti gli unit test pds2.Test
    ///</summary>
    [TestClass()]
    public class ClientTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Ottiene o imposta il contesto dei test, che fornisce
        ///funzionalità e informazioni sull'esecuzione dei test corrente.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Attributi di test aggiuntivi
        // 
        //Durante la scrittura dei test è possibile utilizzare i seguenti attributi aggiuntivi:
        //
        //Utilizzare ClassInitialize per eseguire il codice prima di eseguire il primo test della classe
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Utilizzare ClassCleanup per eseguire il codice dopo l'esecuzione di tutti i test di una classe
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Utilizzare TestInitialize per eseguire il codice prima di eseguire ciascun test
        //
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //Utilizzare TestCleanup per eseguire il codice dopo l'esecuzione di ciascun test
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        private class testWindow : IMainWindow
        {
           public  event ClipboardMessageDelegate shareClipboard;
            public testWindow()
            {
                sendMessage += _set;
            }
            private void _set(string s){
                msg=s;
            }
            public string msg;
            public void send(String s)
            {
                if (sendMessage != null)
                    sendMessage(s);
            }
           public event StringMessage  sendMessage;

             


            public event Action shareVideo;

 
        }
        IPAddress add = IPAddress.Parse("127.0.0.1");
        /// <summary>
        ///Test per Costruttore ClientSide
        ///</summary>
        [TestMethod()]
        public void ClientConstructorTest()
        {
            testWindow mcw = new testWindow();
                  // TODO: Eseguire l'inizializzazione a un valore appropriato
            Client target = new Client(mcw);
            target.configure("user","password", "127.0.0.1",2626);
          
           TcpListener ltext = new TcpListener(add, 2626);
           ltext.Start();
           try
           {
               ThreadPool.QueueUserWorkItem(new WaitCallback(_chatSocket), ltext);
               Thread.Sleep(1000);
               try
               {
                   target.Connect();
               }
               catch (ArgumentException ex)
               {
                 
                   Assert.Fail("Exception on connection\n" + ex.Message);
               }
               mcw.send("ciao");
               TextMessage m = new TextMessage();
               m.messageType = MessageType.TEXT;
               m.message = "urca urca from pool";

               m.sendMe(clie.GetStream());
           }
            

           finally
           {
               ltext.Stop();
           }
        }
        private void _acceptSocket(Object o )
        {
            TcpListener list = (TcpListener)o;
            list.Start();
            TcpClient clie=list.AcceptTcpClient();
           

        }
        TcpClient clie;
        private void _chatSocket(Object o)
        {
            TcpListener list = (TcpListener)o;
            list.Start();
            clie = list.AcceptTcpClient();
            ChallengeMessage chal = new ChallengeMessage();
            chal.salt = new Random().Next().ToString();
            chal.sendMe(clie.GetStream());
            Assert.AreEqual(ResponseChallengeMessage.recvMe(clie.GetStream()).pswMd5,
                Pds2Util.createPswMD5("password",chal.salt));
            
            TcpListener lvideo = new TcpListener(add, 0);
            lvideo.Start();
            TcpListener lclip = new TcpListener(add, 0);
            lclip.Start();
           
            ThreadPool.QueueUserWorkItem(new WaitCallback(_acceptSocket), lvideo);
            ThreadPool.QueueUserWorkItem(new WaitCallback(_acceptSocket), lclip);
            ConfigurationMessage conf = new ConfigurationMessage(
            ((IPEndPoint)lvideo.LocalEndpoint).Port,
             ((IPEndPoint)lclip.LocalEndpoint).Port,
             "welcome");
            conf.sendMe(clie.GetStream());

            TextMessage msg =  TextMessage.recvMe(clie.GetStream());
            Assert.AreEqual("ciao", msg.message);
       
        }
       

    }
}
