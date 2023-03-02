using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    class Program
    {
        static List<Player> players = new List<Player>();
        static List<Property> board = new List<Property>();
        static List<Card> chance = new List<Card>();
        static List<Card> chest = new List<Card>();
        static Random dice = new Random();


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

        public static void Main(string[] args)
        {
            string path = @"E:\DissRepository\DissRepository\Monopoly\board.csv";
            var parser = new StreamReader(path);
            var headerLine = parser.ReadLine();
            while (!parser.EndOfStream)
            {
                var line = parser.ReadLine();
                var row = line.Split(",");
                board.Add(new Property(row[0], Convert.ToInt32(row[1]), row[2], row[3], Convert.ToInt32(row[4]), Convert.ToInt32(row[5]), Convert.ToInt32(row[6]), Convert.ToInt32(row[7]), Convert.ToInt32(row[8]), Convert.ToInt32(row[9]), Convert.ToInt32(row[10]), Convert.ToInt32(row[11])));
            }

            path = @"E:\DissRepository\DissRepository\Monopoly\chance.csv";
            parser = new StreamReader(path);
            headerLine = parser.ReadLine();
            while (!parser.EndOfStream)
            {
                var line = parser.ReadLine();
                var row = line.Split(",");
                chance.Add(new Card("chance", row[0], Convert.ToInt32(row[1]), row[2]));
            }

            path = @"E:\DissRepository\DissRepository\Monopoly\chest.csv";
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

            for (int i = 0; i < numPlayers; i++)
            {
                string name = InputString(string.Format("\nPlease enter player {0}'s name.", i + 1), 1, 25);
                players.Add(new Player(name, startMoney, goMoney, goBonus));
            }

            while (players.Count > 1)
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
                    p.DoubleReset();
                    while (!turnEnded)
                    {

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
                            Console.WriteLine("You are currently at {0} and have {1}", board[p.GetPosition()].GetName(), p.GetMoney());
                        }
                        List<string> option = GenOptions(p, turnEnded, hasRolled, canRoll, paid);
                        Console.WriteLine("You can: \n");
                        for(int i = 0; i < option.Count; i++)
                        {
                            Console.WriteLine("{0}: {1}", i, option[i]);
                        }
                        int choice = InputInt("\n Enter the number of your choice: ", 0, option.Count - 1);
                        PerformAction(p, option, choice, ref canRoll, ref hasRolled, ref turnEnded, ref paid, ref diceRoll, ref bankrupt, ref eliminated);
                    }
                    if(players.Count - eliminated.Count <= 1)
                    {
                        break;
                    }
                }
                foreach(Player p in eliminated)
                {
                    players.Remove(p);
                }
            }
            Console.WriteLine("{0} wins", players[0].GetName());
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
                "View Position information",
                "View Player Data",
                "Make Trade"
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

                if (p.GetProperties().Count > 0)
                {
                    options.Add("Mortgage/Sell Property");
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

                if (p.GetMonopolies().Count > 0)
                {
                    options.Add("Build Houses");
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

        private static void PerformAction(Player p, List<string> options, int choice, ref bool canRoll, ref bool hasRolled, ref bool turnEnded, ref bool paid, ref int diceRoll, ref bool bankrupt, ref List<Player> eliminated)
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

            }
            if(action== "Buy Property")
            {
                p.BuyProperty(board[p.GetPosition()]);
                board[p.GetPosition()].Bought(p);
                string colour = board[p.GetPosition()].GetColour();
                int count = 0;
                int ownedCount = 0;
                foreach(Property prop in board)
                {
                    if (prop.GetColour() == colour && prop.GetColour() != "NA")
                    {
                        count++;
                    }
                }
                foreach(Property prop in p.GetProperties())
                {
                    if (prop.GetColour() == colour)
                    {
                        ownedCount++;
                    }
                }
                if(count == ownedCount)
                {
                    foreach(Property prop in p.GetProperties())
                    {
                        if(prop.GetColour() == colour)
                        {
                            p.AddMonopolies(prop);
                        }
                    }
                }
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
                int number = InputInt("\nEnter the number corresponding to the desired property.", 0, properties.Count);
                if(number < properties.Count)
                {
                    if (!mortgage)
                    {
                        p.SellProperty(properties[number]);
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
                int number = InputInt("\nEnter the number corresponding to the desired property.", 0, mortgages.Count);
                if(number < mortgages.Count)
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
                int number = InputInt("\nEnter the number corresponding to the desired property.", 0, buildable.Count);
                if(number < buildable.Count)
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
                int number = InputInt("\nEnter the number corresponding to the desired property.", 0, sellable.Count);
                if(number < sellable.Count)
                {
                    sellable[number].SellHouse(p);
                }
                else
                {
                    Console.WriteLine("Cancelled");
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
                int number = InputInt("Enter the number corresponding to the player you want to trade with", 0, otherPlayers.Count - 1);
                Player partner = otherPlayers[number];
                
                bool complete = false;

                while (!complete)
                {
                    Console.WriteLine("These are {0}'s properties", partner.GetName());
                    for (int i = 0; i < partner.GetTradeable().Count; i++)
                    {
                        Console.WriteLine("{0}: {1}", i, partner.GetTradeable()[i].GetName());
                    }
                    Console.WriteLine("{0}: None");
                    number = InputInt("Enter the corresponding number of the property you want", 0, partner.GetTradeable().Count);
                    if(number == partner.GetTradeable().Count)
                    {
                        Console.WriteLine("None of their properties have been added.");
                        complete = true;
                    }
                    else
                    {
                        tradeIn.Add(partner.GetTradeable()[number]);
                        Console.WriteLine("{0} has been added to the deal", partner.GetTradeable()[number].GetName());
                        string check = InputString("Would you like to add any more of their properties to the deal? y/n", 1, 1);
                        if(check.ToLower() == "y")
                        {
                            complete = true;
                        }
                    }
                }
                complete = false;
                while (!complete)
                {
                    Console.WriteLine("These are your properties");
                    for (int i = 0; i < p.GetTradeable().Count; i++)
                    {
                        Console.WriteLine("{0}: {1}", i, p.GetTradeable()[i].GetName());
                    }
                    Console.WriteLine("{0}: None");
                    number = InputInt("Enter the corresponding number of the property you want to trade", 0, p.GetTradeable().Count);
                    if (number == partner.GetTradeable().Count)
                    {
                        Console.WriteLine("None of their properties have been added.");
                        complete = true;
                    }
                    else
                    {
                        tradeIn.Add(partner.GetTradeable()[number]);
                        Console.WriteLine("{0} has been added to the deal", p.GetTradeable()[number].GetName());
                        string check = InputString("Would you like to add any more of your properties to the deal? y/n", 1, 1);
                        if (check.ToLower() == "y")
                        {
                            complete = true;
                        }
                    }
                }
                moneyIn = InputInt("How much money would you like to recieve for the deal?", 0, partner.GetMoney());
                moneyOut = InputInt("How much money would you like to give for the deal?", 0, p.GetMoney());

                Console.WriteLine("{0}, do you agree to this deal?");
                int agree = InputInt("0 for agree, 1 for disagree", 0, 1);

                if(agree == 0)
                {
                    foreach(Property prop in tradeIn)
                    {
                        partner.SendProperty(prop, p);
                    }
                    foreach (Property prop in tradeOut)
                    {
                        p.SendProperty(prop, partner);
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
                }

            }
        }

    }
}

    