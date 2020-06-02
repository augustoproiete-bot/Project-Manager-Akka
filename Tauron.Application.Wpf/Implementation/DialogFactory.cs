using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using Ookii.Dialogs.Wpf;
using Tauron.Application.Wpf.AppCore;

namespace Tauron.Application.Wpf.Implementation
{
    public sealed class DialogFactory : IDialogFactory
    {
        private readonly System.Windows.Window _mainWindow;

        public DialogFactory(Dispatcher currentDispatcher, IMainWindow mainWindow)
        {
            _mainWindow = mainWindow.Window;
            CurrentDispatcher = currentDispatcher;
        }

        private Dispatcher CurrentDispatcher { get; }

        public void FormatException(System.Windows.Window? owner, Exception exception)
        {
            ShowMessageBox(owner, $"Type: {exception.GetType().Name} \n {exception.Message}", "Error", MsgBoxButton.Ok, MsgBoxImage.Error);
        }

        public MsgBoxResult ShowMessageBox(System.Windows.Window? owner, string text, string caption, MsgBoxButton button, MsgBoxImage icon)
        {
            return CurrentDispatcher.Invoke(
                () => (MsgBoxResult) MessageBox.Show(owner ?? _mainWindow, text, caption, (MessageBoxButton) button, (MessageBoxImage) icon));
        }


        public IEnumerable<string> ShowOpenFileDialog(Window? owner, bool checkFileExists, string defaultExt, bool dereferenceLinks, string filter,
            bool multiSelect, string title, bool validateNames, bool checkPathExists, out bool? result)
        {
            bool? tempresult = null;

            try
            {
                return CurrentDispatcher.Invoke(() =>
                {
                    var dialog = new VistaOpenFileDialog
                    {
                        CheckFileExists = checkFileExists,
                        DefaultExt = defaultExt,
                        DereferenceLinks =
                            dereferenceLinks,
                        Filter = filter,
                        Multiselect = multiSelect,
                        Title = title,
                        ValidateNames = validateNames,
                        CheckPathExists = checkPathExists
                    };

                    TranslateDefaultExt(dialog);

                    tempresult = owner != null
                        ? dialog.ShowDialog(owner)
                        : dialog.ShowDialog(_mainWindow);

                    return tempresult == false
                        ? Enumerable.Empty<string>()
                        : dialog.FileNames;
                });
            }
            finally
            {
                result = tempresult;
            }
        }

        public string? ShowOpenFolderDialog(System.Windows.Window? owner, string description, Environment.SpecialFolder rootFolder, bool showNewFolderButton,
            bool useDescriptionForTitle, out bool? result)
        {
            Argument.NotNull(owner, nameof(owner));

            bool? tempresult = null;
            try
            {
                return CurrentDispatcher.Invoke(
                    () =>
                    {
                        var dialog = new VistaFolderBrowserDialog
                        {
                            Description = description,
                            RootFolder = rootFolder,
                            ShowNewFolderButton = showNewFolderButton,
                            UseDescriptionForTitle = useDescriptionForTitle
                        };

                        tempresult = owner != null
                            ? dialog.ShowDialog(owner)
                            : dialog.ShowDialog(_mainWindow);

                        return tempresult == false
                            ? null
                            : dialog.SelectedPath;
                    });
            }
            finally
            {
                result = tempresult;
            }
        }

        public string? ShowOpenFolderDialog(System.Windows.Window? owner, string description, string rootFolder, bool showNewFolderButton, bool useDescriptionForTitle, out bool? result)
        {
            bool? tempresult = null;
            try
            {
                return CurrentDispatcher.Invoke(
                    () =>
                    {
                        var dialog = new VistaFolderBrowserDialog
                        {
                            Description = description,
                            SelectedPath = rootFolder,
                            ShowNewFolderButton = showNewFolderButton,
                            UseDescriptionForTitle = useDescriptionForTitle
                        };

                        tempresult = owner != null
                            ? dialog.ShowDialog(owner)
                            : dialog.ShowDialog(_mainWindow);

                        return tempresult == false
                            ? null
                            : dialog.SelectedPath;
                    });
            }
            finally
            {
                result = tempresult;
            }
        }

        public string? ShowSaveFileDialog(System.Windows.Window? owner, bool addExtension, bool checkFileExists, bool checkPathExists, string defaultExt, bool dereferenceLinks, string filter,
            bool createPrompt, bool overwritePrompt, string title, string initialDirectory, out bool? result)
        {
            bool? tempresult = null;

            try
            {
                return CurrentDispatcher.Invoke(
                    () =>
                    {
                        var dialog = new VistaSaveFileDialog
                        {
                            AddExtension = addExtension,
                            CheckFileExists = checkFileExists,
                            DefaultExt = defaultExt,
                            DereferenceLinks = dereferenceLinks,
                            Filter = filter,
                            Title = title,
                            CheckPathExists = checkPathExists,
                            CreatePrompt = createPrompt,
                            OverwritePrompt = overwritePrompt,
                            InitialDirectory = initialDirectory
                        };

                        TranslateDefaultExt(dialog);

                        tempresult = owner != null
                            ? dialog.ShowDialog(owner)
                            : dialog.ShowDialog(_mainWindow);

                        return tempresult == false ? null : dialog.FileName;
                    });
            }
            finally
            {
                result = tempresult;
            }
        }

        private void TranslateDefaultExt([NotNull] VistaFileDialog dialog)
        {
            if (string.IsNullOrWhiteSpace(dialog.DefaultExt)) return;

            var ext = "*." + dialog.DefaultExt;
            var filter = dialog.Filter;
            var filters = filter.Split('|');
            for (var i = 1; i < filters.Length; i += 2)
                if (filters[i] == ext)
                    dialog.FilterIndex = 1 + (i - 1) / 2;
        }
    }
}