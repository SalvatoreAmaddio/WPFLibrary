using Designer.Custom;
using SAR;
using SARWPF;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// <summary>
    /// Interaction logic for LangTab.xaml
    /// </summary>
    public partial class LangTab : Window
    {
        Country? country;
        
        public LangTab()
        {
            InitializeComponent();          
            Combo.ItemsSource = WorldMap.Countries;
            Combo.SelectedItem = Sys.CultureManager.DefaultCountry;
        }

        private void ConfirmClick(object sender, RoutedEventArgs e)
        {
            if (Combo.SelectedItem == null)
            {
                ErrorDialog errorDialog = new("No country has been selected.");
                errorDialog.ShowDialog();
                return;
            }

            Sys.JSONManager.FileName="Setting";
            Sys.JSONManager.WriteAsJSON<Country>((Country)(Combo.SelectedItem));
            this.CloseAndOpen(new ActionConfirmedDialog("Softwares'settings\r\nhave been successfully saved!\r\nRestart the software to apply the changes.", "GOT IT", "Changes Saved"),true);
        }

        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            country = (Country)Combo.SelectedItem;
            Display.Content = (country == null) ? "??" : $"Currency: {country.Currency.CurrencySymbol}";                        
        }

    }
}