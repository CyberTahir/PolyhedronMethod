using System;
using System.Windows;

namespace PolyhedronMethod
{
    public partial class MainWindow : Window
    {
        private readonly Polyhedron p;
        public MainWindow()
        {
            InitializeComponent();

            p = new Polyhedron(x => x[0], null, 0, 0, 0, 0);
            Log.ItemsSource = p.Log;
        }
        private void Search(map f, double[][] dots)
        {
            double alpha = Convert.ToDouble(Alpha.Text);
            double beta = Convert.ToDouble(Beta.Text);
            double gamma = Convert.ToDouble(Gamma.Text);
            double epsilon = Convert.ToDouble(Epsilon.Text);

            p.Update(f, dots, alpha, beta, gamma, epsilon);
            p.FindExtr();

            Data.Visibility = Visibility.Hidden;
            Log.Visibility = Visibility.Visible;

            Log.UpdateLayout();
        }
        private void OpenData(object sender, RoutedEventArgs e)
        {
            Log.Visibility = Visibility.Hidden;
            Data.Visibility = Visibility.Visible;

            p.Log.Clear();
        }
        private void Test1_Click(object sender, RoutedEventArgs e)
        {
            double[][] dots = { new double[] { 8, 9 }, new double[] { 10, 11 }, new double[] { 8, 11 } };
            Search((double[] x) => (4 * (x[0] - 5) * (x[0] - 5)) + ((x[1] - 6) * (x[1] - 6)), dots);
        }
        private void Test2_Click(object sender, RoutedEventArgs e)
        {
            double[][] dots = { new double[] { 0.5, 1 }, new double[] { 0, 0.5 }, new double[] { 1, 0.5 } };
            Search((double[] x) => (2 * x[0] * x[0]) + x[0] * x[1] + x[1] * x[1], dots);
        }
    }

}
