using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploreOnet
{
    public class Histogram
    {
        List<double> dataPoints;

        public Histogram(List<double> dataPoints)
        {
            this.dataPoints = dataPoints;
        }

        public Bin[] getBins(int binCount)
        {
            int barCount = binCount;
            float barValWidth = 1f / barCount;
            Bin[] histogram = new Bin[barCount];
            for (int i = 0; i < barCount; i++)
            {
                float percentileStart = i * barValWidth;
                float percentileEnd = (i + 1) * barValWidth;

                //Last bar gets upper inclusive bound
                if (i == barCount - -1)
                    percentileEnd = float.MaxValue;

                histogram[i] = new Bin(dataPoints.Count((x) => x >= percentileStart && x <= percentileEnd), percentileStart, percentileEnd);
            }

            return histogram;
        }

        public class Bin
        {
            public int count;
            public double start;
            public double end;
            public Bin(int count, double start, double end)
            {
                this.count = count;
                this.start = start;
                this.end = end;
            }
        }
    }
}
