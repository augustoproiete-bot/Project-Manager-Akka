using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Dialogs
{
    [PublicAPI]
    [System.Windows.Markup.ContentProperty("Main")]
    [DefaultProperty("Main")]
    [TemplatePart(Name = "DialogContent", Type = typeof(ContentControl))]
    [TemplatePart(Name = "MainContent", Type = typeof(ContentControl))]
    public class DialogHost : Control
    {
        static DialogHost() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogHost), new FrameworkPropertyMetadata(typeof(DialogHost)));

        public static readonly DependencyProperty DialogProperty = DependencyProperty.Register(
            "Dialog", typeof(object), typeof(DialogHost), new PropertyMetadata(default, (o, args) => ((DialogHost)o).DialogContent?.SetValue(ContentControl.ContentProperty, args.NewValue)));

        public object Dialog
        {
            get => GetValue(DialogProperty);
            set => SetValue(DialogProperty, value);
        }

        public static readonly DependencyProperty MainProperty = DependencyProperty.Register(
            "Main", typeof(object), typeof(DialogHost), new PropertyMetadata(default, (o, args) => ((DialogHost)o).MainContent?.SetValue(ContentControl.ContentProperty, args.NewValue)));

        public object Main
        {
            get => GetValue(MainProperty);
            set => SetValue(MainProperty, value);
        }

        public DialogHost()
        {
            DialogCoordinator
               .InternalInstance
               .HideDialogEvent += () =>
                                   {
                                       if (MainContent != null)
                                       {
                                           MainContent.IsEnabled = true;
                                           MainContent.Visibility = Visibility.Visible;
                                       }

                                       if (DialogContent == null) return;
                                       DialogContent.Content = null;
                                       DialogContent.IsEnabled = false;
                                       DialogContent.Visibility = Visibility.Collapsed;
                                   };

            DialogCoordinator
               .InternalInstance
               .ShowDialogEvent += o =>
                                   {
                                       if (MainContent != null)
                                       {
                                           MainContent.IsEnabled = false;
                                           MainContent.Visibility = Visibility.Collapsed;
                                       }

                                       if (DialogContent == null) return;
                                       DialogContent.Content = o;
                                       DialogContent.IsEnabled = true;
                                       DialogContent.Visibility = Visibility.Visible;
                                   };
        }

        private ContentControl? DialogContent { get; set; }

        private ContentControl? MainContent { get; set; }

        public override void OnApplyTemplate()
        {
            MainContent = GetTemplateChild("MainContent") as ContentControl;
            DialogContent = GetTemplateChild("DialogContent") as ContentControl;

            if (MainContent != null)
                MainContent.Content = Main;
            if (DialogContent != null)
                DialogContent.Content = Dialog;

            base.OnApplyTemplate();
        }
    }
}
