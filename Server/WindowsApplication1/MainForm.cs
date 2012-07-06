using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

//alby
using System.Diagnostics;
using Server.worker;

namespace Server
{

  

    public partial class MainForm : Form
    {
        
        
        public Form af;
        public bool  first = true;//alby
        private delegate void UpdateStatusCallback(string strMessage);

        //alby
        public static Keys kstart;
        public static Keys kend;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        

        // The event handler for application exit
        public void OnApplicationExit(object sender, EventArgs e)
        {

            server.stop();
        }
       

        //for clipboard
        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        IntPtr nextClipboardViewer;
        private ChatServer server;
        
        public MainForm()
        {
            InitializeComponent();
            server = new ChatServer();
            //this.Width = Screen.PrimaryScreen.Bounds.Width/ 3;
            //this.Height = Screen.PrimaryScreen.Bounds.Height / 4;
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
            this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
           this.setTasti(Keys.Up, Keys.Down);
            
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            this.AutoSize = true;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            //register envets
            server.ChangeClipbordStatus += changeClipboardStatus;
           
        }

        //alby
        public void setTasti(Keys start, Keys end)
        {
            kstart = start;
            kend = end;
        }


        public CaptureType getTipoCattura()
        {
            return tipoCattura;
        }

        //alby - tolta funzione "attiva()"-->era inutile..
        void FormB_FormClosed(object sender, FormClosedEventArgs e)
        {

            Autenticazione frm = (Autenticazione)sender;

            if (server.setted)
            {
                StartToolStripMenuItem.Enabled = true;
                endToolStripMenuItem.Enabled = false;
            }
        }
        //alby end
        private CaptureType tipoCattura = CaptureType.FULL_SCREEN;
        void FormImpostazioni_FormClosed(object sender, FormClosedEventArgs e)
        {
            Impostazioni imp = (Impostazioni)sender;
            tipoCattura = imp.getTipoCattura();//alby
        }

       

      
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Chiusura server
            if (server.connected)
            {
                server.stop();
               
                server.closeClip();
            }

            this.Close();
        }

        public  void start()
        {
            server.start(tipoCattura);
            endToolStripMenuItem.Enabled = true;
            StartToolStripMenuItem.Enabled = false;
            label1.ForeColor = Color.Green;
            label1.Text = "CAPTURE: active";
        }

     
       
      
        public  void end()
        {
            if (endToolStripMenuItem.Enabled)
            {
               
                    server.stop();
                   
                 
                    endToolStripMenuItem.Enabled = false;
                    StartToolStripMenuItem.Enabled = true;
                    label1.ForeColor = Color.Red;
                    label1.Text = "CAPTURE: disabled";
                }
                
            
        }
        //alby3 end

        //Apertura condivisione monitor
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            endToolStripMenuItem.Enabled = true;
            start();
        }

        //Chiusura condivisione monitor
        private void endToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartToolStripMenuItem.Enabled = true;
            end();
        }

       
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Create a new instance of the ChatServer object
          
            //startListeningToolStripMenuItem.Text = "Start listening";
            btnSend.Enabled = false;
            txtMessage.Enabled = false;
            this.FormClosing += new FormClosingEventHandler(Form1_Closing);
            this.txtLog.ForeColor = System.Drawing.Color.Green;
            this.txtLog.Font = new Font(txtLog.SelectionFont, FontStyle.Italic);
            this.txtLog.Text = "To use the program you have to let clients connect to you.\r\nSelect File -> Connect, fill in the fields and click ok.\r\nOnce connected you can share your desktop through File -> Start Recording, if you want to change what to share, go to Options -> Recording Options\r\n\r\n";
            label1.Size = new System.Drawing.Size(15, 17);
       
            label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }

    
        private void connettiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!server.connected)
            {
                if (first == true && !server.setted)
                {
                    //Apertura Autenticazione
                    af = new Autenticazione(server);
                    af.ShowDialog();
                    //utente preme annulla
                   
                    if (!server.setted)
                        return;
                }
                // check if the port choosen by the user is free
                

              
               
              

                // Start listening for connections
                try
                {
                  server.StartListening();
                }
                catch (ArgumentException ae)
                {
                        MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
 
                }
                //connettiMonitor();
                if (first == true)
                {
                    // Hook the StatusChanged event handler to mainServer_StatusChanged
                    server.StatusChanged += decoupleStatus;
                    first = false;
                }
                this.txtLog.Font = new Font(txtLog.SelectionFont, FontStyle.Regular);
                //alby5
                txtLog.SelectionColor = Color.Black;
                txtLog.AppendText(" ");
                // Show that we started to listen for connections
                txtLog.SelectionColor = Color.Red;
                txtLog.AppendText("Monitoring for connections...\r\n\r\n");
                connettiToolStripMenuItem.Text = "Disconnetti";
                StartToolStripMenuItem.Enabled = true;
                endToolStripMenuItem.Enabled = false;
                btnSend.Enabled = true;
                txtMessage.Enabled = true;
 
            }
            else
            {
                //alby
                end();

                server.stop();             
                server.closeClip();
                connettiToolStripMenuItem.Text = "Connetti";
                //alby5
                txtLog.SelectionColor = Color.Black;
                txtLog.AppendText(" ");
                //alby5 end
                txtLog.SelectionColor = Color.Red;
                txtLog.AppendText("Stop listening to connections\r\n\r\n");
                btnSend.Enabled = false;
                bntClipboard.Enabled = false;
                txtMessage.Enabled = false;
                StartToolStripMenuItem.Enabled = false;
                endToolStripMenuItem.Enabled = false;
                

                
            }
        }

        
       


       

        private void impostazioniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (StartToolStripMenuItem.Enabled == false && endToolStripMenuItem.Enabled == true)
                MessageBox.Show("Note: read-only options. To change the options first stop the current capture.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //Apertura impostazioni
            Impostazioni iif = new Impostazioni(server);
            iif.Activate();
            iif.Show();
            //iif.setScreen(x_s,y_s,w_s,h_s);

            //passo il delegato di tipo FormClosedEventHandler alla sorgente dell'evento. L'evento Ela 
            //chiusura della finestra Impostazioni
            iif.FormClosed += new FormClosedEventHandler(FormImpostazioni_FormClosed);
        }



        //alby5
        /*

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == 'S') start();
            if (e.KeyChar == 'E') end();
            base.OnKeyPress(e);
        }
        */

        private void opzioniConnessioneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Apertura Autenticazione
            af = new Autenticazione(server);//alby
            af.Activate();
            af.Show();

            //passo il delegato di tipo FormClosedEventHandler alla sorgente dell'evento. L'evento Ela 
            //chiusura della finestra Autenticazione
            af.FormClosed += new FormClosedEventHandler(FormB_FormClosed);
        }



        public void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            server.stop();
           
        }
        private void decoupleStatus(string msg)
        {
            this.Invoke(new Message(UpdateStatus), msg);
        }
        private void UpdateStatus(string strMessage)
        {
            
            // Updates the log with the message
            string search = " says: ";
            int posizione = 0;
            if ((posizione = strMessage.IndexOf(search)) != -1)
            {
                string user = strMessage.Substring(0, posizione);
                string msg = strMessage.Substring(posizione);

                //alby5
                txtLog.SelectionColor = Color.Black;
                txtLog.AppendText(" ");
                //alby5 end

                txtLog.SelectionColor = Color.Blue;
                txtLog.AppendText(user + " ");
                txtLog.SelectionColor = Color.Black;
                txtLog.AppendText(msg + "\r\n\r\n");
            }
            else
            {
                //alby5
                txtLog.SelectionColor = Color.Black;
                txtLog.AppendText(" ");
                //alby5 end
                txtLog.SelectionColor = Color.Red;
                this.txtLog.Font = new Font(txtLog.SelectionFont, FontStyle.Regular);
                txtLog.AppendText(strMessage + "\r\n\r\n");
            }
        }

        //@dany modifiche
        private void btnSend_Click(object sender, EventArgs e)
        {
            string fix = txtMessage.Text.Replace( "\r\n", " ");
            txtMessage.Text = fix;
            if (txtMessage.Lines.Length >= 1)
            {
                //string fix = System.Text.RegularExpressions.Regex.Replace(txtMessage.Text, @"^\s*$\n", string.Empty, System.Text.RegularExpressions.RegexOptions.Multiline);
                server.SendMessage("ChatServer", fix);
                //txtMessage.Lines = null;
            }
            txtMessage.Clear();
        }

        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If the key is Enter
            if (e.KeyChar == (char)13)
            {
                string fix = txtMessage.Text.Replace("\r\n", " ");
                txtMessage.Text = fix;
                if (txtMessage.Lines.Length >= 1)
                {
                    server.SendMessage("ChatServer", fix);
                }
                txtMessage.Clear();
            }
        }




        private delegate void clip(String readon, bool stat);
        private void changeClipboardStatus(string Reason, bool stat)
        {
            this.Invoke(new clip(changeClipboardStatusDelegate), Reason, stat);
        }


        private void changeClipboardStatusDelegate(string Reason, bool stat)
        {
            bntClipboard.Enabled = stat;
            bntClipboard.Text = Reason;
        }

     

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string testo = "To use the program you have to connect to clients.\r\nSelect File -> connect, fill in the fields and click ok.\r\nOnce  you can share your desktop to clients connected through File -> Start Recording, if you want to change what to share, go to Options -> Recording Options\r\n\r\n";
            //txtLog.Clear();
            this.txtLog.SelectionFont = new Font(txtLog.SelectionFont, FontStyle.Italic);
            txtLog.SelectionColor = Color.Green;
            this.txtLog.AppendText(testo);
            //this.txtLog.SelectionStart = txtLog.TextLength;
            //this.txtLog.SelectionColor = System.Drawing.Color.Green;
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            txtLog.SelectionStart = txtLog.Text.Length;

            txtLog.ScrollToCaret();

            txtLog.Refresh();
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    if (server.connected == true)
                    {
                        bntClipboard.Enabled = true;
                    }
                    SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void bntClipboard_Click(object sender, EventArgs e)
        {
            string strclip;
            IDataObject d = Clipboard.GetDataObject();


            if (d.GetDataPresent(DataFormats.Text))  //invio testo
            {
                strclip = (string)d.GetData(DataFormats.Text);
                server.SendClipboard(strclip);
            }
            else if (d.GetDataPresent(DataFormats.FileDrop, true))  //invio file
            {
                //object fromClipboard = d.GetData(DataFormats.FileDrop, true);
                ParameterizedThreadStart thrSendFile = new ParameterizedThreadStart(server.SendClipboardFile);
                Thread thr1 = new Thread(thrSendFile);
                thr1.Start(d);
                //mainServer.SendClipboardFile(fromClipboard);
            }
            else if (Clipboard.ContainsImage())
            {
                Bitmap img = (Bitmap)Clipboard.GetImage();
                server.SendClipboardBitmap(img);
            }
        }

        //alby2
        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartToolStripMenuItem.Text = "Start Capturing(CTRL+" + kstart.ToString() + ")";
            endToolStripMenuItem.Text = "End Capturing(CTRL+" + kend.ToString() + ")";
        }
        //alby2 end



        //alby
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        private void tastiSceltaRapidaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmSelTasti f = new FrmSelTasti(this);
            f.Activate();
            f.Show();
            //TODO check here
        //    f.FormClosed += new FormClosedEventHandler(FrmSelTasti_FormClosed); 
        }

        private  IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        

        
    }
}