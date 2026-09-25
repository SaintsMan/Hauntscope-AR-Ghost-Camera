using Hauntscope.Core.Observables;

namespace Hauntscope.UI.Menu
{
    public sealed class MenuNavigation
    {
        private readonly ObservableValue<MenuScreen> _current = new ObservableValue<MenuScreen>(MenuScreen.Main);

        public IReadOnlyObservableValue<MenuScreen> Current => _current;

        public void Show(MenuScreen screen)
        {
            _current.Value = screen;
        }

        public void Back()
        {
            _current.Value = _current.Value == MenuScreen.Credits ? MenuScreen.Settings : MenuScreen.Main;
        }
    }
}
