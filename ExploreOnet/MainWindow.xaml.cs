using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow current;

        MainControl mainControl;

        public MainWindow()
        {
            current = this;

            InitializeComponent();

            ONETLoaderControl onetLoader = new ONETLoaderControl();
            onetLoader.LoadingFinished += OnetLoader_LoadingFinished;
            mainControlView.Content = onetLoader;
        }

        private void OnetLoader_LoadingFinished()
        {
            Dispatcher.Invoke(EnableUI);
        }

        private void EnableUI()
        {
            mainControl = new MainControl();
            mainControlView.Content = mainControl;
        }

        public void pushControl(UserControl control)
        {
            mainControl.pushControl(control);
        }
        public void popControl()
        {
            mainControl.popControl();
        }
    }
}
