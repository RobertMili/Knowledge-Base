using System.Diagnostics;

namespace HelloWorld;

public class Exceptions
{
    public void PrintMessage()
    {
        TextWriter writer = File.CreateText("file.txt");
        var list =  new List<int>();
        try
        {
            writer.WriteLine("Hello2");
            var x = list[0];
        }
        catch (IOException e)
        {
            Console.WriteLine(e.StackTrace);
        }
        catch (Exception e)
        {
            Console.WriteLine("Catch eveything!");
        }
        finally
        {
            writer.Dispose();
            Console.WriteLine("I always run");
        }

        using (var writer2 = File.CreateText("file3.txt")) ;
        {
            
        }
    }
}