 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using pds2.Shared.Messages;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;

namespace pds2.Test
{
    
    
    /// <summary>
    ///Classe di test per SendableTest.
    ///Creata per contenere tutti gli unit test SendableTest
    ///</summary>
    [TestClass()]
    public class SendableTest
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


        /// <summary>
        ///Test per recvMe
        ///</summary>
        [TestMethod()]
        public void recvMeOnMemory()
        {
            Stream me = new MemoryStream();
            TextMessage expected = new TextMessage();
            expected.message = "messaggio";
            expected.username = "usrname";
            expected.messageType = MessageType.TEXT;
            TextMessage actual;
            expected.sendMe(me);
            me.Position=0;
            actual =  TextMessage.recvMe(me);
            Assert.AreEqual(expected.message, actual.message);
            Assert.AreEqual(expected.username, actual.username);
            Assert.AreEqual(expected.messageType, actual.messageType);
          
        }
        /// <summary>
        ///Test per recvMe
        ///</summary>
        [TestMethod()]
        public void recvMeOnNework()

        {
           
                TcpListener tl = new TcpListener(IPAddress.Parse("127.0.0.1"), 0);
                tl.Start();
                int porta_scr = ((IPEndPoint)tl.LocalEndpoint).Port;
                IAsyncResult sres = tl.BeginAcceptTcpClient(null, null);
                TcpClient t2 = new TcpClient("127.0.0.1", porta_scr);
                TcpClient t1 = tl.EndAcceptTcpClient(sres);
                try
                {
                    Stream me = t1.GetStream();
                TextMessage expected = new TextMessage();
                expected.message = "messaggio";
                expected.username = "usrname";
                expected.messageType = MessageType.TEXT;
                TextMessage actual;
                expected.sendMe(me);

                actual = TextMessage.recvMe(t2.GetStream());
                Assert.AreEqual(expected.message, actual.message);
                Assert.AreEqual(expected.username, actual.username);
                Assert.AreEqual(expected.messageType, actual.messageType);
            }
            finally
            {
                t1.Close();
                t2.Close();
            }
        }
        /// <summary>
        ///Test per recvMe
        ///</summary>
        [TestMethod()]
        public void recvMeOnNeworkWait()
        {
            
            TcpListener tl = new TcpListener(IPAddress.Parse("127.0.0.1"), 0);
            tl.Start();
            int porta_scr = ((IPEndPoint)tl.LocalEndpoint).Port;
            IAsyncResult sres = tl.BeginAcceptTcpClient(null, null);
            TcpClient t2 = new TcpClient("127.0.0.1", porta_scr);
            TcpClient t1 = tl.EndAcceptTcpClient(sres);
            try
            {
                Stream me = t1.GetStream();
                TextMessage expected = new TextMessage();
                expected.message = "messaggio";
                expected.username = "usrname";
                expected.messageType = MessageType.TEXT;
                ArrayList par = new ArrayList(2);
                par.Add(t2.GetStream());
                par.Add(expected);
                new Thread(new ParameterizedThreadStart(test)).Start(par);
                TextMessage actual =  TextMessage.recvMe(t1.GetStream());
                Assert.AreEqual(expected.message, actual.message);
                Assert.AreEqual(expected.username, actual.username);
                Assert.AreEqual(expected.messageType, actual.messageType);

            } catch (Exception e)
            {
                Assert.Fail(e.Message);
                throw e;
            }
            finally
            {
                t1.Close();
                t2.Close();
            }
        }
        private void test(Object o)
        {
            try
            {
                Stream s = (Stream)((ArrayList)o)[0];
                TextMessage expected = (TextMessage)((ArrayList)o)[1];
                Thread.Sleep(1000);
                expected.sendMe(s);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
                throw e;
            }
        }
       

       

    }
}
