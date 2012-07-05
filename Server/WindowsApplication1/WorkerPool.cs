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
using System.Net;
using System.Net.Sockets;

namespace Server.worker
{

    public enum CaptureType
    {
        ACTIVE_WINDOW,
        SCREEN_AREA,
        FULL_SCREEN
    }


    public class WorkerPool
    {
      
        public  CaptureType tipoCattura        {get;set;}
        internal volatile bool _shouldStop;
     

        public const Int32 CURSOR_SHOWING = 0x00000001;

        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
        public readonly ChatServer server;
        public WorkerPool(ChatServer server )
        {
            this.server = server;

            

        }

      
       



        
    
        public void RequestStop()
        {
            _shouldStop = true;
        }

        public void RequestStart()
        {

            _shouldStop = false;
            //TODO add more threads
            Worker worker=null;
            switch (tipoCattura)
            {
                case CaptureType.ACTIVE_WINDOW:
                    worker = new ActiveWindowWorker(this);
                    break;
                case CaptureType.FULL_SCREEN:
                    worker = new FullScreenWorker(this);
                    break;
                case CaptureType.SCREEN_AREA:
                    worker = new ScreenAreaWorker(this,x, y, w, h);
                break;
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(worker.doWork));
        }


        
    }
}

