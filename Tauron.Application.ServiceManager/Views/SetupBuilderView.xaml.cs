﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tauron.Application.ServiceManager.ViewModels;
using Tauron.Application.Wpf;

namespace Tauron.Application.ServiceManager.Views
{
    /// <summary>
    /// Interaktionslogik für SetupBuilderView.xaml
    /// </summary>
    public partial class SetupBuilderView
    {
        public SetupBuilderView(IViewModel<SetupBuilderViewModel> model)
            : base(model)
        {
            InitializeComponent();
        }
    }
}