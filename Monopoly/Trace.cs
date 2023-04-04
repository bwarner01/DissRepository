using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Trace
    {
        public double[] state { get; set; }
        public string action { get; set; }
        public float value { get; set; }

        public Trace(double[] state, string action, float value) 
        {
            this.state = state;
            this.action = action;
            this.value = value;
        }

    }
}
