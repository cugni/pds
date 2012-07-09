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



        
        protected override void OnRender(DrawingContext dc)
        {
            ImageMessage msg=new ImageMessage();
            if (queue.TryDequeue(out msg))
            {
                    //dc.DrawImage(Pds2Util.Bitmap2BitmapSource(msg.bitmap),
                    //new Rect(msg.img_size.X, msg.img_size.Y,
                    //    msg.img_size.Width,
                    //    msg.img_size.Height));
            }
            else
            {
                base.OnRender(dc);
            }
        }
       
    }
}
