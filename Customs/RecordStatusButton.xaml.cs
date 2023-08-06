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

namespace SARWPF
{
    /// <summary>
    /// Interaction logic for RecordStatusButton.xaml
    /// </summary>
    public partial class RecordStatusButton : Button
    {
        public RecordStatusButton()=>InitializeComponent();

        #region IsDirtyProperty
        public static readonly DependencyProperty IsDirtyProperty
        = Sys.Binder.Register<bool, RecordStatusButton>(nameof(IsDirty), true, false, IsDirtyPropertyPropChanged);

        private static void IsDirtyPropertyPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)=>
        ((RecordStatusButton)d).UpdateContent();

        public bool IsDirty
        {
            get => (bool)GetValue(IsDirtyProperty);
            set => SetValue(IsDirtyProperty, value);
        }
        #endregion

        void UpdateContent()
        {
            if (IsDirty)
            {
                Content = "*";
                this.Foreground = Brushes.LightYellow;
            }
            else
            {
                Content = "⮞";
                this.Foreground = Brushes.Black;
            }
        }

    }
}
