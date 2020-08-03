using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    ///     Interaktionslogik für LanguageSelectorDialogView.xaml
    /// </summary>
    public partial class LanguageSelectorDialogView : ILanguageSelectorDialog
    {
        public LanguageSelectorDialogView()
        {
            InitializeComponent();

            Loaded += (sender, args) => ((LanguageSelectorDialogViewModel) DataContext).OnLoad();
        }


        public Task<AddLanguageDialogResult?> Init(IEnumerable<CultureInfo> initalData)
        {
            var result = new TaskCompletionSource<AddLanguageDialogResult?>();

            DataContext = new LanguageSelectorDialogViewModel(c => result.SetResult(c == null ? null : new AddLanguageDialogResult(c)), initalData.Contains!, Dispatcher);

            return result.Task;
        }
    }

    public abstract class LanguageSelectable : ObservableObject
    {
        private readonly Action<LanguageSelectable> _isSelectedAction;
        private bool _isSelected;

        protected LanguageSelectable(Action<LanguageSelectable> isSelected)
        {
            _isSelectedAction = isSelected;
        }

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

        public abstract CultureInfo Info { get; }
    }

    public sealed class SubLanguage : LanguageSelectable
    {
        public SubLanguage(CultureInfo info, Action<LanguageSelectable> isSelected)
            : base(isSelected)
        {
            Info = info;
        }

        public override CultureInfo Info { get; }
    }

    public sealed class LanguageGroup : LanguageSelectable
    {
        public LanguageGroup(Action<LanguageSelectable> isSelected, CultureInfo baseInfo)
            : base(isSelected)
        {
            Info = baseInfo;
            List = new List<SubLanguage>();
        }

        public override CultureInfo Info { get; }

        public bool IsFiltered { get; set; }

        public bool IsNotFiltered => !IsFiltered;

        public List<SubLanguage> List { get; }
    }

    public sealed class LanguageSelectorDialogViewModel : ObservableObject
    {
        private readonly GroupDictionary<CultureInfo, CultureInfo> _cultures;
        private readonly Dispatcher _dispatcher;
        private readonly Predicate<CultureInfo> _filter;
        private readonly ICollectionView _view;

        private LanguageSelectable? _current;
        private bool _isLoading = true;

        private int _position;
        private string _filterContent = string.Empty;

        public LanguageSelectorDialogViewModel(Action<CultureInfo?> selector, Predicate<CultureInfo> filter, Dispatcher dispatcher)
        {
            _filter = filter;
            _dispatcher = dispatcher;
            _cultures = new GroupDictionary<CultureInfo, CultureInfo>();

            foreach (var info in CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(c => c.EnglishName))
            {
                if (Equals(info, CultureInfo.InvariantCulture) || info.IsNeutralCulture) continue;

                _cultures.Add(info.Parent, info);
            }

            RejectCommand = new SimpleCommand(() => selector(null));
            AddCommand = new SimpleCommand(o => IsSomethingSelected, o => selector(_current?.Info));

            _view = CollectionViewSource.GetDefaultView(LanguageGroups);
            _view.Filter += FilterLang;
        }

        private bool FilterLang(object obj)
        {
            if (string.IsNullOrWhiteSpace(_filterContent)) return true;

            if (obj is LanguageGroup group)
            {
                var lang = group.Info;
                if (lang.Name.Contains(FilterContent) || lang.DisplayName.Contains(FilterContent) || lang.EnglishName.Contains(FilterContent))
                    return true;
                return false;
            }

            return true;
        }

        public ObservableCollection<LanguageGroup> LanguageGroups { get; } = new ObservableCollection<LanguageGroup>();

        private bool IsSomethingSelected => _current != null;

        public string FilterContent
        {
            get => _filterContent;
            set
            {
                if (value == _filterContent) return;
                _filterContent = value;
                OnPropertyChanged();
                _view.Refresh();
            }
        }

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

        public void OnLoad()
        {
            for (var i = 0; i < 15; i++)
            {
                if (_position == _cultures.Count)
                {
                    IsLoading = false;
                    return;
                }

                var (key, value) = _cultures.ElementAt(_position);

                var group = new LanguageGroup(SelectionChanged, key);
                group.IsFiltered = _filter(key);

                foreach (var info in value.Where(c => !_filter(c))) group.List.Add(new SubLanguage(info, SelectionChanged));

                if (!group.IsFiltered || group.List.Count != 0)
                    LanguageGroups.Add(group);

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

            if (_current != null)
                _current.IsSelected = false;

            _current = selectable;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}