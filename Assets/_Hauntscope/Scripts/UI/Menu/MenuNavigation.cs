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
            switch (_current.Value)
            {
                case MenuScreen.Credits:
                    _current.Value = MenuScreen.Settings;
                    break;
                case MenuScreen.BestiaryCard:
                    _current.Value = MenuScreen.Bestiary;
                    break;
                default:
                    _current.Value = MenuScreen.Main;
                    break;
            }
        }
    }
}
