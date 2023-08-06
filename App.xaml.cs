using Recordsource.Model;
using SAR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Designer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

         Sys.DatabaseManager.ConnectionString = "Server=sql4.freemysqlhosting.net;Database=sql4497931;Uid=sql4497931;Pwd=2Tf1qm9dEG;Allow User Variables=true; AllowZeroDateTime=True; ConvertZeroDateTime=True;default command timeout=10000;"; ;
         Sys.DatabaseManager.AddDatabaseTable(
             new MySQLDatabaseTable<Item>(),
             new MySQLDatabaseTable<Offer>(),
             new MySQLDatabaseTable<Barcode>(),
             new MySQLDatabaseTable<Department>()
             );
        }
    }
}
