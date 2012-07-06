using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Threading;
using System.Runtime.InteropServices;
using Shared.Message;

namespace Server.worker{
  
   public abstract class Worker{
      
        protected  Point p;
        private  WorkerPool father;
        private int nMilli=1000;
        private Bitmap oldBitmap;
        protected static IntPtr m_HBitmap;
        protected Cursor cursor = Cursors.Arrow;
        protected Rectangle img_size;
        protected Rectangle tot_img_size;
        
        public Worker(WorkerPool father){
             
            this.father=father;
        }
        
        public void doWork(Object nada){
            int loops = 0;
              while (!father._shouldStop){
                 
                  Bitmap bmpScreenshot =getBitmap();
                  if (isModifiedBitmap(bmpScreenshot))
                  {
                      ImageMessage msg = new ImageMessage();
                      msg.bitmap = bmpScreenshot;
                      msg.total_img_size = tot_img_size;
                      msg.img_size = tot_img_size; //TODO finché non suddivido
                      msg.img_size.X = 0;
                      msg.img_size.Y= 0;
                      ThreadPool.QueueUserWorkItem(new WaitCallback(father.server.SendImmage), msg);
                      Console.WriteLine("sends: " + loops++);
                  }
                  
                   Thread.Sleep(nMilli);
              }
        }
       protected abstract Bitmap getBitmap();
       [DllImport("msvcrt.dll", CallingConvention = CallingConvention.StdCall)]
       static extern int memcmp(IntPtr b1, IntPtr b2, int count);
       private bool isModifiedBitmap(Bitmap a)
        {
            if (oldBitmap == null||!a.Size.Equals(oldBitmap.Size))
            {
                oldBitmap = a;
                return true;
            }
                // Create a new bitmap.
                // Lock the bitmap's bits. 
                bool modified = false;
                Rectangle rect = new Rectangle(0, 0, a.Width, a.Height);
                BitmapData bmpData =
                    a.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    a.PixelFormat);
                BitmapData old =
                         oldBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                         oldBitmap.PixelFormat);
               int ret=memcmp(bmpData.Scan0,old.Scan0, old.Height*old.Width*4);
               Console.WriteLine("===>"+ret);
                if (ret != 0)
                    modified = true;
                a.UnlockBits(bmpData);
                oldBitmap.UnlockBits(old);
                return modified;

               
         }
        protected void creaBitmapCursore(Graphics g,int cursorX, int cursorY)
        {            Rectangle rCursor = new Rectangle(cursorX, cursorY, cursor.Size.Width, cursor.Size.Height);
            cursor.Draw(g, rCursor);
        }
        protected bool SetCursor(int x_new, int y_new, int w_new, int h_new)
        {
            if ((p.X >= x_new) && (p.Y >= y_new) && (p.X <= (x_new + w_new)) && (p.Y <= (y_new + h_new)))
            {
                p.X = p.X - x_new;
                p.Y = p.Y - y_new;
                return true;
            }
            return false;
        }
      
   }
    class FullScreenWorker: Worker{
        public FullScreenWorker(WorkerPool father):base(father){
            this.tot_img_size = Screen.PrimaryScreen.Bounds;
        }

        protected override Bitmap getBitmap()
        {
           
                p = Cursor.Position;
                Graphics gfxScreenshot = null;
                Bitmap bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                 Screen.PrimaryScreen.Bounds.Height,
                                 PixelFormat.Format32bppArgb);


                  
                        // Create a graphics object from the bitmap. 
                        gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                        // Take the screenshot from the upper left corner to the right bottom corner. 
                        gfxScreenshot.CopyFromScreen(tot_img_size.X,
                                                     tot_img_size.Y,
                                                     0,
                                                     0,
                                                     tot_img_size.Size,
                                                     CopyPixelOperation.SourceCopy);                       
                        creaBitmapCursore(gfxScreenshot, p.X, p.Y);
                        //catturo la finestra attiva in questo istante
                gfxScreenshot.Dispose();
                return bmpScreenshot;
        }
     }

    
    class ScreenAreaWorker : Worker{
      
        public ScreenAreaWorker(WorkerPool father,int p_x, int p_y, int w, int h):base(father){
            tot_img_size.X= p_x;
            tot_img_size.Y = p_y;
            tot_img_size.Width = w;
            tot_img_size.Height = h;
            point_s= new Point(p_x, p_y);
        }
         
        
          private readonly Point point_s;
  
           protected override Bitmap getBitmap()
        {

           
                p = Cursor.Position;
                Graphics gfxScreenshot = null;
                Bitmap bmpScreenshot = new Bitmap(tot_img_size.Width,
                                 tot_img_size.Height,
                                 PixelFormat.Format32bppArgb);
                Size size = tot_img_size.Size;
            // Create a graphics object from the bitmap. 
            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            // Take the screenshot from the upper left corner to the right bottom corner. 
            gfxScreenshot.CopyFromScreen(point_s.X,
                                        point_s.Y,
                                        0,
                                        0,
                                        size,
                                        CopyPixelOperation.SourceCopy);

            if (SetCursor(point_s.X, point_s.Y, size.Width, size.Height))
                creaBitmapCursore(gfxScreenshot, p.X, p.Y);
                gfxScreenshot.Dispose();
              
              return bmpScreenshot;
             
        }
    }
    class ActiveWindowWorker : Worker
    {
    
      
        protected Pen pen;
        
       
        
      
           
        public const Int32 CURSOR_SHOWING = 0x00000001;
        public ActiveWindowWorker(WorkerPool father):base(father) {
            pen = new Pen(Brushes.Red);
            pen.Width = 2.0F;
           
        }
           // This method will be called when the thread is started.
         protected override Bitmap getBitmap()   {

           
                p = Cursor.Position;

                        RECT rct = GetForegroundWindow();

                        tot_img_size.Width = Math.Abs(rct.Left - rct.Right);
                        tot_img_size.Height = Math.Abs(rct.Bottom - rct.Top);

                        Bitmap bmpScreenshot = new Bitmap(tot_img_size.Width,
                                         tot_img_size.Height,
                                         PixelFormat.Format32bppArgb);

                            if (rct.Bottom > Screen.PrimaryScreen.WorkingArea.Bottom)
                                tot_img_size.Height = Math.Abs(Screen.PrimaryScreen.WorkingArea.Bottom - rct.Top);

                            // Create a graphics object from the bitmap. 
                           Graphics    gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                            // Take the screenshot from the upper left corner to the right bottom corner. 
                            gfxScreenshot.CopyFromScreen(rct.Left,
                                                        rct.Top,
                                                        0,
                                                        0,
                                                        tot_img_size.Size,
                                                        CopyPixelOperation.SourceCopy);
                            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                            if (SetCursor(rct.Left, rct.Top, tot_img_size.Width, tot_img_size.Height))
                            {
                                creaBitmapCursore(gfxScreenshot,p.X, p.Y);
                            }

                        
                        //catturo una porzione dello schermo
                   
                gfxScreenshot.Dispose();
            return bmpScreenshot;
        }

       
        public static RECT GetForegroundWindow()
        {

            IntPtr hwnd = WIN32_API.GetForegroundWindow();

            RECT rct;

            WIN32_API.GetWindowRect(hwnd, out rct);

            return rct;

        }
    
 
}
    }

