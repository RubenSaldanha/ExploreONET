using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploreOnet
{
    public class Property
    {
        public string name;
        public string type;

        public Property(string name, string type)
        {
            this.name = name;
            this.type = type;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
