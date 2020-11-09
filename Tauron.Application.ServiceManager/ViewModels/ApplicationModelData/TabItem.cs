using Tauron.Application.Wpf;

namespace Tauron.Application.ServiceManager.ViewModels.ApplicationModelData
{
    public sealed class TabItem : ObservableObject
    {
        private object _header;
        private IViewModel _content;
        private string _metadata;

        public object Header
        {
            get => _header;
            set
            {
                if (Equals(value, _header)) return;
                _header = value;
                OnPropertyChanged();
            }
        }

        public IViewModel Content
        {
            get => _content;
            set
            {
                if (Equals(value, _content)) return;
                _content = value;
                OnPropertyChanged();
            }
        }

        public string Metadata
        {
            get => _metadata;
            set
            {
                if (value == _metadata) return;
                _metadata = value;
                OnPropertyChanged();
            }
        }

        public TabItem(object header, IViewModel content)
        {
            _header = header;
            _content = content;
        }
    }
}