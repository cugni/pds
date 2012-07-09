using System;
using pds2.Shared.Messages;
namespace pds2.Shared
{
    public delegate void StringMessage(String message);
    public interface IMainWindow
    {
        
        event StringMessage sendMessage;
        event ClipboardMessageDelegate shareClipboard;
        event Action shareVideo;
       
    }
}
