namespace HelloWorld;

public class Events
{
    public void printMessage()
    {
        var rooster = new Rooster();
        var person = new Person();
        rooster.DoCaw();
        rooster.Caw += person.SayImUp;
        rooster.DoCaw();
        Console.ReadKey();


    }

    class Rooster
    {
        public event Action Caw;

        public void DoCaw()
        {
            if (Caw != null)
            {
                Console.WriteLine("Not null");
                Caw();
            }
            else
            {
                Console.WriteLine("Null");
            }

        }
    }

    class Person
    {
        public void SayImUp()
        {
            Console.WriteLine("I'm up");
        }
    }
}