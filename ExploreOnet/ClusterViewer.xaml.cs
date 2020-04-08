using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for ClusterViewer.xaml
    /// </summary>
    public partial class ClusterViewer : UserControl
    {
        public ClusterViewer(object cluster)
        {
            InitializeComponent();

            Type cType = cluster.GetType();
            Console.WriteLine("receive type: " + cType.FullName);
            Type eType = typeof(ClusterNode<>);
            Console.WriteLine("compare type: " + eType.FullName);

            if (cType != eType)
                throw new Exception();

            Type underType = cType.GetGenericArguments()[0];

            MethodInfo methodDefinition = GetType().GetMethod("Initialize", new Type[] {cType });
            MethodInfo method = methodDefinition.MakeGenericMethod(underType);
            //method.Invoke();
        }

        public void Initialize<T>(ClusterNode<T> cluster)
        {

        }
    }
}
