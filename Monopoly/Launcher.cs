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
            string filePath = @"E:\DissRepository\DissRepository\Monopoly\bin\results.csv";
            int noGames = 1000;
            string name1 = "RLbot";
            string name2 = "bot2";
            int agent1 = 3;
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
