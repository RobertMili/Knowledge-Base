namespace HelloWorld;

public class Generics
{
    public void printMessage()
    {
        var box = new Box<IInterface>();
        IInterface i = new MyClass();
        box.MyValue = i;

        Console.ReadKey();
    }

    public class Box<T> where T : IInterface
    {
        public T MyValue { get; set; }
    }

    public interface IInterface
    {
        int Generic { get; set; }
    }

    public class MyClass : IInterface
    {
        public int Generic { get; set; }
    }
}