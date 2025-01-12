namespace HelloWorld;

public class Threading
{
    public void printMessage()
    {
        var thread = new Thread(run);
        thread.Start();


        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(100);
            Console.WriteLine("Hello from thread 1");
        }
    }

    private static void run()
    {
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(  50);
            Console.WriteLine("Hello from thread 2");

        }
    }
}