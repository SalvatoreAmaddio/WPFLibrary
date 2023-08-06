using MvvmHelpers.Commands;
using SAR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static SAR.Sys;

namespace SARWPF
{
    /// <summary>
    /// Interaction logic for SearchBox.xaml
    /// </summary>
    public partial class SearchBox : TextBox
    {

        public SearchBox()
        {
            InitializeComponent();

            ContextMenu = new CustomContextMenu(new() {
                new(SarResources.Get<ImageSource>("Copy"), new Command(Copy)),//0
                new(SarResources.Get<ImageSource>("Cut"), new Command(Cut)),//1
                new(SarResources.Get<ImageSource>("Paste"), new Command(Paste)),//2
                new(SarResources.Get<ImageSource>("Selection"), new Command(SelectAll)),//3

                new(SarResources.Get<ImageSource>("CopyAll"), new Command(CopyAll)),//4

                new(SarResources.Get<ImageSource>("UndoRes"), new Command(UndoMe)),//5
                new(SarResources.Get<ImageSource>("Redo"), new Command(RedoMe))//6
            });

            Sys.Binder.BindUp(this, nameof(Text), this, ShowPlaceHolderProperty, BindingMode.TwoWay, new StringToPlaceHolderVisibilityConverter());
            Sys.Binder.BindUp(this, nameof(IsHyperlink), this, ForegroundProperty, BindingMode.TwoWay, new BoolToBrushConverter());
            Sys.Binder.BindUp(this, nameof(IsHyperlink), this, CursorProperty, BindingMode.TwoWay, new BoolToCursorConverter());

            Sys.Binder.MultiBindUp(
                            this,
                            ShowClearButtonProperty,
                            new StringAndBoolToVisibilityConverter(),
                            Sys.Binder.QuickBindUp(this,nameof(Text)),
                            Sys.Binder.QuickBindUp(this, nameof(IsFocused),BindingMode.OneWay)
                            );
        }

        #region TextInputManager
        public static readonly DependencyProperty TextInputManagerProperty
        = Sys.Binder.Register<TextInputManager, SearchBox>(nameof(TextInputManager), true, null);

        public TextInputManager TextInputManager
        {
            get => (TextInputManager)GetValue(TextInputManagerProperty);
            set => SetValue(TextInputManagerProperty, value);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            if (TextInputManager == null) return;

            if (!TextInputManager.AllowInput(e.Text))
            {
                e.Handled = true;
                return;
            }
        }
        #endregion

        #region ShowClearButtonProperty
        public Visibility ShowClearButton
        {
            get => (Visibility)GetValue(ShowClearButtonProperty);
            set => SetValue(ShowClearButtonProperty, value);
        }

        public static readonly DependencyProperty ShowClearButtonProperty
        = Sys.Binder.Register<Visibility, SearchBox>(nameof(ShowClearButton), true, Visibility.Hidden);
        #endregion

        #region ShowPlaceHolder
        public static readonly DependencyProperty ShowPlaceHolderProperty
        = Sys.Binder.Register<Visibility, SearchBox>(nameof(ShowPlaceHolder), true, Visibility.Visible);

        public Visibility ShowPlaceHolder
        {
            get => (Visibility)GetValue(ShowPlaceHolderProperty);
            set => SetValue(ShowPlaceHolderProperty, value);
        }
        #endregion

        #region IsHyperlink
        public static readonly DependencyProperty IsHyperlinkProperty
        = Sys.Binder.Register<bool, SearchBox>(nameof(IsHyperlink), true, false);

        public bool IsHyperlink
        {
            get => (bool)GetValue(IsHyperlinkProperty);
            set => SetValue(IsHyperlinkProperty, value);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (!IsHyperlink) return;
            Uri? uri;
            if (Uri.TryCreate(Text, UriKind.Absolute, out uri))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = uri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
        }
        #endregion

        #region PlaceHolder
        public static readonly DependencyProperty PlaceHolderProperty
        = Sys.Binder.Register<string, SearchBox>(nameof(PlaceHolder), true, string.Empty);

        public string PlaceHolder
        {
            get => (string)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }
        #endregion

        #region CommandFunctions
        void RedoMe() => Redo();
        void UndoMe() => Undo();    
        void CopyAll()
        {
            SelectAll();
            Copy();
        }
        #endregion

        #region OnTextChangedAndComboBoxManagement
        protected override void OnTextChanged(TextChangedEventArgs e)
        {           
            base.OnTextChanged(e);
            bool isokay = CheckInputInComboBox();
            
            if (!isokay)
                Dispatcher.BeginInvoke(new Action(() => Undo()));
            
        }        

        bool CheckInputInComboBox()
        {
            if (TemplatedParent is Combo)
            {
                string temptext = $"{Text}".ToLower();
                Combo combo = (Combo)TemplatedParent;
                var list = combo.ItemsSource.Cast<object>().ToList();
                var found = list.Any(s =>FindMe(s));
                //if (!found)
                //{
                //    MessageBox.Show($"The input '{temptext.ToUpper()}' is not valid.", "INVALID INPUT");
                //    return false;
                //}
            }
            return true;
        }

        bool FindMe(object? s)
        {
            if (s == null) return true;
            string? x = s?.ToString();
            if (x == null) return true;
            return x.ToLower().StartsWith($"{Text}".ToLower());
        }
        #endregion

        #region ClearButtonEventAndFocusManagement        
        private void RoundButtonClicked(object sender, RoutedEventArgs e)=>
        Text = string.Empty;

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus != null && e.NewFocus is Button) 
                Text = string.Empty;            
        }
        #endregion
    }

    #region TextInputManagerClass
    public abstract class TextInputManager
    {
        public abstract bool AllowInput(string text);
    }
    #endregion

    #region PairsClass
    public class ObjectAndCommandPair
    {
        public object? Resource { get; set; }
        public ICommand Command { get; set; }

        public ObjectAndCommandPair(object? resource, ICommand command)
        {
            Resource = resource;
            Command = command;
        }
    }
    #endregion

    #region CustomContextMenuClass
    public class CustomContextMenu : ContextMenu
    {
        public CustomContextMenu(List<ObjectAndCommandPair> ResourceList)
        {
            AddChild(MakeMenuItem("_Copy", (ImageSource?)ResourceList[0]?.Resource, ResourceList[0].Command));
            AddChild(MakeMenuItem("_Cut", (ImageSource?)ResourceList[1]?.Resource, ResourceList[1].Command));
            AddChild(MakeMenuItem("_Paste", (ImageSource?)ResourceList[2]?.Resource, ResourceList[2].Command));
            AddChild(new Separator());            

            AddChild(MakeMenuItem("_Select All", (ImageSource?)ResourceList[3]?.Resource, ResourceList[3].Command));
            AddChild(new Separator());

            AddChild(MakeMenuItem("_Copy All", (ImageSource?)ResourceList[4]?.Resource, ResourceList[4].Command));
            AddChild(new Separator());

            AddChild(MakeMenuItem("_Undo", (ImageSource?)ResourceList[5]?.Resource, ResourceList[5].Command));
            AddChild(MakeMenuItem("_Redo", (ImageSource?)ResourceList[6]?.Resource, ResourceList[6].Command));

        }

        MenuItem MakeMenuItem(string header,ImageSource? source,ICommand? command)=>
        new()
            {
                Header=header,
                Icon=new Image() { Source=source},
                Command=command,          
            };            
        }
    #endregion

    #region StringToClearButtonVisibilityConverterClass
    public class StringAndBoolToVisibilityConverter : IMultiValueConverter
    {
        object[]? Values;
        string? Text;
        bool IsFocused;
        bool HasText => Text?.Length > 0;
        bool Show => HasText && IsFocused;
        bool Hide => HasText && !IsFocused;

        public Visibility VisibilityResult 
        { 
            get
            {
                if (Show)
                    return Visibility.Visible;
                
                if (Hide)
                    return Visibility.Hidden;
                
                return Visibility.Hidden;
            }
        }
      
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Text = values[0].ToString();
            IsFocused = System.Convert.ToBoolean(values[1]);
            return VisibilityResult;
        }

        public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)=>
        Values;
    }
    #endregion

    #region StringToPlaceHolderVisibilityConverterClass
    public class StringToPlaceHolderVisibilityConverter : IValueConverter 
    {
        string? Text;
        Visibility VisibilityResult => (Text?.Length > 0) ? Visibility.Hidden : Visibility.Visible;

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Text = value?.ToString();
            return VisibilityResult;
        }

        public virtual object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        =>Text;
    }
    #endregion

    #region BoolToForegroundConverter
    public class BoolToBrushConverter : IValueConverter
    {
        object? Value;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Value = value;
            bool result = System.Convert.ToBoolean(value);
            return (result) ? Brushes.RoyalBlue : Brushes.Black; 
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)=> Value;
    }
    #endregion

    #region
    public class BoolToCursorConverter : IValueConverter
    {
        object? Value;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Value = value;
            bool result = System.Convert.ToBoolean(value);
            return (result) ? Cursors.Hand : Cursors.Arrow;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>Value;
    }
    #endregion
}