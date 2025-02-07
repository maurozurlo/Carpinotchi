namespace Cashier
{
    [System.Flags]
    public enum ItemType
    {
        _ = 0,
        veggie = 1 << 0,
        meat = 1 << 1,
        drink = 1 << 2,
        cheese = 1 << 3,
        misc = 1 << 4,
    }
}
