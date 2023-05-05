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
    public class Program
    {
        static List<Player> players = new List<Player>();
        static List<Property> board = new List<Property>();
        static List<Card> chance = new List<Card>();
        static List<Card> chest = new List<Card>();
        static Random dice = new Random();
        static StochasticAgent stochAg = new StochasticAgent();
        static StatisticalAgent statAg = new StatisticalAgent();


        static int startMoney = 1500;
        static int goMoney = 200;
        static int goBonus = 100;
        static int bail = 50;
        static int parkingMoneyValue = 0;
        static int numPlayers = 2;

        static bool mortgage = false;
        static bool jailRent = false;
        static bool doubleRentMonopoly = false;
        static bool freeParkingMoney = false;
        static bool evenBuild = false;
        private int gamesCompleted;

        public string Game(string player1, int agent1, string player2, int agent2, RLAgent RLagent)
        {
            players.Clear();
            board.Clear();
            chance.Clear();
            chest.Clear();
            string path = @"E:\DissRepository-main\Monopoly\board.csv";
            var parser = new StreamReader(path);
            var headerLine = parser.ReadLine();
            int goes = 0;
            bool draw = false;
            while (!parser.EndOfStream)
            {
                var line = parser.ReadLine();
                var row = line.Split(",");
                board.Add(new Property(row[0], Convert.ToInt32(row[1]), row[2], row[3], Convert.ToInt32(row[4]), Convert.ToInt32(row[5]), Convert.ToInt32(row[6]), Convert.ToInt32(row[7]), Convert.ToInt32(row[8]), Convert.ToInt32(row[9]), Convert.ToInt32(row[10]), Convert.ToInt32(row[11])));
            }

            path = @"E:\DissRepository-main\Monopoly\chance.csv";
            parser = new StreamReader(path);
            headerLine = parser.ReadLine();
            while (!parser.EndOfStream)
            {
                var line = parser.ReadLine();
                var row = line.Split(",");
                chance.Add(new Card("chance", row[0], Convert.ToInt32(row[1]), row[2]));
            }

            path = @"E:\DissRepository-main\Monopoly\chest.csv";
            parser = new StreamReader(path);
            headerLine = parser.ReadLine();
            while (!parser.EndOfStream)
            {
                var line = parser.ReadLine();
                var row = line.Split(",");
                chest.Add(new Card("chest", row[0], Convert.ToInt32(row[1]), row[2]));
            }

            Shuffle(chance);
            Shuffle(chest);

            //for (int i = 0; i < numPlayers; i++)
            //{
            //    string name = InputString(string.Format("\nPlease enter player {0}'s name.", i + 1), 1, 25);
            //    int agentSelection = InputInt(string.Format("\nInsert player type:\n0: Human \n1: Stochastic Agent \n2: Hard Coded Agent \n3: RL Agent"), 0, 3);
            //    players.Add(new Player(name, startMoney, goMoney, goBonus, agentSelection));
            //}
            players.Add(new Player(player1, startMoney, goMoney, goBonus, agent1));
            players.Add(new Player(player2, startMoney, goMoney, goBonus, agent2));

            while (players.Count > 1 && !draw)
            {               
                List<Player> eliminated = new List<Player>();
                foreach(Player p in players)
                {                   
                    Console.WriteLine("\n It's {0}'s turn", p.GetName());
                    int diceRoll = 0;
                    bool hasRolled = false;
                    bool turnEnded = false;
                    bool canRoll = true;
                    bool paid = false;
                    bool bankrupt = false;
                    bool triedSell = false;
                    bool triedBuild = false;
                    bool triedTrade = false;
                    bool triedUnmortgage = false;
                    p.DoubleReset();
                    int choice = 0;
                    while (!turnEnded)
                    {
                        statAg.CalculateStockpile(board);
                        if (board[p.GetPosition()].GetName().Equals("Jail"))
                        {
                            if(!p.IsJailed())
                            {
                                Console.WriteLine("You are currently at {0} (just visiting) and have £{1}", board[p.GetPosition()].GetName(), p.GetMoney());
                            }
                            else
                            {
                                Console.WriteLine("You are currently in {0} and have £{1}", board[p.GetPosition()].GetName(), p.GetMoney());
                            }
                        }
                        else
                        {
                            Console.WriteLine("You are currently at {0} and have {1}. Games completed: {2}", board[p.GetPosition()].GetName(), p.GetMoney(), gamesCompleted);
                        }
                        List<string> option = GenOptions(p, turnEnded, hasRolled, canRoll, paid);
                        Console.WriteLine("You can: \n");
                        for(int i = 0; i < option.Count; i++)
                        {
                            Console.WriteLine("{0}: {1}", i, option[i]);
                        }
                        
                        switch (p.GetAgent())
                        {
                            case Player.Agents.Human:
                            {
                                choice = InputInt("\n Enter the number of your choice: ", 0, option.Count - 1);
                                break;
                            }
                            case Player.Agents.Stochastic:
                            {
                                choice = stochAg.SelectOption(option);
                                Console.WriteLine(choice);
                                break;
                            }
                            case Player.Agents.Statistic:
                            {
                                choice = statAg.SelectOption(option, p, board[p.GetPosition()]);
                                Console.WriteLine(choice);
                                break;
                            }
                            case Player.Agents.RL:
                                {
                                    Trade trade = new Trade(p, board);
                                    List<string> AIOptions = new List<string>();
                                    foreach(string o in option)
                                    {
                                        AIOptions.Add(o);
                                    }
                                    if (triedSell)
                                    {
                                        AIOptions.Remove("Mortgage/Sell Property");
                                        AIOptions.Remove("Sell Houses");
                                    }
                                    if (triedBuild)
                                    {
                                        AIOptions.Remove("Build Houses");
                                    }
                                    if (triedTrade)
                                    {
                                        AIOptions.Remove("Make Trade");
                                    }
                                    if (triedUnmortgage)
                                    {
                                        AIOptions.Remove("Unmortgage Property");
                                    }
                                    AIOptions.Remove("View Your Property");
                                    AIOptions.Remove("View Position information");
                                    AIOptions.Remove("View Player Data");
                                    AIOptions.Remove("Declare Bankrupcy");
                                    if(trade.GetTIn() == null)
                                    {
                                        AIOptions.Remove("Make Trade");
                                    }
                                    if(option.Count == 2)
                                    {
                                        choice = 0;
                                    }
                                    //else if(option.Exists(x => x == "Roll The Dice"))
                                    //{
                                    //    choice = option.FindIndex(0, x => x == "Roll The Dice");
                                    //}
                                    else
                                    {
                                        if(goes - numPlayers <= 0)
                                        {
                                            State state = new State(p, board, players);
                                            string selection = RLagent.FirstAction(state, AIOptions);
                                            choice = option.FindIndex(0, x => x == selection);
                                        }
                                        else
                                        {
                                            State state = new State(p, board, players);
                                            double reward = CalculateReward(p);
                                            string selection = RLagent.SelectAction(state, reward, AIOptions);
                                            choice = option.FindIndex(0, x => x == selection);
                                        }
                                    }
                                    Console.WriteLine(choice);
                                    break;
                                }
                        }
                        
                        PerformAction(p, option, choice, ref canRoll, ref hasRolled, ref turnEnded, ref paid, ref diceRoll, ref bankrupt, ref eliminated, ref triedSell, ref triedBuild, ref triedTrade, ref triedUnmortgage);                        
                    }
                    goes += 1;
                    if (goes > 2000)
                    {
                        draw = true;
                        break;
                    }
                    if (players.Count - eliminated.Count <= 1 || draw)
                    {
                        break;
                    }
                }
                foreach(Player p in eliminated)
                {
                    players.Remove(p);
                }
            }
            if (draw)
            {
                gamesCompleted++;
                Console.WriteLine("The game has ended in a slatemate");
                RLagent.AgentEnd(-10);
                return "draw";
            }
            else
            {
                gamesCompleted++;
                Console.WriteLine("{0} wins", players[0].GetName());
                if (players[0].GetAgent() == Player.Agents.RL)
                {
                    RLagent.AgentEnd(10);
                }
                else
                {
                    RLagent.AgentEnd(-10);
                }
                return players[0].GetName();
            }
        }

        public static void Shuffle(List<Card> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = dice.Next(n + 1);
                Card value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static int RollDice(Random dice, ref bool doubles)
        {
            int rollOne = dice.Next(1, 7);
            int rollTwo = dice.Next(1, 7);
            if (rollOne == rollTwo)
            {
                doubles = true;
            }
            else
            {
                doubles = false;
            }
            return rollOne + rollTwo;
        }

        private static int InputInt(string message, int min, int max)
        {
            Console.WriteLine(message);
            int num;
            while (true)
            {

                if (Int32.TryParse(Console.ReadLine(), out num) && num >= min && num <= max)
                {
                    Console.WriteLine("\n***\n");
                    break;
                }
                else
                {
                    Console.WriteLine("Please enter an integral value between {0} and {1}.", min, max);
                }
            }
            return num;
        }

        private static string InputString(string message, int min_len, int max_len)
        {
            Console.WriteLine(message);
            string input;
            while (true)
            {
                input = Console.ReadLine();
                if (input.Length >= min_len && input.Length <= max_len)
                {
                    Console.WriteLine("\n***\n");
                    break;
                }
                else
                {
                    Console.WriteLine("Please enter a string of length between {0} and {1}.", min_len, max_len);
                }
            }
            return input;
        }

        private static List<string> GenOptions(Player p, bool turnEnded, bool hasRolled, bool canRoll, bool paid)
        {
            List<string> options = new List<string>
            {
                "View Your Property",
                "View Position Information",
                "View Player Data",
                
            };

            if (!p.IsJailed())
            {
                if (canRoll)
                {
                    options.Add("Roll The Dice");
                }

                if (hasRolled && !board[p.GetPosition()].GetOwned() && !board[p.GetPosition()].GetMortgaged() && p.GetMoney() > board[p.GetPosition()].GetPrice() && (board[p.GetPosition()].GetPropertyType() == "Property" || board[p.GetPosition()].GetPropertyType() == "Station" || board[p.GetPosition()].GetPropertyType() == "Utility"))
                {
                    options.Add("Buy Property");
                }

            }
            else if (p.GetTurnsInJail() < 4 && p.GetTurnsInJail() > 0)
            {
                if (canRoll)
                {
                    options.Add("Roll Dice To Get Out Of Jail");
                    options.Add("Pay To Get Out Of Jail");
                    if (p.GetHasPardon())
                    {
                        options.Add("Use Get Out Jail Free Card");
                    }
                }
            }
            else
            {
                options.Add("Pay To Get Out Of Jail");
                if (p.GetHasPardon())
                {
                    options.Add("Use Get Out Jail Free Card");
                }
            }
                
            if (!canRoll && p.GetMoney() >= 0)
            {
                options.Add("End Turn");
                options.Add("Make Trade");
            }

            if (p.GetProperties().Count > 0)
            {
                bool hasUnMortgage = false;
                foreach (Property prop in p.GetTradeable())
                {
                    if (prop.GetMortgaged() == false)
                    {
                        hasUnMortgage = true;
                    }
                }
                if (hasUnMortgage)
                {
                    options.Add("Mortgage/Sell Property");
                }
                bool hasMortgage = false;
                foreach (Property prop in p.GetProperties())
                {
                    if (prop.GetMortgaged() == true)
                    {
                        hasMortgage = true;
                    }
                }
                if (hasMortgage)
                {
                    options.Add("Unmortgage Property");
                }
            }

            if (p.CanBuild())
            {
                options.Add("Build Houses");              
            }
            bool hasHouses = false;
            foreach (Property prop in p.GetProperties())
            {
                if (prop.GetHouses() > 0)
                {
                    hasHouses = true;
                }
            }
            if (hasHouses)
            {
                options.Add("Sell Houses");
            }

            if (board[p.GetPosition()].GetOwned() && !board[p.GetPosition()].GetMortgaged() && board[p.GetPosition()].GetOwner() != p && hasRolled == true && paid == false)
            {               
                Console.WriteLine("This property is owned by {0}!", board[p.GetPosition()].GetOwner().GetName());
                if (!jailRent && board[p.GetPosition()].GetOwner().IsJailed())
                {
                    Console.WriteLine("They are in jail so you do not have to pay rent");
                }
                else
                {
                    options.Clear();
                    options.Add("Pay Rent");
                }              
            }

            if(board[p.GetPosition()].GetPropertyType() == "Tax" && hasRolled == true && paid == false)
            {
                options.Clear();
                options.Add("Pay Tax");
            }

            if(board[p.GetPosition()].GetPropertyType() == "Parking" && paid == false && hasRolled == true && freeParkingMoney == true && parkingMoneyValue > 0) 
            {
                options.Clear();
                options.Add("Claim Free Parking Money");
            }

            if (board[p.GetPosition()].GetPropertyType() == "Chest" && paid == false && hasRolled == true)
            {
                options.Clear();
                options.Add("Draw Community Chest Card");
            }

            if (board[p.GetPosition()].GetPropertyType() == "Chance" && paid == false && hasRolled == true)
            {
                options.Clear();
                options.Add("Draw Chance Card");
            }
            options.Add("Declare Bankrupcy");

            return options;
        }

        private static void PerformAction(Player p, List<string> options, int choice, ref bool canRoll, ref bool hasRolled, ref bool turnEnded, ref bool paid, ref int diceRoll, ref bool bankrupt, ref List<Player> eliminated, ref bool triedSell, ref bool triedBuild, ref bool triedTrade, ref bool triedUnmortgage)
        {
            string action = options[choice];

            if (action == "Declare Bankrupcy")
            {
                p.Bankrupt();
                turnEnded = true;
                eliminated.Add(p);
            }
            if (action== "View Your Property")
            {
                Console.WriteLine("You own: \n");
                foreach(Property prop in p.GetProperties())
                {
                    Console.WriteLine("Name: {0}, Colour: {1}, Price: {2}, Build Price: {3} Rent: {4}, Houses: {5} Mortgaged: {6}", prop.GetName(), prop.GetColour(), prop.GetPrice(), prop.GetBuildPrice(), prop.GetRent(diceRoll), prop.GetHouses(), prop.GetMortgaged());
                }
                if(p.GetProperties().Count == 0)
                {
                    Console.WriteLine("Nothing");
                }
            }
            if(action== "View Position information")
            {
                Console.WriteLine("You are on: {0}", board[p.GetPosition()].GetName());
            }
            if(action== "View Player Data")
            {
                foreach(Player player in players)
                {
                    Console.WriteLine("Name: {0}, Money: {1}", player.GetName(), player.GetMoney());
                }
            }
            if(action== "Roll The Dice")
            {
                bool doubles = false;
                diceRoll = RollDice(dice, ref doubles);
                if(doubles && p.GetDoubleCount() < 2)
                {
                    Console.WriteLine("You rolled a {0} with a double {1}", diceRoll, diceRoll / 2);
                    p.IncrementDouble();
                    canRoll = true;
                    paid = false;
                    hasRolled = true;
                    p.Move(diceRoll);
                }
                else if (doubles && p.GetDoubleCount() == 2)
                {
                    Console.WriteLine("You rolled 3 doubles in a row and will be sent to jail");
                    canRoll = false;
                    hasRolled = true;
                    p.GoToJail();
                }
                else
                {
                    Console.WriteLine("You rolled a {0}", diceRoll);
                    canRoll = false;
                    hasRolled = true;
                    paid = false;
                    p.Move(diceRoll);
                }

                if(board[p.GetPosition()].GetPropertyType() == "GoToJail")
                {
                    Console.WriteLine("Go To Jail");
                    hasRolled = true;
                    canRoll = false;
                    p.GoToJail();
                }
                triedBuild = false;
                triedSell = false;
                triedTrade = false;
                triedUnmortgage = false;

            }
            if(action== "Buy Property")
            {
                p.BuyProperty(board[p.GetPosition()]);
                board[p.GetPosition()].Bought(p);
                string colour = board[p.GetPosition()].GetColour();
                p.CheckForMonopolies(colour, board);
            }
            if(action== "Mortgage/Sell Property")
            {
                List<Property> properties = p.GetTradeable();
                Console.WriteLine("You can sell/mortgage one of the following properties:\n");
                for(int i = 0; i < properties.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, properties[i].GetName());
                }
                Console.WriteLine("{0}: Cancel", properties.Count);
                int number = properties.Count;
                switch (p.GetAgent())
                {
                    case Player.Agents.Human:
                        {
                            number = InputInt("\nEnter the number corresponding to the desired property.", 0, properties.Count);
                            break;
                        }
                    case Player.Agents.Stochastic:
                        {
                            number = stochAg.SelectItem(properties.Count);
                            Console.WriteLine(number);
                            break;
                        }
                    case Player.Agents.Statistic:
                        {
                            number = statAg.SelectSellProperty(properties, p);
                            Console.WriteLine(number);
                            break;
                        }
                    case Player.Agents.RL:
                        {
                            number = statAg.SelectSellProperty(properties, p);
                            Console.WriteLine(number);
                            break;
                        }
                }
                if (number < properties.Count)
                {
                    if (!mortgage)
                    {
                        p.SellProperty(properties[number]);
                        p.RemoveMonopolies(properties[number]);
                        Console.WriteLine("You have sold {0} and recieved {1}", properties[number].GetName(), properties[number].GetPrice()/2);
                    }
                    else
                    {
                        p.Mortgage(properties[number]);
                        Console.WriteLine("You have mortgaged {0} and recieved {1}", properties[number].GetName(), properties[number].GetPrice() / 2);
                    }
                }
                else
                {
                    Console.WriteLine("Cancelled");
                    triedSell = true;
                }
            }
            if(action== "Unmortgage Property")
            {
                List<Property> mortgages = new List<Property>();
                foreach(Property prop in p.GetProperties())
                {
                    if (prop.GetMortgaged())
                    {
                        mortgages.Add(prop);
                    }
                }
                Console.WriteLine("You can unmortgage one of the following properties:\n");
                for (int i = 0; i < mortgages.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, mortgages[i].GetName());
                }
                Console.WriteLine("{0}: Cancel", mortgages.Count);
                int number = mortgages.Count;
                switch (p.GetAgent())
                {
                    case Player.Agents.Human:
                        {
                            number = InputInt("\nEnter the number corresponding to the desired property.", 0, mortgages.Count);
                            break;
                        }
                    case Player.Agents.Stochastic:
                        {
                            number = stochAg.SelectItem(mortgages.Count);
                            Console.WriteLine(number);
                            break;
                        }
                    case Player.Agents.Statistic:
                        {
                            number = statAg.SelectUnmortgage(mortgages, p);
                            Console.WriteLine(number);
                            break;
                        }
                    case Player.Agents.RL:
                        {
                            number = statAg.SelectUnmortgage(mortgages, p);
                            Console.WriteLine(number);
                            break;
                        }
                }
                if (number < mortgages.Count)
                {
                    if(p.GetMoney() > (mortgages[number].GetPrice()/2)+(mortgages[number].GetPrice() / 20))
                    {
                        p.Unmortgage(mortgages[number]);
                        Console.WriteLine("{0} is now unmortgaged", mortgages[number].GetName());
                    }
                    else
                    {
                        Console.WriteLine("You cannot afford to unmortgage this property");
                    }
                }
                else
                {
                    Console.WriteLine("Cancelled");
                    triedUnmortgage = true;
                }
            }
            if(action== "Build Houses")
            {
                List<Property> sets = p.GetMonopolies();
                List<Property> buildable = new List<Property>();
                Console.WriteLine("You can build on: \n");
                foreach(Property prop in sets)
                {
                    if(prop.GetHouses() < 5 && prop.IsBuildable(sets))
                    {
                        buildable.Add(prop);
                    }
                }
                for (int i = 0; i < buildable.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, buildable[i].GetName());
                }
                Console.WriteLine("{0}: Cancel", buildable.Count);
                int number = buildable.Count;
                switch (p.GetAgent())
                {
                    case Player.Agents.Human:
                        {
                            number = InputInt("\nEnter the number corresponding to the desired property.", 0, buildable.Count);
                            break;
                        }
                    case Player.Agents.Stochastic:
                        {
                            number = stochAg.SelectItem(buildable.Count);
                            Console.WriteLine(number);
                            break;
                        }
                    case Player.Agents.Statistic:
                        {
                            number = statAg.SelectBuildable(buildable, p);
                            Console.WriteLine(number);
                            break;
                        }
                    case Player.Agents.RL:
                        {
                            number = statAg.SelectBuildable(buildable, p);
                            Console.WriteLine(number);
                            break;
                        }
                }
                if (number < buildable.Count)
                {
                    if(p.GetMoney() > buildable[number].GetBuildPrice())
                    {
                        buildable[number].Build();
                        if (buildable[number].GetHouses() < 5)
                        {
                            Console.WriteLine("There are now {0} houses on {1}.", buildable[number].GetHouses(), buildable[number].GetName());
                        }
                        else
                        {
                            Console.WriteLine("There is now a hotel on {0} and it cannot be built on further.", buildable[number].GetName());
                        }
                    }
                    else
                    {
                        Console.WriteLine("You cannot afford to build here");
                    }
                }
                else
                {
                    Console.WriteLine("Cancelled");
                    triedBuild = true;
                }
            }
            if(action== "Sell Houses")
            {
                List<Property> sellable = new List<Property>();
                foreach(Property prop in p.GetMonopolies())
                {
                    if (prop.GetHouses() != 0)
                    {
                        sellable.Add(prop);
                    }
                }
                for(int i = 0; i < sellable.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, sellable[i].GetName());
                }
                Console.WriteLine("{0}: Cancel", sellable.Count);
                int number = sellable.Count;
                switch (p.GetAgent())
                {
                    case Player.Agents.Human:
                        {
                            number = InputInt("\nEnter the number corresponding to the desired property.", 0, sellable.Count);
                            break;
                        }
                    case Player.Agents.Stochastic:
                        {
                            number = stochAg.SelectItem(sellable.Count);
                            Console.WriteLine(number);
                            break;
                        }
                    case Player.Agents.Statistic:
                        {
                            number = statAg.SelectSellHouse(sellable, p);
                            Console.WriteLine(number);
                            break;
                        }
                    case Player.Agents.RL:
                        {
                            number = statAg.SelectSellHouse(sellable, p);
                            Console.WriteLine(number);
                            break;
                        }
                }
                if (number < sellable.Count)
                {
                    sellable[number].SellHouse(p);
                }
                else
                {
                    Console.WriteLine("Cancelled");
                    triedSell = true;
                }
            }
            if(action== "Roll Dice To Get Out Of Jail")
            {
                bool doubles = false;
                RollDice(dice, ref doubles);
                if (doubles)
                {
                    p.GetOutOfJail();
                    Console.WriteLine("You have rolled a double and got out of jail");
                    canRoll = false;
                }
                else
                {
                    p.IncrementJail();
                    Console.WriteLine("You are still in jail");
                    canRoll = false;
                }
            }
            if(action== "Use Get Out Jail Free Card")
            {
                Console.WriteLine("You have used your get out of jail free card");
                Card card = p.UsePardonCard();
                if(card.Corc == "chance")
                {
                    chance.Add(card);
                }
                if(card.Corc == "chest")
                {
                    chest.Add(card);
                }
            }
            if(action== "Pay To Get Out Of Jail")
            {
                p.PayOutOfJail(bail);
                Console.WriteLine("You have payed your way out of jail");
                canRoll = false;
            }
            if(action== "End Turn")
            {
                p.DoubleReset();
                turnEnded = true;
            }
            if(action== "Pay Rent")
            {
                paid = true;
                int rent = board[p.GetPosition()].GetRent(diceRoll);
                Player owner = board[p.GetPosition()].GetOwner();
                if(owner.GetMonopolies().Exists(x => x == board[p.GetPosition()]) && board[p.GetPosition()].GetHouses() == 0  && doubleRentMonopoly)
                {
                    p.PayRent(owner, rent * 2);
                    Console.WriteLine("You payed {0} to {1}", rent * 2, owner.GetName());
                }
                else
                {
                    p.PayRent(owner, rent);
                    Console.WriteLine("You payed {0} to {1}", rent, owner.GetName());
                }               
            }
            if(action== "Pay Tax")
            {
                paid = true;
                p.Pay(board[p.GetPosition()].GetPrice());
                Console.WriteLine("You paid {0} in tax", board[p.GetPosition()].GetPrice());

                if (freeParkingMoney)
                {
                    parkingMoneyValue += board[p.GetPosition()].GetPrice();
                }
            }
            if(action== "Claim Free Parking Money")
            {
                paid = true;
                Console.WriteLine("You have landed on free parking and recieved £{0}", parkingMoneyValue);
                p.GetPaid(parkingMoneyValue);
                parkingMoneyValue = 0;

            }
            if(action== "Draw Community Chest Card")
            {
                paid = true;
                bool moved = false;
                Card drawn = chest[0];
                Console.WriteLine("You have drawn: {0}", drawn.Text);
                chest.Remove(drawn);
                if(drawn.Type != "Item")
                {
                    chest.Add(drawn);
                }
                p.UseCard(drawn, players, ref moved);
                if(moved)
                {
                    hasRolled = true;
                    paid = false;
                }
            }
            if(action== "Draw Chance Card")
            {
                paid = true;
                bool moved = false;
                Card drawn = chance[0];
                Console.WriteLine("You have drawn: {0}", drawn.Text);
                chance.Remove(drawn);
                if (drawn.Type != "Item")
                {
                    chance.Add(drawn);
                }
                p.UseCard(drawn, players, ref moved);
                if (moved)
                {
                    hasRolled = true;
                    paid = false;
                }
            }
            if(action== "Make Trade")
            {
                Trade trade = new Trade(p, board);
                List<Property> ownedProperty = p.GetTradeable();
                List<Property> tradeIn = new List<Property>();
                List<Property> tradeOut = new List<Property>();
                int moneyIn = 0;
                int moneyOut = 0;
                paid = true;
                Console.WriteLine("Who do you want to trade with?\n");
                List<Player> otherPlayers = new List<Player>();
                foreach(Player other in players)
                {
                    if(other != p)
                    {
                        otherPlayers.Add(other);
                    }                   
                }
                for(int i = 0; i < otherPlayers.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, otherPlayers[i].GetName());
                }
                Console.WriteLine("{0}: Cancel", otherPlayers.Count());
                int number = otherPlayers.Count - 1;
                switch (p.GetAgent())
                {
                    case Player.Agents.Human:
                        {
                            number = InputInt("\nEnter the number corresponding to the desired player.", 0, otherPlayers.Count);
                            break;
                        }
                    case Player.Agents.Stochastic:
                        {
                            number = stochAg.SelectItem(otherPlayers.Count);
                            Console.WriteLine(number);
                            break;
                        }
                    case Player.Agents.RL:
                        {
                            if(trade.GetTIn() == null)
                            {
                                number = otherPlayers.Count;
                            }
                            else
                            {
                                number = otherPlayers.FindIndex(0, x => x == trade.GetTIn().GetOwner());
                            }
                            break;
                        }
                    //For RL if trade propIn is null return otherPlayers.count, else return findindex of propIn.owner
                }
                bool cancelled = false;
                if(number == otherPlayers.Count)
                {
                    Console.WriteLine("Trade Cancelled");
                    triedTrade = true;
                    cancelled = true;
                }
                if (!cancelled)
                {
                    Player partner = otherPlayers[number];
                    List<Property> wantedProperty = partner.GetTradeable();

                    bool complete = false;

                    while (!complete)
                    {
                        Console.WriteLine("These are {0}'s properties", partner.GetName());
                        for (int i = 0; i < wantedProperty.Count; i++)
                        {
                            Console.WriteLine("{0}: {1}", i, wantedProperty[i].GetName());
                        }
                        Console.WriteLine("{0}: None", wantedProperty.Count);
                        number = wantedProperty.Count;
                        switch (p.GetAgent())
                        {
                            case Player.Agents.Human:
                                {
                                    number = InputInt("\nEnter the number corresponding to the desired property.", 0, wantedProperty.Count);
                                    break;
                                }
                            case Player.Agents.Stochastic:
                                {
                                    number = stochAg.SelectItem(wantedProperty.Count);
                                    Console.WriteLine(number);
                                    break;
                                }
                            case Player.Agents.RL:
                                {
                                    number = wantedProperty.FindIndex(0, x => x == trade.GetTIn());
                                    Console.WriteLine(number);
                                    break;
                                }
                        }
                        if (number == wantedProperty.Count)
                        {
                            Console.WriteLine("None of their properties have been added.");
                            complete = true;
                        }
                        else
                        {
                            tradeIn.Add(wantedProperty[number]);
                            Console.WriteLine("{0} has been added to the deal", wantedProperty[number].GetName());
                            wantedProperty.Remove(wantedProperty[number]);
                            string check = "n";
                            switch (p.GetAgent())
                            {
                                case Player.Agents.Human:
                                    {
                                        check = InputString("Would you like to add any more of your properties to the deal? y/n", 1, 1);
                                        break;
                                    }
                                case Player.Agents.Stochastic:
                                    {
                                        Console.WriteLine("Would you like to add any more of your properties to the deal? y/n");
                                        check = stochAg.YorN();
                                        Console.WriteLine(check);
                                        break;
                                    }
                                case Player.Agents.RL:
                                    {
                                        check = "n";
                                        break;
                                    }
                            }
                            if (check.ToLower() != "y")
                            {
                                complete = true;
                            }
                        }
                    }
                    complete = false;
                    while (!complete)
                    {
                        Console.WriteLine("These are your properties");
                        for (int i = 0; i < ownedProperty.Count; i++)
                        {
                            Console.WriteLine("{0}: {1}", i, ownedProperty[i].GetName());
                        }
                        Console.WriteLine("{0}: None", ownedProperty.Count);
                        number = ownedProperty.Count;
                        switch (p.GetAgent())
                        {
                            case Player.Agents.Human:
                                {
                                    number = InputInt("\nEnter the number corresponding to the desired property.", 0, ownedProperty.Count);
                                    break;
                                }
                            case Player.Agents.Stochastic:
                                {
                                    number = stochAg.SelectItem(ownedProperty.Count);
                                    Console.WriteLine(number);
                                    break;
                                }
                            case Player.Agents.RL:
                                {
                                    if(trade.GetTOut().Count > 0)
                                    {
                                        number = ownedProperty.FindIndex(0, x => x == trade.GetTOut()[0]);
                                        Console.WriteLine(trade.GetTOut()[0].GetName());
                                        trade.RemoveTrade(trade.GetTOut()[0]);
                                    }
                                    else
                                    {
                                        number = ownedProperty.Count;
                                    }
                                    break;
                                }
                        }
                        if (number == ownedProperty.Count)
                        {
                            Console.WriteLine("None of your properties have been added.");
                            complete = true;
                        }
                        else
                        {
                            tradeOut.Add(ownedProperty[number]);

                            Console.WriteLine("{0} has been added to the deal", ownedProperty[number].GetName());
                            ownedProperty.Remove(ownedProperty[number]);
                            string check = "n";
                            switch (p.GetAgent())
                            {
                                case Player.Agents.Human:
                                    {
                                        check = InputString("Would you like to add any more of your properties to the deal? y/n", 1, 1);
                                        break;
                                    }
                                case Player.Agents.Stochastic:
                                    {
                                        Console.WriteLine("Would you like to add any more of your properties to the deal? y/n");
                                        check = stochAg.YorN();
                                        Console.WriteLine(check);
                                        break;
                                    }
                                case Player.Agents.RL:
                                    {
                                        if(trade.GetTOut().Count == 0)
                                        {
                                            check = "n";
                                        }
                                        else
                                        {
                                            check = "y";
                                        }
                                        break;
                                    }
                            }

                            if (check.ToLower() != "y")
                            {
                                complete = true;
                            }
                        }
                    }
                    switch (p.GetAgent())
                    {
                        case Player.Agents.Human:
                            {
                                moneyIn = InputInt("How much money would you like to recieve for the deal?", 0, partner.GetMoney());
                                break;
                            }
                        case Player.Agents.Stochastic:
                            {
                                moneyIn = stochAg.SelectItem(partner.GetMoney());
                                Console.WriteLine(moneyIn);
                                break;
                            }
                        case Player.Agents.RL:
                            {
                                moneyIn = trade.GetMIn();
                                Console.WriteLine(moneyIn);
                                break;
                            }
                    }
                    switch (p.GetAgent())
                    {
                        case Player.Agents.Human:
                            {
                                moneyOut = InputInt("How much money would you like to give for the deal?", 0, p.GetMoney());
                                break;
                            }
                        case Player.Agents.Stochastic:
                            {
                                moneyOut = stochAg.SelectItem(p.GetMoney());
                                Console.WriteLine(moneyOut);
                                break;
                            }
                        case Player.Agents.RL:
                            {
                                if (p.GetMoney() > trade.GetMOut())
                                {
                                    moneyOut = trade.GetMOut();
                                }
                                else
                                {
                                    moneyOut = 0;
                                }
                                Console.WriteLine(moneyOut);
                                break;
                            }
                    }

                    Console.WriteLine("{0}, do you agree to this deal?", partner.GetName());
                    int agree = 1;
                    switch (partner.GetAgent())
                    {
                        case Player.Agents.Human:
                            {
                                agree = InputInt("0 for agree, 1 for disagree", 0, 1);
                                break;
                            }
                        case Player.Agents.Stochastic:
                            {
                                agree = stochAg.SelectItem(1);
                                Console.WriteLine(agree);
                                break;
                            }
                        case Player.Agents.Statistic:
                            {
                                agree = statAg.AssessTrade(tradeOut, tradeIn, moneyOut, moneyIn);
                                Console.WriteLine(agree);
                                break;
                            }
                        case Player.Agents.RL:
                            {
                                agree = statAg.AssessTrade(tradeOut, tradeIn, moneyOut, moneyIn);
                                Console.WriteLine(agree);
                                break;
                            }
                    }

                    if (agree == 0)
                    {
                        foreach (Property prop in tradeIn)
                        {
                            partner.SendProperty(prop, p);
                            prop.SetOwner(p);
                            string colour = prop.GetColour();
                            p.CheckForMonopolies(colour, board);
                            partner.RemoveMonopolies(prop);
                        }
                        foreach (Property prop in tradeOut)
                        {
                            p.SendProperty(prop, partner);
                            prop.SetOwner(partner);
                            string colour = prop.GetColour();
                            partner.CheckForMonopolies(colour, board);
                            p.RemoveMonopolies(prop);
                        }
                        p.Pay(moneyOut);
                        p.GetPaid(moneyIn);
                        partner.Pay(moneyIn);
                        partner.GetPaid(moneyOut);
                        Console.WriteLine("Trade Completed");
                    }
                    else
                    {
                        Console.WriteLine("Trade Cancelled");
                        triedTrade = true;
                    }
                }
                

            }
        }

        public List<string> GetResults(int noGames, string player1, int agent1, string player2, int agent2, RLAgent RLagent) 
        { 
            List<string> results = new List<string>();
            for(int i = 0; i < noGames; i++)
            {
                results.Add(Game(player1, agent1, player2, agent2, RLagent));
                Thread.Sleep(0);
            }
            return results;
        }

        public double CalculateReward(Player p)
        {
            Dictionary<string, int> setValue = new Dictionary<string, int>
            {
                { "Brown", 1 },
                { "Blue", 2 },
                { "Pink", 3 },
                { "Orange", 4 },
                { "Red", 5 },
                { "Yellow", 6 },
                { "Green", 7 },
                { "Purple", 8 }
            };

            double reward = 0;

            double totalMoney = 0;

            foreach(Player player in players)
            {
                List<string> ownedColours = new List<string>();
                if (!player.Equals(p))
                {
                    foreach(Property prop in player.GetProperties())
                    {
                        reward--;
                        reward -= prop.GetHouses();
                    }

                    foreach(Property prop in player.GetMonopolies())
                    {
                        if(!(ownedColours.FindIndex(0, x => x == prop.GetColour()) == -1))
                        {
                            ownedColours.Add(prop.GetColour());
                        }
                    }
                    foreach(string colour in ownedColours)
                    {
                        reward -= setValue[colour];
                    }
                }
                else
                {
                    foreach (Property prop in player.GetProperties())
                    {
                        reward++;
                        reward += prop.GetHouses();
                    }

                    foreach (Property prop in player.GetMonopolies())
                    {
                        if (!(ownedColours.FindIndex(0, x => x == prop.GetColour()) == -1))
                        {
                            ownedColours.Add(prop.GetColour());
                        }
                    }
                    foreach (string colour in ownedColours)
                    {
                        reward += setValue[colour];
                    }
                }

                totalMoney += player.GetMoney();
            }

            double moneyF;
            if (totalMoney != 0) 
            {
                moneyF = p.GetMoney() / totalMoney;
            }
            else
            {
                moneyF = 1/players.Count;
            }

            reward = (reward / (players.Count * 5)) / (1 + Math.Abs(reward / (players.Count * 5)));

            reward = reward + (1/players.Count) * moneyF;

            return reward;
        }
    }
}

    