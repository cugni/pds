using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pds2.Shared.Messages;

namespace pds2.Shared
{
   
   public abstract class   IConnection
    {
         public abstract event ConnectioState connectionStateEvent;
         public abstract event TextMessageDelegate receivedMessage;
         public abstract event ImageMessageDelegate receivedVideo;
         public abstract event ClipboardMessageDelegate receivedClipboard;
         protected readonly IMainWindow mcw;
         protected volatile bool _connect = false;
         protected string _username;
         public string Username
         {
             get
             {
                 return _username;
             }
             set
             {
                 if (_connect)
                     throw new ArgumentException("It is not allowed to change settings while the pool is running");
                 _username = value;
             }
         }
         public IConnection(IMainWindow mcw)
         {
             this.mcw = mcw;
         }
         public bool IsConnect
         {
             get
             {
                 return _connect;
             }
         }
         protected bool _configured = false;
         public bool IsConfigured { get { return _configured; } }
         public abstract void Connect();
         public abstract void Disconnect();
         
    }
}
