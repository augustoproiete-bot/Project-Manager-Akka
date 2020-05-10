using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf
{
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    [PublicAPI]
    public enum MsgBoxImage
    {
        None = 0,
        Error = 16,
        Hand = 16,
        Stop = 16,
        Question = 32,
        Exclamation = 48,
        Warning = 48,
        Asterisk = 64,
        Information = 64
    }

    [PublicAPI]
    public enum MsgBoxButton
    {
        Ok = 0,
        OkCancel = 1,
        YesNoCancel = 3,
        YesNo = 4
    }

    [PublicAPI]
    public enum MsgBoxResult
    {
        None = 0,
        Ok = 1,
        Cancel = 2,
        Yes = 6,
        No = 7
    }

    [PublicAPI]
    public interface IDialogFactory
    {
        void FormatException(System.Windows.Window? owner, Exception exception);

        MsgBoxResult ShowMessageBox(System.Windows.Window? owner, string text, string caption, MsgBoxButton button, MsgBoxImage icon);


        IEnumerable<string> ShowOpenFileDialog(Window? owner,
            bool checkFileExists, string defaultExt,
            bool dereferenceLinks, string filter,
            bool multiSelect, string title,
            bool validateNames,
            bool checkPathExists,
            out bool? result);


        string? ShowOpenFolderDialog(System.Windows.Window? owner, string description, Environment.SpecialFolder rootFolder, bool showNewFolderButton, bool useDescriptionForTitle, out bool? result);

        string? ShowOpenFolderDialog(System.Windows.Window? owner, string description, string rootFolder, bool showNewFolderButton, bool useDescriptionForTitle, out bool? result);


        string? ShowSaveFileDialog(System.Windows.Window? owner, bool addExtension, bool checkFileExists, bool checkPathExists, string defaultExt, bool dereferenceLinks, string filter,
            bool createPrompt, bool overwritePrompt, string title, string initialDirectory, out bool? result);
    }
}