using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace Tauron.Application.Localizer.Core
{

    [DefaultProperty("Content")]
    [PublicAPI]
    [TemplatePart(Name="PART_Content", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_Top", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_Title", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_Bottom", Type = typeof(ContentPresenter))]
    public class DialogBase : Control
    {
        private ContentPresenter? _content;

        private ContentPresenter? _top;

        private ContentPresenter? _buttom;

        private TextBlock? _title;

        static DialogBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogBase), new FrameworkPropertyMetadata(typeof(DialogBase)));
        }

        public static readonly DependencyProperty DialogTitleFontSizeProperty = DependencyProperty.Register(
            "DialogTitleFontSize", typeof(int), typeof(DialogBase), new PropertyMetadata(default(int)));

        public int DialogTitleFontSize
        {
            get => (int) GetValue(DialogTitleFontSizeProperty);
            set => SetValue(DialogTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(object), typeof(DialogBase), new PropertyMetadata(default(object), 
                (o, args) => ((DialogBase)o).ContentChanged(args)));

        private void ContentChanged(DependencyPropertyChangedEventArgs args)
        {
            if(_content == null) return;
            _content.Content = args.NewValue;
        }

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(DialogBase), new PropertyMetadata(default(string),
                (o, args) => ((DialogBase)o).TitleChanged(args)));

        private void TitleChanged(DependencyPropertyChangedEventArgs args)
        {
            if(_title == null) return;
            _title.Text = args.NewValue as string;
        }

        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TopProperty = DependencyProperty.Register(
            "Top", typeof(object), typeof(DialogBase), new PropertyMetadata(default(object),
                (o, args) => ((DialogBase)o).TopChanged(args)));

        private void TopChanged(DependencyPropertyChangedEventArgs args)
        {
            if(_top == null) return;
            _top.Content = args.NewValue;
        }

        public object Top
        {
            get => GetValue(TopProperty);
            set => SetValue(TopProperty, value);
        }
        

        public static readonly DependencyProperty BottomProperty = DependencyProperty.Register(
            "Bottom", typeof(object), typeof(DialogBase), new PropertyMetadata(default(object), 
                (o, args) => ((DialogBase)o).ButtomChaned(args)));

        private void ButtomChaned(DependencyPropertyChangedEventArgs args)
        {
            if(_buttom == null) return;
            _buttom.Content = args.NewValue;
        }

        public object Bottom
        {
            get => GetValue(BottomProperty);
            set => SetValue(BottomProperty, value);
        }

        public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register(
            "ContentTemplate", typeof(DataTemplate), typeof(DialogBase), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ContentTemplate
        {
            get => (DataTemplate) GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

        public override void OnApplyTemplate()
        {
            _content = (ContentPresenter?) GetTemplateChild("PART_Content");
            _top = (ContentPresenter?) GetTemplateChild("PART_Top");
            _title = (TextBlock?) GetTemplateChild("PART_Title");
            _buttom = (ContentPresenter?) GetTemplateChild("PART_Bottom");

            base.OnApplyTemplate();
        }
    }
}
