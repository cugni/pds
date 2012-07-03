﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

public class SocketforClient
{
    TcpClient socket;
    private double cont;//alby

    //Costruttore dell' oggetto socket utilizzato per la cattura del monitor
    public SocketforClient(string host, int port)
    {
        socket = new TcpClient(host, port);
        cont = 0;//alby
    }

    //Riceve dal server una serie di Bytes e li trasforma in una Bitmap
    //alby
    public bool Receive(ref Bitmap bitm)
    {
        if (socket.GetStream().DataAvailable)
        {
            cont = 0;
            Stream stm = socket.GetStream();
            IFormatter formatter = new BinaryFormatter();
            bitm = (Bitmap)formatter.Deserialize(stm);
            return true;
        }
        else
        {
            if (cont >= 15)
            {
                bitm = new Bitmap(Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width);
                return false;
            }
            cont++;
            return true;
        }
    }


}