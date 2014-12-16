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
                new Bot().Run();
            }
            catch (Exception ex)
            {
                var exitMsg = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                Console.Write("Bot crashed. Exception was: {0}", exitMsg);
            }
        }
    }
}