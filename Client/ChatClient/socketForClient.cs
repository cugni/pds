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
    public TcpClient socket;
    private double cont;
    //Costruttore dell' oggetto socket utilizzato per la cattura del monitor
    public SocketforClient(string host, int port)
    {
        socket = new TcpClient(host, port);
        cont = 0;
    }

     


}