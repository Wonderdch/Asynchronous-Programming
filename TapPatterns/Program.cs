using System;
using System.Threading.Tasks;

namespace TapPatterns
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("start");
            var manager = new TapPatternManager();
            await manager.LaunchAsync();
            Console.ReadLine();
        }

    }
}
