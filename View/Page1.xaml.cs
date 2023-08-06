using SAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SAR.Sys;

namespace Designer.View
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        MicrosoftPDFManager man = new();
        public Page1()
        {
            InitializeComponent();
        }

        private void SetPDFPrinter_Click(object sender, RoutedEventArgs e)
        {
            man.ChangePort();
            MessageBox.Show("Done");
        }

        private void ResetPDFPrinter_Click(object sender, RoutedEventArgs e)
        {
            man.ResetPort();
            MessageBox.Show("Done");
        }
    }
}
