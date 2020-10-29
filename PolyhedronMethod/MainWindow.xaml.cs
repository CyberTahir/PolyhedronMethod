using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PolyhedronMethod
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Polyhedron p;
        public MainWindow()
        {
            InitializeComponent();

            p = new Polyhedron(x => x[0], null, 0, 0, 0, 0);
            Log.ItemsSource = p.Log;
        }

        private void Search(Func<double[], double> f, double[][] dots, double alpha, double beta, double gamma, double epsilon)
        {
            p.Update(f, dots, alpha, beta, gamma, epsilon);
            p.FindExtr();

            Data.Visibility = Visibility.Hidden;
            Log.Visibility = Visibility.Visible;

            Log.UpdateLayout();
        }

        private void OpenData()
        {
            Log.Visibility = Visibility.Hidden;
            Data.Visibility = Visibility.Visible;

            p.Log.Clear();
        }

        private void Test1_Click(object sender, RoutedEventArgs e)
        {
            Func<double[], double> f = (x => 4 * (x[0] - 5) * (x[0] - 5) + (x[1] - 6) * (x[1] - 6));
            double[][] dots = { new double[] { 8, 9 }, new double[] { 10, 11 }, new double[] { 8, 11 } };
            double alpha = Convert.ToDouble(Alpha.Text);
            double beta = Convert.ToDouble(Beta.Text);
            double gamma = Convert.ToDouble(Gamma.Text);
            double epsilon = Convert.ToDouble(Epsilon.Text);

            Search(f, dots, alpha, beta, gamma, epsilon);
        }

        private void Test2_Click(object sender, RoutedEventArgs e)
        {
            Func<double[], double> f = (x => 2 * x[0] * x[0] + x[0] * x[1] + x[1] * x[1]);
            double[][] dots = { new double[] { 0.5, 1 }, new double[] { 0, 0.5 }, new double[] { 1, 0.5 } };
            double alpha = Convert.ToDouble(Alpha.Text);
            double beta = Convert.ToDouble(Beta.Text);
            double gamma = Convert.ToDouble(Gamma.Text);
            double epsilon = Convert.ToDouble(Epsilon.Text);

            Search(f, dots, alpha, beta, gamma, epsilon);
        }

        private void NewTest_Click(object sender, RoutedEventArgs e)
        {
            OpenData();
        }
    }

    class Polyhedron
    {
        private Func<double[], double> function;
        private double[][] X;
        private int n;

        private double alpha;
        private double beta;
        private double gamma;
        private double epsilon;

        private double[] xl;
        private double[] xh;
        private double[] xs;
        private double[] center;

        private int l, h, k;

        private string logText;

        public Polyhedron(Func<double[], double> f, double[][] dots, double a, double b, double c, double e)
        {
            Log = new ObservableCollection<ListBoxItem>();
            Update(f, dots, a, b, c, e);
        }

        public void Update(Func<double[], double> f, double[][] dots, double a, double b, double c, double e)
        {
            function = f;
            X = dots;
            n = dots == null ? 0 : dots.Length - 1;

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
            k = 0;

            logText = "";
            Log.Clear();
        }

        private string ArrToString(double[] x)
        {
            string result = "(" + Convert.ToString(x[0]);

            for (int i = 1; i < n; ++i)
            {
                result += ", " + Convert.ToString(x[i]);
            }

            return result + ")";
        }

        private double[] Reflect(double[] x1, double[] x2)
        {
            double[] result = new double[n];

            for (int i = 0; i < n; ++i)
            {
                result[i] = x1[i] + alpha * (x1[i] - x2[i]);
            }

            logText += "Выполнено отражение: " + ArrToString(result) + "\n";

            return result;
        }

        private double[] Compress(double[] x1, double[] x2)
        {
            double[] result = new double[n];

            for (int i = 0; i < n; ++i)
            {
                result[i] = x1[i] + beta * (x2[i] - x1[i]);
            }

            logText += "Выполнено сжатие: " + ArrToString(result) + "\n";

            return result;
        }

        private double[] Stretch(double[] x1, double[] x2)
        {
            double[] result = new double[n];

            for (int i = 0; i < n; ++i)
            {
                result[i] = x1[i] + gamma * (x2[i] - x1[i]);
            }

            logText += "Выполнено растяжение: " + ArrToString(result) + "\n";

            return result;
        }

        private void Reduction()
        {
            for (int j = 0; j <= n; ++j)
            {
                if (j == l) continue;

                for (int i = 0; i < n; ++i)
                {
                    X[j][i] = 0.5 * (xl[i] + X[j][i]);
                }
            }

            logText += "Выполнена редукция\n";
        }

        private void FindDots()
        {
            double y_min, y_max, y;

            if (function(X[0]) > function(X[1]))
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

            y_min = function(xl);
            y_max = function(xh);

            for (int i = 2; i <= n; ++i)
            {
                y = function(X[i]);

                if (y < y_min)
                {
                    xl = X[i];
                    l = i;
                    y_min = y;
                }

                if (y > y_max)
                {
                    xs = xh;

                    xh = X[i];
                    h = i;

                    y_max = y;
                }
            }

            logText += "Лучшая точка xl = " + ArrToString(xl) + "\n";
            logText += "Худшая точка xh = " + ArrToString(xh) + "\n";
        }

        private void FindGravityCenter()
        {
            for (int i = 0; i < n; ++i)
            {
                center[i] = 0;

                for (int j = 0; j <= n; ++j)
                {
                    center[i] += X[j][i];
                }

                center[i] -= xh[i];
                center[i] /= n;
            }

            logText += "Центр тяжести x2 = " + ArrToString(center) + "\n";
        }

        private bool EndProccess()
        {
            double y = function(center);
            double sigma = 0;

            for (int i = 0; i <= n; ++i)
            {
                sigma += (function(X[i]) - y) * (function(X[i]) - y);
            }

            return sigma <= (n + 1) * epsilon;
        }

        private bool DoCycle()
        {
            FindDots();
            FindGravityCenter();

            if (EndProccess()) return false;

            double[] x3 = Reflect(center, xh);
            double[] x4;
            double y3 = function(x3);

            if (y3 <= function(xl))
            {
                x4 = Stretch(center, x3);
                double y4 = function(x4);

                if (y4 < function(xl))
                {
                    X[h] = x4;
                    logText += "Заменяем вершину xh на вершину x4";
                }
                else
                {
                    X[h] = x3;
                    logText += "Заменяем вершину xh на вершину x3";
                }
            }
            else if (function(xs) < y3 && y3 <= function(xh))
            {
                x4 = Compress(center, xh);
                X[h] = x4;
                logText += "Заменяем вершину xh на вершину x5";
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

            return true;
        }

        private void WriteLog()
        {
            Log.Add(new ListBoxItem
            {
                Background = Brushes.LightBlue,
                Content = "Итерация " + Convert.ToString(k) + ":"
            });

            Log.Add(new ListBoxItem
            {
                Background = Brushes.LightGray,
                Content = logText
            });

            logText = "";
        }

        public ObservableCollection<ListBoxItem> Log { get; }

        public double[] FindExtr()
        {
            while (DoCycle())
            {
                WriteLog();
            }

            Log.Add(new ListBoxItem
            {
                Background = Brushes.LightGreen,
                Content = "Найдена лучшая вершина x* = " + ArrToString(xl)
            });

            logText = "";

            return xl;
        }
    }

}
