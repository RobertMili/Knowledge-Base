// See https://aka.ms/new-console-template for more information


using ClassDesignCase;

class Program
{
    static void Main(string[] args)
    {
        var d1 = new Distance(5);
        var d2 = new Distance(6);
        var d3 = d1 + d2;
        System.Console.WriteLine("As meter " + d3.AsMeaters());
        System.Console.WriteLine("As miles " + d3.AsMiles());
        Console.ReadKey();
    }
}