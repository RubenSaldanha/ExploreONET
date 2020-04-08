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
    /// Interaction logic for PropertyListViewer.xaml
    /// </summary>
    public partial class PropertyListViewer : UserControl
    {
        List<Property> properties;

        public PropertyListViewer(string type)
        {
            InitializeComponent();

            properties = ONETExplorer.current.properties.FindAll((x) => x.type == type);

            searchLabel.Content = "Search " + type + " :";
            ocpCountLabel.Content = "from a list of " + properties.Count;
            //searchTb.TextChanged += searchTb_TextChanged;

            for(int i=0;i<properties.Count;i++)
            {
                //Add new
                Button bb = new Button();
                bb.Content = properties[i].name;
                bb.Width = 400;
                bb.Height = 30;
                bb.DataContext = properties[i];
                bb.Click += Suggestion_Clicked;
                bb.HorizontalAlignment = HorizontalAlignment.Center;
                bb.HorizontalContentAlignment = HorizontalAlignment.Center;
                listPanel.Children.Add(bb);
            }

            clusterView.Content = ONETExplorer.current.propertyCluster.getPanel();
        }
        //private void searchTb_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    string text = (sender as TextBox).Text;
        //    PopulateSuggestions(text);
        //}
        //private void PopulateSuggestions(string sug)
        //{
        //    resultPanel.Children.Clear();

        //    if (properties.Find((x) => x.name == sug) != null || sug.Length < 2)
        //        return;

        //    for (int i = 0; i < properties.Count; i++)
        //    {
        //        if (properties[i].name.Contains(sug))
        //        {
        //            //Add new
        //            Button bb = new Button();
        //            bb.Content = properties[i].name;
        //            bb.Width = 400;
        //            bb.Height = 50;
        //            bb.DataContext = properties[i];
        //            bb.Click += Suggestion_Clicked;
        //            bb.HorizontalAlignment = HorizontalAlignment.Center;
        //            bb.HorizontalContentAlignment = HorizontalAlignment.Center;
        //            resultPanel.Children.Add(bb);
        //        }
        //    }
        //}
        private void Suggestion_Clicked(object sender, RoutedEventArgs e)
        {
            Property selected = (Property)(sender as Button).DataContext;
            MainWindow.current.pushControl(new PropertyVisualizer(selected));
        }

    }
}
