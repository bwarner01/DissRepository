using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class StatisticalAgent
    {
        private Random picker = new Random();
        private int stockpileValue = 0;

        public void CalculateStockpile(List<Property> board)
        {
            foreach (Property p in board)
            {
                if(p.GetRent(12) > stockpileValue)
                {
                    stockpileValue = p.GetRent(12);
                }
            }
        }

        public int SelectOption(List<string> options, Player p, Property prop)
        {
            if(options.Count == 2)
            {
                return 0;
            }
            if(options.Exists(x => x == "Buy Property") && p.GetMoney() > prop.GetPrice() + stockpileValue)
            {
                return options.FindIndex(0, x => x == "Buy Property");
            }
            else if (options.Exists(x => x == "Roll The Dice"))
            {
                return options.FindIndex(0, x => x == "Roll The Dice");
            }
            else if (options.Exists(x => x == "End Turn"))
            {
                return options.FindIndex(0, x => x == "End Turn");
            }
            else
            {
                return options.FindIndex(0, x => x == "Declare Bankrupcy");
            }
        }

        public int SelectBuildable(List<Property> buildable, Player p)
        {
            foreach(Property prop in buildable)
            {
                if(prop.GetBuildPrice() + stockpileValue  < p.GetMoney())
                {
                    return buildable.FindIndex(0, x => x == prop);

                }
            }
            return buildable.Count;
        }

        public int AssessTrade(List<Property> tradeIn, List<Property> tradeOut, int moneyIn, int moneyOut)
        {
            int inValue = moneyIn;
            int outValue = moneyOut;
            foreach (Property p in tradeIn)
            {
                inValue += p.GetPrice();
            }
            foreach (Property p in tradeOut)
            {
                outValue += p.GetPrice();
            }
            if(inValue >= outValue)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
