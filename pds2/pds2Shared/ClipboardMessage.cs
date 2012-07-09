using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace pds2.Shared.Messages
{
    public enum ClipBoardType
    {
        TEXT=0,
        BITMAP=1,
        FILE=2
    }
    public delegate void ClipboardMessageDelegate(ClipboardMessage msg);
    [Serializable]
    public class ClipboardMessage: SendableObj<ClipboardMessage>
    {
        
        public ClipBoardType clipboardType;
        public string text;
        public Bitmap bitmap;
        public byte[] filedata;
        public string filename;
        public string username;
        public ClipboardMessage(string usrname)
        {
            this.username = usrname;
        }



    }
}
