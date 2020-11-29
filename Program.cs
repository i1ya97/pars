using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearRegression;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace ConsoleApp1
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            List<ChartData> x  = new List<ChartData>();
            var startDate = new DateTime(2017,1,1);
            var endDate = new DateTime(2020, 10, 31);
            while (startDate < endDate)
            {
                var current = GetLastDayPrevMonth(startDate.Month, startDate.Year);
                var url = "https://br.so-ups.ru/webapi/api/CommonInfo/GenConsum?priceZone[]=1&startDate="+startDate.ToString("yyyy.MM.dd") + "&endDate=" + current.ToString("yyyy.MM.dd");
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                using (var webClient = new WebClient())
                {
                    var response = JsonSerializer.Deserialize<Data[]>(webClient.DownloadString(url));
                    var obtainingEnergi = 0.0;
                    var consumotion = 0.0;
                    foreach (var value in response[0].m_Item2)
                    {
                        obtainingEnergi += value.E_USE_FACT;
                        consumotion += value.GEN_FACT;
                    }
                    x.Add(new ChartData(startDate.ToString("yyyy-MM"), obtainingEnergi, consumotion));
                }
                startDate = current.AddDays(1);
            }
            double[] xa = new double[x.Count];
            for(var i = 0; i < x.Count; i++)
            {
                xa[i] = x[i].obtainingEnergi;
            }
            double[] yProm = {
                61204.25, 56812.45, 59366.33, 56037.17, 53024.33, 47386.39, 47742.44, 48709.50, 49831.17, 56334.39, 58639.69, 59704.84,
                50188.77, 51740.49, 55216.59, 47608.97, 37660.01, 35147.82, 35380.11, 37136.68, 36712.63, 47274.06, 50803.98, 54937.97,
                49762.55, 49965.69, 51640.98, 46224.61, 38514.87, 34846.21, 35477.07, 37962.99, 37344.19, 46507.78, 49517.89, 51022.99,
                46149.89, 46257.10, 47768.53, 40504.90, 38589.26, 34048.87, 36266.92, 38141.27, 40078.05, 46751.42};

            double[] yNeProm = {
                74492.91, 69638.85, 64530.56, 59911.29, 52317.79, 46461.63, 47646.68, 48133.71, 52891.26, 63554.52, 69206.72, 68982.92,
                77023.24, 73078.67, 70994.24, 62862.90, 51578.53, 48122.02, 48872.46, 49816.10, 53136.65, 63728.05, 69611.74, 73676.61,
                83913.26, 75302.32, 71098.11, 63813.79, 56487.62, 50565.36, 50409.78, 51438.65, 56359.33, 67659.10, 72243.87, 72673.59,
                76498.74, 75527.86, 67136.09, 58040.57, 53081.61, 47205.39, 48803.74, 51767.99, 56215.40, 63349.27};


            double[] ySelesk = {
                195.79, 221.08, 199.00, 203.15, 187.17, 172.66, 153.12, 162.06, 157.99, 158.13, 165.12, 169.47,
                186.05, 198.04, 185.23, 157.48, 127.88, 133.57, 162.26, 158.58, 151.74, 147.34, 177.38, 126.71,
                115.45, 101.94, 93.89, 93.31, 70.48, 76.12, 62.44, 71.50, 84.92, 94.58, 108.48, 129.50,
                128.16, 142.41, 144.61, 198.08, 112.61, 126.34, 117.653, 104.35, 129.09, 168.78
            };

            double[] yNas = {
                98982.56, 95221.48, 97544.62, 95801.16, 87381.06, 82573.06, 74196.38, 74650.19, 84656.63, 93752.76, 94832.29, 103230.06,
                105149.91, 106643.38, 111144.21, 95796.12, 84553.71, 81816.20, 79506.51, 77479.11, 84328.02, 94802.17, 98137.08, 115456.36,
                114656.61, 99873.40, 108627.49, 95362.29, 92550.24, 82009.44, 83745.05, 83436.92, 88804.92, 99271.39, 107235.22, 115364.01,
                108024.44, 106106.46, 109164.12, 113164.76, 109427.23, 89236.53, 87009.36, 88083.48, 93754.88, 97509.82
            };

            double[] yPoter = {
                54521.39, 36691.81, 44129.91, 31641.13, 20952.64, 12946.32, 14572.92, 16409.61, 25289.56, 39156.98, 42576.72, 55197.56,
                51326.42, 42633.86, 54252.90, 27543.36, 17444.11, 11604.92, 15205.45, 16697.18, 25026.57, 39331.88, 43457.84, 61208.95,
                57930.42, 39243.72, 47546.69, 27166.78, 22908.52, 11188.33, 16971.09, 17831.36, 31281.95, 42870.94, 46150.37, 51426.98,
                50632.69, 41781.00, 49433.51, 31938.61, 22751.33, 11346.13, 14056.38, 17438.08, 24635.79, 41115.97
            };

            int index = 0;
            foreach (var point in x)
            {
                point.production = yProm[index];
                point.nonWill = yNeProm[index];
                point.agracultural = ySelesk[index];
                point.population = yNas[index];
                point.losses = yPoter[index];
                index++;
            }
            double a = 0.9;
            for (int year = 0; year < 3; year++)
            {
                double[] meanSquare = new double[12];
                for (int i = x.Count / 12 * 12 - 36; i < x.Count / 12 * 12; i++)
                {
                    meanSquare[i % 12] += x[i].obtainingEnergi;
                }
                double[] twoForecast = new double[12];
                double[] threeForecast = new double[12];
                double[] fourForecadt = new double[12];
                for (var i = 0; i < 12; i++)
                {
                    meanSquare[i] = meanSquare[i] / 3;
                    twoForecast[i] = OldForecast(a, meanSquare[i], x[x.Count / 12 * 12 - 36 + 12 + i].obtainingEnergi);
                    threeForecast[i] = OldForecast(a, meanSquare[i], x[x.Count / 12 * 12 - 36 + 24 + i].obtainingEnergi);
                    fourForecadt[i] = FinalForecast(a, twoForecast[i], threeForecast[i], meanSquare[i]);
                }
                if(x.Count % 12 == 0)
                {
                    for(int i = 0; i < 12; i++)
                    {
                        var date = DateTime.Parse(x[x.Count - 1].date).AddMonths(1).ToString("yyyy-MM");
                        x.Add(new ChartData(date, fourForecadt[x.Count % 12], 0));
                    }
                } else {
                    while (x.Count % 12 != 0)
                    {
                        var date = DateTime.Parse(x[x.Count - 1].date).AddMonths(1).ToString("yyyy-MM");
                        x.Add(new ChartData(date, fourForecadt[x.Count % 12], 0));
                    }
                }
            }
            var nullspace = Regression(xa, yProm);
            Console.WriteLine("Данные помышленности: " + nullspace);

            var nullspaceneprom = Regression(xa, yNeProm);
            Console.WriteLine("Данные непомышленности: " + nullspaceneprom);

            var nullspaceselsk = Regression(xa, ySelesk);
            Console.WriteLine("Данные непомышленности: " + nullspaceselsk);

            var nullspacenas = Regression(xa, yNas);
            Console.WriteLine("Данные населения: " + nullspacenas);

            var nullspapoter = Regression(xa, yPoter);
            Console.WriteLine("Данные потерь: " + nullspapoter);
            foreach (var point in x)
            {
                if (point.production == 0)
                {
                    point.production = nullspace.Item1 + point.obtainingEnergi * nullspace.Item2;
                }
                if (point.nonWill == 0)
                {
                    point.nonWill = nullspaceneprom.Item1 + point.obtainingEnergi * nullspaceneprom.Item2;
                }
                if (point.agracultural == 0)
                {
                    point.agracultural = nullspaceselsk.Item1 + point.obtainingEnergi * nullspaceselsk.Item2;
                }
                if (point.population == 0)
                {
                    point.population = nullspacenas.Item1 + point.obtainingEnergi * nullspacenas.Item2;
                }
                if (point.losses == 0)
                {
                    point.losses = nullspapoter.Item1 + point.obtainingEnergi * nullspapoter.Item2;
                }
            }
            var json = JsonSerializer.Serialize<List<ChartData>>(x);
            var nk = 0;
        }
        public static double OldForecast(double a, double meanSquare, double years) 
        {
            double y = 0;
            y = a * years + (1 - a) * meanSquare;
            return y;
        }
        public static double FinalForecast(double a ,double twoForecast, double threeForecast, double meanSquare)
        {
            double y = 0;
            y = a * twoForecast + a * (1 - a) * meanSquare + (((1 - a)*(1-a)) * (meanSquare + twoForecast + threeForecast)/3);
            return y;
        } 
        public static DateTime GetLastDayPrevMonth(int month, int year)
        {
            if(month == 12)
            {
                return new DateTime(year+1, 1, 1).AddDays(-1);
            } else
            {
                return new DateTime(year, month + 1, 1).AddDays(-1);
            }
        }

        public static Tuple<double, double> Regression(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException($"Все образцы векторов должны иметь одинаковую длину. Однако были предоставлены векторы с несовпадающей длиной {x.Length} и ​​{y.Length}. Выборка с индексом i задается значением с индексом i каждого предоставленного вектора.");
            }

            if (x.Length <= 1)
            {
                throw new ArgumentException($"Для регрессии запрошенного порядка требуется не менее {2} образцов. Были предоставлены только образцы {x.Length}.");
            }

            double mx = 0.0;
            double my = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                mx += x[i];
                my += y[i];
            }

            mx /= x.Length;
            my /= y.Length;

            double covariance = 0.0;
            double variance = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                double diff = x[i] - mx;
                covariance += diff * (y[i] - my);
                variance += diff * diff;
            }

            var b = covariance / variance;
            return new Tuple<double, double>(my - b * mx, b);
        }
    }
    class Data
    {
        public int id { get; set; }
        public int m_Item1 { get; set; }
        public Interval[] m_Item2 { get; set; }
    }

    class Interval
    {
        public double E_USE_FACT { get; set; }
        public string M_DATE { get; set; }
        public double GEN_FACT { get; set; }
    }

    class ChartData
    {
        public string date { get; set; }
        public double obtainingEnergi { get; set; }
        public double consumotion { get; set; }
        public double production { get; set; }
        public double nonWill { get; set; }
        public double agracultural { get; set; }
        public double population { get; set; }
        public double losses { get; set; }

        public ChartData(string date, double obtainingEnergi, double consumotion)
        {
            this.date = date;
            this.obtainingEnergi = obtainingEnergi;
            this.consumotion = consumotion;
        }
    }
        
}