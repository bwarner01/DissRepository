﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class StatisticalAgent
    {
        //To complete: Unmorgatge calculation, What actions to perform in jail, How to put together a trade

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

        public int SelectOption(List<string> options, Player p, Property currentSpace)
        {
            bool buildPossible = false;
            bool unmortgagePossible = false;
            List<Property> buildable = new List<Property>();
            
            foreach (Property prop in p.GetMonopolies())
            {
                if (prop.GetHouses() < 5 && prop.IsBuildable(p.GetMonopolies()))
                {
                    buildable.Add(prop);
                }
            }

            foreach (Property prop in buildable)
            {
                if (prop.GetBuildPrice() < p.GetMoney() - stockpileValue)
                {
                    buildPossible = true;
                }
            }

            foreach(Property prop in p.GetProperties())
            {
                if (prop.GetMortgaged() && p.GetMoney() > (prop.GetPrice() / 2) + (prop.GetPrice() / 2 / 10) + stockpileValue)
                {
                    unmortgagePossible = true;
                }
            }

            if(options.Count == 2)
            {
                return 0;
            }
            else if (options.Exists(x => x == "Use Get Out Jail Free Card"))
            {
                return options.FindIndex(0, x => x == "Use Get Out Jail Free Card");
            }
            else if (options.Exists(x => x == "Pay To Get Out Of Jail") && p.GetMoney() > 50)
            {
                return options.FindIndex(0, x => x == "Pay To Get Out Of Jail");
            }
            else if(options.Exists(x => x == "Roll Dice To Get Out Of Jail"))
            {
                return options.FindIndex(0, x => x == "Roll Dice To Get Out Of Jail");
            }
            else if(options.Exists(x => x == "Buy Property") && p.GetMoney() > currentSpace.GetPrice() + stockpileValue)
            {
                return options.FindIndex(0, x => x == "Buy Property");
            }
            else if (options.Exists(x => x == "Roll The Dice"))
            {
                return options.FindIndex(0, x => x == "Roll The Dice");
            }
            else if (options.Exists(x => x == "Unmortgage Property")  && unmortgagePossible)
            {
                return options.FindIndex(0, x => x == "Unmortgage Property");
            }
            else if (buildPossible && options.Exists(x => x == "Build Houses"))
            {
                return options.FindIndex(0, x => x == "Build Houses");
            }
            else if (p.GetMoney() < 0 && options.Exists(x => x == "Sell Houses") || (p.IsJailed() && p.GetMoney() < 50 && options.Exists(x => x == "Sell Houses")))
            {
                return options.FindIndex(0, x => x == "Sell Houses");
            }
            else if (p.GetMoney() < 0 && options.Exists(x => x == "Mortgage/Sell Property") || (p.IsJailed() && p.GetMoney() < 50 && options.Exists(x => x == "Mortgage/Sell Property")) )
            {
                Console.WriteLine(options.FindIndex(0, x => x == "Mortgage/Sell Property"));
                return options.FindIndex(0, x => x == "Mortgage/Sell Property");
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
                if(prop.GetBuildPrice()  < p.GetMoney())
                {
                    return buildable.FindIndex(0, x => x == prop);

                }
            }
            return buildable.Count;
        }

        public int SelectSellHouse(List<Property> hasHouses, Player p)
        {
            if(p.GetMoney() < 50)
            {
                foreach (Property prop in hasHouses)
                {
                    if ((prop.GetBuildPrice() / 2) + p.GetMoney() > 0)
                    {
                        return hasHouses.FindIndex(0, x => x == prop);
                    }
                }
                return picker.Next(0, hasHouses.Count);
            }
            return hasHouses.Count;
        }

        public int SelectSellProperty(List<Property> sellable, Player p)
        {
            if (p.GetMoney() > 50)
            {
                return sellable.Count;
            }
            foreach (Property prop in sellable)
            {
                if ((prop.GetPrice() / 2) + p.GetMoney() > 0)
                {
                    return sellable.FindIndex(0, x => x == prop);
                }
            }
            return picker.Next(0, sellable.Count);
           
            
        }

        public int SelectUnmortgage(List<Property> mortgages, Player p)
        {
            foreach(Property prop in mortgages)
            {
                if (p.GetMoney() > (prop.GetPrice() / 2) + (prop.GetPrice() / 2 / 10) + stockpileValue)
                {
                    return mortgages.FindIndex(0, x => x == prop);
                }
            }
            return mortgages.Count;
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
