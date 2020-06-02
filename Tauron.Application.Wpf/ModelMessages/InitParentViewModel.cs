namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class InitParentViewModel
    {
        public InitParentViewModel(IViewModel model)
        {
            Model = model;
        }

        public IViewModel Model { get; }
    }
}