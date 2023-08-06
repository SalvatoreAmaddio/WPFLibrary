using SARWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using SAR;
using System.Windows.Data;
using System.Globalization;
using Designer.Custom;

namespace SARWPF
{
    public class FormView : Grid
    {
        protected RecordTracker Tracker=new();
        protected ProgressBar ProgressBar =new() { IsIndeterminate=true};
        protected FormContent FormContent =new();
        protected FormHeader FormHeader =new();
        protected NoWifiStackPanel stack = new();

        protected RecordStatusButton RecordStatusButton = new RecordStatusButton();

        ColumnDefinition columnRecordStatusButton = new () { Width = new(20) };
        ColumnDefinition columnContent = new () { Width = new(1, GridUnitType.Star) };
        RowDefinition rowHeader = new() { Height = new(0) };
        RowDefinition rowContent = new () { Height = new(1, GridUnitType.Star)};
        RowDefinition rowProgressBar = new () { Height = new(10) };
        RowDefinition rowTracker = new () { Height = new(30)};

        public FormView()
        {
            ColumnDefinitions.Add(columnRecordStatusButton);//0
            ColumnDefinitions.Add(columnContent);//1

            RowDefinitions.Add(rowHeader);//0
            RowDefinitions.Add(rowContent);//1
            RowDefinitions.Add(rowProgressBar);//2
            RowDefinitions.Add(rowTracker);//3

            Children.Add(RecordStatusButton);

            SetRow(RecordStatusButton, 1);
            SetColumn(RecordStatusButton, 0);

            Children.Add(ProgressBar);
            SetRow(ProgressBar, 2);
            SetColumn(ProgressBar, 0);
            SetColumnSpan(ProgressBar, 2);
                
            Children.Add(Tracker);
            SetRow(Tracker, 3);
            SetRowSpan(Tracker, 2);
            SetColumnSpan(Tracker, 2);
            
            Children.Add(stack);
            SetColumn(stack, 1);
            SetRow(stack, 3);
        
            Sys.Binder.BindUp(this, nameof(HeaderHeight), rowHeader, RowDefinition.HeightProperty);
            Sys.Binder.BindUp(this, nameof(ShowRecordStatusButton), columnRecordStatusButton, ColumnDefinition.WidthProperty,BindingMode.TwoWay, new BoolToGridLengthConverter(20));
            Sys.Binder.BindUp(this, nameof(ShowProgressBar), rowProgressBar, RowDefinition.HeightProperty, BindingMode.TwoWay, new BoolToGridLengthConverter(10));
            Sys.Binder.BindUp(this, nameof(ShowRecordTracker), rowTracker, RowDefinition.HeightProperty, BindingMode.TwoWay, new BoolToGridLengthConverter(30));
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded is FormContent)
            {
                FormContent = (FormContent)visualAdded;
                SetRow(FormContent, 1);
                SetColumn(FormContent, 1);
                object? datacontext = ((FrameworkElement)FormContent).DataContext;
                if (datacontext is IAbstractController)
                {
                    IAbstractController? abstractController = (IAbstractController)((FrameworkElement)FormContent).DataContext;
                    Sys.Binder.BindUp(abstractController, "SelectedRecord", this, ModelProperty);
                    Sys.Binder.BindUp(abstractController, "DataSource", Tracker, RecordTracker.RecordSourceProperty, BindingMode.OneWay);
                    Sys.Binder.BindUp(abstractController, "IsLoading", ProgressBar, ProgressBar.IsIndeterminateProperty, BindingMode.OneWay);
                    return;
                } 
            }

            if (visualAdded is FormHeader)
            {
                FormHeader = (FormHeader)visualAdded;
                SetRow(FormHeader, 0);
                SetColumn(FormHeader, 0);
                SetColumnSpan(FormHeader, 2);
            }
        }

        #region HeaderHeight
        public static readonly DependencyProperty HeaderHeightProperty
        = Sys.Binder.Register<GridLength, FormView>(nameof(HeaderHeight), true, GridLength.Auto, null,true,true,true);

        public GridLength HeaderHeight
        {
            get => (GridLength)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }
        #endregion

        #region ShowRecordStatusButton
        public static readonly DependencyProperty ShowRecordStatusButtonProperty
        = Sys.Binder.Register<bool, FormView>(nameof(ShowRecordStatusButton), true, true, null,true,true,true);

        public bool ShowRecordStatusButton
        {
            get => (bool)GetValue(ShowRecordStatusButtonProperty);
            set => SetValue(ShowRecordStatusButtonProperty, value);
        }
        #endregion

        #region ShowProgressBar
        public static readonly DependencyProperty ShowProgressBarProperty
        = Sys.Binder.Register<bool, FormView>(nameof(ShowProgressBar), true, true, null,true,true,true);

        public bool ShowProgressBar
        {
            get => (bool)GetValue(ShowProgressBarProperty);
            set => SetValue(ShowProgressBarProperty, value);
        }
        #endregion

        #region ShowRecordTracker
        public static readonly DependencyProperty ShowRecordTrackerProperty
        = Sys.Binder.Register<bool, FormView>(nameof(ShowRecordTracker), true, true, null,true,true,true);

        public bool ShowRecordTracker
        {
            get => (bool)GetValue(ShowRecordTrackerProperty);
            set => SetValue(ShowRecordTrackerProperty, value);
        }
        #endregion

        #region ModelProperty
        public static readonly DependencyProperty ModelProperty
        = Sys.Binder.Register<IAbstractModel?, FormView>(nameof(Model), true, null,
        (d,e) => Sys.Binder.BindUp(e.NewValue, "IsDirty", ((FormView)d).RecordStatusButton, RecordStatusButton.IsDirtyProperty)
        ,true,true,true);

        public IAbstractModel? Model
        {
            get => (IAbstractModel?)GetValue(ModelProperty);
            set 
            {
                SetValue(ModelProperty, value);
            }
        }
        #endregion

    }

    public class BoolToGridLengthConverter : IValueConverter
    {
        double defaultValue;
        object? Value;
        bool Result => System.Convert.ToBoolean(Value);

        public BoolToGridLengthConverter(double _defaultValue)=>
        defaultValue = _defaultValue;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Value=value;
            return (!Result) ? new GridLength(0) : new GridLength(defaultValue); 
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Value;
    }

    public class FormContent : Border
    {

    }

    public class FormHeader : Border
    {
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (visualAdded is Panel)
                Sys.Binder.BindUp(visualAdded, "Background", this, BackgroundProperty);
        }
    }
}