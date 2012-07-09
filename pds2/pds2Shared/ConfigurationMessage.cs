using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pds2.Shared.Messages
{
     [Serializable]
    public class ConfigurationMessage : SendableObj<ConfigurationMessage>
    {
        public int video_port;
        public int clip_port;
        public string message;
        public bool success = false;
        public ConfigurationMessage(string message)
        {
            this.message = message;
        }
        public ConfigurationMessage(int video_port, int clip_port, string message)
        {
            success = true;
            this.video_port = video_port;
            this.clip_port = clip_port;
            this.message = message;
        }
    }
}
