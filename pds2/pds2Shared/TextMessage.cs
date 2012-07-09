using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace pds2.Shared.Messages
{
    public enum MessageType
    {
        ADMIN = 0,
        TEXT = 1,
        USER_JOIN = 2,
        USER_LEAVE = 3,
        DISCONNECT = 4,
        CLIENT_LOGIN = 5,
        VIDEO_START = 6,
        VIDEO_STOP = 7
    }
  
    public delegate void TextMessageDelegate(TextMessage msg);
      [Serializable]
    public class TextMessage : SendableObj<TextMessage>
    {
        public MessageType messageType;
        public String username;
        public String message;
        public TextMessage(){
        }

        public TextMessage(MessageType messageType, String meta, String message)
        {
            this.messageType = messageType;
            this.username = meta;
            this.message = message;
        }


    }
}
