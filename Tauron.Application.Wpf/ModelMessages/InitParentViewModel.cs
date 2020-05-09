namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class InitParentViewModel
    {
        public IViewModel Model { get; }

        public InitParentViewModel(IViewModel model) => Model = model;
    }
}