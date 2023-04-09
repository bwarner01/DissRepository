using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Trade
    {
        Property ?tradeIn;
        List<Property> tradeOut = new List<Property>();
        int moneyIn = 0;
        int moneyOut = 0;

        Dictionary<string, int> setValue = new Dictionary<string, int>
            {
                { "Brown", 0 },
                { "Blue", 0 },
                { "Pink", 0 },
                { "Orange", 0 },
                { "Red", 0 },
                { "Yellow", 0 },
                { "Green", 0 },
                { "Purple", 0 }
            };

        public Trade(Player p, List<Property> board)
        {
            tradeIn = DesiredProperty(p, board);
            tradeOut = UnwantedProperty(p);
            int money = MoneyInvolved();
            if (money < 0)
            {
                moneyOut = Math.Abs(money);
            }
        }

        public Property GetTIn()
        {
            return tradeIn;
        }

        public List<Property> GetTOut()
        {
            return tradeOut;
        }

        public int GetMIn()
        {
            return moneyIn;
        }

        public int GetMOut()
        {
            return moneyOut;
        }

        public void RemoveTrade(Property prop)
        {
            tradeOut.Remove(prop);
        }

        private Property? DesiredProperty(Player p, List<Property> board)
        {
            Property? desired = null;
            string wanted;
            foreach(Property prop in p.GetProperties())
            {
                setValue[prop.GetColour()]++;
            }
            foreach(KeyValuePair<string, int> pair in setValue)
            {
                if((pair.Key == "Brown" || pair.Key == "Purple") && pair.Value == 1)
                {
                    wanted = pair.Key;
                    desired = AssignProp(p, board, wanted);
                    if(desired != null)
                    {
                        break;
                    }                    
                }
                else if(pair.Value == 2)
                {
                    wanted = pair.Key;
                    desired = AssignProp(p, board, wanted);
                    if (desired != null)
                    {
                        break;
                    }
                }
            }
            return desired;

        }

        private List<Property> UnwantedProperty(Player p)
        {
            List<Property> unwantedProperty = new List<Property>();
            foreach (Property prop in p.GetProperties())
            {
                if(!(prop.GetColour() == "Brown" || prop.GetColour() == "Purple") && setValue[prop.GetColour()] != 2 && setValue[prop.GetColour()] != 3)
                {
                    unwantedProperty.Add(prop);
                    int totalValue = 0;
                    foreach(Property prop2 in unwantedProperty)
                    {
                        totalValue += prop2.GetPrice();
                    }
                    if(totalValue >= tradeIn.GetPrice())
                    {
                        break;
                    }
                }
            }
            return unwantedProperty;
        }

        private int MoneyInvolved()
        {
            int total = 0;
            foreach(Property prop in tradeOut)
            {
                total += prop.GetPrice();
            }
            return total - tradeIn.GetPrice();
        }

        private Property? AssignProp(Player p, List<Property> board, string wanted)
        {
            Property? desired = null;
            foreach (Property prop in board)
            {
                if (prop.GetColour() == wanted && !prop.GetOwner().Equals(p) && !prop.GetOwner().Equals(null))
                {
                    desired = prop;
                    break;
                }
            }
            return desired;
        }
    }
}
