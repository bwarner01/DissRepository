using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class RLAgent
    {
        private double epsilon;
        private double alpha;
        private double gamma;
    }

    public class State
    {
        private int pPosition {  get; set; }
        private int pMoney {  get; set; }
        private List<Property> ownedProperties { get; set; }
        private List<Property> board { get; set; }
        private List<Player> playerList { get; set; }

        public State(int pPosition, int pMoney, List<Property> ownedProperties, List<Property> board, List<Player> playerList) 
        {
            this.pPosition = pPosition;
            this.pMoney = pMoney;
            this.ownedProperties = ownedProperties;
            this.board = board;
            this.playerList = playerList;
        }
    }
}
