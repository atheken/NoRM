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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NorMan.Model;

namespace NorMan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected ConnectionAttributes _connProto = new ConnectionAttributes();
        public MainWindow()
        {
            InitializeComponent();
            var connDialog = new ConnectionDialog();
            if (connDialog.ShowDialog() ?? false)
            {
                _connProto.Server = connDialog.txtServer.Text;
                _connProto.Port = Int32.Parse(connDialog.txtPort.Text);
                _connProto.User = connDialog.txtUsername.Text;
                _connProto.Password = connDialog.txtPassword.Text;
            }

        }
    }
}
