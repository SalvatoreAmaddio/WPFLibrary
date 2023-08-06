using SAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SARWPF
{
    public class Curtain : Grid
    {
        bool DropDownIsOpen { get; set; }=false;
        double ExpandTo { get; set; } = 200;
        string Show;
        string Hide;
        bool skip=true;
        static SolidColorBrush DefaultBackground { get=> new(Sys.GetColor("#FFF0F0F0")); } 
        Button switcher = new()
        {
            BorderThickness = new(0),
            HorizontalAlignment = HorizontalAlignment.Right, 
            ToolTip="Info",
            Background = DefaultBackground
        };

        public Curtain() 
        {
            Show = Sys.SarResources.Get<string>("First");
            Hide = Sys.SarResources.Get<string>("Last");
            switcher.Content = Hide;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Background = Brushes.White;
            RowDefinitions.Add(new RowDefinition { Height=new(33)});
            RowDefinitions.Add(new RowDefinition { Height = new(1,GridUnitType.Star) });
            switcher.Click += ToggleButtonClicked;
            Children.Add(switcher);
            SetZIndex(this,1);
            Width = 20;
            Height = 30;
            SetRow(this,0);
            SetRowSpan(this,2);

            InputManager.Current.PreProcessInput += (sender, e) =>
            {
                if (e.StagingItem.Input is MouseButtonEventArgs)
                      GlobalClickEventHandler(sender,(MouseButtonEventArgs)e.StagingItem.Input);
            };
            skip = false;
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (!skip)
            {
                if (visualAdded is not Border)
                    throw new ArgumentException("Child can only be of type Border.");

                Grid.SetRow((UIElement)visualAdded, 1);
            }
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        double ParentWindowMeasurements()
        {
            bool found = false;
            FrameworkElement baseObject=this;

            while(!found)
            {
                var parent = baseObject.Parent;
                bool IsWidow = parent is Window;

                if (IsWidow)
                {
                    Border border = (Border)Children[1];
                    return 
                        ((FrameworkElement)parent).ActualHeight
                        - RowDefinitions[0].ActualHeight
                        - border.Margin.Top - border.Margin.Bottom
                        -border.BorderThickness.Top - border.BorderThickness.Bottom;
                } 
                
                baseObject = (FrameworkElement)parent;                
            }

            throw new Exception("Failed to find Window Parent");
        }
        
        DoubleAnimation CreateAnimation()
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
            animation.EasingFunction = new ExponentialEase();
            animation.Completed += (object? sender, EventArgs e) =>
            {
                switcher.Content = (DropDownIsOpen) ? Show : Hide;
                switcher.Background = (DropDownIsOpen) ? Brushes.Transparent : DefaultBackground;
            };

            return animation;
        }

        private void ToggleButtonClicked(object sender, RoutedEventArgs e) {
            DoubleAnimation heightAnimation = CreateAnimation();
            DoubleAnimation widthAnimation = CreateAnimation();

            if (DropDownIsOpen)
            {
                widthAnimation.To = 20;
                heightAnimation.To = 33;
            } 
            else
            {
                widthAnimation.To = ExpandTo;
                heightAnimation.To = ParentWindowMeasurements();
            }

             DropDownIsOpen = !DropDownIsOpen;

             BeginAnimation(WidthProperty, widthAnimation);
             BeginAnimation(HeightProperty, heightAnimation);
        }

        void GlobalClickEventHandler(object sender, EventArgs e)
        {
            if (!IsMouseOver && DropDownIsOpen)
                ToggleButtonClicked(sender, new());
        }
    
    }
}
