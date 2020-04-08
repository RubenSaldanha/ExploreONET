using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for ONETLoaderControl.xaml
    /// </summary>
    public partial class ONETLoaderControl : UserControl
    {
        public delegate void ONETLoadingFinishedHandler();
        public event ONETLoadingFinishedHandler LoadingFinished;

        Timer checkup;
        ONETExplorer onetExp;

        int? totalTasks;

        public ONETLoaderControl()
        {
            InitializeComponent();

            onetExp = new ONETExplorer();

            onetExp.load();

            checkup = new Timer();
            checkup.Interval = 200;
            checkup.AutoReset = true;
            checkup.Elapsed += Checkup_Elapsed;
            checkup.Start();
        }

        int elapsCounter;
        private void Checkup_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(Checkup);
        }
        public void Checkup()
        {
            elapsCounter++;
            if (onetExp.loadTasks != null)
            {
                if (onetExp.loadTasks.Count > 0)
                {
                    //Keep checking
                    if (totalTasks == null)
                        totalTasks = onetExp.loadTasks.Count;

                    double percentilePoint = (totalTasks.Value - onetExp.loadTasks.Count) / (double)totalTasks;
                    loadingBar.Width = percentilePoint * 600;

                    string currentTaskName;
                    try
                    {
                        currentTaskName = "Loading - " + onetExp.loadTasks.First()?.Method.Name + "".PadRight((elapsCounter / 5) % 4, '.');
                    }
                    catch (InvalidOperationException exp)
                    {
                        currentTaskName = "Finished ";
                    }
                    loadingLabel.Content = currentTaskName;
                }
                else
                {
                    Finish();
                }
            }
        }

        public void Finish()
        {
            checkup.AutoReset = false;
            checkup.Dispose();
            LoadingFinished?.Invoke();
        }
    }
}
