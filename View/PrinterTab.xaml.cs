using Designer.Custom;
using SAR;
using SARWPF;
using System;
using System.CodeDom;
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
using System.Windows.Shapes;

namespace Designer.View
{
    public partial class PrinterTab : Window
    {
        public PrinterTab()
        {
            InitializeComponent();
            Combo.ItemsSource = Sys.PrinterManager.AllPrinters();
            Combo.SelectedItem = Sys.PrinterManager.DefaultPrinter;
        }

        private void ConfirmClick(object sender, RoutedEventArgs e)
        {
            if (Combo is null || Combo.SelectedItem is null || Combo.SelectedItem.ToString() is null)
            {
                ErrorDialog errorDialog = new("No printer has been selected.");
                errorDialog.ShowDialog();
                return;
            }

            Sys.JSONManager.FileName = "PrinterSetting";
            Sys.JSONManager.WriteAsJSON<string>(Combo.SelectedItem.ToString());
            Sys.PrinterManager.DefaultPrinter = Combo.SelectedItem.ToString();
            this.CloseAndOpen(new ActionConfirmedDialog(),true);
        }

    }
}
