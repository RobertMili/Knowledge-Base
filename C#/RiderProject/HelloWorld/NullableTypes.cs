namespace HelloWorld;

public class NullableTypes
{
    public void PrintMessage()
    {
        int? nullable = null;
        nullable = 8; // Nullable types can be assigned a value or null
        Console.WriteLine(nullable);
        Console.ReadKey();
    }
}