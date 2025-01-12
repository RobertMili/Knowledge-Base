namespace HelloWorld;

public class Delegate
{

    delegate void myDelegate();

    public void printMessage()
    {
        IList<int> x = new List<int>() { 10 };

        myDelegate func;
        if (x.Count > 0)
        {
            func = new myDelegate(print);
        }
        else
        {
            func = new myDelegate(printListEmpty);
        }
        func.Invoke();

        Console.ReadKey();
    }

    private static void print()
    {
        Console.WriteLine("List is not empty");
    }
    private static void printListEmpty()
    {
        Console.WriteLine("List is empty");
    }

    public void printMessage2()
    {
        var funct = new Func<int, int>(print);
        var c = funct.Invoke(5);
        Console.WriteLine(c);
        Console.ReadKey();
    }

    private int print(int arg)
    {
        Console.WriteLine("Hello " + arg);
        return arg + 1;
    }
}