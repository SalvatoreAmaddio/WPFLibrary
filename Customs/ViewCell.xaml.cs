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
    /// Interaction logic for ViewCell.xaml
    /// </summary>
    public partial class ViewCell : Grid
    {
        public GridLength RecordStatusColumnGridLength { get; set; } = new(20);
        public ColumnDefinition RecordStatusColumn { get; set; }
        ColumnDefinition column2 = new ColumnDefinition() { Width = new(1, GridUnitType.Star) };
        protected RecordStatusButton RecordStatusButton = new();

        public ViewCell()
        {
            InitializeComponent();
            RecordStatusColumn = new() { Width = RecordStatusColumnGridLength };
            ColumnDefinitions.Add(RecordStatusColumn);
            ColumnDefinitions.Add(column2);
            Children.Add(RecordStatusButton);
            SetRow(RecordStatusButton, 0);
            SetColumn(RecordStatusButton, 0);
        }

        public bool IsSuitable()=>Children[1] is Grid;

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (visualAdded is RecordStatusButton) return;
            FrameworkElement element = (FrameworkElement)visualAdded;
            SetColumn(element, 1);
            Sys.Binder.BindUp(element.DataContext, "IsDirty", RecordStatusButton, RecordStatusButton.IsDirtyProperty);
        }


    }
}
