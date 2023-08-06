using SAR;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Forms.VisualStyles;
using System.Collections;
using System.Globalization;
using System.Text;

namespace SARWPF
{
    #region Notes
    //ItemsPresenter? Presenter;
    //var x=ItemContainerGenerator.ContainerFromItem(Items.CurrentItem);
    //ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(x);
    #endregion

    public partial class Lista : ListView, ISelector
    {
        RowDefinition HeaderHeightRow { get; set; } = new();
        Grid HeaderGrid { get; } = new();
        ViewCell? ViewCell;
        SelectionColorBackgroundManager? LastUnselected;
        Brush SelectedColor { get => (Brush)this.FindResource("SelectedColor"); }
        public ListaEventLighter Semaforo { get; set; } = new();

        public Lista() 
        {
            InitializeComponent();
            HeaderGrid.ColumnDefinitions.Add(new() { Width = new(30) });
            Semaforo.GreenLight += OnGreenLight;
            SetValue(headersPropertyKey, new HeaderCollection());
            Sys.Binder.BindUp(this, nameof(ItemsSource), Semaforo, ListaEventLighter.ItemsSourceFlagProperty, BindingMode.TwoWay,new PropertyToTriggerEventConverter(Semaforo));
            Sys.Binder.BindUp(this, nameof(AbstractListRestructurer), Semaforo, ListaEventLighter.AbstractListRestructurerFlagProperty, BindingMode.TwoWay, new PropertyToTriggerEventConverter(Semaforo));
            Sys.Binder.BindUp(this, nameof(FilterDataContext), Semaforo, ListaEventLighter.FilterDataContextFlagProperty, BindingMode.TwoWay, new PropertyToTriggerEventConverter(Semaforo));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            HeaderBorder = (Border)Template.FindName(nameof(HeaderBorder), this);
            HeaderBorder.Child = HeaderGrid;
            HeaderHeightRow = (RowDefinition)Template.FindName(nameof(HeaderHeightRow), this);
            ViewCell = ItemTemplate.LoadContent() as ViewCell;
            if (ViewCell is null) throw new Exception("ViewCell cannot be null!");
            Sys.Binder.BindUp(ViewCell, "RecordStatusColumnGridLength", HeaderGrid.ColumnDefinitions[0], ColumnDefinition.WidthProperty, BindingMode.TwoWay);
            Sys.Binder.BindUp(this, nameof(HeaderHeight), HeaderHeightRow, RowDefinition.HeightProperty, BindingMode.TwoWay);
        }

        public ContentPresenter? GetContentPresenter(DependencyObject item)=> FindVisualChild<ContentPresenter>(item);

        private static childItem? FindVisualChild<childItem>(DependencyObject? obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject? child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem item)
                {
                    return item;
                }
                else
                {
                    childItem? childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        #region HeaderStyle
        public static readonly DependencyProperty HeaderStyleProperty
        = Sys.Binder.Register<Style, Lista>(nameof(HeaderStyle), true, null, HeaderStylePropertyChanged, true, true, true);

        private static void HeaderStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Lista)d).ApplyHeaderStyle((Style)e.NewValue);

        public Style HeaderStyle
        {
            get => (Style)GetValue(HeaderStyleProperty);
            set => SetValue(HeaderStyleProperty, value);
        }

        void ApplyHeaderStyle(Style style)
        {
            foreach (var child in HeaderGrid.Children.OfType<FrameworkElement>())
                child.Style = style;
        }
        #endregion

        #region Header
        private static readonly DependencyPropertyKey headersPropertyKey =
        Sys.Binder.RegisterKey<HeaderCollection, Lista>(nameof(Header),true,new HeaderCollection(), CollectionChanged);

        public static readonly DependencyProperty HeaderProperty = headersPropertyKey.DependencyProperty;

        public HeaderCollection Header => (HeaderCollection)GetValue(HeaderProperty);

        private static void CollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Lista)d).InGrid((HeaderCollection)e.NewValue);

        void InGrid(HeaderCollection collection)
        {
            StringBuilder defaultColumnsWidth = new StringBuilder();
            HeaderGrid.Children.Clear();
            
            foreach (var d in collection)
            {
                d.Style = HeaderStyle;
                defaultColumnsWidth.Append("*,");
                HeaderGrid.Children.Add(d);
            }

            if (!string.IsNullOrEmpty(HeaderColumnsWidth)) return;
            var len = defaultColumnsWidth.Length;
            if (len > 0) defaultColumnsWidth = defaultColumnsWidth.Remove(len - 1, 1);

            HeaderColumnsWidth=defaultColumnsWidth.ToString();
        }
        #endregion

        #region HeaderBorder
        public static readonly DependencyProperty HeaderBorderProperty = Sys.Binder.Register<Border, Lista>(nameof(HeaderBorder), true, null, null, true, true, true);

        public Border HeaderBorder
        {
            get => (Border)GetValue(HeaderBorderProperty);
            private set => SetValue(HeaderBorderProperty, value);
        }
        #endregion

        #region HeaderGridStyle
        public static readonly DependencyProperty HeaderGridStyleProperty
        = Sys.Binder.Register<Style, Lista>(nameof(HeaderGridStyle), true, null, HeaderGridStylePropertyChanged, true, true, true);

        private static void HeaderGridStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Lista)d).HeaderGrid.Style = (Style)e.NewValue;

        public Style HeaderGridStyle
        {
            get => (Style)GetValue(HeaderGridStyleProperty);
            set => SetValue(HeaderGridStyleProperty, value);
        }
        #endregion

        #region HeaderBorderStyle
        public static readonly DependencyProperty HeaderBorderStyleProperty
        = Sys.Binder.Register<Style, Lista>(nameof(HeaderBorderStyle), true, null, HeaderBorderStylePropertyChanged, true, true, true);

        private static void HeaderBorderStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Lista)d).HeaderBorder.Style = (Style)e.NewValue;

        public Style HeaderBorderStyle
        {
            get => (Style)GetValue(HeaderBorderStyleProperty);
            set => SetValue(HeaderBorderStyleProperty, value);
        }
        #endregion

        #region HeaderColumnsWidth
        public static readonly DependencyProperty HeaderColumnsWidthProperty
        = Sys.Binder.Register<string, Lista>(nameof(HeaderColumnsWidth), true, string.Empty, HeaderColumnsPropertyChanged, true, true, true);

        private static void HeaderColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Lista)d).InColumns(e.NewValue.ToString());

        public string HeaderColumnsWidth
        {
            get => (string)GetValue(HeaderColumnsWidthProperty);
            set => SetValue(HeaderColumnsWidthProperty, value);
        }

        void InColumns(string columns)
        {
            if (string.IsNullOrEmpty(columns)) return;

            var ColumnCount =HeaderGrid.ColumnDefinitions.Count;

            if (ColumnCount>1)
                HeaderGrid.ColumnDefinitions.RemoveRange(1, ColumnCount - 1);

            string[] columnsArray = columns.Split(',');

            foreach (string column in columnsArray)
                HeaderGrid.ColumnDefinitions.Add(Decipher(column));
        }

        ColumnDefinition Decipher(string value)
        {
            ColumnDefinition columnDefinition = new();
            GridLength len;

            if (value.Equals("*"))
                len = new(1, GridUnitType.Star);

            if (value.Contains("*") && value.Length > 1)
            {
                int index = value.IndexOf("*");
                value = value.Remove(index, 1);
                len = new(Convert.ToDouble(value), GridUnitType.Star);
            }


            if (double.TryParse(value, out _))
                len = new(Convert.ToDouble(value));

            columnDefinition.Width = len;
            return columnDefinition;
        }
        #endregion

        #region HeaderHeight
        public static readonly DependencyProperty HeaderHeightProperty
        = Sys.Binder.Register<GridLength, Lista>(nameof(HeaderHeight), true, GridLength.Auto, null,true,true,true);

        public GridLength HeaderHeight
        {
            get => (GridLength)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }
        #endregion

        #region FilterDataContext
        public static readonly DependencyProperty FilterDataContextProperty
        = Sys.Binder.Register<object?, Lista>(nameof(FilterDataContext), true, null, null,true,true,true);

        public object? FilterDataContext
        {
            get => (object?)GetValue(FilterDataContextProperty);
            set => SetValue(FilterDataContextProperty, value);
        }
        #endregion

        #region AbstractListRestructurerProperty
        public static readonly DependencyProperty AbstractListRestructurerProperty
        = Sys.Binder.Register<AbstractListRestructurer?, Lista>(nameof(AbstractListRestructurer), true, null, null);

        public AbstractListRestructurer? AbstractListRestructurer
        {
            get => (AbstractListRestructurer?)GetValue(AbstractListRestructurerProperty);
            set => SetValue(AbstractListRestructurerProperty, value);
        }
        #endregion

        void OnGreenLight(object? sender, SemaforoEventArgs e)
        {
            if (e.IsGreen())
            {
                AbstractListRestructurer?.SetSelector(this);
                AbstractListRestructurer?.SetDataContext(FilterDataContext);
                AbstractListRestructurer?.Run();
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (e.AddedItems.Count <= 0) return;
            IAbstractModel? model = (IAbstractModel?)e.AddedItems[0];
            if (model == null) return;
            if (model.IsNewRecord)
            {
                this.ScrollIntoView(Items[Items.Count - 1]);
                return;
            }
            this.ScrollIntoView(e.AddedItems[0]);
        }

        void OnListViewItemSelected(object sender, RoutedEventArgs e)=>LastUnselected?.ResetOriginalBackground();

        void OnListViewItemUnselected(object sender, RoutedEventArgs e)
        {

            ListViewItem item = (ListViewItem)sender;
//            LastSelected?.ResetOriginalBackground();            
            if (!this.IsKeyboardFocusWithin)
            {
                LastUnselected = new(item);
                item.Background = SelectedColor;
            }
            e.Handled = true;
            return;
        }

        public void BindUp(object sender, string prop)=>Sys.Binder.BindUp(sender, prop, this, ItemsSourceProperty);
    }

    #region SelectionColorBackgroundManager
    public class SelectionColorBackgroundManager
    {
        ListViewItem LastUnselected;
        Brush OriginalBackground;

        public SelectionColorBackgroundManager(ListViewItem _lastUnselected)
        {
            LastUnselected= _lastUnselected;
            OriginalBackground= _lastUnselected.Background.Clone();
        }

        public void ResetOriginalBackground()=>LastUnselected.Background=OriginalBackground;
    }
    #endregion

    #region ListaEventLighter
    public class ListaEventLighter : FrameworkElement {

        SemaforoEventArgs args = new();
        public event EventHandler<SemaforoEventArgs>? GreenLight;

        public void TriggerEvent() => GreenLight?.Invoke(this, args);

        public bool ItemsSourceFlag
        {
            get => (bool)GetValue(ItemsSourceFlagProperty);
            set => SetValue(ItemsSourceFlagProperty, value);
        }

        public bool AbstractListRestructurerFlag
        {
            get => (bool)GetValue(AbstractListRestructurerFlagProperty);
            set => SetValue(AbstractListRestructurerFlagProperty, value);
        }

        public bool FilterDataContextFlag
        {
            get => (bool)GetValue(FilterDataContextFlagProperty);
            set => SetValue(FilterDataContextFlagProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceFlagProperty
        = Sys.Binder.Register<bool, ListaEventLighter>(nameof(ItemsSourceFlag), true, false, FlagPropertyChanged);

        public static readonly DependencyProperty AbstractListRestructurerFlagProperty
        = Sys.Binder.Register<bool, ListaEventLighter>(nameof(AbstractListRestructurerFlag), true, false, FlagPropertyChanged);

        public static readonly DependencyProperty FilterDataContextFlagProperty
        = Sys.Binder.Register<bool, ListaEventLighter>(nameof(FilterDataContextFlag), true, false, FlagPropertyChanged);

        private static void FlagPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListaEventLighter semaforo = (ListaEventLighter)d;
            bool value= Convert.ToBoolean(e.NewValue);
            switch (e.Property.Name)
            {
                case nameof(ItemsSourceFlagProperty):
                    semaforo.args.ItemsSourceFlag = value;
                break;
                case nameof(AbstractListRestructurerFlagProperty):
                    semaforo.args.AbstractListRestructurerFlag = value;
                break;
                case nameof(FilterDataContextFlagProperty):
                    semaforo.args.FilterDataContextFlag = value;
                break;
            }

            semaforo.TriggerEvent();
        }
    }
    #endregion

    #region SemaforoEventArgs
    public class SemaforoEventArgs : EventArgs
    {
        public bool ItemsSourceFlag { get; set; } = false;
        public bool AbstractListRestructurerFlag { get; set; } = false;
        public bool FilterDataContextFlag { get; set; } = false;
        public bool IsGreen() => ItemsSourceFlag && AbstractListRestructurerFlag && FilterDataContextFlag;
    }
    #endregion

    #region ISelector
    public interface ISelector
    {
        public object? FilterDataContext { get; set; }
        public AbstractListRestructurer? AbstractListRestructurer { get; set; }
        public void BindUp(object sender, string prop);
        public ListaEventLighter Semaforo { get; set; }
    }
    #endregion

    #region Converter
    public class PropertyToTriggerEventConverter : IValueConverter
    {
        object? oldvalue;
        public ListaEventLighter semaforo;

        public PropertyToTriggerEventConverter(ListaEventLighter _semaforo)=>
        semaforo= _semaforo;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (oldvalue==null)
            {
                oldvalue = value;
            } 
            else
            {
                if (!oldvalue.Equals(value))
                {
                    oldvalue = value;
                    semaforo.TriggerEvent();
                    return true;
                }
            }

            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region HeaderCollection
    public class HeaderCollection : FreezableCollection<FrameworkElement> { }
    #endregion
}