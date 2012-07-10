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
using System.Windows.Shapes;

namespace pds2.ServerSide
{
    /// <summary>
    /// Logica di interazione per Impostazioni.xaml
    /// </summary>
    public partial class Impostazioni : UserControl
    {
        public Impostazioni()
        {
            InitializeComponent();
            this.Loaded += this.Impostazioni_Load;
        }
   
        
        private bool flagInit;
        private bool disabilita;
        private string Window_desc = "Capture the portion of the screen corresponding to the acrive window in a certain moment.(Default)";
        private string Screen_desc = "Capture the desired portion of the screen";
        private string FullScreen_desc = "Capture the entire screen";

        private int x_s, y_s, w_s, h_s;
        private double fattoreScalaL = System.Windows.SystemParameters.PrimaryScreenWidth / 100;
        private double fattoreScalaH = System.Windows.SystemParameters.PrimaryScreenHeight / 100;

        public  void setTipoCattura(CaptureType tipoCattura)
        {
            pool.tipoCattura = tipoCattura;
             
        }

        public CaptureType getTipoCattura()
        {
            return pool.tipoCattura;
        }

        public void setScreen(int x, int y, int w, int h)
        {
            x_s = x;
            y_s = y;
            w_s = w;
            h_s = h;
        }
        private WorkerPool pool;
        public void setWorkingPool(WorkerPool server)
        {
            this.pool = server;
            disabilita = true;
            InitializeComponent();

        }


        private void salva(object sender, EventArgs e)
        {
            if (windowType.IsChecked == true) setTipoCattura(CaptureType.ACTIVE_WINDOW);
            if (screenType.IsChecked == true)
            {
                try
                {
                    setScreen(int.Parse(textx.Text), int.Parse(texty.Text), int.Parse(textw.Text), int.Parse(texth.Text));
                }
                catch { }
                setTipoCattura(CaptureType.SCREEN_AREA);
                pool.x = x_s;
                pool.y = y_s;
                pool.w = w_s;
                pool.h = h_s;
                
            }
            if (fullScreenType.IsChecked == true) setTipoCattura(CaptureType.FULL_SCREEN);
            
        }

      

        public void Impostazioni_Load(object sender, EventArgs e)
        {

           
            flagInit = true;
           
            setTipoCattura(pool.tipoCattura);
            System.Drawing.Rectangle r = pool.tot_img_size;

            setScreen(r.X, r.Y, r.Width, r.Height);
            switch (pool.tipoCattura)
            {
                case CaptureType.ACTIVE_WINDOW:
                    windowType.IsChecked = true; region.IsEnabled = false;
                    break;
                case CaptureType.SCREEN_AREA:
                    screenType.IsChecked = true; region.IsEnabled = true;
                    break;
                case CaptureType.FULL_SCREEN:
                    fullScreenType.IsChecked = true; region.IsEnabled = false;
                    break;
            }

            
             

            flagInit = false;


        }

       

        private void aggiornaText()
        {
            textx.Text = x_s.ToString();
            texty.Text = y_s.ToString();
            textw.Text = w_s.ToString();
            texth.Text = h_s.ToString();
        }

        private void aggiornaText(string s1, string s2, string s3, string s4)
        {
            textx.Text = s1;
            texty.Text = s2;
            textw.Text = s3;
            texth.Text = s4;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

            region.IsEnabled = false;
            if ((bool)windowType.IsChecked)
                aggiornaText("", "", "", "");
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

            region.IsEnabled = true;
            if (((bool)screenType.IsChecked) && (!flagInit)) selezionaParziale();
            aggiornaText();


        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

            region.IsEnabled = false;

            if ((bool)fullScreenType.IsChecked) 
                aggiornaText("0",
               "0",
                System.Windows.SystemParameters.PrimaryScreenWidth.ToString(),
               System.Windows.SystemParameters.PrimaryScreenHeight.ToString());
        }

         

        private void selezionaParziale()
        {
           System.Drawing.Rectangle rect;
            FrmSelezione form5 = new FrmSelezione();
            form5.ShowDialog();
            rect = form5.getRect();
            setScreen(rect.X, rect.Y, rect.Width, rect.Height);
            //aggiornaTrackbar();

        }

        private void disableSelezione(object sender, EventArgs e)
        {
            region.IsEnabled = false;

        }
        private void abilitaSelezione(object sender, EventArgs e)
        {
            region.IsExpanded = true;
            region.IsEnabled = true;
           
        }
        
        
        private void default_Click(object sender, EventArgs e)
        {
            setScreen(200, 200, 500, 500);
            aggiornaText();
        }

        private void seleziona_Click(object sender, EventArgs e)
        {
            selezionaParziale();
            aggiornaText();
        }
    }
}
