using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuronDotNet;
using NeuronDotNet.Core;
using NeuronDotNet.Core.Backpropagation;
using NeuronDotNet.Core.Initializers;

namespace Monopoly
{
    public class RLAgent
    {

        public State lastState { get; set; }
        public string lastAction { get; set; }

        public List<string> allActions = new List<string>();

        public int epoch;

        private double epsilon { get; set; }
        private double alpha { get; set; }
        private double gamma { get; set; }
        private double lamda { get; set; }

        public List<Trace> traces = new List<Trace>();

        NeuronDotNet.Core.Network network;


        public RLAgent(int inputCount) 
        {
            LinearLayer inputLayer = new LinearLayer(inputCount + 1);
            SigmoidLayer hiddenLayer = new SigmoidLayer(150);
            LinearLayer outputLayer = new LinearLayer(1);

            new BackpropagationConnector(inputLayer, hiddenLayer).Initializer = new RandomFunction(-0.5, 0.5);
            new BackpropagationConnector(hiddenLayer, outputLayer).Initializer = new RandomFunction(-0.5, 0.5);

            this.network = new BackpropagationNetwork(inputLayer, outputLayer);

            this.network.SetLearningRate(0.2);
            this.network.Initialize();


            this.epsilon = 0.5;
            this.alpha = 0.2;
            this.gamma = 0.95;
            this.lamda = 0.8;

            epoch = 1;

            allActions.Add("Make Trade");
            allActions.Add("Roll The Dice");
            allActions.Add("Buy Property");
            allActions.Add("Roll Dice To Get Out Of Jail");
            allActions.Add("Pay To Get Out Of Jail");
            allActions.Add("Use Get Out Jail Free Card");
            allActions.Add("Mortgage/Sell Property");
            allActions.Add("Unmortgage Property");
            allActions.Add("Build Houses");
            allActions.Add("Sell Houses");
            allActions.Add("End Turn");
        }

        public string FirstAction(State state, Dictionary<List<string>,float> actions, List<string> options)
        {
            epoch++;

            string action = "";

            Dictionary<string, double> QValues = CalculateQValues(state);

            action = EpsilonGreedy(QValues, options);

            lastAction = action;
            lastState = state;

            traces.Add(new Trace(state.ToVector(), action, 1));

            return action;
        }

        public string SelectAction(State state, double reward, ref Dictionary<string, double> actions, List<string> options)
        {
            epoch++;

            string action = "";

            Dictionary<string, double> QValues = CalculateQValues(state);

            action = EpsilonGreedy(QValues, options);

            double QValue = 0;
            bool exists = false;

            exists = UpdateQTraces(state, actions, action, reward);
            QValue = QLearning(lastState, lastAction, state, action, reward);

            TrainNueral(lastState, lastAction, QValue);

            if (!exists)
            {
                traces.Add(new Trace(lastState.ToVector(), lastAction, 1));
            }

            lastAction = action;
            lastState = state;

            return action;
        }

        public Network GetNetwork()
        {
            return this.network;
        }

        public void SetNetwork(Network network)
        {
            this.network = network;
        }

        public double[] CreateInput(State state, string action)
        {
            List<double> input = new List<double>();

            input.Add((double)(allActions.FindIndex(0, x => x == action) + 1) / 10);
            foreach(float val in state.ToVector())
            {
                input.Add(Convert.ToDouble(val));
            }

            return input.ToArray();
        }

        public void TrainNueral(State state, string action, double output)
        {
            double[] input = CreateInput(state, action);
            double[] tmp = { output };

            TrainingSample sample = new TrainingSample(input, tmp);

            network.Learn(sample, 0, epoch);
        }

        public string EpsilonGreedy(Dictionary<string, double> QValues, List<string> options)
        {
            string selectedAction = "";

            Random rnd = new Random();
            double val = rnd.NextDouble();

            if(val >= this.epsilon)
            {
                selectedAction = FindMaxValues(QValues);
            }
            else
            {
                selectedAction = options[rnd.Next(0, options.Count)];
            }

            return selectedAction;
        }

        public string FindMaxValues(Dictionary<string, double> tempQ)
        {
            List<string> maxKeyValues = new List<string>();
            double maxVal = -1;

            foreach(KeyValuePair<string, double> kvp in tempQ)
            {
                if(kvp.Value > maxVal)
                {
                    maxKeyValues.Clear();
                    maxVal = kvp.Value;
                }
                if(kvp.Value == maxVal)
                {
                    maxKeyValues.Add(kvp.Key);
                }
            }

            Random rand = new Random();
            string action = maxKeyValues[rand.Next(maxKeyValues.Count)];

            return action;
        }

        public double QLearning(State lastState, string lastAction, State nextState, string bestAction, double reward)
        {
            double QValue = network.Run(CreateInput(lastState,lastAction)).First();
            double previousQ = QValue;

            double newQ = network.Run(CreateInput(nextState, bestAction)).First();

            QValue += alpha * (reward + gamma * newQ - previousQ);

            return QValue;
        }

        public Dictionary<string, double> CalculateQVaules(State state)
        {
            Dictionary<string, double> tempQ = new Dictionary<string, double>();
            foreach (string action in allActions)
            {
                double[] input = CreateInput(state, action);
                double value = network.Run(input)[0];

                tempQ.Add(action, value);
            }

            return tempQ;
        }

        public bool UpdateQTraces(State state, string action, double reward)
        {
            bool found = false;
        }

        public bool CheckSimilar(State state1, State state2)
        {
            bool similar = true;

            double[] state1Vector = state1.ToVector();
            double[] state2Vector = state2.ToVector();

            double moneyComp = Math.Abs(state1Vector[11] - state2Vector[11]) + Math.Abs(state1Vector[12] - state2Vector[12]);
            if (moneyComp >= 0.1)
            {
                similar = false;
            }

            if (!(state1Vector[10] + 0.075 > state2Vector[10] && state1Vector[10] - 0.075 < state2Vector[10]))
            {
                similar = false;
            }

            if (!similar)
            {
                return similar;
            }

            for (int i = 0; i < 10;  i++)
            {
                if (!(state1Vector[i] == state2Vector[i]))
                {
                    similar = false;
                }
            }

            return similar;

        }
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

        public double[] ToVector()
        {
            double[] propertyValues = new double[10];
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

            double[] stateVector = new double[13];
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
