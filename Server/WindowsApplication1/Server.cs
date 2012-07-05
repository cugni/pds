using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Server.worker;
using System.Net;
using System.Drawing;

namespace Server
{
    public delegate void Message(String msg);
    public delegate void ReasonedCallback(string Reason,bool enable);
    public partial class ChatServer
    {
        public event ReasonedCallback ChangeClipbordStatus;
      
        //public event EventHandler connectedEvent;
        
        private readonly WorkerPool worker;

        public CaptureType captureType
        {
            get
            {
                return worker.tipoCattura;
            }
        }
        public static void handleException(Exception e)
        {
            Console.Write(e.StackTrace);
            //TODO think about a better policy
        }


        public void stop()
        {

            worker.RequestStop();
            
        }
        public void start(CaptureType captureType)
        {

           

            if (captureType.Equals(CaptureType.SCREEN_AREA))
            {
                worker.x = this.x_s;
                worker.y = this.y_s;
                worker.w = this.w_s;
                worker.h = this.h_s;
            }

            worker.tipoCattura = captureType;
            worker.RequestStart();

        }


        private string _nick, _psw;
        private int _port;
        public string nick
        {
            get
            {
                return _nick;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Insert a valid nick name");
                _nick = value;
                _set |= 0x1;
            }
        }
        public string password
        {
            get
            {
                return _psw;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Insert a valid password");
                _psw = value;
                _set |= 0x2;


            }
        }
        private int _set = 0;
        public bool setted
        {
            get
            {
                return _set == 0xF;
            }
        }
        public void setIp(string n)
        {
            try
            {
                this.ipAddress = IPAddress.Parse(n);
                _set |= 0x4;
            }
            catch
            {
                throw new ArgumentException("Insert a valid Ip address!");

            }
        }
        public int port
        {
            get
            {
                return _port;
            }
            set
            {
                if (value < 0 || value > 65535)
                    throw new ArgumentException("Invalid port value");
                _port = value;
                _set |= 0x8;
            }
        }
        private int x_s = 50, y_s = 50, w_s = 200, h_s = 200;
        public void setDimensioniParz(int x, int y, int w, int h)
        {
            x_s = x;
            y_s = y;
            w_s = w;
            h_s = h;
        }

        public Rectangle getRect()
        {
            return new Rectangle(x_s, y_s, w_s, h_s);
        }
    }
}
