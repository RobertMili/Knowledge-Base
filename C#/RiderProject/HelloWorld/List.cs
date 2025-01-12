namespace HelloWorld;

public class List
{
    public void PrintMessage()
    {
        IList<int> list = new List<int>() { 1, 2, 3, 4, 5 };
        var evenNumbers = list.Where(isEven).ToList();
        var oddNumbers = list.Last(i => i % 2 != 0);

        
        Console.WriteLine(string.Join(", ", evenNumbers));
        Console.ReadKey();
    }

    private bool isEven(int n)
    {
        return n % 2 == 0;
    }
}