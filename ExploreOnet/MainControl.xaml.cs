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
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        List<UserControl> controls;

        public MainControl()
        {
            InitializeComponent();

            controls = new List<UserControl>();
        }

        public void pushControl(UserControl control)
        {
            controls.Add(control);
            viewControl.Content = control;
        }
        public void popControl()
        {
            if (controls.Count > 1)
            {
                controls.RemoveAt(controls.Count - 1);
                viewControl.Content = controls[controls.Count - 1];
            }
        }

        private void OccupationBut_Click(object sender, RoutedEventArgs e)
        {
            pushControl(new OccupationListViewer());
        }

        private void AbiltiesBut_Click(object sender, RoutedEventArgs e)
        {
            pushControl(new PropertyListViewer("Ability"));
        }

        private void KnowledgeBut_Click(object sender, RoutedEventArgs e)
        {
            pushControl(new PropertyListViewer("Knowledge"));
        }

        private void SkillsBut_Click(object sender, RoutedEventArgs e)
        {
            pushControl(new PropertyListViewer("Skill"));
        }

        private void backBut_Click(object sender, RoutedEventArgs e)
        {
            popControl();
        }
    }
}
