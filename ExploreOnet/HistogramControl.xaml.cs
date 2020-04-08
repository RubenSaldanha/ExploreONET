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
    /// Interaction logic for HistogramControl.xaml
    /// </summary>
    public partial class HistogramControl : UserControl
    {
        Histogram hist;

        public HistogramControl(Histogram hist, int barCount)
        {
            InitializeComponent();

            this.hist = hist;

            Histogram.Bin[] bins = hist.getBins(barCount);

            float maxVal = bins.Max((x) => x.count);

            //Bar visuals
            float barHeight = 400;
            float barWidth = 40;
            for (int i = 0; i < barCount; i++)
            {
                Button bar = new Button();
                bar.Height = barHeight * (bins[i].count / maxVal);
                bar.Width = barWidth;
                bar.Background = new SolidColorBrush(Colors.CadetBlue);
                bar.VerticalAlignment = VerticalAlignment.Bottom;
                barPanel.Children.Add(bar);

                Label lbl = new Label();
                lbl.Height = 30;
                lbl.Width = barWidth;
                lbl.Content = Math.Round(bins[i].start,2);
                lbl.HorizontalContentAlignment = HorizontalAlignment.Left;
                lbl.BorderBrush = new SolidColorBrush(Colors.Black);
                lbl.BorderThickness = new Thickness(1, 0, 0, 0);
                axisPanel.Children.Add(lbl);
            }
        }
    }
}
