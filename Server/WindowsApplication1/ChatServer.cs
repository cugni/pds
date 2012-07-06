using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Server.worker;
using Shared.Message;
using System.Runtime.CompilerServices;

namespace Server
{
    public class TransferException : Exception
    {
      public  TransferException(string message)
            : base(message)
        {
        }
    }
   
    // This delegate is needed to specify the parameters we're passing with our event
   
    public delegate void UpdateClipboardCallback(System.Collections.Specialized.StringCollection paths);

    public partial class ChatServer
    {
        //events
        
        public event Message StatusChanged;
       
        // This hash table stores users and connections (browsable by user)
        private Hashtable htUsers = Hashtable.Synchronized(new Hashtable()); 
        // This hash table stores connections and users (browsable by connection)
        private Hashtable htConnections = Hashtable.Synchronized (new Hashtable()); 
        //Will store connections for screen sharing
        private Hashtable tcpClientsMonitor = Hashtable.Synchronized (new Hashtable());
        private Hashtable tcpClipboard = Hashtable.Synchronized (new Hashtable());
        // Will store the IP address passed to it
        private IPAddress ipAddress;
        private int  porta_scr, num_client = 0;
        //private TcpClient tcpClient;
        // The event and its argument will notify the form when a user has connected, disconnected, send message, etc.
        

        private TcpListener listenerMonitor;
        private TcpClient clientMonitor;
        
        // The constructor sets the IP address to the one retrieved by the instantiating object
        public ChatServer()
        {          
            worker = new WorkerPool(this);
        }

        // The threadListening that will hold the connection listener
        private Thread thrListener;

        // The TCP object that listens for connections
        private TcpListener tlsClient;
        private TcpListener ClipboardSocket;

        // Will tell the while loop to keep monitoring for connections
        bool ServRunning = false;

        

        // Add the user to the hash tables
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddUser(TcpClient tcpUser, string strUsername)
        {
            // First add the username and associated connection to both hash tables
            htUsers.Add(strUsername, tcpUser);
            htConnections.Add(tcpUser, strUsername);

            num_client++;
            // Tell of the new connection to all other users and to the server form
            SendAdminMessage(htConnections[tcpUser] + " has joined us");
        }

        // Remove the user from the hash tables
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveUser(TcpClient tcpUser)
        {
            // If the user is there
            if (htConnections[tcpUser] != null)
            {
                    // Remove the user from the hash table
                    htUsers.Remove(htConnections[tcpUser]);

                    // First show the information and tell the other users about the disconnection
                    SendAdminMessage(htConnections[tcpUser] + " has left us");

                    tcpClientsMonitor.Remove(htConnections[tcpUser]);
                    tcpClipboard.Remove(htConnections[tcpUser]);
                    htConnections.Remove(tcpUser);
                                 
            }
        }

       

        // Send administrative messages
        public void SendAdminMessage(string Message)
        {
            StreamWriter swSenderSender;

            // First of all, show in our application who says what


            StatusChanged("Administrator: " + Message);

            // Create an array of TCP clients, the size of the number of users we have
            TcpClient[] tcpClients = new TcpClient[htUsers.Count];
            // Copy the TcpClient objects into the array
            htUsers.Values.CopyTo(tcpClients, 0);
            // Loop through the list of TCP clients
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // Try sending a message to each
                try
                {
                    // If the message is blank or the connection is null, break out
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // Send the message to the current user in the loop
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine("Administrator: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // If there was a problem, the user is not there anymore, remove him
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        // Send messages from one user to all the others
        public void SendMessage(string From, string Message) 
        {
            StreamWriter swSenderSender;

            // First of all, show in our application who says what
            StatusChanged(From + " says: " + Message);

            // Create an array of TCP clients, the size of the number of users we have
            TcpClient[] tcpClients = new TcpClient[htUsers.Count];
            // Copy the TcpClient objects into the array
            htUsers.Values.CopyTo(tcpClients, 0);
            // Loop through the list of TCP clients
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // Try sending a message to each
                try
                {
                    // If the message is blank or the connection is null, break out
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // Send the message to the current user in the loop
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine(From + " says: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // If there was a problem, the user is not there anymore, remove him
                {
                    RemoveUser(tcpClients[i]);
                    
                }
            }
        }

        private volatile bool _connected = false;
        public  bool connected
        {
            get
            {
                return _connected;
            }

        }
        public void StartListening()
        {
            IPEndPoint[] tcpConnInfoArray = System.Net.NetworkInformation
                    .IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            foreach (IPEndPoint endpoint in tcpConnInfoArray)
                if (endpoint.Port == _port)
                {
                    throw new ArgumentException("Connection failed: the selected port is already used by the system");
                    
                }
            tlsClient = new TcpListener(ipAddress, _port);
            _connected = true;
            // Start the TCP listener and listen for connections
            tlsClient.Start();

            // The while loop will check for true in this before checking for connections
            ServRunning = true;

            socketforMonitor();
            socketClipboard();

            // Start the new tread that hosts the listener
            thrListener = new Thread(KeepListening);
            thrListener.Start();
          

           
        }

        private void socketClipboard()
        {
            ClipboardSocket = new TcpListener(ipAddress, 48231);
            ClipboardSocket.Start();
        }
        private TcpClient tcpClient;
        Thread threadListening;
        private void KeepListening()
        {
            
            // While the server is running
            
            while (_connected)
            {
                //MessageBox.Show("inizio keeplistening!");
                // Accept a pending connection
                try
                {
                    tcpClient = tlsClient.AcceptTcpClient();
                    // Create a new instance of Connection
                     //create an object ParameterizedThreadStart
                    ParameterizedThreadStart thrSender = new ParameterizedThreadStart(AcceptClient);
                     threadListening= new Thread(thrSender);
                    //start the new threadListening with the parameter needed
                    threadListening.Start(tcpClient);
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    return;
                }
                
            }

            // server is stopping, so delete all active users
            RemoveAllUser();
            //MessageBox.Show("ho finito keeplistening!");
            //newConnection.setRunning();
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public  void RemoveAllUser()
        {
          
                htUsers.Clear();
                tcpClientsMonitor.Clear();
                htConnections.Clear();
                tcpClipboard.Clear();
            
        }

        
       
           

        public void CloseConnection(TcpClient tcpClient, StreamReader srReceiver, StreamWriter swSender)
        {
            // Close the currently open objects
            _connected = false;
            tcpClient.Close();
            srReceiver.Close();
            swSender.Close();
            
        }

        
        // Occures when a new client is accepted
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void AcceptClient(object tcpCon)
        {
            bool finito = false;
            string strResponse;
            TcpClient tcpClient = (TcpClient)tcpCon;
            StreamReader srReceiver = new System.IO.StreamReader(tcpClient.GetStream());
            StreamWriter swSender = new System.IO.StreamWriter(tcpClient.GetStream());

            MD5 md5 = new MD5CryptoServiceProvider();
            string passmd5 = BitConverter.ToString(md5.ComputeHash(ASCIIEncoding.Default.GetBytes(_psw)));

            // Read the account information from the client
            string currUser = srReceiver.ReadLine();

            // We got a response from the client
            if (currUser != "")
            {
                if (num_client == 4)
                {
                    swSender.WriteLine("0|Too many clients connected to server. Try again later");
                    swSender.Flush();
                    CloseConnection(tcpClient, srReceiver, swSender);
                    return;
                }
                // Store the user name in the hash table
                if (htUsers.Contains(currUser) == true)
                {
                    // 0 means not connected
                    swSender.WriteLine("0|This username already exists.");
                    swSender.Flush();
                    CloseConnection(tcpClient, srReceiver, swSender);
                    return;
                }
                else if (currUser == "Administrator")
                {
                    // 0 means not connected
                    swSender.WriteLine("0|This username is reserved.");
                    swSender.Flush();
                    CloseConnection(tcpClient, srReceiver, swSender);
                    return;
                }
                // checking password
                else if (!passmd5.Equals(srReceiver.ReadLine()))
                {
                    // 0 means not connected
                    swSender.WriteLine("0|The password is not valid !");
                    swSender.Flush();
                    CloseConnection(tcpClient, srReceiver, swSender);
                    return;
                }
                else
                {
                    // 1 means connected successfully
                    swSender.WriteLine("1");
                    swSender.Flush();

                    //send the port for screen sharing
                    swSender.WriteLine(porta_scr);
                    swSender.Flush();

                    // Add the user to the hash tables and start listening for messages from him
                    AddUser(tcpClient, currUser);
                    //lister for connections to screen share
                    AcceptMonitor(currUser);
                    AcceptClipboard(currUser);
                }
            }
            else
            {
                CloseConnection(tcpClient, srReceiver, swSender);
                return;
            }

            try
            {
                // Keep waiting for a message from the user
                while (ServRunning == true)
                {
                    if (tcpClient.GetStream().DataAvailable)
                    {
                        if (((strResponse = srReceiver.ReadLine()) != "#####"))
                        {
                            // If it's invalid, remove the user
                            if (strResponse == null)
                            {
                                RemoveUser(tcpClient);
                                //MessageBox.Show("rimosso utente!");
                            }
                            else
                            {
                                // Otherwise send the message to all the other users
                                SendMessage(currUser, strResponse);
                            }
                        }
                        else
                        {
                            finito = true;
                            break;
                        }
                    }
                }

            }
            catch
            {
                // If anything went wrong with this user, disconnect him
                RemoveUser(tcpClient);
               
            }
            if (finito == true)
            {
                RemoveUser(tcpClient);
              
            }
            //MessageBox.Show("ads chiudo connessione!");
            CloseConnection(tcpClient, srReceiver, swSender);
        }

        private void AcceptClipboard(string currUser)
        {
            TcpClient my_tcp;
            my_tcp = ClipboardSocket.AcceptTcpClient();
            
            tcpClipboard.Add(currUser, my_tcp);

            //create an object ParameterizedThreadStart
            ParameterizedThreadStart threadSender = new ParameterizedThreadStart(ReceiveFile);
            Thread clipthread = new Thread(threadSender);
            clipthread.SetApartmentState(ApartmentState.STA);
            //start the new threadListening with the parameter needed
            clipthread.Start(my_tcp);
        }

        public void socketforMonitor()
        {
            
                listenerMonitor = new TcpListener(ipAddress, 0);
                listenerMonitor.Start();
                porta_scr = ((IPEndPoint)listenerMonitor.LocalEndpoint).Port;
             
        }

        public void AcceptMonitor(string currUser)
        {
         
                clientMonitor = listenerMonitor.AcceptTcpClient();
                tcpClientsMonitor.Add(currUser, clientMonitor);
           
        }


        //prende un Bitmap e invia al client in formato serializzato
        public void SendImmage(Object input)
        {

            ImageMessage diff = (ImageMessage)input;
            IFormatter formatter = new BinaryFormatter();

            // Create an array of TCP clients, the size of the number of users we have
            TcpClient[] tcpClientM = new TcpClient[tcpClientsMonitor.Count];
            // Copy the TcpClient objects into the array
            tcpClientsMonitor.Values.CopyTo(tcpClientM, 0);
            // Loop through the list of TCP clients
            for (int i = 0; i < tcpClientM.Length; i++)
            {
                try
                {
                    NetworkStream stream1 = tcpClientM[i].GetStream();
                    formatter.Serialize(stream1, diff);
                }
                catch
                {//remove the tcpClient
                    this.RemoveUser(tcpClientM[i]);

                }
               
            }

        }

        public void SendClipboard(string text)
        {
            TcpClient[] tcpClients = new TcpClient[tcpClipboard.Count];
            tcpClipboard.Values.CopyTo(tcpClients, 0);
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // Try sending a message to each
               
                    // If the message is blank or the connection is null, break out
                    if (tcpClients[i] == null)
                    {
                        continue;
                    }
                    // Send the message to the current user in the loop                  
                    StreamWriter sw = new StreamWriter(tcpClients[i].GetStream());
                    sw.WriteLine("Text");
                    sw.Flush();
                    sw = null;
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(text);
                    NetworkStream sendstr = tcpClients[i].GetStream();
                    sendstr.Write(sendBytes, 0, sendBytes.Length);
                
            }
            SendAdminMessage("server just shared his clipboard containing text with us!");
        }

        private void ReceiveFile(object stream)
        {
            byte[] buff = new byte[1024];
            int j;
            string text, user_sharing;
            System.Collections.Specialized.StringCollection paths = new System.Collections.Specialized.StringCollection();
            
            TcpClient s = (TcpClient) stream; 
            NetworkStream netStream = s.GetStream();
            StreamReader str = new StreamReader(s.GetStream());
            bool abort= false;

             while (ServRunning==true)
             {
                 string what;
                 try
                 {
                      what = str.ReadLine();
                 }
                 catch (IOException)
                 {
                     return;
                 }
                    if (MessageBox.Show("E' stata condivisa una clipboard.\n Accettarla, sovrascrivendo la clipboard attuale?", "Clipboard condivisa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        continue;
 
                    }
                   
                    user_sharing = what.Substring(4);
                    if (what.Substring(0, 4).Equals("File")) //someone is sharing a file-clopboard
                    {
                        ChangeClipbordStatus("Receving Clipboard...",false);

                       
                        user_sharing = what.Substring(5);
                        paths.Clear();
                        string qta = what.Substring(4, 1);
                        for (j = 0; j < int.Parse(qta); j++)
                        {
                            string tmp = str.ReadLine();
                            if (tmp.Equals("Abort"))
                            {
                                abort = true;
                                break;
                            }
                            abort = false;
                            string fileName = tmp.Substring(6);
                            byte[] clientData = Convert.FromBase64String(str.ReadLine());
                            string frdir=Path.GetFullPath(@".\File ricevuti\");
                            if (!System.IO.File.Exists(frdir))
                            {
                                System.IO.Directory.CreateDirectory(frdir);
                            }
                            BinaryWriter bWrite = new BinaryWriter(File.Open(frdir + fileName, FileMode.Create));
                            bWrite.Write(clientData, 4 + fileName.Length, clientData.Length - 4 - fileName.Length);
                            bWrite.Close();
                            paths.Add(Path.GetFullPath(@".\File ricevuti\") + fileName);

                            //send file to others users
                            TcpClient[] tcpClients = new TcpClient[tcpClipboard.Count];
                            String[] users = new String[htUsers.Count];
                            tcpClipboard.Values.CopyTo(tcpClients, 0);
                            htUsers.Keys.CopyTo(users, 0);
                            for (int i = 0; i < tcpClients.Length; i++)
                            {
                                // Try sending a message to each
                                    // If the message is blank or the connection is null, break out
                                    if (tcpClients[i] == null || user_sharing == users[i])
                                    {
                                        continue;
                                    }
                                    StreamWriter sw = new StreamWriter(tcpClients[i].GetStream());
                                    sw.WriteLine("File");
                                    sw.Flush();
                                    // Send the message to the current user in the loop
                                    sw.WriteLine("File: " + Path.GetFileName(fileName));
                                    sw.Flush();
                                    sw.WriteLine(Convert.ToBase64String(clientData));
                                    sw.Flush();
                                    sw = null;
                                
                                
                            }
                        }
                        if (abort == false)
                        {
                            if (int.Parse(qta) == 1)
                                SendAdminMessage(user_sharing + " just shared his clipboard containing a file with us!");
                            else
                                SendAdminMessage(user_sharing + " just shared his clipboard containing some files with us!");
                            UpdateClipboard( paths);
                        }
                        ChangeClipbordStatus("Share Clipboard",false);
                       
                        
                    }
                    else if (what.Substring(0, 4).Equals("Text"))
                    {
                        ChangeClipbordStatus("Receving Clipboard...", false);
                      

                        byte[] bytes = new byte[s.ReceiveBufferSize];

                        // Read can return anything from 0 to numBytesToRead. 
                        // This method blocks until at least one byte is read.
                        netStream.Read(bytes, 0, (int)s.ReceiveBufferSize);
                        string textcrc = Encoding.ASCII.GetString(bytes);
                        text = TrimFromZero(textcrc);
                        //text = str.ReadLine();
                        TcpClient[] tcpClients = new TcpClient[tcpClipboard.Count];
                        String[] users = new String[htUsers.Count];
                        tcpClipboard.Values.CopyTo(tcpClients, 0);
                        htUsers.Keys.CopyTo(users, 0);
                        for (int i = 0; i < tcpClients.Length; i++)
                        {
                            // Try sending a message to each
                            // If the message is blank or the connection is null, break out
                                if (tcpClients[i] == null || user_sharing == users[i])
                                {
                                    continue;
                                }
                                // Send the message to the current user in the loop

                                StreamWriter sw = new StreamWriter(tcpClients[i].GetStream());
                                sw.WriteLine("Text");
                                sw.Flush();
                                sw = null;

                                NetworkStream sendstr = tcpClients[i].GetStream();
                                Byte[] sendBytes = Encoding.ASCII.GetBytes(text);
                                sendstr.Write(sendBytes, 0, sendBytes.Length);
                            
                           
                        }

                        IDataObject ido = new DataObject();
                        ido.SetData(text);
                        Clipboard.SetDataObject(ido, true);
                        ChangeClipbordStatus("Share Clipboard", true);
                        SendAdminMessage(user_sharing + " just shared his clipboard containing text with us!");
                    }
                    else if (what.Substring(0, 4).Equals("Imag"))
                    {
                        ChangeClipbordStatus("Receving Clipboard...", false);

                        Stream stm = s.GetStream();
                        IFormatter formatter = new BinaryFormatter();
                        Bitmap bitm = (Bitmap)formatter.Deserialize(stm);

                        TcpClient[] tcpClients = new TcpClient[tcpClipboard.Count];
                        String[] users = new String[htUsers.Count];
                        tcpClipboard.Values.CopyTo(tcpClients, 0);
                        htUsers.Keys.CopyTo(users, 0);
                        for (int i = 0; i < tcpClients.Length; i++)
                        {
                            // Try sending a message to each
                                // If the message is blank or the connection is null, break out
                                if (tcpClients[i] == null || user_sharing == users[i])
                                {
                                    continue;
                                }
                                // Send the message to the current user in the loop
                                NetworkStream sendstr = tcpClients[i].GetStream();

                                StreamWriter sw = new StreamWriter(tcpClients[i].GetStream());
                                sw.WriteLine("Imag");
                                sw.Flush();

                                IFormatter myformat = new BinaryFormatter();
                                myformat.Serialize(sendstr, bitm);

                            
                            
                        }
                        Clipboard.SetImage(bitm);
                        ChangeClipbordStatus("Shared Clipboard", true);
                        SendAdminMessage(user_sharing + " just shared his clipboard containing an image with us!");
                    }
                
             
            }
        }
        private void UpdateClipboard(System.Collections.Specialized.StringCollection paths)
        {

            Clipboard.SetFileDropList(paths);
        }
       

        private string TrimFromZero(string input)
        {
            int index = input.IndexOf('\0');
            if (index < 0)
                return input;
            return input.Substring(0, index);
        }


        public void SendClipboardFile(object d)
        {
            int qta = 0;
            IDataObject f = (IDataObject)d;
            object fromClipboard = f.GetData(DataFormats.FileDrop, true);
            foreach (string sourceFileName in (Array)fromClipboard)
                qta++;
            TcpClient[] tcpClients = new TcpClient[tcpClipboard.Count];
            tcpClipboard.Values.CopyTo(tcpClients, 0);
            ChangeClipbordStatus("Sharing Clipboard...", false);
          
            
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // Try sending a message to each
                try
                {
                    // If the message is blank or the connection is null, break out
                    if (tcpClients[i] == null)
                    {
                        continue;
                    }
                    StreamWriter sw = new StreamWriter(tcpClients[i].GetStream());
                    sw.WriteLine("File"+qta.ToString());
                    sw.Flush();
                    foreach (string sourceFileName in (Array)fromClipboard)
                    {
                        try
                        {
                            // Conversione del file in stringa
                            byte[] fileNameByte = Encoding.ASCII.GetBytes(Path.GetFileName(sourceFileName));
                            byte[] fileData = File.ReadAllBytes(sourceFileName);
                            byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
                            BitConverter.GetBytes(fileNameByte.Length).CopyTo(clientData, 0);
                            fileNameByte.CopyTo(clientData, 4);
                            fileData.CopyTo(clientData, 4 + fileNameByte.Length);
                            // Send the message to the current user in the loop
                           
                                sw.WriteLine("File: " + Path.GetFileName(sourceFileName));
                                sw.Flush();
                                sw.WriteLine(Convert.ToBase64String(clientData));
                                sw.Flush();
                            
                        }
                        catch (Exception ex)// If there was a problem, the user is not there anymore, remove him
                        {
                            throw new TransferException("Sharing failed: it's impossible to copy a directory\n" + ex.Message);                          
                            
                        }
                    }
                    sw = null;
                }
                catch (Exception ex)
                {
                    string res="Share Clipboard error\n" + ex.Message;
                    ChangeClipbordStatus(res, false);
                    throw new TransferException(res);
                }
            }
            if (qta==1)
                SendAdminMessage("server just shared his clipboard containing a file with us!");
            else
                SendAdminMessage("server just shared his clipboard containing some files with us!");
            ChangeClipbordStatus("Share Clipboard",true);
         
             
        }

        public void closeClip()
        {
            ClipboardSocket.Stop();
        }

        public void SendClipboardBitmap(Bitmap bitm)
        {
             TcpClient[] tcpClients = new TcpClient[tcpClipboard.Count];
             String[] users = new String[htUsers.Count];
             tcpClipboard.Values.CopyTo(tcpClients, 0);
             htUsers.Keys.CopyTo(users, 0);
             for (int i = 0; i < tcpClients.Length; i++)
             {
                // Try sending a message to each
                
                // If the message is blank or the connection is null, break out
                if (tcpClients[i] == null)
                    {
                        continue;
                    }
                // Send the message to the current user in the loop
                NetworkStream sendstr = tcpClients[i].GetStream();

                StreamWriter sw = new StreamWriter(tcpClients[i].GetStream());
                sw.WriteLine("Imag");
                sw.Flush();

                IFormatter myformat = new BinaryFormatter();
                myformat.Serialize(sendstr, bitm);
 
             }
             SendAdminMessage("server just shared his clipboard containing an image with us!");
        }
    }
}
