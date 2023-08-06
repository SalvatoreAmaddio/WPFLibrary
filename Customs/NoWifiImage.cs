using Newtonsoft.Json.Linq;
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
using static SAR.Sys;

namespace Designer.Custom
{
    public class NoWifiStackPanel : StackPanel
    {
        NoWifiLabel NoWifiLabel = new();

        public NoWifiStackPanel() 
        {
            Orientation = Orientation.Horizontal;
            HorizontalAlignment = HorizontalAlignment.Right;
            VerticalAlignment = VerticalAlignment.Center;
            Children.Add(new NoWifiImage());
            Children.Add(NoWifiLabel);
            this.Visibility = Visibility.Hidden;
            InternetConnection.StatusEvent += InternetConnection_Status_Event;
        }

        private void InternetConnection_Status_Event(object? sender, InternetConnection.ConnectionCheckerArgs e)
        {            
            Visibility = Convert.ToBoolean(e.IsConnected) ? Visibility.Hidden : Visibility.Visible;
            NoWifiLabel.Content= e.Message;
        }

    }

    public class NoWifiImage : Image
    {
        public NoWifiImage() 
        {
            Source = SarResources.Get<ImageSource>("NoWifi");
            VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Stretch =System.Windows.Media.Stretch.Uniform;
            ToolTip = "Icon by Kyoz";
            Margin = new(0, 0, 5, 0);
        }
    }

    public class NoWifiLabel : Label
    {
        public NoWifiLabel()
        {
            Padding = new(0);
            Foreground = Brushes.Red;
            FontWeight = FontWeights.Bold;
            VerticalAlignment = VerticalAlignment.Center;
            VerticalContentAlignment = VerticalAlignment.Center;
            FontSize = 20;
        }
    }
}
