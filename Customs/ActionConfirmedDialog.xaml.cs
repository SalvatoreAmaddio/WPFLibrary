using SARWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SARWPF
{
    /// <summary>
    /// Interaction logic for ActionConfirmedDialog.xaml
    /// </summary>
    public partial class ActionConfirmedDialog : AbstractDialog
    {

        public ActionConfirmedDialog(string message = "",string buttonText="THANKS",string title="Changes Applied")
        {
            InitializeComponent();
            Title= title;
            if (message.Length>0) MessageText.Text = message;
            OkButton.Content=buttonText;

        }

        private void CloseClick(object sender, RoutedEventArgs e) => Close();
    }
}
