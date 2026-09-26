using Hauntscope.Core.Observables;

namespace Hauntscope.UI.Menu
{
    public sealed class MenuNavigation
    {
        private readonly ObservableValue<MenuScreen> _current = new ObservableValue<MenuScreen>(MenuScreen.Main);
        private readonly ObservableValue<ShopTab> _shopTab = new ObservableValue<ShopTab>(ShopTab.Lasers);

        public IReadOnlyObservableValue<MenuScreen> Current => _current;

        public IReadOnlyObservableValue<ShopTab> CurrentShopTab => _shopTab;

        public void Show(MenuScreen screen)
        {
            _current.Value = screen;
        }

        // The loadout row opens the shop on the gear tab; the menu button opens it on lasers.
        public void ShowShop(ShopTab tab)
        {
            _shopTab.Value = tab;
            _current.Value = MenuScreen.Shop;
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
