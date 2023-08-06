using Designer.Custom;
using SAR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SARWPF
{
    public class LoadingMask : Window
    {
        Image DefaultBorderImage { get; set; } = new();
        Grid DefaultMainGrid { get; set; } = new();
        ProgressBar ProgressBar { get; set; } = new();
        Border ImageBorder { get; set; } = new();
        RowDefinition DefaultRow1 { get; set; } = new();
        RowDefinition DefaultRow2 { get; set; } = new();
        RowDefinition DefaultRow3 { get; set; } = new();

        #region RequiresInternetConnectionProperty
        public static readonly DependencyProperty RequiresInternetConnectionProperty
        = Sys.Binder.Register<bool, LoadingMask>(nameof(RequiresInternetConnection), true, true, null, true, true);

        public bool RequiresInternetConnection
        {
            get => (bool)GetValue(RequiresInternetConnectionProperty);
            set => SetValue(RequiresInternetConnectionProperty, value);
        }
        #endregion

        #region MainWindowProperty
        public static readonly DependencyProperty MainWindowProperty
        = Sys.Binder.Register<Window, LoadingMask>(nameof(MainWindow), true, null, null, true, true);

        public Window MainWindow
        {
            get => (Window)GetValue(MainWindowProperty);
            set => SetValue(MainWindowProperty, value);
        }
        #endregion

        #region ImageBorderPaddingProperty
        public static readonly DependencyProperty ImageBorderMarginProperty
        = Sys.Binder.Register<Thickness, LoadingMask>(nameof(ImageBorderMargin), true, new(0, 0, 0, 5), null, true, true);

        public Thickness ImageBorderMargin
        {
            get => (Thickness)GetValue(ImageBorderMarginProperty);
            set => SetValue(ImageBorderMarginProperty, value);
        }
        #endregion

        #region ImageBorderPaddingProperty
        public static readonly DependencyProperty ImageBorderPaddingProperty
        = Sys.Binder.Register<Thickness, LoadingMask>(nameof(ImageBorderPadding), true, new(5), null, true, true);

        public Thickness ImageBorderPadding
        {
            get => (Thickness)GetValue(ImageBorderPaddingProperty);
            set => SetValue(ImageBorderPaddingProperty, value);
        }
        #endregion

        #region ImageBorderThicknessProperty
        public static readonly DependencyProperty ImageBorderThicknessProperty
        = Sys.Binder.Register<Thickness, LoadingMask>(nameof(ImageBorderThickness), true, new(1), null, true, true);

        public Thickness ImageBorderThickness
        {
            get => (Thickness)GetValue(ImageBorderThicknessProperty);
            set => SetValue(ImageBorderThicknessProperty, value);
        }
        #endregion

        #region MainGridMarginProperty
        public static readonly DependencyProperty MainGridMarginProperty
        = Sys.Binder.Register<Thickness, LoadingMask>(nameof(MainGridMargin), true, new(5), null, true, true);

        public Thickness MainGridMargin
        {
            get => (Thickness)GetValue(MainGridMarginProperty);
            set => SetValue(MainGridMarginProperty, value);
        }
        #endregion

        #region ImageBorderCornerRadiusProperty
        public static readonly DependencyProperty ImageBorderCornerRadiusProperty
        = Sys.Binder.Register<CornerRadius, LoadingMask>(nameof(ImageBorderCornerRadius), true, new(10), null, true, true);

        public CornerRadius ImageBorderCornerRadius
        {
            get => (CornerRadius)GetValue(ImageBorderCornerRadiusProperty);
            set => SetValue(ImageBorderCornerRadiusProperty, value);
        }
        #endregion

        #region ImageBorderBrushProperty
        public static readonly DependencyProperty ImageBorderBrushProperty
        = Sys.Binder.Register<Brush, LoadingMask>(nameof(ImageBorderBrush), true, Brushes.Black, null, true, true);

        public Brush ImageBorderBrush
        {
            get => (Brush)GetValue(ImageBorderBrushProperty);
            set => SetValue(ImageBorderBrushProperty, value);
        }
        #endregion

        #region ImageBorderBackgroundProperty
        public static readonly DependencyProperty ImageBorderBackgroundProperty
        = Sys.Binder.Register<Brush, LoadingMask>(nameof(ImageBorderBackground), true, Brushes.White, null, true, true);

        public Brush ImageBorderBackground
        {
            get => (Brush)GetValue(ImageBorderBackgroundProperty);
            set => SetValue(ImageBorderBackgroundProperty, value);
        }
        #endregion

        #region ImageSourceLogoProperty
        public static readonly DependencyProperty ImageLogoSourceProperty
        = Sys.Binder.Register<ImageSource, LoadingMask>(nameof(ImageSourceLogo), true, null, null, true, true);

        public ImageSource ImageSourceLogo
        {
            get => (ImageSource)GetValue(ImageLogoSourceProperty);
            set => SetValue(ImageLogoSourceProperty, value);
        }
        #endregion

        #region Row1HeightProperty
        public static readonly DependencyProperty Row1HeightProperty
        = Sys.Binder.Register<GridLength, LoadingMask>(nameof(Row1Height), true, new(1, GridUnitType.Star), null, true, true, true);

        public GridLength Row1Height
        {
            get => (GridLength)GetValue(Row1HeightProperty);
            set => SetValue(Row1HeightProperty, value);
        }
        #endregion

        #region Row2HeightProperty
        public static readonly DependencyProperty Row2HeightProperty
        = Sys.Binder.Register<GridLength, LoadingMask>(nameof(Row2Height), true, new(50), null, true, true, true);

        public GridLength Row2Height
        {
            get => (GridLength)GetValue(Row2HeightProperty);
            set => SetValue(Row2HeightProperty, value);
        }
        #endregion

        #region Row3HeightProperty
        public static readonly DependencyProperty Row3HeightProperty
        = Sys.Binder.Register<GridLength, LoadingMask>(nameof(Row3Height), true, new(20), null, true, true, true);

        public GridLength Row3Height
        {
            get => (GridLength)GetValue(Row3HeightProperty);
            set => SetValue(Row3HeightProperty, value);
        }
        #endregion

        readonly Task SettingPrinterTask;
        readonly Task SettingCultureTask;
        readonly Task KeepTryingToConnectTask;

        public LoadingMask() 
        {
            Sys.Merge();
            KeepTryingToConnectTask = InternetConnection.TaskTryUntilConnect();
            SettingCultureTask = Sys.CultureManager.SetCulture();
            SettingPrinterTask = Sys.PrinterManager.SetPrinter();
            Title = "Welcome";   
            Height = 450;
            Width = 450;
            ResizeMode=ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            DefaultMainGrid.RowDefinitions.Add(DefaultRow1);
            DefaultMainGrid.RowDefinitions.Add(DefaultRow2);
            DefaultMainGrid.RowDefinitions.Add(DefaultRow3);

            SetProgressBar();
            CreateImageBorder();
            AddNoIternetLayer();
            AddChild(DefaultMainGrid);
            Loaded += BasicLoadingWindow_Loaded;

            Sys.Binder.BindUp(this, nameof(ImageSourceLogo),DefaultBorderImage,Image.SourceProperty);
            Sys.Binder.BindUp(this, nameof(Row1Height), DefaultRow1, RowDefinition.HeightProperty);
            Sys.Binder.BindUp(this, nameof(Row2Height), DefaultRow2, RowDefinition.HeightProperty);
            Sys.Binder.BindUp(this, nameof(Row3Height), DefaultRow3, RowDefinition.HeightProperty);

            Sys.Binder.BindUp(this, nameof(Row2Height), ProgressBar, ProgressBar.IsIndeterminateProperty, BindingMode.TwoWay, new GridLengthToBooleanConverter());

            Sys.Binder.BindUp(this, nameof(MainGridMargin), DefaultMainGrid, Grid.MarginProperty);
            
            Sys.Binder.BindUp(this, nameof(ImageBorderBackground), ImageBorder, Border.BackgroundProperty);
            Sys.Binder.BindUp(this, nameof(ImageBorderBrush), ImageBorder, Border.BorderBrushProperty);
            Sys.Binder.BindUp(this, nameof(ImageBorderCornerRadius), ImageBorder, Border.CornerRadiusProperty);
            Sys.Binder.BindUp(this, nameof(ImageBorderThickness), ImageBorder, Border.BorderThicknessProperty);
            Sys.Binder.BindUp(this, nameof(ImageBorderPadding), ImageBorder, Border.PaddingProperty);
            Sys.Binder.BindUp(this, nameof(ImageBorderMargin), ImageBorder, Border.MarginProperty);
        }

        void SetProgressBar()
        {
            ProgressBar.IsIndeterminate = true;
            DefaultMainGrid.Children.Add(ProgressBar);
            Grid.SetRow(ProgressBar, 2);
        }

        void AddNoIternetLayer()
        {
            NoWifiStackPanel stack = new() { HorizontalAlignment=HorizontalAlignment.Center};

            Border NoInternetBorder = new()
            {
                Background = ImageBorderBackground,
                BorderBrush = ImageBorderBrush,
                CornerRadius = ImageBorderCornerRadius,
                BorderThickness = ImageBorderThickness,
                Padding = new Thickness(3),
                Margin = ImageBorderMargin,
                Child = stack,
            };
            
            DefaultMainGrid.Children.Add(NoInternetBorder);
            Grid.SetRow(NoInternetBorder, 1);
        }

        void CreateImageBorder()
        {
            ImageBorder = new()
            {
                Background = ImageBorderBackground,
                BorderBrush = ImageBorderBrush,
                CornerRadius = ImageBorderCornerRadius,
                BorderThickness = ImageBorderThickness,
                Padding = ImageBorderPadding,
                Margin = ImageBorderMargin,
                Child = DefaultBorderImage,
            };
            DefaultMainGrid.Children.Add(ImageBorder);
            Grid.SetRow(ImageBorder, 0);
        }

        private async void BasicLoadingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.WhenAll(SettingCultureTask, KeepTryingToConnectTask,SettingPrinterTask);
            await KeepTryingToConnectTask;
            Row2Height = new(0);
            await Sys.DatabaseManager.FecthDatabaseTablesData();
            _ = InternetConnection.CheckingInternetConnection();
            Visibility = Visibility.Hidden;
            MainWindow.Show();
            Close();
        }
    }

    public class GridLengthToBooleanConverter : IValueConverter
    {
        object? Value;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Value = value;
            return (((GridLength)value).Value > 0) ? false : true;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)=>
        Value;
    }
}
