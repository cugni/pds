using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using Shared;
using System.Runtime.Serialization;

namespace Shared.Message
{
    [Serializable]
    public class ImageMessage {
        public Rectangle img_size;
        public Rectangle total_img_size;
        public Bitmap bitmap;

       
    }
}
