using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace PolyhedronMethod
{
    internal delegate double map(double[] x);
    class Polyhedron
    {
        private map function;
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

        public ObservableCollection<ListBoxItem> Log { get; }
        private string logText;

        public Polyhedron(map f, double[][] dots, double a, double b, double c, double e)
        {
            Log = new ObservableCollection<ListBoxItem>();
            Update(f, dots, a, b, c, e);
        }

        public void Update(map f, double[][] dots, double a, double b, double c, double e)
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
            return "(" + string.Join(", ", x) + ")";
        }

        private double[] Reflect(double[] x1, double[] x2)
        {
            double[] result = (double[])x1.Select((x, i) => x + alpha * (x - x2[i]));

            logText += "Выполнено отражение: " + ArrToString(result) + "\n";

            return result;
        }

        private double[] Compress(double[] x1, double[] x2)
        {
            double[] result = (double[])x1.Select((x, i) => x + beta * (x2[i] - x));

            logText += "Выполнено сжатие: " + ArrToString(result) + "\n";

            return result;
        }

        private double[] Stretch(double[] x1, double[] x2)
        {
            double[] result = (double[])x1.Select((x, i) => x + gamma * (x2[i] - x));

            logText += "Выполнено растяжение: " + ArrToString(result) + "\n";

            return result;
        }

        private void Reduction()
        {
            for (int i = 0; i <= n; ++i)
            {
                if (i == l)
                {
                    continue;
                }

                for (int j = 0; j < n; ++j)
                {
                    X[i][j] = 0.5 * (xl[j] + X[i][j]);
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
            double sigma = X.Aggregate(0, (sum, x) => sum + (int)Math.Pow(function(x) - y, 2));

            return sigma <= (n + 1) * epsilon;
        }

        private bool DoCycle()
        {
            FindDots();
            FindGravityCenter();

            if (EndProccess())
            {
                return false;
            }

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

        public void FindExtr()
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
        }
    }
}
