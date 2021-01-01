using System;

namespace JsonMasher.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var cliOptions = new CliOptions();
            cliOptions.Parse(args);
            if (cliOptions.ShouldShowHelp)
            {
                cliOptions.WriteHelp();
                Environment.Exit(0);
            }
            var runner = new Runner();
            runner.Initialize(cliOptions);
            runner.Run();
        }
    }
}
