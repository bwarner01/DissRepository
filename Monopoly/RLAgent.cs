using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tensorflow;

namespace Monopoly
{
    public class RLAgent
    {
        private TF_Tensor inputTensor;
        
        private double epsilon { get; set; }
        private double alpha { get; set; }
        private double gamma { get; set; }

        Dictionary<State, Dictionary<string, double>> qTable = new Dictionary<State, Dictionary<string, double>>();


    }

    public class State
    {
        private Player p { get; set; }
        private List<Property> board { get; set; }
        private List<Player> playerList { get; set; }

        public State(Player p, List<Property> board, List<Player> playerList) 
        {
            this.p = p;
            this.board = board;
            this.playerList = playerList;
        }

        public float[] ToVector()
        {
            float[] propertyValues = new float[10];
            Dictionary<string, int> owned = new Dictionary<string, int>();
            foreach(Property prop in p.GetProperties())
            {
                if (prop.GetColour()=="NA")
                {
                    if (prop.GetPropertyType() == "Station")
                    {
                        if (owned.ContainsKey("Station"))
                        {
                            owned["Station"]++;
                        }
                        else
                        {
                            owned["Station"] = 1;
                        }
                    }
                    else if (prop.GetPropertyType() == "Utility")
                    {
                        if (owned.ContainsKey("Utility"))
                        {
                            owned["Utility"]++;
                        }
                        else
                        {
                            owned["Utility"] = 1;
                        }
                    }
                }
                else if (owned.ContainsKey(prop.GetColour()))
                {
                    owned[prop.GetColour()]++;
                }
                else
                {
                    owned[prop.GetColour()] = 1;
                }
            }
            if (owned.ContainsKey("Brown"))
            {
                int houses = CheckHouses(p.GetProperties() ,"Brown");
                propertyValues[0] = ((owned["Brown"] * 6) + houses)/17;
            }
            else
            {
                propertyValues[0] = 0;
            }
            if (owned.ContainsKey("Blue"))
            {
                int houses = CheckHouses(p.GetProperties(), "Blue");
                propertyValues[1] = ((owned["Blue"] * 4) + houses) / 17;
            }
            else
            {
                propertyValues[1] = 0;
            }
            if (owned.ContainsKey("Pink"))
            {
                int houses = CheckHouses(p.GetProperties(), "Pink");
                propertyValues[2] = ((owned["Pink"] * 4) + houses) / 17;
            }
            else
            {
                propertyValues[2] = 0;
            }
            if (owned.ContainsKey("Orange"))
            {
                int houses = CheckHouses(p.GetProperties(), "Orange");
                propertyValues[3] = ((owned["Orange"] * 4) + houses) / 17;
            }
            else
            {
                propertyValues[3] = 0;
            }
            if (owned.ContainsKey("Red"))
            {
                int houses = CheckHouses(p.GetProperties(), "Red");
                propertyValues[4] = ((owned["Red"] * 4) + houses) / 17;
            }
            else
            {
                propertyValues[4] = 0;
            }
            if (owned.ContainsKey("Yellow"))
            {
                int houses = CheckHouses(p.GetProperties(), "Yellow");
                propertyValues[5] = ((owned["Yellow"] * 4) + houses) / 17;
            }
            else
            {
                propertyValues[5] = 0;
            }
            if (owned.ContainsKey("Green"))
            {
                int houses = CheckHouses(p.GetProperties(), "Green");
                propertyValues[6] = ((owned["Green"] * 4) + houses) / 17;
            }
            else
            {
                propertyValues[6] = 0;
            }
            if (owned.ContainsKey("Purple"))
            {
                int houses = CheckHouses(p.GetProperties(), "Purple");
                propertyValues[7] = ((owned["Purple"] * 6) + houses) / 17;
            }
            else
            {
                propertyValues[7] = 0;
            }
            if (owned.ContainsKey("Station"))
            {
                propertyValues[8] = (owned["Station"] * 3) / 17;
            }
            else
            {
                propertyValues[8] = 0;
            }
            if (owned.ContainsKey("Utility"))
            {
                propertyValues[9] = (owned["Utility"] * 6) / 17;
            }
            else
            {
                propertyValues[9] = 0;
            }

            float positionIdnetity = p.GetPosition() / 40;

            float moneyProportion = 0;
            float propertyProportion = 0;
            float totalMoney = 0;
            float totalOwned = 0;

            foreach(Property prop in board)
            {
                if (prop.GetOwned())
                {
                    totalOwned++;
                }
            }
            foreach(Player play in playerList)
            {
                totalMoney += play.GetMoney();
            }

            moneyProportion = p.GetMoney() / totalMoney;
            propertyProportion = totalOwned / p.GetProperties().Count;

            float[] stateVector = new float[13];
            stateVector[0] = propertyValues[0];
            stateVector[1] = propertyValues[1];
            stateVector[2] = propertyValues[2];
            stateVector[3] = propertyValues[3];
            stateVector[4] = propertyValues[4];
            stateVector[5] = propertyValues[5];
            stateVector[6] = propertyValues[6];
            stateVector[7] = propertyValues[7];
            stateVector[8] = propertyValues[8];
            stateVector[9] = propertyValues[9];
            stateVector[10] = positionIdnetity;
            stateVector[11] = moneyProportion;
            stateVector[12] = propertyProportion;

            return stateVector;

        }

        public int CheckHouses(List<Property> props, string colour)
        {
            int lowest = 6;
            foreach (Property prop in props)
            {
                if (prop.GetColour() == colour)
                {
                    if (prop.GetHouses() < lowest)
                    {
                        lowest = prop.GetHouses();
                    }
                }
            }
            if (lowest == 6)
            {
                return 0;
            }
            else
            {
                return lowest;
            }
        }
    }
}
