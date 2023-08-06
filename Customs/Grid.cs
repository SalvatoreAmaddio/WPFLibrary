using SAR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace SARWPF
{
    public class Grid2 : Grid
    {


        #region RowDefinitions
        public static readonly DependencyProperty RowDefinitionProperty
        = Sys.Binder.Register<string, Grid2>(nameof(RowDefinition2), true, null, RowDefinitionPropertyChanged, true, true, true);

        private static void RowDefinitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Grid2 grid = (Grid2)d;
            grid.SetRows();
        }

        public string RowDefinition2
        {
            get => (string)GetValue(RowDefinitionProperty);
            set => SetValue(RowDefinitionProperty, value);
        }

        void SetRows()
        {
            List<string> s = RowDefinition2.Split(",").ToList();
            RowDefinitions.Clear();
            foreach (string c in s)
            {
                RowDefinitions.Add(new() { Height = DetermineGridLen(c) });
            }
        }
        #endregion

        #region ColumnDefinitions
        public static readonly DependencyProperty ColumnsDefinitionProperty
        = Sys.Binder.Register<string, Grid2>(nameof(ColumnsDefinition2), true, null, ColumnsDefinitionPropertyChanged, true, true, true);

        private static void ColumnsDefinitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Grid2 grid = (Grid2)d;
            grid.SetColumns(e.OldValue?.ToString());
        }

        public string ColumnsDefinition2
        {
            get => (string)GetValue(ColumnsDefinitionProperty);
            set => SetValue(ColumnsDefinitionProperty, value);
        }

        void SetColumns(string? old)
        {
            List<string> oldvalues = new();
            if (old!=null)
            {
                oldvalues = old.Split(",").ToList();
            }

            List<string> s = ColumnsDefinition2.Split(",").ToList();
            bool changed = oldvalues.Count != s.Count;

            if (changed)
            {
                ColumnDefinitions.Clear();
            }

            int index = 0;
            foreach (string c in s)
            {
                if (!changed)
                {
                    ColumnDefinitions[index].Width = DetermineGridLen(c);
                    index++;
                }
                else
                {
                    ColumnDefinitions.Add(new() { Width = DetermineGridLen(c) });
                }
            }
        }
        #endregion

        GridLength DetermineGridLen(string val)
        {
            GridUnitType unitType =GetUnitType(val);
            double value = ExtractNumber(val);

            if (unitType.Equals(GridUnitType.Star) && value==0)
            {
                value = 1;
            }
           return new GridLength(value,unitType);
        }

        GridUnitType GetUnitType(string val)
        {
            if (val.ToLower().Contains("auto"))
            {
                return GridUnitType.Auto;
            }

            if (val.Contains("*"))
            {
                return GridUnitType.Star;
            }

            return GridUnitType.Pixel;
        }

        double ExtractNumber(string a)
        {
            string b = string.Empty;

            for (int i = 0; i < a.Length; i++)
            {
                if (Char.IsDigit(a[i]))
                    b += a[i];
            }

            return (b.Length > 0) ? Double.Parse(b) : 0;
        }
    }
   
}
