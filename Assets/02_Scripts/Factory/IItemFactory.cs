public interface IItemFactory
{
    IItem GetItem(EItemType type);
    void ReturnItem(IItem item);
}