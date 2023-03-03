using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Player
    {
        private string name;
        private int money;
        private int position;
        private List<Property> properties;
        private bool hasMonopoly;
        private List<Property> monopolies;
        private bool jailed;
        private int turnsInJail;
        private int goMoney;
        private int goBonus;
        private int doubleCount;
        private bool hasPardon;
        private Card? pardonCard;
        public enum Agents {Human, Stochastic, Statistic, RL}
        private Agents agent;

        public Player(string name, int money, int goMoney, int goBonus, int agentSelection)
        {
            this.name = name;
            this.money = money;
            position = 0;
            properties = new List<Property>();
            hasMonopoly = false;
            monopolies = new List<Property>();
            jailed = false;
            turnsInJail = 0;
            this.goMoney = goMoney;
            this.goBonus = goBonus;
            doubleCount = 0;
            hasPardon = false;
            pardonCard = null;
            SetAgent(agentSelection);
        }

        public void SetAgent(int agentSelection)
        {
            if (agentSelection == 0)
            {
                agent = Agents.Human;
            }
            else if (agentSelection == 1)
            {
                agent = Agents.Stochastic;
            }
            else if (agentSelection == 2)
            {
                agent = Agents.Statistic;
            }
            else if (agentSelection == 3)
            {
                agent = Agents.RL;
            }
            else
            {
                agent = Agents.Human;
            }
        }

        public Agents GetAgent()
        {
            return agent;
        }

        public string GetName()
        {
            return name;
        }
        public int GetMoney()
        {
            return money;
        }
        public int GetPosition()
        {
            return position;
        }
        public List<Property> GetProperties()
        {
            return properties;
        }
        public List<Property> GetMonopolies()
        {
            return monopolies;
        }
        public List<Property> GetTrains()
        {
            List<Property> trains = new List<Property>();
            foreach(Property p in properties) 
            {
                if(p.GetPropertyType() == "Station")
                {
                    trains.Add(p);
                }
            }
            return trains;
        }
        public List<Property> GetUtilities()
        {
            List<Property> utilities = new List<Property>();
            foreach (Property p in properties)
            {
                if (p.GetPropertyType() == "Utility")
                {
                    utilities.Add(p);
                }
            }
            return utilities;
        }
        public bool GetHasMonopoly()
        {
            return hasMonopoly;
        }
        public bool IsJailed()
        {
            return jailed;
        }
        public int GetTurnsInJail()
        {
            return turnsInJail;
        }
        public bool GetHasPardon()
        {
            return hasPardon;
        }
        public int GetDoubleCount()
        {
            return doubleCount;
        }

        public void Pay(int amount)
        {
            money -= amount;
        }
        
        public void GetPaid(int amount)
        {
            money += amount;
        }

        public void AddMonopolies(Property prop)
        {
            monopolies.Add(prop);
        }

        public void PayRent(Player owner, int rent)
        {
            money -= rent;
            owner.GetPaid(rent);
        }

        public void GoToJail()
        {
            position = 10;
            jailed = true;
            turnsInJail = 1;
        }

        public void GetOutOfJail()
        {
            position = 10;
            jailed = false;
            turnsInJail = 0;
        }

        public void PayOutOfJail(int cost)
        {
            money -= cost;
            GetOutOfJail();
        }

        public Card UsePardonCard()
        {
            Card card = pardonCard;
            jailed = false;
            GetOutOfJail();
            hasPardon = false;
            pardonCard = null;
            return card;
        }

        public void IncrementJail()
        {
            turnsInJail++;
        }

        public void IncrementDouble()
        {
            doubleCount++;
        }

        public void DoubleReset()
        {
            doubleCount = 0;
        }

        public void AddProperty(Property property)
        {
            properties.Add(property);
        }

        public void SendProperty(Property property, Player p) 
        {
            p.AddProperty(property);
            properties.Remove(property);
        }

        public void BuyProperty(Property property)
        {
            money -= property.GetPrice();
            properties.Add(property);
        }

        public void SellProperty(Property property)
        {
            money += property.GetPrice() / 2;
            properties.Remove(property);
            property.ReturnToBank();
        }

        public List<Property> GetTradeable()
        {
            List<Property> tradeable = new List<Property>();
            foreach(Property p in properties)
            {
                if(p.GetHouses() == 0)
                {
                    tradeable.Add(p);
                }
            }
            return tradeable;
        }

        public void Mortgage(Property property)
        {
            money += property.GetPrice() / 2;
            property.Mortgage();
        }

        public void Unmortgage(Property property)
        {
            money -= (property.GetPrice() / 2) + (property.GetPrice() / 2 / 10);
            property.Unmortgage();
        }

        public void Move(int roll)
        {
            position += roll;
            if (position > 40)
            {
                money += goMoney;
                position -= 40;
            }
            else if(position == 40)
            {
                money += goMoney + goBonus;
                position -= 40;
            }
            else if(position < 0)
            {
                position += 40;
            }
        }

        public void UseCard(Card card, List<Player> players, ref bool moved)
        {
            if(card.Type == "Item")
            {
                hasPardon = true;
                pardonCard = card;
            }
            if(card.Type == "Paid")
            {
                money += card.Value;
            }
            if(card.Type == "Pay")
            {
                money -= card.Value;
            }
            if(card.Type == "Move")
            {
                moved = true;
                if (position > card.Value)
                {
                    money += goMoney;
                    if (position == 0)
                    {
                        money += goBonus;
                    }
                }
                position = card.Value;
            }
            if(card.Type == "ABSMove")
            {
                moved = true;
                position += card.Value;
            }
            if (card.Type == "Jail")
            {
                moved = true;
                GoToJail();
            }
            if(card.Type == "Station")
            {
                moved = true;
                if(position > 35 || position < 5)
                {
                    if (position > 5)
                    {
                        money += goMoney;
                    }
                    position = 5;
                }
                if (position > 5 && position < 15)
                {
                    position = 15;
                }
                if (position >15 && position < 25)
                {
                    position = 25;
                }
                if (position > 25 && position < 35)
                {
                    position = 35;
                }
            }
            if(card.Type == "Utility")
            {
                moved = true;
                if(position > 28 || position < 12)
                {
                    if (position > 12)
                    {
                        money += goMoney;
                    }
                    position = 12;
                }
                if(position > 12 && position < 28)
                {
                    position = 28;
                }
            }
            if (card.Type == "House")
            {
                int totalHouses = 0;
                foreach (Property p in properties)
                {
                    totalHouses += p.GetHouses();
                }
                if (card.Corc == "chance")
                {
                    money -= totalHouses*25;
                }
                if (card.Corc == "chest")
                {
                    money -= totalHouses * 40;
                }
            }
            if (card.Type == "PlayerPay")
            {
                int noPlayers = players.Count;
                foreach(Player p in players)
                {
                    p.GetPaid(card.Value);
                }
                money -= noPlayers*card.Value;
            }
            if (card.Type == "PlayerPaid")
            {
                int noPlayers = players.Count;
                foreach (Player p in players)
                {
                    p.Pay(card.Value);
                }
                money += noPlayers * card.Value;
            }
        }

        public void Bankrupt()
        {
            foreach(Property p in properties)
            {
                p.ReturnToBank();
            }
        }

    }
}
