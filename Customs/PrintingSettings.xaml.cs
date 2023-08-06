using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static SAR.Sys;

namespace SARWPF
{
    /// <summary>
    /// Interaction logic for PrintingSettings.xaml
    /// </summary>
    public partial class PrintingSettings : Window
    {
        MicrosoftPDFManager microsoftPDFManager;

        public PrintingSettings(ref MicrosoftPDFManager PDFManager) 
        {
            InitializeComponent();
            microsoftPDFManager= PDFManager;
        }

        private void ConfirmClicked(object sender, RoutedEventArgs e)
        {

            if (FileName.Text.Length==0)
            {
                ErrorDialog errorDialog = new("You must specify a file name");
                errorDialog.ShowDialog();
                return;
            }

            if (FileNamePath.Text.Length == 0)
            {
                ErrorDialog errorDialog = new("You must specify a file path");
                errorDialog.ShowDialog();
                return;
            }

            microsoftPDFManager.SetFileName($"{FileNamePath.Text}\\{FileName}.pdf");
            Close();
        }

        private void PickPathButtonClicked(object sender, RoutedEventArgs e)
        {

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select where to save the pdf.";
                dialog.UseDescriptionForTitle = true;
                DialogResult result = dialog.ShowDialog();
                if (result.ToString().Equals("OK"))
                {
                    FileNamePath.Text = dialog.SelectedPath;
                }
            }


        }
    }
}
