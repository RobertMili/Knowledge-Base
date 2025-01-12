// See https://aka.ms/new-console-template for more information

namespace HelloWorld;

public class LearningClass
{
    static void Main2(string[] args)
    {
        var helloWorld = new HelloWorld();
        var person = new Person { Age = 5, Name = "Robert" };


        Console.WriteLine("This is a age " + person.Age + " and name " + person.Name);
        Console.ReadKey();

        // helloWorld.PrintMessage();
        // Console.WriteLine("Hello worlf from Program class");
        // Main(null);
        // Console.ReadKey();
    }
}

class Person
{
    public int Age { get; set; }
    public string Name { get; set; }
}