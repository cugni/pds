using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;

namespace pds2.ClientSide
{
    /// <summary>
    /// Logica di interazione per Impostazioni.xaml
    /// </summary>
    public partial class Impostazioni : Window
    {
        private Client client;
        public Impostazioni(Client cli)
        {
            this.client = cli;
            InitializeComponent();
        }

        private void username_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void err(String msg)
        {
            errorText.Text = msg;
            errorText.Visibility = Visibility.Visible;

        }
        private void salva_Click(object sender, RoutedEventArgs e)
        {
            if (username.Text.Length == 0)
            {
                err("inserire un usrname");
                return;
            }
            if (pasword.Password.Length == 0)
            {
                err("inserire un password");
                return;
            }
            if (this.serverip.Text.Length == 0)
            {
                err("inserire un serverip");
                return;
            }
            int port=0;
            if (this.porta.Text.Length == 0&& int.TryParse(porta.Text,out port))
            {
                err("inserire una porta valida");
                return;
            }
            port = int.Parse(porta.Text);
            try
            {
                client.configure(username.Text, pasword.Password, serverip.Text, port);

                this.Close();
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show(ae.Message, "ERRORE", MessageBoxButton.OK);
            }
        }

        
        private void annulla_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        

        
    }
}
