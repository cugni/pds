
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
using pds2.Shared;
using System.Collections.Concurrent;
using pds2.Shared.Messages;

namespace pds2.ServerSide
{

    public enum CaptureType
    {
        ACTIVE_WINDOW,
        SCREEN_AREA,
        FULL_SCREEN
    }


    public class WorkerPool
    {

        public CaptureType tipoCattura { get; set; }
        protected volatile bool _shouldStop;
        public bool IsConnect
        {
            get { return _shouldStop; }
        }
         

        public const Int32 CURSOR_SHOWING = 0x00000001;
        public Rectangle tot_img_size = new Rectangle(0, 0, 200, 200);

        public int x { get { return tot_img_size.X; } set { tot_img_size.X = value; } }
        public int y { get { return tot_img_size.Y; } set { tot_img_size.Y = value; } }
        public int w { get { return tot_img_size.Width; } set { tot_img_size.Width = value; } }
        public int h { get { return tot_img_size.Height; } set { tot_img_size.Height = value; } }
        public readonly IConnection server;
        private BlockingCollection<ImageMessage> videoQueue;
        public WorkerPool(IConnection server, BlockingCollection<ImageMessage> videoQueue)
        {
            this.server = server;

            this.videoQueue = videoQueue;


        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        public void RequestStart()
        {

            _shouldStop = false;
            //TODO add more threads
            Workers worker = null;
            switch (tipoCattura)
            {
                case CaptureType.ACTIVE_WINDOW:
                    worker = new ActiveWindowWorker( videoQueue);
                    break;
                case CaptureType.FULL_SCREEN:
                    worker = new FullScreenWorker( videoQueue);
                    break;
                case CaptureType.SCREEN_AREA:
                    worker = new ScreenAreaWorker(videoQueue, x, y, w, h);
                    break;
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(worker.doWork));
        }

    }


       
}

