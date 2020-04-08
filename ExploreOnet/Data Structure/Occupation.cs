using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploreOnet
{
    public class Occupation
    {
        public string code;
        public string title;
        public string description;

        public Occupation(string code, string title, string description)
        {
            this.code = code;
            this.title = title;
            this.description = description;
        }

        public double getPropertyValue(Property ppt)
        {
            return ONETExplorer.current.occupationProperties.Find((x) => x.ocp == this && x.property == ppt).value;
        }

        public override string ToString()
        {
            return title;
        }
    }
}
