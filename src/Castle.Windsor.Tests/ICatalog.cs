namespace Castle.Windsor.Tests;

public interface ICatalog
{
    void AddItem();

    void RemoveItem(object item);
}