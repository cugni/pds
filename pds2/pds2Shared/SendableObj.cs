using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace pds2.Shared.Messages
{   
    [Serializable]
    public abstract class SendableObj<T>
    {
        /// <summary>
        /// This method sends the object size followed by the serialized object
        /// </summary>
        /// <param name="s">
        /// The stream where to write
        /// </param>
        public virtual void sendMe(Stream s)
        {
            BinaryFormatter b = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            b.Serialize(ms, this);
            byte[] bytes=ms.ToArray();
            byte[] size = BitConverter.GetBytes(Convert.ToInt32(bytes.Length));
            s.Write(size.Concat(bytes).ToArray(), 0, bytes.Length+4);            
        }
        /// <summary>
        /// This method sends the object size followed by the serialized object in async mode
        /// </summary>
        /// <param name="s">
        /// The stream where to write
        /// </param>
        public virtual IAsyncResult sendMeAsync(Stream s,AsyncCallback callback,Object state)
        {
            BinaryFormatter b = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
          
            b.Serialize(ms, this);
            byte[] bytes = ms.ToArray();
            byte[] size = BitConverter.GetBytes(Convert.ToInt32(bytes.Length));
            return s.BeginWrite(bytes.Concat(size).ToArray(), 0, bytes.Length + 4, callback,state);
        }
        /// <summary>
        /// It waits for a SendableObj objects on a Stream
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static T recvMe(Stream me)
        {
            byte[] size = new byte[4];
            int start = 0;
            do
            {
                int sent= me.Read(size, start, 4-start);
                if (sent == 0) throw new IOException("Stream close");
                start += sent;
            } while (start < 4);
            Int32 len = BitConverter.ToInt32(size,0);
            byte[] ob = new byte[len];
            start = 0;
            do
            {
                int sent= me.Read(ob, start, len-start);
                if (sent == 0) throw new IOException("Stream close");
                start += sent;
            } while (start < len);
            BinaryFormatter b = new BinaryFormatter();
           return (T) b.Deserialize(new MemoryStream(ob));
        }
    }
}
