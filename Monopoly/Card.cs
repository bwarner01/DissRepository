using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Card
    {
        private string corc;
        private string type;
        private int value;
        private string text;

        public Card(string corc, string type, int value, string text)
        {
            this.corc = corc;
            this.type = type;
            this.value = value;
            this.text = text;
        }

        public string Corc { get { return this.corc; } }

        public string Type { get { return this.type; } }    

        public int Value { get { return this.value; } }

        public string Text { get { return this.text; } }
    }
}
