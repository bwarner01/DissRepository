using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Trace
    {
        public State state { get; set; }
        public string action { get; set; }
        public double value { get; set; }

        public Trace(State state, string action, double value) 
        {
            this.state = state;
            this.action = action;
            this.value = value;
        }

    }
}
