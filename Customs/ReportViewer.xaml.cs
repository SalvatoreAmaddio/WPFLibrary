using SAR;
using SARWPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using static SAR.Sys;

namespace SARWPF
{
    public partial class ReportViewer : FormView, INotifyPropertyChanged
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean SetDefaultPrinter(String name);
        public event PropertyChangedEventHandler? PropertyChanged;

        #region PageTrackerProperty
        public static readonly DependencyProperty PageTrackerProperty
        = Sys.Binder.Register<string, ReportViewer>(nameof(PageTracker), true, "No Pages", null, true, true, true);

        public string PageTracker
        {
            get => (string)GetValue(PageTrackerProperty);
            set => SetValue(PageTrackerProperty, value);
        }
        #endregion

        #region SelectedPageProperty
        IPageReport? _selectedPage;
        public IPageReport? SelectedPage
        {
            get => _selectedPage;
            set
            {
                OnPropertyChanged(ref value, ref _selectedPage,nameof(SelectedPage));
                OnPageAddedEvent(this, new(value, PagesSource.Count));
            }
        }
        #endregion

        ListOfPages _pagesSource = new();
        public ListOfPages PagesSource { get => _pagesSource; set => _pagesSource = value; }

        double _boxviewWidth = 900;
        double _boxviewHeight = 900;

        public double BoxViewWidth
        {
            get => _boxviewWidth;
            set=> OnPropertyChanged(ref value, ref _boxviewWidth,nameof(BoxViewWidth));
        }

        public double BoxViewHeight
        {
            get => _boxviewHeight;
            set=> OnPropertyChanged(ref value, ref _boxviewHeight,nameof(BoxViewHeight));
        }
        MicrosoftPDFManager microsoftPDFManager = new();

        public ReportViewer(List<IPageReport> pages)
        {
            InitializeComponent();
            PagesSource.PageAddedEvent += OnPageAddedEvent;
            SetDefaultPrinter(Sys.PrinterManager.DefaultPrinter);
            PrinterComboBox.ItemsSource = Sys.PrinterManager.AllPrinters();
            PrinterComboBox.SelectedItem = Sys.PrinterManager.DefaultPrinter;
            Children.Remove(Tracker);
            RemoveLogicalChild(Tracker);
            RemoveVisualChild(Tracker);

            SetRow(PageTrackerBorder, 3);
            SetRowSpan(PageTrackerBorder, 2);
            SetColumnSpan(PageTrackerBorder, 2);

            Sys.Binder.BindUp(this, nameof(PageTracker), PageTrackerLabel, Label.ContentProperty);
            if (pages.Count == 0) return;
            PagesSource.AddPage(pages.First());
            SelectedPage = PagesSource.FirstOrDefault();
        }

        void OnPropertyChanged<T>(ref T value, ref T backProp, string prop) 
        {
            value = backProp;
            PropertyChanged?.Invoke(this, new(prop));
        } 

        void OnPageAddedEvent(object? sender, PageAddedEventArgs e) => PageTracker = e.PageTracker;

        async void OnPrintClicked(object sender, RoutedEventArgs e)
        {
            if (PagesSource.Count == 0)
            {
                ErrorDialog errorDialog = new("There are no pages to print");
                errorDialog.ShowDialog();
                return;
            }

            PrintingPDFDialog printingPDFDialog = new(async (pb) =>await PrintAsync(pb));
            await printingPDFDialog.ShowMe();
        }

        async Task PrintAsync(ProgressBar pb)
        {
            PrintDialog printDialog = new()
            {
                UserPageRangeEnabled = false,
                PrintTicket = new PrintTicket
                {
                    InputBin = InputBin.AutoSelect,
                    CopyCount = 1
                }
            };
            pb.Value = 15;

            ReadOnlyCollection<PageMediaSize> PageSizes = printDialog.PrintQueue.GetPrintCapabilities().PageMediaSizeCapability;
            pb.Value = 30;

            await Task.Delay(1000);

            try
            {
                printDialog.PrintTicket.PageMediaSize = PageSizes.First(s => s.ToString().Contains("ISOA4"));
            }
            catch
            {
                throw new Exception("Can't find A4 Format.");
            }
            pb.Value = 45;

            BookPaginator bookPaginator = new(PagesSource);
            pb.Value = 60;

            if (printDialog?.PrintTicket?.PageMediaSize?.Width == null || printDialog.PrintTicket.PageMediaSize.Height == null) throw new Exception("PageMediaSize does not have width and height");
            pb.Value = 75;

            bookPaginator.PageSize = new Size((double)printDialog.PrintTicket.PageMediaSize.Width, (double)printDialog.PrintTicket.PageMediaSize.Height);
            pb.Value = 90;

            printDialog.PrintDocument(bookPaginator, $"Document From ReportViewer");
            pb.Value = 100;
            await Task.Delay(1000);
        }

        void OnZoomInClicked(object sender, RoutedEventArgs e)
        {
            BoxViewWidth += 100;
            BoxViewHeight += 100;
        }

        void ZoomOutClicked(object sender, RoutedEventArgs e)
        {
            BoxViewWidth -= 100;
            BoxViewHeight -= 100;
        }

        void ResetZoomClicked(object sender, RoutedEventArgs e)
        {
            BoxViewWidth = 900;
            BoxViewHeight = 900;
        }

        void PreviousPageClicked(object sender, RoutedEventArgs e)
        {
            int index = PagesSource.IndexOf(SelectedPage);
            if (index <= 0) return;
            SelectedPage = PagesSource[index - 1];
            Book.ScrollIntoView(SelectedPage);
            var x = SelectedPage.Me.PointFromScreen(new Point(0, 0));
            var y = SelectedPage.Me.PointToScreen(new Point(0, 0));
        }

        void NextPageClicked(object sender, RoutedEventArgs e)
        {
            int index = PagesSource.IndexOf(SelectedPage);
            if (index >= PagesSource.Count - 1) return;
            SelectedPage = PagesSource[index + 1];
            Book.ScrollIntoView(SelectedPage);
        }

        void FirstPageClicked(object sender, RoutedEventArgs e)
        {
            if (PagesSource.Count <= 0) return;
            SelectedPage = PagesSource[0];
            Book.ScrollIntoView(SelectedPage);
        }

        void LastPageClicked(object sender, RoutedEventArgs e)
        {
            if (PagesSource.Count <= 0) return;
            SelectedPage = PagesSource.LastOrDefault();
            Book.ScrollIntoView(SelectedPage);
        }

        void PrinterSelected(object sender, SelectionChangedEventArgs e)
        {
            string oldValue = string.Empty;
            if (e.RemovedItems!=null && e.RemovedItems.Count > 0)
            {
                oldValue = e.RemovedItems[e.RemovedItems.Count - 1].ToString();
            }

            if (PrinterComboBox.SelectedItem==null)
            {
                ErrorDialog errorDialog = new("No printer has been selected");
                errorDialog.ShowDialog();
                PrinterComboBox.SelectedItem = oldValue;
                PrinterComboBox.Dispatcher.BeginInvoke(() =>
                {
                    PrinterComboBox.Text = oldValue;
                });
                return;
            }
                
            SetDefaultPrinter(PrinterComboBox?.SelectedItem?.ToString());
        }

        private void SettingButtonClicked(object sender, RoutedEventArgs e)
        {
            PrintingSettings printingSettings = new(ref microsoftPDFManager);
            printingSettings.ShowDialog();
        }
    }

    public interface IPageReport
    {
        public int PageNumber { get; set; }
        public double BreakAt { get; }
        public bool BreakPage();
        public Visual Me { get; }
        public IPageReport CopyOverflow();
    }

    #region ListOfPages
    public class ListOfPages : List<IPageReport>, IDocumentPaginatorSource
    {
        int page = 0;
        public event EventHandler<PageAddedEventArgs>? PageAddedEvent;

        public ListOfPages(IEnumerable<IPageReport> source) : base(source) { }
        public ListOfPages() { }

        public DocumentPaginator DocumentPaginator => new BookPaginator(this);

        public void AddPages(List<IPageReport> pages)
        {
            foreach (var page in pages) AddPage(page);
        }

        public void AddPage(IPageReport obj)
        {
            obj.PageNumber = ++page;
            Add(obj);
            PageAddedEvent?.Invoke(this, new(obj,page));
            bool keepAdding = obj.BreakPage();
            if (!keepAdding) return;
            AddPage(obj.CopyOverflow());
        }
    }
    #endregion

    #region PageAddedEventArgs
    public class PageAddedEventArgs : EventArgs
    {
        public PageAddedEventArgs(IPageReport? page, int pageCount)
        {
            PageReport = page;
            PageCount = pageCount;
        }

        public IPageReport? PageReport { get; set; }
        public int CurrentPageNumber { get => (PageReport == null) ? 1 : 0; }
        public int PageCount = 1;
        public string PageTracker => $"Page: {CurrentPageNumber} of {PageCount}";
    }
    #endregion

    #region BookPaginator
    public class BookPaginator : DocumentPaginator
    {
        ListOfPages PagesSource;
        Size _size = new();

        public BookPaginator(IEnumerable<IPageReport> source) => PagesSource = new(source);

        public override bool IsPageCountValid => PagesSource.Count > 0;

        public override int PageCount => PagesSource.Count;

        public override Size PageSize { get => _size; set => _size = value; }

        public override IDocumentPaginatorSource Source => PagesSource;

        public override DocumentPage GetPage(int pageNumber)
        {
            Visual visual = (PageReport)PagesSource.First(s => s.PageNumber == pageNumber + 1);
            DocumentPage page = new(visual, PageSize, new(), new());
            return page;
        }
    }
    #endregion

    #region ReportComponents
    public class ReportMainGrid : Grid
    {
        public ReportHeader Header { get; set; }
        public ReportBody Body { get; set; }
        public ReportFooter Footer { get; set; }

        #region HeaderHeightProperty
        public static readonly DependencyProperty HeaderHeightProperty
        = Sys.Binder.Register<GridLength, ReportMainGrid>(nameof(HeaderHeight), true, new(100), null, true, true, true);

        public GridLength HeaderHeight
        {
            get => (GridLength)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }
        #endregion

        #region FooterHeightProperty
        public static readonly DependencyProperty FooterHeightProperty
        = Sys.Binder.Register<GridLength, ReportMainGrid>(nameof(FooterHeight), true, new(50), null, true, true, true);

        public GridLength FooterHeight
        {
            get => (GridLength)GetValue(FooterHeightProperty);
            set => SetValue(FooterHeightProperty, value);
        }
        #endregion

        protected RowDefinition RowHeader { get; set; } = new();
        protected RowDefinition RowBody { get; set; } = new() { Height = new(1, GridUnitType.Star) };
        protected RowDefinition RowFooter { get; set; } = new();

        public ReportMainGrid()
        {
            Width = 793.70;
            Height = 1122.51;

            MaxHeight = Height;
            MaxWidth = Width;

            RowDefinitions.Add(RowHeader);
            RowDefinitions.Add(RowBody);
            RowDefinitions.Add(RowFooter);

            Sys.Binder.BindUp(this, nameof(HeaderHeight), RowHeader, RowDefinition.HeightProperty);
            Sys.Binder.BindUp(this, nameof(FooterHeight), RowFooter, RowDefinition.HeightProperty);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            bool IsReportComponent = visualAdded is IReportComponents;
            if (!IsReportComponent) return;
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            switch (visualAdded)
            {
                case ReportHeader:
                    Grid.SetRow((UIElement)visualAdded, 0);
                    break;
                case ReportBody:
                    Grid.SetRow((UIElement)visualAdded, 1);
                    break;
                case ReportFooter:
                    Grid.SetRow((UIElement)visualAdded, 2);
                    break;
            }
        }
    }

    public interface IReportComponents
    {

    }

    public abstract class ReportBorder : Border, IReportComponents
    {
        protected ReportBorder()
        {
            BorderThickness = new(0.5);
            BorderBrush = Brushes.Black;
            Padding = new(0.5);
        }

        public double GetExternalInternalSizes()
        {
            double x = 0;
            x += BorderThickness.Left;
            x += BorderThickness.Right;
            x += BorderThickness.Top;
            x += BorderThickness.Bottom;

            x += Padding.Left;
            x += Padding.Right;
            x += Padding.Top;
            x += Padding.Bottom;

            x += Margin.Left;
            x += Margin.Right;
            x += Margin.Top;
            x += Margin.Bottom;
            return x;
        }
    }

    public class ReportHeader : ReportBorder
    {

    }

    public class ReportBody : ReportBorder
    {
        List<Map> RemovedElements = new();
        public double BreakAt { get; set; }
        public override UIElement Child
        {
            get => base.Child;
            set
            {
                bool isGrid = value is Grid;
                if (!isGrid) throw new Exception("Body should only host a Grid as child.");
                base.Child = value;
            }
        }
        Grid MainBody
        {
            get => (Grid)Child;
        }

        public Grid NewBody()
        {
            Grid MainBody = new();

            var rows = RemovedElements.DistinctBy(s => s.Row).ToList();
            var columns = RemovedElements.DistinctBy(s => s.Column).ToList();

            foreach (var row in rows)
                MainBody.RowDefinitions.Add(new RowDef(row));

            foreach (var col in columns)
                MainBody.ColumnDefinitions.Add(new ColDef(col));

            foreach (Map removedchild in RemovedElements)
            {
                var child = XAMLCloner.Copy<UIElement>(removedchild.Child);
                MainBody.Children.Add(child);
                int rowindex = GetIndex(MainBody?.RowDefinitions, removedchild);
                int columnindex = GetIndex(MainBody?.ColumnDefinitions, removedchild, false);
                Grid.SetRow(child, rowindex);
                Grid.SetColumn(child, columnindex);
                Grid.SetColumnSpan(child, removedchild.ColumnSpan);
                Grid.SetRowSpan(child, removedchild.RowSpan);

                if (removedchild.TextCut)
                    ((Label)child).Content = removedchild.NextStream;
            }

            return MainBody;
        }

        static int GetIndex(IList? collection, Map map, bool row = true)
        {
            var source = collection?.Cast<IDefinitionBaseInfo>().ToList();
            for (int index = 0; index <= source?.Count - 1; index++)
            {
                if (source == null) return -1;
                var currentDefinition = source[index];
                bool exists = currentDefinition.Index == (row ? map.Row : map.Column);
                if (exists) return index;
            }
            return -1;
        }

        public bool BreakPage()
        {
            double Y1 = 0;
            double Y2 = 0;
            double TextHeight = 0;
            bool StartNewPage = false;
            RemovedElements.Clear();

            foreach (UIElement child in MainBody.Children.Cast<UIElement>().ToList())
            {
                int column = Grid.GetColumn(child);
                if (column > 0) continue;
                int row = Grid.GetRow(child);

                Label? childLabel = (child is Label) ? (Label)child : null;
                TextHeight = ListString.GetTextHeight(childLabel);

                if (MainBody.ColumnDefinitions.Count == 0 || MainBody.RowDefinitions.Count == 0)
                {
                    throw new Exception("Grid in main body must have at least one row and one column defined");
                }

                GridLength columnLength = MainBody.ColumnDefinitions[column].Width;

                GridLength rowLength = MainBody.RowDefinitions[row].Height;
                GridUnitType rowUnitType = rowLength.GridUnitType;

                bool TextCut = (rowUnitType.Equals(GridUnitType.Star));
                double rowHeight = TextCut ? TextHeight : rowLength.Value;

                Y2 += rowHeight;
                Y1 = Y2 - rowHeight;
                StartNewPage = (Y2 + GetExternalInternalSizes()) >= BreakAt;

                if (!StartNewPage) continue;

                string removedString = string.Empty;
                if (TextCut && childLabel != null)
                {
                    ListString list = new(childLabel, (BreakAt - Y1));//630=x
                    list.RunUntil();
                    removedString = list.RemovedText();
                    RemovedElements.Add(new(child, removedString, rowLength, columnLength, TextCut));
                }
                else
                {
                    var ChildrenInRow = MainBody.Children.Cast<UIElement>().ToList().Where(s => Grid.GetRow(s) == row);
                    foreach (var childInRow in ChildrenInRow)
                    {
                        GridLength colLength = MainBody.ColumnDefinitions[Grid.GetColumn(childInRow)].Width;
                        MainBody.Children.Remove(childInRow);
                        RemovedElements.Add(new(childInRow, string.Empty, rowLength, colLength, false));
                    }
                }
            }

            //REMOVE ROWS
            var source = RemovedElements.DistinctBy(s => s.Row).Where(s => s.TextCut == false).OrderBy(s => s.Row).ToList();
            if (source.Count > 0)
            {
                var startIndex = source.First().Row;
                MainBody.RowDefinitions.RemoveRange(startIndex, source.Count);
            }
            return StartNewPage;

        }
    }

    public class ReportFooter : ReportBorder
    {
        #region PageNumberStringFormatProperty
        public static readonly DependencyProperty PageNumberStringFormatProperty
        = Sys.Binder.Register<string, ReportFooter>(nameof(PageNumberStringFormat), true, "Page: {0}", null, true, true, true);

        public string PageNumberStringFormat
        {
            get => (string)GetValue(PageNumberStringFormatProperty);
            set => SetValue(PageNumberStringFormatProperty, value);
        }
        #endregion

        #region PageNumberProperty
        public static readonly DependencyProperty PageNumberProperty
        = Sys.Binder.Register<int, ReportFooter>(nameof(PageNumber), true, 1, null, true, true, true);

        public int PageNumber
        {
            get => (int)GetValue(PageNumberProperty);
            set => SetValue(PageNumberProperty, value);
        }
        #endregion

    }
    #endregion

    #region PageReportClass
    public class PageReport : ReportMainGrid, IPageReport
    {
        public Visual Me { get => this; }
        public double BreakAt => Height - RowHeader.Height.Value - RowFooter.Height.Value;

        #region C
        private static readonly DependencyPropertyKey s_reportPageComponentsPropertyKey =
        DependencyProperty.RegisterReadOnly(
          name: nameof(ReportComponents),
          propertyType: typeof(FreezableCollection<ReportBorder>),
          ownerType: typeof(PageReport),
          typeMetadata: new FrameworkPropertyMetadata() { PropertyChangedCallback = s_reportPageComponentsPropertyKeyChanged }
        );

        private static void s_reportPageComponentsPropertyKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((PageReport)d).OnReportComponentsChanged((FreezableCollection<ReportBorder>)e.NewValue);

        protected void OnReportComponentsChanged(FreezableCollection<ReportBorder> ReportComponents)
        {
            Children.Clear();
            foreach (var x in ReportComponents)
            {
                switch (x)
                {
                    case ReportHeader:
                        Header = (ReportHeader)x;
                        break;
                    case ReportBody:
                        Body = (ReportBody)x;
                        Body.BreakAt = BreakAt;
                        break;
                    case ReportFooter:
                        Footer = (ReportFooter)x;
                        Sys.Binder.BindUp(this, nameof(PageNumber), Footer, ReportFooter.PageNumberProperty);
                        break;
                }
                Children.Add(x);
            }
        }

        public static readonly DependencyProperty ReportComponentsProperty =
        s_reportPageComponentsPropertyKey.DependencyProperty;

        public FreezableCollection<ReportBorder> ReportComponents =>
        (FreezableCollection<ReportBorder>)GetValue(ReportComponentsProperty);
        #endregion

        #region PageNumberProperty
        public static readonly DependencyProperty PageNumberProperty
        = Sys.Binder.Register<int, PageReport>(nameof(PageNumber), true, 1, null, true, true, true);

        public int PageNumber
        {
            get => (int)GetValue(PageNumberProperty);
            set => SetValue(PageNumberProperty, value);
        }
        #endregion

        public PageReport(ReportHeader header, ReportBody body, ReportFooter footer) : this()
        {
            ReportComponents.Add(header);
            ReportComponents.Add(body);
            ReportComponents.Add(footer);
        }
        public PageReport()=>SetValue(s_reportPageComponentsPropertyKey, new FreezableCollection<ReportBorder>());
        
        public IPageReport CopyOverflow()
        {
            ReportHeader header = XAMLCloner.NewInstanceOf<ReportHeader>(Header);
            ReportBody body = new()
            {
                BreakAt = this.BreakAt,
                Child = Body.NewBody()
            };

            ReportFooter footer = XAMLCloner.NewInstanceOf<ReportFooter>(Footer);
            return new PageReport(header, body, footer);
        }

        public bool BreakPage() => Body.BreakPage();
    }
    #endregion

    #region columnRowDefinitionSupport
    public interface IDefinitionBaseInfo
    {
        public int Index { get; set; }
    }

    public class ColDef : ColumnDefinition, IDefinitionBaseInfo
    {
        public int Index { get; set; } = -1;

        public ColDef(Map map)
        {
            Width = new(map.ColumnLength.Value, map.ColumnLength.GridUnitType);
            Index = map.Column;
        }
    }

    public class RowDef : RowDefinition, IDefinitionBaseInfo
    {
        public int Index { get; set; } = -1;

        public RowDef(Map map)
        {
            Height = new(map.RowLength.Value, map.RowLength.GridUnitType);
            Index = map.Row;
        }
    }
    #endregion

    #region XAMLCloner
    public class XAMLCloner
    {
        public static T NewInstanceOf<T>(T obj)
        {
            if (obj == null) throw new Exception("Cannot create a New Instance of null");
            Type t = obj.GetType();
            object? instance = Activator.CreateInstance(t);
            if (instance == null) throw new Exception($"Something went wrong when creating a New Instance of {t}");
            return (T)instance!;
        }

        public static T Copy<T>(object obj)
        {
            string savedObject = XamlWriter.Save(obj);
            StringReader stringReader = new StringReader(savedObject);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            T copiedObject = (T)XamlReader.Load(xmlReader);
            ManageExceptions(obj, copiedObject);
            return copiedObject;
        }

        static void ManageExceptions(object original, object copied)
        {
            bool IsGrid = original is Grid;
            if (IsGrid) FixGrid((Grid)original, (Grid)copied);
        }

        static void FixGrid(Grid original, Grid copied)
        {
            copied.ColumnDefinitions.Clear();
            copied.RowDefinitions.Clear();

            foreach (ColumnDefinition columnDefinition in original.ColumnDefinitions)
                copied.ColumnDefinitions.Add(new() { Width = columnDefinition.Width });

            foreach (RowDefinition rowDefinition in original.RowDefinitions)
                copied.RowDefinitions.Add(new() { Height = rowDefinition.Height });

        }

    }
    #endregion

    #region MapClass
    public class Map
    {
        public UIElement Child;
        public int Row { get; }
        public int Column { get; }
        public int RowSpan { get; }
        public int ColumnSpan { get; }
        public string NextStream { get; }
        public GridLength RowLength { get; }
        public GridLength ColumnLength { get; }
        public bool TextCut { get; }

        public Map(UIElement child, string nextStream, GridLength rowLength, GridLength columnLength, bool textCut)
        {
            Child = child;
            Row = Grid.GetRow(child);
            Column = Grid.GetColumn(child);
            RowSpan = Grid.GetRowSpan(child);
            ColumnSpan = Grid.GetColumnSpan(child);
            NextStream = nextStream;
            RowLength = rowLength;
            ColumnLength = columnLength;
            TextCut = textCut;
        }
    }
    #endregion

    #region ListString
    public class ListString : List<string>
    {
        Label label;
        string RebuiltText = string.Empty;
        public bool Stop = false;
        public double MaxValue { get; set; }
        List<string> RemovedStrings = new();

        public ListString(Label _label, bool makesource = false)
        {
            label = _label;
            if (makesource)
                Replace(ExtrapolateSourceFromLabelText(label));
        }

        public ListString(Label _label, double _maxvalue = 0)
        {
            label = _label;
            MaxValue = _maxvalue;
        }

        public void Replace(IEnumerable<string> source)
        {
            Clear();
            AddRange(source);
        }

        public void RebuildText()
        {
            RebuiltText = string.Empty;
            foreach (string item in this)
                RebuiltText += item + Environment.NewLine;
        }

        public string RemovedText()
        {
            RemovedStrings.Reverse();
            string Text = string.Empty;
            foreach (string item in RemovedStrings)
                Text += item + Environment.NewLine;

            return Text;
        }

        public void ShouldRemoveLast()
        {
            string stringtoremove = this.Last();
            int index = this.Count - 1;

            if (string.IsNullOrEmpty(this[index - 1]))
                stringtoremove = Environment.NewLine + stringtoremove;

            RemovedStrings.Add(stringtoremove);

            if (Count == 1)
                Clear();
            else if (Count > 1)
                RemoveAt(index);
        }

        public void UpdateTextControl() =>
        label.Content = RebuiltText;

        public Size Measure() => MeasureString(RebuiltText);

        public double TextHeight() => MeasureString(RebuiltText).Height;

        public double Adjust()
        {
            ShouldRemoveLast();
            RebuildText();
            UpdateTextControl();
            return TextHeight();
        }

        public void RunUntil(double _maxvalue = 0)
        {
            if (_maxvalue > 0) MaxValue = _maxvalue;
            while (!Stop)
            {
                Replace(ExtrapolateSourceFromLabelText(label));
                Stop = Adjust() <= MaxValue;
            }
        }

        static Size MeasureString(Label? label)
        {
            if (label == null) return new Size();
            string candidate = ExtrapolateTextFromLabel(label);
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch),
            label.FontSize,
            Brushes.Black,
            new NumberSubstitution(),
                VisualTreeHelper.GetDpi(label).PixelsPerDip);
            Size size = new Size(formattedText.Width, formattedText.Height);
            size.Height += label.Padding.Bottom + label.Padding.Top + label.Margin.Top + label.Margin.Top;
            return size;
        }

        Size MeasureString(string candidate)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch),
            label.FontSize,
            Brushes.Black,
            new NumberSubstitution(),
                VisualTreeHelper.GetDpi(label).PixelsPerDip);
            Size size = new Size(formattedText.Width, formattedText.Height);
            size.Height += label.Padding.Bottom + label.Padding.Top + label.Margin.Top + label.Margin.Top;
            return size;
        }

        public static string ExtrapolateTextFromLabel(Label label) =>
        label.ToString().Remove(0, (label.GetType().FullName + ":").Length).Trim().ReplaceLineEndings(Environment.NewLine);

        public static IEnumerable<string> ExtrapolateSourceFromLabelText(Label label) =>
        ExtrapolateTextFromLabel(label).Split(Environment.NewLine).ToList();

        public static ListString CreateFromLabel(Label label) => new(label, true);

        public static double GetTextHeight(Label? label) => MeasureString(label).Height;
    }
    #endregion

}
