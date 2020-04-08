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
    /// Interaction logic for OccupationVisualizer.xaml
    /// </summary>
    public partial class OccupationVisualizer : UserControl
    {
        Occupation occupation;
        public OccupationVisualizer(Occupation occupation)
        {
            InitializeComponent();

            this.occupation = occupation;

            titleLabel.Content = occupation.title;
            description.Text = occupation.description;

            dataPointsLabel.Content = "Data entries: " + ONETExplorer.current.occupationPropertiesIdxOccupation[occupation].Count;
            averagePropertyLabel.Content = "Average competency value: " + Math.Round(ONETExplorer.current.occupationPropertiesIdxOccupation[occupation].Average((x) => x.value),3);
            competencyPercentile.Content = "Overall competency percentile: " + Math.Round(ONETExplorer.current.occupationPercentiles[occupation]*100,1) + "%";

            List<OccupationProperty> ocpP = ONETExplorer.current.occupationProperties.FindAll((x) => x.ocp == occupation);

            //Abilities
            List<OccupationProperty> ocpAb = ocpP.FindAll((x) => x.property.type == "Ability");
            ocpAb = ocpAb.OrderBy((x) => x.value).ToList();
            for(int i=0;i<5;i++)
            {
                OccupationProperty occ = ocpAb[ocpAb.Count - 1 - i];

                Button abilityBut = new Button();
                abilityBut.Content = String.Format(" {0,6} : {1}", Math.Round(occ.value, 2), " : " + occ.property.name);
                abilityBut.Click += PropertyBut_Click;
                abilityBut.DataContext = occ.property;
                abilityBut.Height = 50;

                abilityPanel.Children.Add(abilityBut);
            }

            //Knowledge
            List<OccupationProperty> ocpKb = ocpP.FindAll((x) => x.property.type == "Knowledge");
            ocpKb = ocpKb.OrderBy((x) => x.value).ToList();
            for (int i = 0; i < 5; i++)
            {
                OccupationProperty occ = ocpKb[ocpKb.Count - 1 - i];

                Button propertyBut = new Button();
                propertyBut.Content = String.Format(" {0,6} : {1}", Math.Round(occ.value, 2), " : " + occ.property.name);
                propertyBut.Click += PropertyBut_Click;
                propertyBut.DataContext = occ.property;
                propertyBut.Height = 50;

                knowledgePanel.Children.Add(propertyBut);
            }

            //Skills
            List<OccupationProperty> ocpSb = ocpP.FindAll((x) => x.property.type == "Skill");
            ocpSb = ocpSb.OrderBy((x) => x.value).ToList();
            for (int i = 0; i < 5; i++)
            {
                OccupationProperty occ = ocpSb[ocpSb.Count - 1 - i];

                Button propertyBut = new Button();
                propertyBut.Content = String.Format(" {0,6} : {1}", Math.Round(occ.value, 2), " : " + occ.property.name);
                propertyBut.Click += PropertyBut_Click;
                propertyBut.DataContext = occ.property;
                propertyBut.Height = 50;

                skillPanel.Children.Add(propertyBut);
            }

            //Related occupations
            var a =  ONETExplorer.current.occupationMetric[occupation].ToList();
            a = a.OrderBy((x) => x.Value).ToList();

            for (int i = 0; i < 10; i++)
            {
                Occupation tgtOcp = a[i].Key;

                //Do not consider self (should be the first element)
                if (tgtOcp == occupation)
                    continue;

                double dist = a[i].Value;
                Button bb = new Button();
                bb.Content = String.Format("{0,10} : {1}", Math.Round(dist, 4), tgtOcp.title);
                bb.HorizontalAlignment = HorizontalAlignment.Stretch;
                bb.HorizontalContentAlignment = HorizontalAlignment.Left;
                bb.Height = 50;
                bb.DataContext = tgtOcp;
                bb.Click += Bb_Click;
                relatedPanel.Children.Add(bb);
            }

            //Properties histogram
            Histogram hist = new Histogram(ONETExplorer.current.occupationPropertiesIdxOccupation[occupation].Select((x) => x.value).ToList());
            competenciesHistogramControl.Content = new HistogramControl(hist, 10);
        }

        private void PropertyBut_Click(object sender, RoutedEventArgs e)
        {
            Property pp = (sender as Button).DataContext as Property;
            MainWindow.current.pushControl(new PropertyVisualizer(pp));
        }

        private void Bb_Click(object sender, RoutedEventArgs e)
        {
            Occupation clickedOccupation = (sender as Button).DataContext as Occupation;
            MainWindow.current.pushControl(new OccupationVisualizer(clickedOccupation));
        }
    }
}
