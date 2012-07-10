using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Concurrent;
using pds2.Shared.Messages;
using pds2.Shared;

namespace pds2.ClientSide
{
    class VideoCanvas:Canvas
    {
        public ConcurrentQueue<ImageMessage> queue=new ConcurrentQueue<ImageMessage>();



        private ImageSourceConverter conv = new ImageSourceConverter();
        private BitmapSource bs = null;
        

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            ImageMessage msg=new ImageMessage();
            while (queue.TryDequeue(out msg))
            {
                MemoryStream stream=new MemoryStream(msg.bitmap);
                JpegBitmapDecoder dec = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                BitmapSource img=dec.Frames[0];
                    dc.DrawImage(img,
                    new Rect(msg.img_size.X, msg.img_size.Y,
                        msg.img_size.Width,
                        msg.img_size.Height));
                    this.Width = msg.total_img_size.Width;
                    this.Height = msg.total_img_size.Height;
                    
            }
            
        }
       
    }
}
