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
using Shared.Message;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;

namespace ChatClient
{
    public class Worker
    {
        private PictureBox p;
        private Form1 f;
        private SocketforClient s;
        
        private bool ret;


        private volatile bool _shouldStop;


        public Worker(Form1 f, PictureBox p1, SocketforClient s)
        {
            this.f = f;
            this.s = s;
            this.p = p1;
        }

        public void DoWork()
        {
            int ik=0;

            while (!_shouldStop)
            {
                try
                {
                    NetworkStream stm = s.socket.GetStream();
                    if (!stm.DataAvailable)
                    {
                        Thread.Sleep(1000); //TODO fix it
                        continue;
                        
                    }
                    IFormatter formatter = new BinaryFormatter();

                    ImageMessage btmp =(ImageMessage)formatter.Deserialize(stm);

                     
                    //La Invoke serve per poter effettuare operazioni di cross-thread, in questo caso
                    //devo modificare la pictureBox del Form da qui, che è un thread diverso rispetto a quello che ha
                    //inizializzato la Form. Il parametro change serve per istanziare il delegate e verrà richiamato nel
                    //thread del form
                    f.Invoke(new aggiornaPicture(f.change), btmp);
                }
                catch (System.Runtime.Serialization.SerializationException es)
                {
                    
                    Console.WriteLine("--> " + ik+"--"+es.Message);
                    ik++;
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERRORE VIDEO: " + e.Message);
                    _shouldStop = true;
                    return;
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


