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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SARWPF
{
    /// <summary>
    /// Interaction logic for Combo.xaml
    /// </summary>
    public partial class Combo : ComboBox, ISelector
    {

        public ListaEventLighter Semaforo { get; set; } = new();

        public Combo()
        {
            Semaforo.GreenLight += OnGreenLight;
            InitializeComponent();
            Sys.Binder.BindUp(this, nameof(ItemsSource), Semaforo, ListaEventLighter.ItemsSourceFlagProperty, BindingMode.TwoWay, new PropertyToTriggerEventConverter(Semaforo));
            Sys.Binder.BindUp(this, nameof(AbstractListRestructurer), Semaforo, ListaEventLighter.AbstractListRestructurerFlagProperty, BindingMode.TwoWay, new PropertyToTriggerEventConverter(Semaforo));
            Sys.Binder.BindUp(this, nameof(FilterDataContext), Semaforo, ListaEventLighter.FilterDataContextFlagProperty, BindingMode.TwoWay, new PropertyToTriggerEventConverter(Semaforo));
            Sys.Binder.BindUp(this, nameof(SelectedItem), this, Combo.ShowPlaceHolderProperty,BindingMode.TwoWay, new ObjectToVisibilityConverter());
        }

        private void OnGreenLight(object? sender, SemaforoEventArgs e)
        {
            if (e.IsGreen())
            {
                AbstractListRestructurer?.SetSelector(this);
                AbstractListRestructurer?.SetDataContext(FilterDataContext);
                AbstractListRestructurer?.Run();
            }
        }

        #region FilterDataContext
        public static readonly DependencyProperty FilterDataContextProperty
        = Sys.Binder.Register<object?, Combo>(nameof(FilterDataContext), true, null, FilterDataContextPropChanged, true, true, true);

        private static void FilterDataContextPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var x = ((Combo)d).AbstractListRestructurer;
            x?.SetDataContext(e.NewValue);
            x?.RestructureLogic();
        }

        public object? FilterDataContext
        {
            get => (object?)GetValue(FilterDataContextProperty);
            set => SetValue(FilterDataContextProperty, value);
        }
        #endregion

        #region AbstractListRestructurerProperty
        public static readonly DependencyProperty AbstractListRestructurerProperty
        = Sys.Binder.Register<AbstractListRestructurer?, Combo>(nameof(AbstractListRestructurer), true, null, AbstractBaseFilterOrderPropChanged);

        public AbstractListRestructurer? AbstractListRestructurer
        {
            get => (AbstractListRestructurer?)GetValue(AbstractListRestructurerProperty);
            set => SetValue(AbstractListRestructurerProperty, value);
        }

        private static void AbstractBaseFilterOrderPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Combo)d).FilterUp();

        void FilterUp()
        {
            AbstractListRestructurer?.SetSelector(this);
            AbstractListRestructurer?.RestructureLogic();
        }
        #endregion

        #region TextInputManagerProperty
        public static readonly DependencyProperty TextInputManagerProperty
        = Sys.Binder.Register<TextInputManager, Combo>(nameof(TextInputManager), true, null);

        public TextInputManager TextInputManager
        {
            get => (TextInputManager)GetValue(TextInputManagerProperty);
            set => SetValue(TextInputManagerProperty, value);
        }
        #endregion

        #region PlaceHolderProperty
        public static readonly DependencyProperty PlaceHolderProperty
        = Sys.Binder.Register<string, Combo>(nameof(PlaceHolder), true, string.Empty);

        public string PlaceHolder
        {
            get => (string)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }
        #endregion

        #region ShowPlaceHolder
        public static readonly DependencyProperty ShowPlaceHolderProperty
        = Sys.Binder.Register<Visibility, Combo>(nameof(ShowPlaceHolder), true, Visibility.Visible);

        public Visibility ShowPlaceHolder
        {
            get => (Visibility)GetValue(ShowPlaceHolderProperty);
            set => SetValue(ShowPlaceHolderProperty, value);
        }
        #endregion

        public void BindUp(object sender, string prop) =>Sys.Binder.BindUp(sender, prop, this, ItemsSourceProperty);
    }

    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)=>
        (value==null) ? Visibility.Visible : Visibility.Hidden;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}