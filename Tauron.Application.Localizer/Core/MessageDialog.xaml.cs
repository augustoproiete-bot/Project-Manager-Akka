﻿using System;
using System.Windows;

namespace Tauron.Application.Localizer.Core
{
    /// <summary>
    /// Interaktionslogik für MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog
    {
        private readonly Action<bool?>? _result;
        private readonly bool _canCnacel;

        public MessageDialog(string title, string content, Action<bool?>? result, bool canCnacel)
        {
            _result = result;
            _canCnacel = canCnacel;
            InitializeComponent();
            Title = title;
            ContentBox.Text = content;

            if (!canCnacel)
                CancelButton.Visibility = Visibility.Hidden;
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            if (_canCnacel)
                _result?.Invoke(true);
            else
                _result?.Invoke(null);
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) => _result?.Invoke(false);
    }
}
