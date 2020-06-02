using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using JetBrains.Annotations;

namespace Tauron.Application.Localizer.Core
{
    [DefaultProperty("Content")]
    [ContentProperty("Content")]
    [PublicAPI]
    [TemplatePart(Name = "PART_Content", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_Top", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_Title", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_Bottom", Type = typeof(ContentPresenter))]
    public class DialogBase : Control
    {
        public static readonly DependencyProperty DialogTitleFontSizeProperty = DependencyProperty.Register(
            "DialogTitleFontSize", typeof(int), typeof(DialogBase), new PropertyMetadata(default(int)));

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(object), typeof(DialogBase), new PropertyMetadata(default,
                (o, args) => ((DialogBase) o).ContentChanged(args)));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(DialogBase), new PropertyMetadata(default(string),
                (o, args) => ((DialogBase) o).TitleChanged(args)));

        public static readonly DependencyProperty TopProperty = DependencyProperty.Register(
            "Top", typeof(object), typeof(DialogBase), new PropertyMetadata(default,
                (o, args) => ((DialogBase) o).TopChanged(args)));


        public static readonly DependencyProperty BottomProperty = DependencyProperty.Register(
            "Bottom", typeof(object), typeof(DialogBase), new PropertyMetadata(default,
                (o, args) => ((DialogBase) o).ButtomChaned(args)));

        public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register(
            "ContentTemplate", typeof(DataTemplate), typeof(DialogBase), new PropertyMetadata(default(DataTemplate)));

        private ContentPresenter? _bottom;
        private ContentPresenter? _content;

        private TextBlock? _title;

        private ContentPresenter? _top;

        static DialogBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogBase), new FrameworkPropertyMetadata(typeof(DialogBase)));
        }

        public DialogBase()
        {
            Loaded += OnLoaded;
        }

        public int DialogTitleFontSize
        {
            get => (int) GetValue(DialogTitleFontSizeProperty);
            set => SetValue(DialogTitleFontSizeProperty, value);
        }

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public object Top
        {
            get => GetValue(TopProperty);
            set => SetValue(TopProperty, value);
        }

        public object Bottom
        {
            get => GetValue(BottomProperty);
            set => SetValue(BottomProperty, value);
        }

        public DataTemplate ContentTemplate
        {
            get => (DataTemplate) GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (TryFindResource("Storyboard.Dialogs.Show") is Storyboard res)
                    res.Begin(this);
                else
                    Opacity = 1;
            }));
        }

        private void ContentChanged(DependencyPropertyChangedEventArgs args)
        {
            if (_content == null) return;
            _content.Content = args.NewValue;
        }

        private void TitleChanged(DependencyPropertyChangedEventArgs args)
        {
            if (_title == null) return;
            _title.Text = args.NewValue as string;
        }

        private void TopChanged(DependencyPropertyChangedEventArgs args)
        {
            if (_top == null) return;
            _top.Content = args.NewValue;
        }

        private void ButtomChaned(DependencyPropertyChangedEventArgs args)
        {
            if (_bottom == null) return;
            _bottom.Content = args.NewValue;
        }

        public override void OnApplyTemplate()
        {
            _content = (ContentPresenter?) GetTemplateChild("PART_Content");
            if (_content != null)
                _content.Content = Content;

            _top = (ContentPresenter?) GetTemplateChild("PART_Top");
            if (_top != null)
                _top.Content = Top;

            _title = (TextBlock?) GetTemplateChild("PART_Title");
            if (_title != null)
                _title.Text = Title;

            _bottom = (ContentPresenter?) GetTemplateChild("PART_Bottom");
            if (_bottom != null)
                _bottom.Content = Bottom;

            base.OnApplyTemplate();
        }
    }
}