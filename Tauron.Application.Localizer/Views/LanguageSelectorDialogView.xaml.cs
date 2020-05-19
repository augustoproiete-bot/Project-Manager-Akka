using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Xaml.Behaviors.Core;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    /// Interaktionslogik für LanguageSelectorDialogView.xaml
    /// </summary>
    public partial class LanguageSelectorDialogView : ILanguageSelectorDialogView
    {
        public LanguageSelectorDialogView(Action<CultureInfo?> selector, IDialogCoordinator dialogCoordinator)
        {
            InitializeComponent();

            DataContext = new LanguageSelectorDialogViewModel(async c =>
                                                              {
                                                                  await dialogCoordinator.HideMetroDialogAsync(MainWindowViewModel.MainWindow, this);
                                                                  selector(c);
                                                              });
        }

        public BaseMetroDialog Dialog => this;
    }

    public abstract class LanguageSelectable : ObservableObject
    {
        private bool _isSelected;
        private readonly Action<LanguageSelectable> _isSelectedAction;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                OnPropertyChanged();
                _isSelectedAction(this);
            }
        }

        protected LanguageSelectable(Action<LanguageSelectable> isSelected)
        {
            _isSelectedAction = isSelected;
        }

        public abstract CultureInfo Info { get; }
    }

    public sealed class SubLanguage : LanguageSelectable
    {
        public override CultureInfo Info { get; }

        public SubLanguage(CultureInfo info, Action<LanguageSelectable> isSelected) 
            : base(isSelected) => Info = info;
    }

    public sealed class LanguageGroup : LanguageSelectable
    {
        public override  CultureInfo Info { get; }

        public List<SubLanguage> List { get; }

        public LanguageGroup(Action<LanguageSelectable> isSelected, CultureInfo baseInfo)
            : base(isSelected)
        {
            Info = baseInfo;
            List = new List<SubLanguage>();
        }
    }

    public sealed class LanguageSelectorDialogViewModel : ObservableObject
    {
        private readonly Action<CultureInfo?> _selector;
        public List<LanguageGroup> LanguageGroups { get; } = new List<LanguageGroup>();

        private LanguageSelectable? _current;

        public bool IsSomethingSelected => _current != null;

        public ICommand AddCommand { get; }

        public ICommand RejectCommand { get; }

        public LanguageSelectorDialogViewModel(Action<CultureInfo?> selector)
        {
            _selector = selector;
            GroupDictionary<CultureInfo, CultureInfo> cultures = new GroupDictionary<CultureInfo, CultureInfo>();

            foreach (var info in CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(c => c.EnglishName))
            {
                if(Equals(info, CultureInfo.InvariantCulture) || info.IsNeutralCulture) continue;

                cultures.Add(info.Parent, info);
            }

            foreach (var (key, value) in cultures)
            {
                var group = new LanguageGroup(SelectionChanged, key);

                foreach (var info in value) group.List.Add(new SubLanguage(info, SelectionChanged));

                LanguageGroups.Add(group);
            }

            RejectCommand = new ActionCommand(() => selector(null));
            AddCommand = new SimpleCommand(o => IsSomethingSelected, o => selector(_current?.Info));
        }

        private void SelectionChanged(LanguageSelectable selectable)
        {
            if (selectable == _current)
            {
                if (!_current.IsSelected)
                    _current = null;
                return;
            }

            if(_current != null)
                _current.IsSelected = false;

            _current = selectable;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
