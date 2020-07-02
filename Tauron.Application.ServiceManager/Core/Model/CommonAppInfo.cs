namespace Tauron.Application.ServiceManager.Core.Model
{
    public sealed class CommonAppInfo : ObservableObject
    {
        private ConnectionState _connectionState = ConnectionState.Offline;

        public ConnectionState ConnectionState
        {
            get => _connectionState;
            set
            {
                if (value == _connectionState) return;
                _connectionState = value;
                OnPropertyChanged();
            }
        }
    }
}