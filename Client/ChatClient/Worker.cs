using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;

namespace ChatClient
{
    public class Worker
    {
        private PictureBox p;
        private Form1 f;
        private SocketforClient s;
        private Bitmap btmp;//alby
        private bool ret;//alby


        private volatile bool _shouldStop;


        public Worker(Form1 f, PictureBox p1, SocketforClient s)
        {
            this.f = f;
            this.s = s;
            this.p = p1;
        }

        public void DoWork()
        {

            while (!_shouldStop)
            {

                try
                {
                    //alby
                    ret = s.Receive(ref btmp);



                    //La Invoke serve per poter effettuare operazioni di cross-thread, in questo caso
                    //devo modificare la pictureBox del Form da qui, che è un thread diverso rispetto a quello che ha
                    //inizializzato la Form. Il parametro change serve per istanziare il delegate e verrà richiamato nel
                    //thread del form
                    f.Invoke(new aggiornaPicture(f.change), btmp, ret);
                    //alby end
                    Thread.Sleep(30);
                }
                catch
                {
                    //Bitmap b = new Bitmap("@C:\\Users\\Alberto\\Desktop\\Immagine.png");
                    //f.Invoke(new aggiornaPicture(f.change), b);
                    Thread.Sleep(30);//alby
                }
            }
        }


        public void RequestStop()
        {
            _shouldStop = true;
        }



        public bool isStopped()
        {
            return _shouldStop;
        }
    }
}


