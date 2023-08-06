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
    public partial class Frame : Border
    {
        public Frame()
        {
            InitializeComponent();
            Sys.Binder.BindUp(this, nameof(ImageSource), Picture, Image.SourceProperty);
        }

        public static readonly DependencyProperty ImageSourceProperty
        = Sys.Binder.Register<ImageSource, Frame>(nameof(ImageSource), true, null,null,true,true,true);

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

    }
}
