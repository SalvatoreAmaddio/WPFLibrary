using Recordsource.Model;
using SAR;
using SARWPF;
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
    /// Interaction logic for ItemPanel.xaml
    /// </summary>
    public partial class ItemPanel : Page
    {
        public ItemPanel()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            LoadWin win = new LoadWin();
//            LoadingWindow win = new();
//            Form form = new((Item)((Button)sender).DataContext);
//            form.Show();
              win.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string lista=string.Empty;

            MySQLDatabaseTable<Barcode> MainDB = Sys.DatabaseManager.GetDatabaseTable<Barcode>();
            int index = 1;
            lista = index.ToString() +") " + MainDB.Source.ToString() + " " + MainDB.Source.Origin + " " + MainDB.Source.DataSetBasedOn;

            foreach (var child in MainDB.DataSource.Children)
            {
                index++;
                
                lista = lista + '\n' 
                    + index.ToString() + ") " + child.ToString() + " " + child.Origin + " " + child.DataSetBasedOn;
            }

            MessageBox.Show(lista);

        }
    }
}
