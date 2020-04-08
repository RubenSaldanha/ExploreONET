using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploreOnet
{
    public class OccupationProperty
    {
        public Occupation ocp;
        public Property property;
        public double value;
        public string source;

        public OccupationProperty(Occupation ocp, Property ability, double value, string source)
        {
            this.ocp = ocp;
            this.property = ability;
            this.value = value;
            this.source = source;
        }
    }
}
