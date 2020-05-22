using System;

namespace ProtoTyping
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var str = Console.ReadLine();
                if(str == "exit")
                    break;

                Console.WriteLine(str);
            }
        }
    }
}
