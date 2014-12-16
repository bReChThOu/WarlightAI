// <copyright file="Bot.cs">
//        Copyright (c) 2013 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>10/03/2014</date>
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WarlightAI.IO
{
    /// <summary>
    /// Provides a class that reads and parses the input
    /// </summary>
    public class Bot
    {
        /// <summary>
        /// The parser
        /// </summary>
        private CommandParser parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bot"/> class.
        /// </summary>
        public Bot()
        {
            parser = new CommandParser();
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
            while (true)
            {
                /* Normalize the input:
                 * 1) Trim leading and trailing whitespaces
                 * 2) Replace all whitespaces with a regular space
                 * */
                String line = Console.ReadLine().Trim();
                if (!String.IsNullOrEmpty(line))
                {
                    Regex.Replace(line, "\\s+", " ");

                    //Let the parser deal with it
                    parser.Parse(line);
                }
            }
        }
    }
}
