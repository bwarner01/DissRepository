using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Monopoly
{
    class Launcher
    {        
        public static void Main(string[] args)
        {
            string filePath = @"E:\DissRepository-main\Monopoly\bin\Test.csv";
            int noGames = 1;
            string name1 = "Human Player";
            string name2 = "Standardbot";
            int agent1 = 0;
            int agent2 = 2;
            Program game = new Program();
            //Check value for starting RLAgent
            RLAgent agent = new RLAgent(13);
            List<string> results = game.GetResults(noGames, name1, agent1, name2, agent2, agent);

            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.WriteLine("Round,Winner");
                for (int i = 0; i < noGames; i++)
                {
                    string written = (i + "," + results[i]);
                    file.WriteLine(written);
                }
            }
            
        }
    }
}
