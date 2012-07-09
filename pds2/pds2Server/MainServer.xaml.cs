using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using pds2.Shared;
using pds2.Shared.Messages;

using System.IO;
using System.Drawing;

namespace pds2.ServerSide
{
    /// <summary>
    /// Logica di interazione per MainServerWindow.xaml
    /// </summary>
    public partial class MainServerWindow : Window, IMainWindow
    {
        private readonly IConnection server;
        public event StringMessage sendMessage;
        public event ClipboardMessageDelegate shareClipboard;
        public event Action shareVideo;
        public MainServerWindow()
        {
            server = new Server(this);            
            InitializeComponent();
            server.connectionStateEvent += _setState;
            server.receivedMessage += addChatLogText;
            server.receivedClipboard += _handleClipboard;
        }
        private void _setState(bool connect)
        {
            Dispatcher.Invoke(new ConnectioState(__setState), connect);
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
                conButton.IsChecked = false;
                sendButton.IsEnabled = false;
                shareClipboardButton.IsEnabled = false;
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void addChatLogText(TextMessage msg)
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
      

        

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            _invia();

        }
        private void _invia()
        {
            if (sendMessage != null)
                sendMessage(chatInputField.Text);
            chatInputField.Text = "";

        }
        private void chatInputField_KeyDown(object sender,  KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter)&&server.IsConnect)
            {
                _invia();
            }
        }

        private void _Connetti_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (server.IsConnect)
                {
                    server.Disconnect();                   
                }
                else
                {
                    server.Connect();
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Errore", MessageBoxButton.OK);
            }
        }

        private void startStreamVideo(object sender, RoutedEventArgs e)
        {
            if (shareVideo != null)
                shareVideo();
        }


        private void _handleClipboard(ClipboardMessage cm){

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
                    System.Windows.Forms.Clipboard.SetImage((Bitmap)cm.bitmap);
                    break;
                case ClipBoardType.FILE:
                    chatHistory.AppendText("Ricevuto file in clipboard \n" );
                    System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                    dlg.Description = "Seleziona una cartella dove salvare il file ricevuto";
                    
                    if (dlg.ShowDialog().Equals(  System.Windows.Forms.
                        DialogResult.OK))
                    {
                        string fname = dlg.SelectedPath+"\\"
                            + cm.filename;
                        BinaryWriter bWrite = new BinaryWriter(File.Open(fname, FileMode.Create));
                        chatHistory.AppendText("Salvato file in Ricevuto file in " + fname + " \n");
                        bWrite.Write(cm.filedata);
                        bWrite.Close();
                    }
                    break;

            }
           
        }

        private void _sendClipboard(object sender, EventArgs e)
        {
            ClipboardMessage ms = new ClipboardMessage(server.Username);
           
            IDataObject d = Clipboard.GetDataObject();
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

                ms.bitmap = (Bitmap)System.Windows.Forms.Clipboard.GetImage();

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

         
    }
}
