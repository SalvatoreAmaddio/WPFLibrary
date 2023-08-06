using Designer.Controller;
using Recordsource.Model;
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

namespace Designer.View
{
    /// <summary>
    /// Interaction logic for Form.xaml
    /// </summary>
    public partial class Form : Window
    {
        BarcodeController Controller { get; }

        public Form()
        {
            InitializeComponent();
            Controller = (BarcodeController)DataContext;
        }

        public Form(Item item) : this()
        {
            Controller.Filtro2(item);
        }

    }
}
