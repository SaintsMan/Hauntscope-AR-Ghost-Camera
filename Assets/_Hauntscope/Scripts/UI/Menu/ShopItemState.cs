namespace Hauntscope.UI.Menu
{
    public enum ShopItemState
    {
        Buy,
        CantAfford,
        Equip,
        Equipped,
        Full,
        // Sold for money: the label is the store price.
        Paid,
        // Comes with the full version: the button leads to it.
        FullVersionOnly
    }
}
