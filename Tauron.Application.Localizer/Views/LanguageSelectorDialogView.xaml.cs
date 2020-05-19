using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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
        private readonly IDialogCoordinator _dialogCoordinator;

        public LanguageSelectorDialogView(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            InitializeComponent();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            ((LanguageSelectorDialogViewModel)DataContext).OnLoad();
        }

        public BaseMetroDialog Dialog => this;
        public void Init(Action<CultureInfo?> selector, Predicate<CultureInfo> filter)
        {
            DataContext = new LanguageSelectorDialogViewModel(async c =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(MainWindowViewModel.MainWindow, this);
                selector(c);
            }, filter, Dispatcher);
        }
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

        public bool IsFiltered { get; set; }

        public bool IsNotFiltered => !IsFiltered;

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
        private bool _isLoading = true;
        private readonly Predicate<CultureInfo> _filter;
        private readonly Dispatcher _dispatcher;
        private readonly GroupDictionary<CultureInfo, CultureInfo> _cultures;

        public ObservableCollection<LanguageGroup> LanguageGroups { get; } = new ObservableCollection<LanguageGroup>();

        private LanguageSelectable? _current;

        public bool IsSomethingSelected => _current != null;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (value == _isLoading) return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCommand { get; }

        public ICommand RejectCommand { get; }

        public LanguageSelectorDialogViewModel(Action<CultureInfo?> selector, Predicate<CultureInfo> filter, Dispatcher dispatcher)
        {
            _filter = filter;
            _dispatcher = dispatcher;
            _cultures = new GroupDictionary<CultureInfo, CultureInfo>();

            foreach (var info in CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(c => c.EnglishName))
            {
                if(Equals(info, CultureInfo.InvariantCulture) || info.IsNeutralCulture) continue;

                _cultures.Add(info.Parent, info);
            }

            RejectCommand = new ActionCommand(() => selector(null));
            AddCommand = new SimpleCommand(o => IsSomethingSelected, o => selector(_current?.Info));
        }

        private int _position;

        public void OnLoad()
        {
            for (var i = 0; i < 3; i++)
            {
                if (_position == _cultures.Count)
                {
                    IsLoading = false;
                    return;
                }

                var (key, value) = _cultures.ElementAt(_position);
                {
                    var group = new LanguageGroup(SelectionChanged, key);
                    group.IsFiltered = _filter(key);

                    foreach (var info in value.Where(c => !_filter(c))) group.List.Add(new SubLanguage(info, SelectionChanged));

                    if(group.IsFiltered && group.List.Count == 0) continue;
                    LanguageGroups.Add(group);
                }

                _position++;
            }


            _dispatcher.BeginInvoke(new Action(OnLoad), DispatcherPriority.ApplicationIdle);
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
