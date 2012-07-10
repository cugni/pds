using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace pds2.Shared.Messages
{
    public delegate void  ImageMessageDelegate(ImageMessage msg);
    [Serializable]
    public class ImageMessage : SendableObj<ImageMessage>
    {
        public Rectangle img_size;
        public Rectangle total_img_size;
        public byte[] bitmap;
       

    }
}
