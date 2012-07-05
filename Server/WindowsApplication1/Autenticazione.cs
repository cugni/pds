using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace Server
{
    public partial class Autenticazione : Form
    {
        private ChatServer server;
       
 
        public Autenticazione(ChatServer server)
        {
            InitializeComponent();
            setIpAddress();
            this.server = server;
             

        }

       
        public Autenticazione(ChatServer server, int address, string pass, string port, bool flg)
        {
            InitializeComponent();
            setIpAddress();
            this.server = server;
            txtpass.Text = pass;
            txtport.Text = port;
            txtIp.SelectedIndex = address;
          

        }


       
        private void button1_Click(object sender, EventArgs e)
        {

             try{
                server.nick=txtnick.Text;
                server.password=txtpass.Text;
                try
                {
                    server.port = int.Parse(txtport.Text);
                }
                catch (FormatException ) { throw new ArgumentException("Insert a valid port number"); };
             
            
              if (txtIp.SelectedIndex == -1)throw new ArgumentException("Select an ip address!", "Error");
               
                    server.setIp(txtIp.SelectedItem.ToString());
                    this.Close();
               
            
            }catch(ArgumentException ae){
                MessageBox.Show(ae.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //@dany modifiche
        private void setIpAddress()
        {
            string myHost = System.Net.Dns.GetHostName();

            System.Net.IPHostEntry myIPs = System.Net.Dns.GetHostEntry(myHost);

            // Loop through all IP addresses and display each

            foreach (System.Net.IPAddress myIP in myIPs.AddressList)
            {

                if (myIP.AddressFamily.ToString() == System.Net.Sockets.ProtocolFamily.InterNetwork.ToString())
                {
                     
                    txtIp.Items.Add(myIP.ToString());
                }
            }
            txtIp.DropDownStyle=ComboBoxStyle.DropDownList;
            txtIp.SelectedIndex = -1;
        }

        private void txtnick_TextChanged(object sender, EventArgs e)
        {

        }

      
    }
}