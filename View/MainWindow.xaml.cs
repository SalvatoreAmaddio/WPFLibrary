using Designer.View;
using Recordsource.Model;
using SAR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Designer
{
    public partial class MainWindow : Window
    {
        public MainWindow()=>InitializeComponent();


        private void OpenLanguageTab(object sender, RoutedEventArgs e)
        {
            LangTab tab = new();
            tab.ShowDialog();           
        }

        private void OpenPrintersTab(object sender, RoutedEventArgs e)
        {
            PrinterTab printerTab = new();
            printerTab.ShowDialog();
        }

    }

}
