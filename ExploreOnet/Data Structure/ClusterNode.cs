using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExploreOnet
{
    public class ClusterNode<T>
    {
        public Func<T, T, double> metric;

        public T element;

        public int count;

        ClusterNode<T> part1;
        ClusterNode<T> part2;

        public bool leaf
        {
            get { return element != null; }
        }

        public ClusterNode(Func<T, T, double> metric, T element)
        {
            this.metric = metric;
            this.element = element;
            count = 1;
        }
        public ClusterNode(ClusterNode<T> part1, ClusterNode<T> part2)
        {
            this.part1 = part1;
            this.part2 = part2;

            count = part1.count + part2.count;

            this.metric = part1.metric;
        }

        public double distance(ClusterNode<T> other)
        {
            if (leaf)
            {
                if (other.leaf)
                    return metric(element, other.element);
                else
                    return other.distance(this);
            }
            else
            {
                return (part1.distance(other) * part1.count + part2.distance(other) * part2.count) / count;
            }
        }

        public static ClusterNode<T> createDendogram(List<T> elements, Func<T, T, double> metric)
        {
            List<ClusterNode<T>> clusters = new List<ClusterNode<T>>();

            foreach (T element in elements)
            {
                clusters.Add(new ClusterNode<T>(metric, element));
            }

            //Create and populate structure
            List<List<double>> dists = new List<List<double>>();
            for (int i = 0; i < clusters.Count; i++)
            {
                dists.Add(new List<double>());
                for (int j = 0; j < clusters.Count; j++)
                {
                    dists[i].Add(0);
                }
            }

            //Compute distances
            for (int i = 0; i < clusters.Count; i++)
            {
                for (int j = i; j < clusters.Count; j++)
                {
                    double dist = clusters[i].distance(clusters[j]);
                    dists[i][j] = dist;
                    dists[j][i] = dist;
                }
            }

            //Create dendogram
            while (clusters.Count != 1)
            {
                //Find minimalIndexes
                double minDist = double.MaxValue;
                int p1 = -1;
                int p2 = -1;
                for (int i = 0; i < dists.Count; i++)
                {
                    for (int j = i + 1; j < dists.Count; j++)
                    {
                        if (dists[i][j] < minDist)
                        {
                            minDist = dists[i][j];
                            p1 = i;
                            p2 = j;
                        }
                    }
                }

                //Agglomerate
                ClusterNode<T> newCluster = new ClusterNode<T>(clusters[p1], clusters[p2]);
                //Delete from structure
                if (p1 > p2)
                    throw new Exception("Invalid mechanic, swap p1 and p2");
                //Remove rows
                dists.RemoveAt(p2);
                dists.RemoveAt(p1);
                //Remove columns
                foreach (List<double> lt in dists)
                {
                    lt.RemoveAt(p2);
                    lt.RemoveAt(p1);
                }
                //Remove from main structure
                clusters.RemoveAt(p2);
                clusters.RemoveAt(p1);

                //Add cluster
                clusters.Add(newCluster);
                //add row
                dists.Add(new List<double>());
                //add column and values
                for (int k = 0; k < dists.Count - 1; k++)
                {
                    double newToKDist = newCluster.distance(clusters[k]);
                    dists[k].Add(newToKDist);
                    dists[dists.Count - 1].Add(newToKDist);
                }
                dists[dists.Count - 1].Add(newCluster.distance(newCluster)); //Last element

            }

            return clusters[0];
        }

        public List<T> getElements()
        {
            List<T> list = new List<T>();
            pushElements(list);
            return list;
        }
        public void pushElements(List<T> list)
        {
            if(leaf)
            {
                list.Add(element);
            }
            else
            {
                part1.pushElements(list);
                part2.pushElements(list);
            }
        }


        public UIElement getPanel()
        {
            if(leaf)
            {
                return getLeaf();
            }
            else
            {
                Grid grid = new Grid();
                grid.Margin = new Thickness(2);
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto } );
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                Border bd = new Border();
                bd.BorderThickness = new Thickness(2f, 2f, 2f, 0f);
                bd.BorderBrush = new SolidColorBrush(Colors.Black);
                Grid.SetColumnSpan(bd, 2);
                Grid.SetRowSpan(bd, 2);
                grid.Children.Add(bd);

                //Node label
                Label nodeLabel = new Label();
                nodeLabel.Content = " ";
                nodeLabel.HorizontalAlignment = HorizontalAlignment.Center;
                //DockPanel.SetDock(nodeLabel, Dock.Top);
                Grid.SetColumnSpan(nodeLabel, 2);
                grid.Children.Add(nodeLabel);

                //Left part
                UIElement part1Visual = part1.getPreview();
                Grid.SetRow(part1Visual, 1);
                Grid.SetColumn(part1Visual, 0);
                grid.Children.Add(part1Visual);

                //Right part
                UIElement part2Visual = part2.getPreview();
                Grid.SetRow(part2Visual, 1);
                Grid.SetColumn(part2Visual, 1);
                grid.Children.Add(part2Visual);

                grid.DataContext = this;
                return grid;
            }
        }
        public UIElement getPreview()
        {
            if(leaf)
            {
                return getLeaf();
            }
            else
            {
                int previewCount = 12;
                Button button = new Button();
                List<T> elements = getElements();
                string elementList = "";
                for(int i=0;i<previewCount && i < elements.Count;i++)
                {
                    if (i % 2 == 0)
                        elementList += elements[i / 2];
                    else
                        elementList += elements[elements.Count - 1 - i / 2];

                    elementList += "\n";
                }
                int left = elements.Count - 8;
                if (left > 0)
                    elementList += "... +" + left + " more";

                button.VerticalAlignment = VerticalAlignment.Top;
                button.Content = elementList;
                button.Click += Button_Click;
                button.DataContext = this;
                button.Margin = new Thickness(1);
                return button;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            ExpandPreview(button);
        }
        private void ExpandPreview(Button preview)
        {
            //Get structures and visuals
            Grid parent = preview.Parent as Grid;
            ClusterNode<T> parentNode = parent.DataContext as ClusterNode<T>;
            ClusterNode<T> expandNode = preview.DataContext as ClusterNode<T>;

            //Remove preview
            parent.Children.Remove(preview);
            //Get and add expansion
            UIElement expanded = expandNode.getPanel();
            parent.Children.Add(expanded);


            //Position expansion
            Grid.SetRow(expanded, 1);
            if(parentNode.part1 == expandNode)
            {
                //Left
                Grid.SetColumn(expanded, 0);
            }
            else
            {
                //Right
                Grid.SetColumn(expanded, 1);
            }
        }

        public UIElement getLeaf()
        {
            //print single label
            Label lbl = new Label();
            lbl.Content = element.ToString();
            lbl.VerticalAlignment = VerticalAlignment.Top;
            return lbl;
        }
    }
}
