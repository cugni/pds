using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows.Media;

namespace pds2.Shared
{
    public delegate void ConnectioState(bool conne);
    public class Pds2Util
    {

        public static string createPswMD5(string password, string salt)
        {
            MD5 md5 = new MD5CryptoServiceProvider();           
            List<byte> tocript = new List<byte>();
            tocript.AddRange(ASCIIEncoding.Default.GetBytes(password));
            tocript.AddRange(ASCIIEncoding.Default.GetBytes(salt));//sfida
            return BitConverter.ToString(md5.ComputeHash(tocript.ToArray()));
        }
      
        //public static Bitmap BitmapSource2Bitmap(BitmapSource bitmapImage)
        //{
        //    // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

        //    using (MemoryStream outStream = new MemoryStream())
        //    {
        //        BitmapEncoder enc = new BmpBitmapEncoder();
        //        enc.Frames.Add(BitmapFrame.Create(bitmapImage));
        //        enc.Save(outStream);
        //        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);
        //        outStream.Close();
        //        // return bitmap; <-- leads to problems, stream is closed/closing ...
        //        return new Bitmap(bitmap);
        //    }
        //}
        //public static ImageSource Bitmap2BitmapSource(Bitmap bitmap)
        //{
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        bitmap.Save(ms, ImageFormat.Png);
        //        ms.Position = 0;
        //        BitmapImage bi = new BitmapImage();
        //        bi.BeginInit();
        //        bi.StreamSource = ms;
        //        bi.EndInit();
        //        ms.Close();
        //        return bi;
        //    }
        //}



    }
}
