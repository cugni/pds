using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Server.worker;

namespace Server
{


    //Classe derivata da Form - contentente impostazioni 
    public partial class Impostazioni : Form
    {
      
        private CaptureType tipoCattura;
        private bool flagInit;
        private bool disabilita;
        private string Window_desc = "Capture the portion of the screen corresponding to the acrive window in a certain moment.(Default)";
        private string Screen_desc = "Capture the desired portion of the screen";
        private string FullScreen_desc = "Capture the entire screen";

        private int x_s, y_s, w_s, h_s;
        private int fattoreScalaL = Screen.PrimaryScreen.Bounds.Size.Width / 100;
        private int fattoreScalaH = Screen.PrimaryScreen.Bounds.Size.Height / 100;

        public  void setTipoCattura(CaptureType tipoCattura)
        {
            this.tipoCattura = tipoCattura;
             
        }

        public CaptureType getTipoCattura()
        {
            return tipoCattura;
        }

        public void setScreen(int x, int y, int w, int h)
        {
            x_s = x;
            y_s = y;
            w_s = w;
            h_s = h;
        }
        private ChatServer server;
        public Impostazioni(ChatServer server)
        {
            this.server = server;
            disabilita = true;
            if (MainForm.StartToolStripMenuItem.Enabled) disabilita = false;
            else if (!server.connected) disabilita = false;

            
            InitializeComponent();

        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (windowType.Checked == true) setTipoCattura(CaptureType.ACTIVE_WINDOW);
            if (screenType.Checked == true)
            {
                try
                {
                    setScreen(int.Parse(textBox1.Text), int.Parse(textBox2.Text), int.Parse(textBox3.Text), int.Parse(textBox4.Text));
                }
                catch { }
                setTipoCattura(CaptureType.SCREEN_AREA);
                server.setDimensioniParz(x_s, y_s, w_s, h_s);
            }
            if (fullScreenType.Checked == true) setTipoCattura(CaptureType.FULL_SCREEN);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Impostazioni_Load(object sender, EventArgs e)
        {

           
            flagInit = true;
           
            richTextBox1.BackColor = this.BackColor;
            setTipoCattura(server.captureType);
            Rectangle r = server.getRect();

            setScreen(r.X, r.Y, r.Width, r.Height);
            switch (tipoCattura)
            {
                case CaptureType.ACTIVE_WINDOW:
                    windowType.Checked = true; groupBox2.Enabled = false;
                    break;
                case CaptureType.SCREEN_AREA:
                    screenType.Checked = true; groupBox2.Enabled = true;
                    break;
                case CaptureType.FULL_SCREEN:
                    fullScreenType.Checked = true; groupBox2.Enabled = false;
                    break;
            }

            toolTip1.SetToolTip(windowType, Window_desc);
            toolTip1.SetToolTip(screenType, Screen_desc);
            toolTip1.SetToolTip(fullScreenType, FullScreen_desc);
             

            flagInit = false;

            
            if (disabilita)
            {
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
            }
            

            this.AutoSize = true;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;


        }

       

        private void aggiornaText()
        {
            textBox1.Text = x_s.ToString();
            textBox2.Text = y_s.ToString();
            textBox3.Text = w_s.ToString();
            textBox4.Text = h_s.ToString();
        }

        private void aggiornaText(string s1, string s2, string s3, string s4)
        {
            textBox1.Text = s1;
            textBox2.Text = s2;
            textBox3.Text = s3;
            textBox4.Text = s4;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            richTextBox1.Text = Window_desc;
            groupBox2.Enabled = false;
            if (windowType.Checked) aggiornaText("", "", "", "");
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            richTextBox1.Text = Screen_desc;
            groupBox2.Enabled = true;
            if ((screenType.Checked) && (!flagInit)) selezionaParziale();
            aggiornaText();


        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            richTextBox1.Text = FullScreen_desc;
            groupBox2.Enabled = false;

            if (fullScreenType.Checked) aggiornaText(Screen.PrimaryScreen.Bounds.X.ToString(),
                Screen.PrimaryScreen.Bounds.Y.ToString(),
                Screen.PrimaryScreen.Bounds.Size.Width.ToString(),
                Screen.PrimaryScreen.Bounds.Size.Height.ToString());
        }

        private void TrackBar_changed(object sender, EventArgs e)
        {
            TrackBar t = (TrackBar)sender;
            int val = 0;

            if (t.Name == "trackBar1") val = x_s = t.Value * fattoreScalaL;
            if (t.Name == "trackBar2") val = y_s = t.Value * fattoreScalaH;
            if (t.Name == "trackBar3") val = w_s = t.Value * fattoreScalaL;
            if (t.Name == "trackBar4") val = h_s = t.Value * fattoreScalaH;
            toolTip1.SetToolTip(t, val.ToString());
        }

        private void selezionaParziale()
        {
            Rectangle rect;
            FrmSelezione form5 = new FrmSelezione();
            form5.ShowDialog();
            rect = form5.getRect();
            setScreen(rect.X, rect.Y, rect.Width, rect.Height);
            //aggiornaTrackbar();

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            setScreen(200, 200, 500, 500);
            aggiornaText();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            selezionaParziale();
            aggiornaText();
        }



    }
}