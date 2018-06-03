using System;
using System.Diagnostics;

namespace DataCollection {
    class Runner {
        static void Main (string[] args) {
            Stopwatch stopWatch = new Stopwatch ();
            stopWatch.Start ();
            Ecology eco = new Ecology ();
            eco.RunScraper ().Wait ();
            stopWatch.Stop ();
            Console.WriteLine (String.Format ("Took {0} milliseconds to complete!", stopWatch.ElapsedMilliseconds.ToString ()));
            Console.WriteLine ("\nDone! Press enter to exit.");
            Console.ReadKey ();
        }
    }
}