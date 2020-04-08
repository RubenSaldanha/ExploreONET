using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExploreOnet
{
    /// <summary>
    /// Interaction logic for PropertyVisualizer.xaml
    /// </summary>
    public partial class PropertyVisualizer : UserControl
    {
        Property property;

        public PropertyVisualizer(Property property)
        {
            InitializeComponent();

            this.property = property;

            propertyNameLabel.Content = property.name;
            propertyTypeLabel.Content = property.type;
            string lblCC = "";
            double average = ONETExplorer.current.occupationPropertiesIdxProperty[property].Average((x) => x.value);
            lblCC += "Average: " + average;
            double variance = ONETExplorer.current.occupationPropertiesIdxProperty[property].Average((x) => Math.Pow(x.value - average, 2));
            lblCC += " Variance: " + variance;
            avgLabel.Content = lblCC;

            //Histogram values
            int barCount = 15;
            Histogram hist = new Histogram(ONETExplorer.current.occupationPropertiesIdxProperty[property].Select((x) => x.value).ToList());
            histogramControl.Content = new HistogramControl(hist, barCount);

            //Most proeminent
            List<OccupationProperty> ocps = ONETExplorer.current.occupationProperties.FindAll((x)=> x.property == property);
            ocps.Sort(Comparer<OccupationProperty>.Create((x, y) => y.value.CompareTo(x.value)));
            for(int i=0;i<10;i++)
            {
                OccupationProperty ocpp = ocps[i];
                //Add new
                Button bb = new Button();
                bb.Content =   String.Format("{0,6}" ,Math.Round(ocpp.value, 3)) + " : " + ocpp.ocp.title;
                bb.Width = 400;
                bb.Height = 50;
                bb.DataContext = ocpp.ocp;
                bb.Click += Bb_Click; ;
                bb.HorizontalAlignment = HorizontalAlignment.Center;
                bb.HorizontalContentAlignment = HorizontalAlignment.Center;
                occupationsPanel.Children.Add(bb);
            }
        }

        private void Bb_Click(object sender, RoutedEventArgs e)
        {
            Occupation ocp = (sender as Button).DataContext as Occupation;
            MainWindow.current.pushControl(new OccupationVisualizer(ocp));
        }
    }
}
