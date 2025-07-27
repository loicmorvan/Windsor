namespace Castle.Windsor.Tests;

public interface ICatalog
{
    void AddItem(object item);

    void RemoveItem(object item);
}