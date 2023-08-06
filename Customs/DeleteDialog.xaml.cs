using System;
using System.ComponentModel;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SARWPF
{
   
    public abstract class AbstractDialog : Window, IPersonalDialog
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public void MoveCursorAt(int x, int y) => SetCursorPos(x, y);
        public DialogResponse Response { get; set; } = DialogResponse.OK;
        
        public AbstractDialog()
        {
            SystemSounds.Beep.Play();
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Response.Equals(DialogResponse.VOID))
            {
                Response = DialogResponse.NO;
            }
            base.OnClosing(e);
        }
    }

    public enum DialogResponse
    {
        VOID,
        YES,
        NO,
        OK,
    }

    public partial class DeleteDialog : AbstractDialog
    {

        public DeleteDialog()
        {
            InitializeComponent();
            Response = DialogResponse.VOID;
            Loaded += (sender, e) =>
            {
                Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(delegate ()
                {
                    FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
                    TraversalRequest request = new TraversalRequest(focusDirection);
                    UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                    var relativePoint = NoOption.PointToScreen(new Point(0d, 0d));
                    MoveCursorAt((int)relativePoint.X + 45, (int)relativePoint.Y + 20);
                }));
            };
        }

        private void YesResponseClicked(object sender, RoutedEventArgs e)
        {
            Response = DialogResponse.YES;
            Close();
        }

        private void NoResponseClicked(object sender, RoutedEventArgs e)
        {
            Response = DialogResponse.NO;
            Close();
        }
    }

    public interface IPersonalDialog
    {
        public void MoveCursorAt(int x, int y);
        public DialogResponse Response { get; set; }
    }
}
