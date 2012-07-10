using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using pds2.Shared;
using pds2.Shared.Messages;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;

namespace pds2.ClientSide
{
   
    /// <summary>
    /// Logica di interazione per MainClientWindow.xaml
    /// </summary>
    public partial class MainClientWindow : Window, IMainWindow
    {
        public event StringMessage sendMessage;
        public event ClipboardMessageDelegate shareClipboard;
        public event Action video;
        public IConnection client;
        
     
        public MainClientWindow()
        {
            client = new Client(this);
            InitializeComponent();
            client.connectionStateEvent += _setState;
            client.receivedMessage += addChatLogText;
            client.receivedVideo += _handleImageMessage;
            client.receivedClipboard += _handleClipboard;
        }
        private void _setState(bool connect)
        {
            Dispatcher.Invoke(new ConnectioState(__setState),connect);
        }
        private void __setState(bool connect)
        {
             
            if (connect)
            {
                 conStatus.Text = "Connesso";
                 conButton.IsChecked = true;
                 sendButton.IsEnabled = true;
                 shareClipboardButton.IsEnabled = true;
            }
            else
            {
                conStatus.Text = "Non connesso";
                conButton.IsChecked =false;
                sendButton.IsEnabled = false;
                shareClipboardButton.IsEnabled = false;
            }

        }
        public void addChatLogText(TextMessage msg)
        {
            Dispatcher.Invoke(new TextMessageDelegate(_handleTextMessage), msg);
        }
                
        private void _handleTextMessage(TextMessage msg)
        {
            switch (msg.messageType)
            {
                case MessageType.TEXT:
                    chatHistory.AppendText(msg.username + " : " + msg.message+"\n");
                    break;
                default:
                    chatHistory.AppendText(msg.username + " : " + msg.message+"\n");
                    break;
            }
        }
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);
        public void _handleImageMessage(ImageMessage p)
        {
            Dispatcher.Invoke(new ImageMessageDelegate(__handleImageMessage),p);      
        }
        private void __handleImageMessage(ImageMessage p)
        {
            canvas.queue.Enqueue(p);
            canvas.InvalidateVisual();   
        }
        private void newMsg()
        {
            sendMessage(this.chatInputField.Text);
        }
         

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            newMsg();
        }

      
        private void connetti(object sender, EventArgs e)
        {
            connetti();
        }
        private void chiudi(object sender, RoutedEventArgs e)
        {
            try
            {
                client.Disconnect();
            }
            finally
            {
                this.Close();
            }
        }

        private void connetti()
        {
            if (!imposta())return ; //imposta il client e se annullato ritorna subito
            try
            {
                if (client.IsConnect)
                {
                    client.Disconnect();
                    conStatus.Text = "Non connesso";

                }
                else
                {
                    client.Connect();
                    conStatus.Text = "Connesso";
                }
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show(ae.Message, "Errore!", MessageBoxButton.OK);
                conStatus.Text = "Non connesso";
                conButton.IsChecked = false;
            }
        }
        private void imposta(object sender, RoutedEventArgs e)
        {
            imposta();
        }
        private bool imposta()
        {
            if (!client.IsConfigured)
            {
                Impostazioni imp = new Impostazioni((Client)client);
                imp.Show();
                imp.Closed += connetti;
                return false;
            }
            else
            {
                return true;

            }
            
            
        }

      
        private void _invia()
        {   
            if(sendMessage!=null)
          sendMessage(this.chatInputField.Text);
            chatInputField.Text = "";
        }
        private void chatInputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter)&&client.IsConnect)
            {
                
                _invia();

            }
        }


        public event Action shareVideo;
        private void _sendClipboard(object sender, EventArgs e)
        {
         
            IDataObject d = Clipboard.GetDataObject();
            ClipboardMessage ms = new ClipboardMessage(client.Username);
            


            if (d.GetDataPresent(DataFormats.Text))  //invio testo
            {
                try
                {
                   ms.clipboardType = ClipBoardType.TEXT;
                    ms.text = (string)d.GetData(DataFormats.Text);
                    shareClipboard(ms);                     
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                     
                    return;
                }
            }
            else if (d.GetDataPresent(DataFormats.FileDrop, true))  //invio file
            {
             
                ms.clipboardType = ClipBoardType.FILE;
                object fromClipboard = d.GetData(DataFormats.FileDrop, true);
                foreach (string sourceFileName in (Array)fromClipboard)
                {
                    if (System.IO.Path.GetFileName(sourceFileName).Equals(""))
                    {
                        System.Windows.Forms.MessageBox
                            .Show("Condivisione fallita: impossibie copiare una directory",
                            "Error");
                         
                        return;
                    }
                    FileInfo fleMembers = new FileInfo(sourceFileName);
                    float size = (float)(fleMembers.Length / 1024 / 1024); //MB
                    if (size > 50)
                    {
                        System.Windows.Forms.MessageBox
                            .Show("Impossibile inviare il file " + sourceFileName + ": dimensione troppo grande!", "Error");
                        return;
                    }
                    ms.filename = System.IO.Path.GetFileName(sourceFileName);
                    ms.filedata = File.ReadAllBytes(sourceFileName);
                    shareClipboard(ms);
                }

            }

            else if (Clipboard.ContainsImage())
            {
              
                ms.clipboardType = ClipBoardType.BITMAP;

                ms.bitmap = (Bitmap)(System.Windows.Forms.Clipboard.GetImage());
                
                try
                {
                    shareClipboard(ms);
                }
                catch (Exception exc)
                {
                    
                    MessageBox.Show(exc.ToString());
                    return;
                }
               
            }
            else
            {
                MessageBox.Show("Invio fallito: la clipboard è vuota", "Error");
            }
        }

        private void _handleClipboard(ClipboardMessage cm)
        {

            Dispatcher.Invoke(new ClipboardMessageDelegate(__handleClipboard), cm);
        }
        private void __handleClipboard(ClipboardMessage cm)
        {


            switch (cm.clipboardType)
            {
                case ClipBoardType.TEXT:
                    chatHistory.AppendText("Ricevuto testo in clipboard \n" + cm.text);
                    System.Windows.Clipboard.SetText(cm.text);
                    break;
                case ClipBoardType.BITMAP:
                    chatHistory
                        .AppendText("Ricevuta bitmap in clipboard \n");
                    System.Windows.Forms.Clipboard.SetImage((Bitmap)(cm.bitmap));

                    break;
                case ClipBoardType.FILE:
                    chatHistory.AppendText("Ricevuto file in clipboard \n");
                    System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                    dlg.Description = "Seleziona una cartella dove salvare il file ricevuto";

                    if (dlg.ShowDialog().Equals(System.Windows.Forms.
                        DialogResult.OK))
                    {
                        string fname = dlg.SelectedPath + "\\"
                            + cm.filename;
                        BinaryWriter bWrite = new BinaryWriter(File.Open(fname, FileMode.Create));
                        chatHistory.AppendText("Salvato file in Ricevuto file in " + fname + " \n");
                        bWrite.Write(cm.filedata);
                        bWrite.Close();
                    }


                    break;

            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(client.IsConnect)
            client.Disconnect();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            imposta();
            
        }

        private void InfoClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Created by \nCesare Cugnasco & Mauro Canuto", "Info");
        }

        
    }
}
