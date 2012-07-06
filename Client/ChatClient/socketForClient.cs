using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Shared.Message;

public class SocketforClient
{
    TcpClient socket;
    private double cont;
    //Costruttore dell' oggetto socket utilizzato per la cattura del monitor
    public SocketforClient(string host, int port)
    {
        socket = new TcpClient(host, port);
        cont = 0;
    }

    //Riceve dal server una serie di Bytes e li trasforma in una Bitmap
 
    public ImageMessage Receive ()
    {
        Stream stm = socket.GetStream();
        IFormatter formatter = new BinaryFormatter();
        return (ImageMessage)formatter.Deserialize(stm);
    }


}