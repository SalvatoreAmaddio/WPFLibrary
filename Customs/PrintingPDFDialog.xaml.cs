using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace SARWPF {

    public partial class PrintingPDFDialog : AbstractDialog
    {
        bool progressCompleted = false;
        Func<ProgressBar, Task> ProgressLogic;

        public PrintingPDFDialog(Func<ProgressBar, Task> logic)
        {
            ProgressLogic = logic;
            InitializeComponent();
            Closing += async (sender, e) =>
            {
                if (!progressCompleted) await ShowThenHideProgressText();
                e.Cancel = !progressCompleted;
            };
        }

        public async Task ShowMe()
        {
            Show();
            await ProgressLogic(Pb);
            progressCompleted = true;
            Close();
        }

        async Task ShowThenHideProgressText()
        {
            ProgressText.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            ProgressText.Visibility = Visibility.Hidden;
        }
    }
}