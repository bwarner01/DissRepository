using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Property
    {
        private int price;
        private string name;
        private string type;
        private string colour;
        private bool owned;
        private Player? owner;
        private bool mortgaged;
        private int position;
        private int[] rent = new int[6];
        private int buildPrice;
        private int houses;

        public Property(String name, int position, String type, String colour, int price, int build, int rent, int rent1, int rent2, int rent3, int rent4, int rent5)
        {
            this.name = name;
            this.position = position;
            this.type = type;
            this.colour = colour;
            this.price = price;
            buildPrice = build;
            this.rent[0] = rent;
            this.rent[1] = rent1;
            this.rent[2] = rent2;
            this.rent[3] = rent3;
            this.rent[4] = rent4;
            this.rent[5] = rent5;
            owned = false;
            owner = null;
            mortgaged = false;
            houses = 0;
        }

        public void SetOwner(Player owner)
        {
            this.owner = owner;
        }
        
        public int GetPrice()
        {
            return price;
        }

        public int GetBuildPrice()
        {
            return buildPrice;
        }

        public int GetHouses()
        {
            return houses;
        }

        public string GetColour()
        {
            return colour;
        }

        public string GetName()
        {
            return name;
        }

        public string GetPropertyType()
        {
            return type;
        }

        public bool GetOwned()
        {
            return owned;
        }

        public bool GetMortgaged()
        {
            return mortgaged;
        }

        public int GetPosition()
        {
            return position;
        }

        public int GetRent(int diceRoll)
        {
            if(this.type == "Property")
            {
                return rent[houses];
            }
            if (this.type == "Utility")
            {
                return UtilityRent(diceRoll);
            }
            if (this.type == "Station")
            {
                return TrainRent();
            }
            else
            {
                return 0;
            }
        }

        public void Bought(Player player)
        {
            owner = player;
            owned = true;
        }

        public void ReturnToBank()
        {
            owned = false;
            owner = null;
            houses = 0;
            mortgaged = false;
        }

        public void Mortgage()
        {
            mortgaged = true;
        }
        public void Unmortgage()
        {
            mortgaged = false;
        }

        public void Build()
        {
            houses++;
            owner.Pay(buildPrice);
        }

        public Player? GetOwner()
        {
            return owner;
        }

        public void SellHouse(Player? owner)
        {
            houses--;
            owner.GetPaid(buildPrice / 2);
        }

        public int UtilityRent(int roll)
        {
            if(owner != null)
            {
                int amount = owner.GetUtilities().Count;
                if (amount == 1)
                {
                    return roll * 4;
                }
                else if (amount == 2)
                {
                    return roll * 10;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public int TrainRent()
        {
            if(owner != null)
            {
                int amount = owner.GetTrains().Count;
                if (amount == 1)
                {
                    return 25;
                }
                if (amount == 2)
                {
                    return 50;
                }
                if (amount == 3)
                {
                    return 100;
                }
                if (amount == 4)
                {
                    return 200;
                }
                else
                {
                    return 0;
                }
            }            
            else
            {
                return 0;
            }
         
        }

        public bool IsBuildable(List<Property> properties)
        {
            int minHouses = houses;
            bool maxed = true;
            foreach(Property p in properties)
            {
                if(p.GetColour().Equals(colour) && p.GetHouses() < minHouses)
                {
                    minHouses = p.GetHouses();
                }
                if(p.GetHouses() < 5)
                {
                    maxed = false;
                }
            }
            if (minHouses < houses || maxed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
