using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class StochasticAgent
    {
        private Random picker = new Random();

        public int SelectOption(List<string> options)
        {
            int selection = 0;
            if (options.Exists(x => x == "Mortgage/Sell Property") || options.Exists(x => x == "End Turn") || options.Exists(x => x == "Roll The Dice") || options.Exists(x => x == "Sell Houses") || options.Exists(x => x == "Roll Dice To Get Out Of Jail") || options.Count == 2)
            {
                selection = picker.Next(0, options.Count -2);
            }
            selection = picker.Next(0, options.Count-1);
            return selection;
        }

        public int SelectItem(int range)
        {
            return picker.Next(0, range);
        }
    }
}
