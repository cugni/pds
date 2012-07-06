using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using Shared;
using System.Runtime.Serialization;

namespace Shared.Message
{
    [Serializable()]
    public class ImageMessage : ISerializable
    {
        public Rectangle img_size;
        public Rectangle total_img_size;
        public Bitmap bitmap;
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("img_size", this.img_size);
            info.AddValue("total_img_size", this.total_img_size);
            info.AddValue("bitmap", this.bitmap);
             
        }
       
    }
}
