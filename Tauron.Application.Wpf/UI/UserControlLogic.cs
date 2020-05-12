namespace Tauron.Application.Wpf.UI
{
    public sealed class UserControlLogic : ControlLogicBase<UserControl>
    {
        public UserControlLogic(UserControl userControl, IViewModel model) : base(userControl, model)
        {
        }
    }
}