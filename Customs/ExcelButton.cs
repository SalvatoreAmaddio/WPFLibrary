using SAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SARWPF
{
    public abstract class ParentButton : Button
    {
        private Image Image { get; set; } = new();

        public static readonly DependencyProperty ButtonImageSourceProperty
        = Sys.Binder.Register<ImageSource, ParentButton>(nameof(ButtonImageSource), true, null, null, true, true, true);

        public ImageSource ButtonImageSource
        {
            get => (ImageSource)GetValue(ButtonImageSourceProperty);
            set => SetValue(ButtonImageSourceProperty, value);
        }
        
        protected ParentButton(string tooltip, string key) 
        {
            Padding = new(0);
            Margin = new(0);
            Binding bin = new(nameof(DataContext));
            bin.RelativeSource = RelativeSource.Self;
            bin.BindsDirectlyToSource = true;
            BindingOperations.SetBinding(this, CommandParameterProperty, bin);
            Height = 30;
            AddChild(Image);
            Sys.Binder.BindUp(this, nameof(ButtonImageSource), Image, Image.SourceProperty);
            ToolTip = tooltip;
            ButtonImageSource = Sys.SarResources.Get<ImageSource>(key);
        }
    }

    public class OpenButton : ParentButton
    {
        public OpenButton() : base("Open Record", "OpenFolderIcon")
        {
        }
    }

    public class DeleteButton : ParentButton
    {
        public DeleteButton() : base("Delete Record", "DeleteIcon")
        {
        }
    }

    public class SaveButton : ParentButton
    {
        public SaveButton() : base("Save Record","SaveIcon")
        {
        }
    }

    public class ExcelButton : ParentButton
    {
        public ExcelButton() : base("Excel", "ExcelIcon")
        {
        }
    }
}
