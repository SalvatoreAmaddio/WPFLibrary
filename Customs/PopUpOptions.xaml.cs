using SAR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SAR.Sys;

namespace SARWPF
{
    public partial class PopUpOptions : Grid
    {
        OptionGrid OptionGrid = new();
        ImageSource unfilter { get => SarResources.Get<ImageSource>("UnfilteredIcon"); }
        ImageSource filter { get => SarResources.Get<ImageSource>("FilteredIcon"); }

        public PopUpOptions()
        {
            InitializeComponent();
            Binder.BindUp(this,nameof(IsDropDownOpen),TogglePopupButton, ToggleButton.IsCheckedProperty);
            Binder.BindUp(this, nameof(IsDropDownOpen), MyPopUp, Popup.IsOpenProperty);
            Binder.BindUp(this, nameof(ViewBoxContainerVisibility), OptionGrid.ClearButton, Button.VisibilityProperty);
            Binder.BindUp(this, nameof(ViewBoxContainerVisibility), ViewBoxContainer, Viewbox.VisibilityProperty);
            Binder.BindUp(this, nameof(OptionTickedCount), OptionCount, TextBlock.TextProperty);

            TogglePopupButton.Checked += (sender, e) => FilterImage.Source = filter;
            TogglePopupButton.Unchecked += SetUnfilteredImage;

            TogglePopupButton.MouseLeave += SetUnfilteredImage;
            TogglePopupButton.MouseEnter += (sender, e) => 
            {
                if (OptionTickedCount > 0) return;
                FilterImage.Source = filter;
            };

        }

        void SetUnfilteredImage(object sender, RoutedEventArgs e)
        {
            if (OptionTickedCount > 0) return;
            FilterImage.Source = unfilter;
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded is CheckBoxPanel)
            {
                OptionStack = (CheckBoxPanel)visualAdded;
                Children.Remove(OptionStack);
                Sys.Binder.BindUp(this, nameof(OptionTickedCount), OptionStack, CheckBoxPanel.OptionTickedCountProperty);
                InGrid();
                return;
            }
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        void InGrid()
        {
            OptionGrid.WelcomeOptionStack(OptionStack);

            OptionGrid.ClearAllClickEvent += (s, e) =>
            {
                foreach (object obj in OptionStack.Children)
                    ((CheckBox)obj).IsChecked = false;
            };

            OptionBorder.Child = OptionGrid;
        }

        #region ViewBoxContainerVisibility
        public static readonly DependencyProperty ViewBoxContainerVisibilityProperty
        = Sys.Binder.Register<Visibility, PopUpOptions>(nameof(ViewBoxContainerVisibility), true, Visibility.Hidden);

        public Visibility ViewBoxContainerVisibility
        {
            get => (Visibility)GetValue(ViewBoxContainerVisibilityProperty);
            set => SetValue(ViewBoxContainerVisibilityProperty, value);
        }
        #endregion

        #region OptionTicked
        public static readonly DependencyProperty OptionTickedCountProperty
        = Sys.Binder.Register<int, PopUpOptions>(nameof(OptionTickedCount), true, 0, OptionTickedCountPropertyChanged);

        private static void OptionTickedCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((PopUpOptions)d).ShowHide();

        public int OptionTickedCount
        {
            get => (int)GetValue(OptionTickedCountProperty);
            set => SetValue(OptionTickedCountProperty, value);
        }

        void ShowHide()
        {
            ViewBoxContainerVisibility = (OptionTickedCount == 0) ? Visibility.Hidden : Visibility.Visible;
            FilterImage.Source = (OptionTickedCount == 0) ? unfilter : filter;
        }
        #endregion

        #region IsDropDownOpen
        public static readonly DependencyProperty IsDropDownOpenProperty
        = Sys.Binder.Register<bool, PopUpOptions>(nameof(IsDropDownOpen), true, false);

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }
        #endregion

        #region OptionStack
        public static readonly DependencyProperty OptionStackProperty
        = Sys.Binder.Register<CheckBoxPanel, PopUpOptions>(nameof(OptionStack), true, null,null,true,true,true);

        public CheckBoxPanel OptionStack
        {
            get => (CheckBoxPanel)GetValue(OptionStackProperty);
            set => SetValue(OptionStackProperty, value);
        }
        #endregion

    }

    #region OptionGrid
    public class OptionGrid : Grid
    {
        public Button ClearButton;
        
        public event RoutedEventHandler? ClearAllClickEvent
        {
            add => ClearButton.Click += value;
            remove => ClearButton.Click -= value;
        }

        public OptionGrid()
        {
            RowDefinitions.Add(new() { Height = new(30) });
            RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
            RowDefinitions.Add(new() { Height = new(20) });

            Label header = new()
            {
                Content = "Select one or more option",
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
            };

            ClearButton = new Button()
            {
                Content = "Clear All",
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = Brushes.Transparent,
                BorderThickness = new(0),
                FontStyle = FontStyles.Italic,
            };

            Children.Add(header);
            Grid.SetRow(header, 0);

            Children.Add(ClearButton);
            Grid.SetRow(ClearButton, 2);
        }

        public void WelcomeOptionStack(CheckBoxPanel OptionStack)
        {
            Children.Add(OptionStack);
            Grid.SetRow(OptionStack, 1);
        }
    }

    #endregion

    #region CheckBoxPanel
    public class CheckBoxPanel : StackPanel
    {
        #region OptionTicked
        public static readonly DependencyProperty OptionTickedCountProperty
        = Sys.Binder.Register<int, StackPanel>(nameof(OptionTickedCount), true, 0);

        public int OptionTickedCount
        {
            get => (int)GetValue(OptionTickedCountProperty);
            set => SetValue(OptionTickedCountProperty, value);
        }

        #endregion
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded is not CheckBox)
            {
                throw new InvalidOperationException("Only Check Boxes allowed");
            }
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (visualAdded is not CheckBox) return;
            CheckBox Checkbox = (CheckBox)visualAdded;
            Checkbox.Checked += (sender, e) => {
                OptionTickedCount++;
            };

            Checkbox.Unchecked += (sender, e) => {
                OptionTickedCount--;
            };
        }
    }
    #endregion
}