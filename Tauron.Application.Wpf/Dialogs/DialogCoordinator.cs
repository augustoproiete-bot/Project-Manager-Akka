using System;
using System.Threading.Tasks;
using System.Windows;

namespace Tauron.Application.Wpf.Dialogs
{
    public sealed class DialogCoordinator : IDialogCoordinator
    {
        internal static readonly DialogCoordinator InternalInstance = new DialogCoordinator();

        public static IDialogCoordinator Instance => InternalInstance;

        public event Action<System.Windows.Window>? OnWindowConstructed;

        public Task<bool?> ShowMessage(string title, string message, Action<bool?>? result)
        {
            var resultTask = new TaskCompletionSource<bool?>();
            result = result.Combine(b =>
            {
                HideDialog();
                resultTask.SetResult(b);
            });

            ShowDialog(new MessageDialog(title, message, result, true));

            return resultTask.Task;
        }

        public void ShowMessage(string title, string message) => ShowDialog(new MessageDialog(title, message, b => HideDialog(), false));

        public void ShowDialog(object dialog) => ShowDialogEvent?.Invoke(dialog);

        public void HideDialog() => HideDialogEvent?.Invoke();

        public bool? ShowModalMessageWindow(string title, string message)
        {
            var window = new System.Windows.Window();
            window.Content = new MessageDialog(title, message, b => window.DialogResult = b, true) {Margin = new Thickness(10)};

            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.ResizeMode = ResizeMode.NoResize;
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.WindowStyle = WindowStyle.ToolWindow;
            window.ShowInTaskbar = false;

            OnWindowConstructed?.Invoke(window);

            return window.ShowDialog();
        }

        internal event Action<object>? ShowDialogEvent;

        internal event Action? HideDialogEvent;
    }
}