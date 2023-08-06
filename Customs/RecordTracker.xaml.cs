using SAR;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SARWPF
{
    public partial class RecordTracker : StackPanel
    {
        public RecordTracker()
        {
            InitializeComponent();
            GoNew.Click += GoNewClicked;
            GoNext.Click += GoNextClicked;
            GoPrevious.Click += GoPreviousClicked;
            GoFirst.Click += GoFirstClicked;
            GoLast.Click += GoLastClicked;
        }

        #region RecordSourceProperty
        public static readonly DependencyProperty RecordSourceProperty
        = Sys.Binder.Register<IRecordSource?, RecordTracker>(nameof(RecordSource), true, null, 
        (d,e) => Sys.Binder.BindUp(e.NewValue, "Records", ((RecordTracker)d).RecordIndicator, Label.ContentProperty, BindingMode.OneWay)
        , true,true,true);

        public IRecordSource? RecordSource
        {
            private get => (IRecordSource?)GetValue(RecordSourceProperty);
            set => SetValue(RecordSourceProperty, value);
        }

        #endregion

        private void GoLastClicked(object sender, RoutedEventArgs e)=>
        RecordSource?.GoLast();

        private void GoFirstClicked(object sender, RoutedEventArgs e)=>
        RecordSource?.GoFirst();

        private void GoPreviousClicked(object sender, RoutedEventArgs e)=>
        RecordSource?.GoPrevious();

        private void GoNextClicked(object sender, RoutedEventArgs e)=>
        RecordSource?.GoNext();

        private void GoNewClicked(object sender, RoutedEventArgs e)=>
        RecordSource?.GoNewRecord();
    }
}