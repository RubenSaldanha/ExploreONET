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
    /// Interaction logic for OccupationListViewer.xaml
    /// </summary>
    public partial class OccupationListViewer : UserControl
    {
        public OccupationListViewer()
        {
            InitializeComponent();

            ocpCountLabel.Content = "from a list of " + ONETExplorer.current.occupations.Count + " occupations.";
            searchTb.TextChanged += searchTb_TextChanged;

            MainWindow.current.Dispatcher.Invoke(createCluster);
        }

        public void createCluster()
        {
            clusterViewer.Content = ONETExplorer.current.occupationsCluster.getPanel();
        }

        private void searchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            PopulateSuggestions(text);
        }
        private void PopulateSuggestions(string sug)
        {
            List<Occupation> occupations = ONETExplorer.current.occupations;
            resultPanel.Children.Clear();

            if (occupations.Find((x) => x.title == sug) != null || sug.Length < 2)
                return;

            string pattern = sug;

            List<Occupation> sugOcp = new List<Occupation>();
            for (int i = 0; i < occupations.Count; i++)
            {
                bool titleMatch = System.Text.RegularExpressions.Regex.IsMatch(occupations[i].title, sug, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (titleMatch)
                {
                    sugOcp.Add(occupations[i]);
                }
            }

            //Order by description ammount of words
            sugOcp = sugOcp.OrderBy((x) => System.Text.RegularExpressions.Regex.Matches(x.description, sug, System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count).ToList();

            for (int i = sugOcp.Count - 1; i >= 0; i--)
            {
                //Add new
                Button bb = new Button();
                bb.Content = sugOcp[i].title;
                bb.Width = 400;
                bb.Height = 50;
                bb.DataContext = sugOcp[i];
                bb.Click += Suggestion_Clicked;
                bb.HorizontalAlignment = HorizontalAlignment.Left;
                bb.HorizontalContentAlignment = HorizontalAlignment.Left;
                resultPanel.Children.Add(bb);
            }
        }
        private void Suggestion_Clicked(object sender, RoutedEventArgs e)
        {
            Occupation selected = (Occupation)(sender as Button).DataContext;
            MainWindow.current.pushControl(new OccupationVisualizer(selected));
        }
    }
}
