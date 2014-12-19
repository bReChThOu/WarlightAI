using WarlightAI.IO;
using System;

namespace WarlightAI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var debug = args.Length == 1 && args[0].Equals("debug", StringComparison.OrdinalIgnoreCase);

                new Bot().Run(debug);
            }
            catch (Exception ex)
            {
                var exitMsg = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                Console.Error.Write("Bot crashed. Exception was: {0}", exitMsg);
            }
        }
    }
}