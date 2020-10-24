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

namespace PolyhedronMethod
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Test1_Click(object sender, RoutedEventArgs e)
        {
            Func<double[], double> f = (x => 4 * (x[0] - 5) * (x[0] - 5) + (x[1] - 6) * (x[1] - 6));
            double[][] dots = { new double[] { 8, 9 }, new double[] { 10, 11 }, new double[] { 8, 11 } };
            double alpha = Convert.ToDouble(Alpha.Text);
            double beta = Convert.ToDouble(Beta.Text);
            double gamma = Convert.ToDouble(Gamma.Text);
            double epsilon = Convert.ToDouble(Epsilon.Text);

            Polyhedron p = new Polyhedron(f, dots, alpha, beta, gamma, epsilon);
        }
    }

    class Polyhedron
    {
        private Func<double[], double> function;
        double[][] X;
        int n;

        double alpha;
        double beta;
        double gamma;
        double epsilon;

        double[] xl;
        double[] xh;
        double[] xs;
        double[] center;

        int l, h, s, k;

        public Polyhedron(Func<double[], double> f, double[][] dots, double a, double b, double c, double e)
        {
            function = f;
            X = dots;
            n = dots.Length - 1;

            alpha = a;
            beta = b;
            gamma = c;
            epsilon = e * e;

            xl = new double[n];
            xh = new double[n];
            xs = new double[n];
            center = new double[n];

            l = 0;
            h = 0;
            s = 0;
        }
    
        private double[] Reflect(double[] x1, double[] x2)
        {
            double[] result = new double[n];

            for(int i = 0; i < n; ++i)
            {
                result[i] = x1[i] + alpha * (x1[i] - x2[i]);
            }

            return result;
        }

        private double[] Compress(double[] x1, double[] x2)
        {
            double[] result = new double[n];

            for (int i = 0; i < n; ++i)
            {
                result[i] = x1[i] + beta * (x2[i] - x1[i]);
            }

            return result;
        }

        private double[] Stretch(double[] x1, double[] x2)
        {
            double[] result = new double[n];

            for (int i = 0; i < n; ++i)
            {
                result[i] = x1[i] + gamma * (x2[i] - x1[i]);
            }

            return result;
        }
    
        private void Reduction()
        {
            for(int j = 0; j <= n; ++j)
            {
                if (j == l) continue;

                for(int i = 0; i < n; ++i)
                {
                    X[j][i] = 0.5 * (xl[i] + X[j][i]);
                }
            }
        }

        private void FindDots()
        {
            double y_min, y_max, y;

            if( function(X[0]) > function(X[1]) )
            {
                xl = X[1];
                l = 1;

                xh = X[0];
                h = 0;
            }
            else
            {
                xl = X[0];
                l = 0;

                xh = X[1];
                h = 1;
            }

            xs = xl;
            s = l;

            y_min = function(xl);
            y_max = function(xh);

            for(int i = 2; i <= n; ++i)
            {
                y = function(X[i]);

                if(y < y_min)
                {
                    xl = X[i];
                    l = i;
                    y_min = y;
                }

                if(y > y_max)
                {
                    xs = xh;
                    s = h;

                    xh = X[i];
                    h = i;

                    y_max = y;
                }
            }
        }
    
        private double[] FindGravityCenter()
        {
            for(int i = 0; i < n; ++i)
            {
                center[i] = 0;

                for(int j = 0; j <= n; ++j)
                {
                    center[i] += X[j][i];
                }

                center[i] -= xh[i];
                center[i] /= n;
            }

            return center;
        }
    
        private bool EndProccess()
        {
            double y = function(center);
            double sigma = 0;

            for(int i = 0; i <= n; ++i)
            {
                sigma += (function(X[i]) - y) * (function(X[i]) - y);
            }

            return sigma <= (n + 1) * epsilon;
        }

        private void DoCycle()
        {
            double[] x2 = FindGravityCenter();
            double[] x3 = Reflect(x2, xh);
            double[] x4;

            double y3 = function(x3);

            if( y3 <= function(xl) )
            {
                x4 = Stretch(x2, x3);
                double y4 = function(x4);

                if(y4 < function(xl))
                {
                    X[h] = x4;
                }
                else
                {
                    X[h] = x3;
                }
            }
            else if( function(xs) < y3 && y3 <= function(xh) )
            {
                x4 = Compress(x2, X[h]);
                X[h] = x4;
            }
            else if (y3 > function(xh))
            {
                Reduction();
            }
            else
            {
                X[h] = x3;
            }

            k += 1;
        }
    }

}
