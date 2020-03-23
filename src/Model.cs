using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Forms;
using System.Configuration;

namespace Termo
{
    abstract class SignalObject
    {
        public struct LineParams
        {
            public double Betta;
            public double T0;
            public double Tstart;
            public double Tend;
            public double dT;
        };

        public LineParams MNK(double[] x, double[] y)
        {
            LineParams res;
            res.Betta = 0;
            res.T0 = y[0];
            res.Tend = y[y.Length - 1];
            res.Tstart = y[0];
            res.dT = 0;

            if (x.Length > 0)
            {
                double X = 0, Y = 0, XX = 0, XY = 0;
                for (int i = 0; i < x.Length; i++)
                {
                    X += x[i];
                    XX += x[i] * x[i];
                    Y += y[i];
                    XY += x[i] * y[i];
                }
                res.Betta = (XY * x.Length - X * Y) / (x.Length * XX - X * X);
                res.T0 = (Y - res.Betta * X) / x.Length;
                res.dT = (x[x.Length - 1] - x[0]) / (x.Length - 1);
            }

            return res;
        }
    }
    class Signal : SignalObject
    {
        double[] Array;
        public Signal(double[] _D)
        {
            Array = _D;
        }

        public double[] array
        {
            get { return Array; }
        }
    };
    class SimpleSignal : SignalObject
    {
        Dictionary<string, int> Keys;
        public double Betta;
        public double T0;
        public double Tstart;
        public double Tend;
        public double dT;
        Signal[] DataArray;
        int[] DataIndex;

        public double[] GetSignal(string attr)
        {
            return DataArray[Keys[attr]].array;
        }

        string inputFile;

        public SimpleSignal(string inp, Dictionary<string, int> _Keys, int[] _DataIndex)
        {
            inputFile = inp;
            Keys = _Keys;
            DataIndex = _DataIndex;
            Load(DataIndex);
        }

        void CulcParams()
        {
            double[] x = DataArray[Keys["t"]].array;
            double[] y = DataArray[Keys["T"]].array;

            LineParams param = MNK(x, y);

            Betta = param.Betta;
            T0 = param.T0;
            dT = param.dT;
            Tstart = param.Tstart;
            Tend = param.Tend;
        }

        private void Load(int[] DataIndex)
        {
            List<List<double>> Data;
            FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read);

            if (fs == null) return;

            Data = new List<List<double>>();

            int Cur = DataIndex.Length;                             // Количество необходимых столбцов
            if (DataIndex[DataIndex.Length - 1] == -1) Cur--;
            for (int i = 0; i < Cur; i++)
                Data.Add(new List<double>());

            CultureInfo culture = new CultureInfo("en-us");
            using (StreamReader streamReader = new StreamReader(fs, Encoding.ASCII))
            {
                string line = String.Empty;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.StartsWith("#") || line == string.Empty) continue;
                    line = line.Replace(';', ' ');
                    //line = line.Replace(',', ' ');
                    line = System.Text.RegularExpressions.Regex.Replace(line, @"\s+", " "); // удаляем лишние пробелы
                    line = line.Trim();
                    string[] arr = line.Split();
                    for (int i = 0; i < Cur; i++)
                    {
                        Data[i].Add(double.Parse(arr[DataIndex[i]], culture.NumberFormat));
                    }
                }
            }
            DataArray = new Signal[Data.Count];
            int ii = 0;
            foreach (List<double> R in Data)
            {
                DataArray[ii] = new Signal(R.ToArray());
                ii++;
            }
            CulcParams();
        }
    }

    class Model : SignalObject
    {
        const double R = 8.3144598;
        const double K = 273.15;    //Перевод в кельвин
        const double KK = 1000;    //Перевод в килоджоули
        int[] DataIndex;

        Dictionary<string, int> Keys;
        List<List<SimpleSignal>> ModelSignals;
        List<List<double>> Ycalc, Yteor;
        AVGModel AVG;

        double VyazovkinE0, OzawaE0, FridmanE0,
               Model_n, Model_m, Model_p,
               VyazovkinA, VyazovkinError,
               TemperatureRangeLeft, TemperatureRangeRight;

        List<List<Signal>> ResultSignals;
        List<List<double>> VyazovkinE, OzawaE, FridmanE;
        List<double> alpha;
        List<int> func;

        ViewSignal ViewTG, ViewE, ViewAlpha, ViewEHidd, ViewAlphaHidd;

        public Model(List<List<string>> paths, Dictionary<string, int> _Keys, ViewSignal _ViewTG, ViewSignal _ViewE, ViewSignal _ViewAlpha, ViewSignal _ViewEHidd, ViewSignal _ViewAlphaHidd, int[] _DataIndex) // скорость <эксперимент>
        {
            ViewTG = _ViewTG;
            ViewE = _ViewE;
            ViewAlpha = _ViewAlpha;
            ViewEHidd = _ViewEHidd;
            ViewAlphaHidd = _ViewAlphaHidd;
            DataIndex = _DataIndex;

            Keys = _Keys;
            ModelSignals = new List<List<SimpleSignal>>();

            foreach (List<string> P in paths)
            {
                List<SimpleSignal> newSignalList = new List<SimpleSignal>();
                foreach (string str in P)
                    newSignalList.Add(new SimpleSignal(str, Keys, DataIndex));
                ModelSignals.Add(newSignalList);
            }

            double dBetta = 0;
            bool bFlag = true;

            for (int i = 0; i < ModelSignals.Count; i++)
            {
                if (bFlag || ModelSignals[i][0].Betta > dBetta)
                {
                    dBetta = ModelSignals[i][0].Betta;
                    TemperatureRangeLeft = ModelSignals[i][0].Tstart;
                    TemperatureRangeRight = ModelSignals[i][0].Tend;
                    bFlag = false;
                }
            }

            double Tstart = Tmin(), Tend = Tmax();

            AVG = new AVGModel(this, TemperatureRangeLeft, TemperatureRangeRight, DataIndex);
            TemperatureRangeRight = Math.Ceiling((TemperatureRangeRight - TemperatureRangeLeft) * 0.25);

        }
        public List<double> GetBetta()
        {
            List<double> BettaList = new List<double>();
            int N = AVG.CountSpeeds();
            for (int i = 0; i < N; i++)
                BettaList.Add(AVG.GetBetta(i));
            return BettaList;
        }
        public List<double> GetBettaGrid()
        {
            List<double> BettaList = new List<double>();
            int N = AVG.CountSpeeds();
            for (int i = 0; i < N; i++)
                BettaList.Add(AVG.GetBettaGrid(i));
            return BettaList;
        }

        public void plotEvsAl()
        {
            ViewE.Clear();
            double[] xVyazovkin = VyazovkinE[0].ToArray();
            double[] yVyazovkin = VyazovkinE[1].ToArray();
            ViewE.Add(xVyazovkin, yVyazovkin, "Vyazovkin");

            double[] xOzawa = OzawaE[0].ToArray();
            double[] yOzawa = OzawaE[1].ToArray();
            ViewE.Add(xOzawa, yOzawa, "Ozawa-Flynn-Wall");
            /*
                        double[] xFridman = FridmanE[0].ToArray();
                        double[] yFridman = FridmanE[1].ToArray();
                        ViewE.Add(xFridman, yFridman, "Fridman");
            */
            ViewEHidd.Clear();
            ViewEHidd.Add(xVyazovkin, yVyazovkin, "Vyazovkin");
            ViewEHidd.Add(xOzawa, yOzawa, "Ozawa-Flynn-Wall");
            //   ViewEHidd.Add(xFridman, yFridman, "Fridman");
        }
        public void plotDAlvsAl()
        {
            ViewAlpha.Clear();
            double[] x = Ycalc[0].ToArray();
            double[] y = Ycalc[1].ToArray();
            ViewAlpha.Add(x, y, "Calc");

            double[] xt = Yteor[0].ToArray();
            double[] yt = Yteor[1].ToArray();
            ViewAlpha.Add(xt, yt, "Teor");

            ViewAlphaHidd.Clear();
            ViewAlphaHidd.Add(x, y, "Calc");
            ViewAlphaHidd.Add(xt, yt, "Teor");
        }
        public string GetError()
        {
            return Math.Round(VyazovkinError, 2).ToString();
        }
        public string GetVyazovkinE0()
        {
            return Math.Round(VyazovkinE0, 2).ToString();
        }
        public string GetOzawaE0()
        {
            return Math.Round(OzawaE0, 2).ToString();
        }
        public string GetFridmanE0()
        {
            return Math.Round(FridmanE0, 2).ToString();
        }
        public string GetVyazovkinA()
        {
            return Math.Round(VyazovkinA, 2).ToString();
        }
        public string Getn()
        {
            return Math.Round(Model_n, 3).ToString();
        }
        public string Getm()
        {
            return Math.Round(Model_m, 3).ToString();
        }
        public string Getp()
        {
            return Math.Round(Model_p, 3).ToString();
        }
        public void plotTG_DTG_DTF(int[] _DataIndex)
        {
            for (int i = 0; i < AVG.CountSpeeds(); i++)
            {
                if (_DataIndex[3] != -1)
                {
                    ViewTG.Add(AVG.GetSignal("T", i), AVG.GetSignal("TG", i), AVG.GetSignal("DTA", i), AVG.GetSignal("DTG", i));
                    continue;
                }
                ViewTG.Add(AVG.GetSignal("T", i), AVG.GetSignal("TG", i), null, AVG.GetSignal("DTG", i));
            }
        }
        public double TminPlot()
        {
            double min = AVG.GetSignal("T", 0)[0];
            for (int i = 1; i < AVG.CountSpeeds(); i++)
                if (AVG.GetSignal("T", i)[0] > min) min = AVG.GetSignal("T", i)[0];
            return min;
        }

        public double TmaxPlot()
        {
            double max = AVG.GetSignal("T", 0)[AVG.GetSignal("T", 0).Length - 1];
            for (int i = 1; i < AVG.CountSpeeds(); i++)
                if (AVG.GetSignal("T", i)[AVG.GetSignal("T", i).Length - 1] < max) max = AVG.GetSignal("T", i)[AVG.GetSignal("T", i).Length - 1];
            return max;
        }

        public List<SimpleSignal> this[int i]
        {
            get { return ModelSignals[i]; }
        }
        public void Close()
        {
            //
            Keys = null;
            ModelSignals = null;

            AVG = null;

            ResultSignals = null;
            VyazovkinE = null;
            alpha = null;
            func = null;

            ViewTG = null;
        }

        public int Count
        {
            get { return ModelSignals.Count; }
        }
        public String GetTmax()
        {
            return Math.Floor(TemperatureRangeRight).ToString();
        }
        public String GetTmin()
        {
            return Math.Ceiling(TemperatureRangeLeft).ToString();
        }

        double Tmax()
        {
            double res = 6000;
            foreach (List<SimpleSignal> SS in ModelSignals)
                foreach (SimpleSignal SSS in SS)
                {
                    if (SSS.GetSignal("T")[SSS.GetSignal("T").Length - 1] < res)
                        res = SSS.GetSignal("T")[SSS.GetSignal("T").Length - 1];
                }
            return res;
        }
        double Tmin()
        {
            double res = -274;
            foreach (List<SimpleSignal> SS in ModelSignals)
                foreach (SimpleSignal SSS in SS)
                {
                    if (SSS.GetSignal("T")[0] > res)
                        res = SSS.GetSignal("T")[0];
                }
            return res;
        }





        public int Calc(double startT, double endT) // Возвращает 0 - не ошибок, -1 - только одна скорость нагрева
        {
            ResultSignals = new List<List<Signal>>();
            alpha = new List<double>();
            func = new List<int>();

            List<List<double>> alpha1 = new List<List<double>>();
            List<List<double>> TKelvin = new List<List<double>>();
            List<List<double>> T3;

            double TGa, TGmin, TGb, TGmax, eps = 1.0, Tmax = 0, alMax = 0, alStep;
            int NStep = 1000;

            List<double> b = new List<double>();
            for (int i = 0; i < ModelSignals.Count; i++) b.Add(AVG.GetBetta(i));

            if (b.Count == 1) return -1;

            int index = b.FindIndex(a => a == b.Max());
            TGa = AVG.GetSignal("TG", index)[0];
            TGb = AVG.GetSignal("TG", index)[1];
            for (int Ti = 0; Ti < AVG.GetSignal("T", index).Length; Ti++)
            {
                if (AVG.GetSignal("T", index)[Ti] < startT) TGa = AVG.GetSignal("TG", index)[Ti];
                if (AVG.GetSignal("T", index)[Ti] < endT) TGb = AVG.GetSignal("TG", index)[Ti];
            }

            bool bFlag = true;
            double minDTG = 0;
            for (int i = 0; i < AVG.CountSpeeds(); i++)
            {
                TGmin = AVG.GetSignal("TG", i).Min();
                TGmax = AVG.GetSignal("TG", i).Max();

                List<double> alphaForSpeed = new List<double>();
                List<double> TKelvinForSpeed = new List<double>();

                for (int j = 0; j < AVG.GetSignal("TG", i).Length; j++)
                {
                    if ((AVG.GetSignal("TG", i)[j] < TGb) || (AVG.GetSignal("TG", i)[j] > TGa)) continue;

                    double al = (TGa - AVG.GetSignal("TG", i)[j]) / (TGa - TGb);
                    if (al < 0) al = 0;
                    if (al > 1) al = 1;
                    if (alphaForSpeed.Count == 0) alphaForSpeed.Add(0); else alphaForSpeed.Add(al);
                    TKelvinForSpeed.Add(AVG.GetSignal("T", i)[j] + K);


                    if (i == AVG.CountSpeeds() - 1)
                    {
                        if (bFlag || minDTG > AVG.GetSignal("DTG", i)[j])
                        {
                            minDTG = AVG.GetSignal("DTG", i)[j];
                            Tmax = AVG.GetSignal("T", i)[j] + K;
                            alMax = al;
                            bFlag = false;
                        }

                    }
                }

                alpha1.Add(alphaForSpeed);
                TKelvin.Add(TKelvinForSpeed);
            }

            alStep = 1.0 / (NStep);
            double[,] T = new double[NStep + 1, alpha1.Count + 1];
            for (int i = 0; i <= NStep; i++)
            {
                double al = i * alStep;
                T[i, 0] = al;
                for (int j = 0; j < alpha1.Count; j++)
                    T[i, j + 1] = FindAl(TKelvin, alpha1, al, j);
            }

            double Alpha_Min,Alpha_Max;
            Double.TryParse(ConfigurationManager.AppSettings["Alpha_Min"], out Alpha_Min);            
            Double.TryParse(ConfigurationManager.AppSettings["Alpha_Max"], out Alpha_Max);

            Vyazovkin(out T3, T, b, eps, Alpha_Min, Alpha_Max);                // Находим энергию активации
            FindModel(T3, VyazovkinE0, Tmax, alMax, b, NStep);  //Находим модель процесса  
            VyazovkinE0 /= 1000;

            OzawaFlynnWall(T, b, Alpha_Min, Alpha_Max);
            FridmanMethod(T, Alpha_Min, Alpha_Max, 1000, 1000);

            return 0;
        }

        // ---------------------------------------------------------------------------------------------
        // Расчет по Фридмону
        //---------------------------------------------------------------------------------------------

        private void FridmanMethod(double[,] T, double alA, double alB, double NamberStepT, double NamberStepAl)
        {
            List<double> ColAl = new List<double>();
            List<double> ColE = new List<double>();
            FridmanE = new List<List<double>>();
            List<double> b = GetBetta();
            int n = b.Count;
            double TStep, x, y, al, A, B, Sxy, Sx, Sy, Sx2;


            FridmanE0 = 0;
            TStep = (T[T.GetUpperBound(0), T.GetUpperBound(1)] - T[0, T.GetUpperBound(1)]) / NamberStepT;

            for (int j = 0; j < T.GetUpperBound(0); j++)
            {
                al = T[j, 0];
                if (al < alA || al > alB) continue;
                A = 0; B = 0; Sxy = 0; Sx = 0; Sy = 0; Sx2 = 0;
                for (int k = 1; k <= n; k++)
                {
                    FindALFromAL_DAL(out x, out y, T, k, al, b, TStep);
                    Sxy += x * y;
                    Sx += x;
                    Sy += y;
                    Sx2 += x * x;
                }

                B = (n * Sxy - Sx * Sy) / (n * Sx2 - (Sx * Sx));
                A = (Sy - B * Sx) / n;

                ColAl.Add(al);
                ColE.Add((-B * R) / KK);

                FridmanE0 += -B * R;
            }
            FridmanE0 /= ColE.Count;
            FridmanE0 /= KK;
            FridmanE.Add(ColAl); FridmanE.Add(ColE);
        }

        private void FindALFromAL_DAL(out double x, out double y, double[,] T, int k, double al, List<double> b, double TStep)
        {
            x = 0; y = 0;
            double x0, x1, y0, y1, Temperature = 0;

            for (int j = 1; j < T.GetUpperBound(0); j++)
            {
                if (T[j - 1, 0] > al || T[j, 0] <= al) continue;
                x0 = T[j - 1, 0]; y0 = T[j - 1, k];
                x1 = T[j, 0]; y1 = T[j, k];
                Temperature = ((al - x0) / (x1 - x0)) * (y1 - y0) + y0;
                x = 1 / Temperature;
                Temperature = Temperature + TStep;
                break;

            }

            for (int j = 1; j < T.GetUpperBound(0); j++)
            {
                if (T[j - 1, k] > Temperature || T[j, k] <= Temperature) continue;
                x0 = T[j - 1, k]; y0 = T[j - 1, 0];
                x1 = T[j, k]; y1 = T[j, 0];
                y = ((Temperature - x0) / (x1 - x0)) * (y1 - y0) + y0 - al;
                y = Math.Log(y * b[k - 1]);
                break;

            }
        }


        void SaveDataToFile(SaveFileDialog SaveFile, List<List<double>> Data, string title)
        {
            if (SaveFile.ShowDialog() != DialogResult.OK) return;

            string FileName = SaveFile.FileName;

            FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write);

            if (fs == null) return;

            CultureInfo culture = new CultureInfo("en-us");
            using (StreamWriter streamWriter = new StreamWriter(fs, Encoding.ASCII))
            {
                streamWriter.WriteLineAsync(title);
                for (int j = 0; j < Data[0].Count; j++)
                {
                    string line = String.Empty;
                    for (int i = 0; i < Data.Count; i++)
                        line += Data[i][j].ToString() + "\t";

                    streamWriter.WriteLine(line);
                }
            }
        }

        public void SaveDataE(SaveFileDialog SaveFile)
        {
            List<List<double>> Data = new List<List<double>>();
            Data.Add(VyazovkinE[0]);
            Data.Add(VyazovkinE[1]);
            Data.Add(OzawaE[1]);
            Data.Add(FridmanE[1]);

            SaveDataToFile(SaveFile, Data, "alpha \t VyazovkinE \t OzawaE \t FridmanE");
        }


        public void SaveDataAlpha(SaveFileDialog SaveFile)
        {
            List<List<double>> Data = new List<List<double>>();
            Data.Add(Ycalc[0]);
            Data.Add(Ycalc[1]);
            Data.Add(Yteor[1]);

            SaveDataToFile(SaveFile, Data, "alpha \t calc \t teor");
        }



        // ---------------------------------------------------------------------------------------------
        // Расчет по методу Ozawa–Flynn–Wall
        // ---------------------------------------------------------------------------------------------

        private void OzawaFlynnWall(double[,] T, List<double> b, double alA, double alB)
        {
            List<List<double>> X = new List<List<double>>();
            List<List<double>> Y = new List<List<double>>();
            List<double> ColAl = new List<double>();
            List<double> ColE = new List<double>();
            OzawaE = new List<List<double>>();

            int g = T.GetUpperBound(0),
                w = T.GetUpperBound(1);
            int n = b.Count;
            for (int i = 0; i <= T.GetUpperBound(0); i++)
            {
                if (T[i, 0] < alA) continue;
                if (T[i, 0] > alB) break;
                List<double> RowY = new List<double>();
                List<double> RowX = new List<double>();

                RowY.Add(T[i, 0]);
                RowX.Add(T[i, 0]);
                for (int j = 1; j <= n; j++)
                {
                    RowY.Add(Math.Log(b[j - 1] * Math.Pow(T[i, j], 1.92)));
                    RowX.Add(1 / T[i, j]);
                }
                X.Add(RowX);
                Y.Add(RowY);
            }


            double A, B, Sxy, Sx, Sy, Sx2;
            OzawaE0 = 0;
            for (int i = 0; i < X.Count; i++)
            {
                Sxy = 0; Sx = 0; Sy = 0; Sx2 = 0;
                for (int j = 1; j < X[0].Count; j++)
                {
                    Sxy += X[i][j] * Y[i][j];
                    Sx += X[i][j];
                    Sy += Y[i][j];
                    Sx2 += X[i][j] * X[i][j];
                }
                B = (n * Sxy - Sx * Sy) / (n * Sx2 - (Sx * Sx));
                A = (Sy - B * Sx) / n;

                ColAl.Add(X[i][0]);
                ColE.Add((-B * R / 1.0008) / KK);

                OzawaE0 += (-B * R / 1.0008);
            }
            OzawaE0 /= X.Count;
            OzawaE0 /= KK;
            OzawaE.Add(ColAl); OzawaE.Add(ColE);
        }
        private void Vyazovkin(out List<List<double>> T3, double[,] T, List<double> b, double eps, double alA, double alB)
        {
            //%Нахождение минимума функционала по Вязовскому
            VyazovkinE0 = 0;
            VyazovkinE = new List<List<double>>();
            T3 = new List<List<double>>();
            List<double> ColAl = new List<double>();
            List<double> ColE = new List<double>();
            List<double> ColT = new List<double>();
            List<double> _ColAl = new List<double>();
            List<double> _ColE = new List<double>();

            int N = b.Count - 1;
            double A, B, al, x, f1, f2, Step, n = 1;

            for (int j = 0; j < T.GetLength(0); j++)
            {
                al = T[j, 0];
                // Метод дихотомии для оптимизации
                A = 0;
                B = 10000000;
                while (Math.Abs(B - A) > eps)
                {
                    x = (A + B) / 2;
                    Step = (B - A) / 100.0;
                    f1 = F(T, x - Step, b, j);
                    f2 = F(T, x + Step, b, j);
                    if (f1 > f2) A = x;
                    else B = x;
                }

                ColAl.Add(al);
                ColE.Add((A + B) / 2.0);
                ColT.Add(T[j, N]);

                if (al < alA || al > alB) continue;
                _ColAl.Add(al);
                _ColE.Add(((A + B) / 2.0) / KK);
                VyazovkinE0 += (A + B) / 2.0;
                n++;
            }
            VyazovkinE0 /= n;
            T3.Add(ColAl); T3.Add(ColE); T3.Add(ColT);
            VyazovkinE.Add(_ColAl); VyazovkinE.Add(_ColE);

            x = 0; n = 1;
            for (int i = 0; i < _ColE.Count; i++)
            {
                x += (VyazovkinE0 - _ColE[i] * KK) * (VyazovkinE0 - _ColE[i] * KK);
                n++;
            }
            VyazovkinError = 100 * Math.Sqrt(x / n) / VyazovkinE0;
        }
        private void FindModel(List<List<double>> T, double E0, double Tmax, double alMax, List<double> b, int Nsteps)
        {
            //Нахождение теоретической функции f(al)       
            List<double> al = new List<double>();
            List<double> dAl = new List<double>();
            Ycalc = new List<List<double>>();

            int N = T.Count - 1;
            double x0, x1, y0, y1, Ti, Y05calc, Al, min,
                   T1 = T[N][0],
                   T2 = T[N][T[N].Count - 1],
                   tStep = (T2 - T1) / Nsteps;

            for (int i = 0; i < Nsteps; i++)
            {
                Ti = i * tStep + T1;
                for (int j = 1; j < T[N].Count; j++)
                {
                    if ((T[N][j - 1] <= Ti) && (T[N][j] > Ti))
                    {
                        x0 = T[N][j - 1]; y0 = T[0][j - 1];
                        x1 = T[N][j]; y1 = T[0][j];
                        al.Add(((Ti - x0) / (x1 - x0)) * (y1 - y0) + y0);
                        break;
                    }
                }
            }

            for (int j = 1; j < al.Count; j++) dAl.Add(al[j] - al[j - 1]);

            al.RemoveAt(0);

            Ycalc.Add(al);
            Ycalc.Add(dAl);


            Y05calc = 0; Al = 0.5;
            for (int i = 1; i < Ycalc[0].Count; i++)
            {
                if ((Ycalc[0][i - 1] <= Al) && (Ycalc[0][i] > Al))
                {
                    x0 = Ycalc[0][i - 1]; y0 = Ycalc[1][i - 1];
                    x1 = Ycalc[0][i]; y1 = Ycalc[1][i];
                    Y05calc = ((Al - x0) / (x1 - x0)) * (y1 - y0) + y0;
                    break;
                }
            }

            for (int j = 0; j < Ycalc[1].Count; j++) Ycalc[1][j] /= Y05calc;

            // Подбираем А и f(альфа) для разных p,n,m
            double RSS = 0, A = 0;
            List<List<double>> ModelParam = new List<List<double>>();

            for (double p = 0.0; p <= 1.0; p += 1.0)
                for (double n = 0.0; n <= 2; n += 0.01)
                    for (double m = 0.1; m <= 3.0; m += 0.2)
                    {
                        List<List<double>> Yt;
                        Paint(out Yt, out A, T1, E0, Tmax, alMax, m, n, p, b[b.Count - 1], tStep);
                        if (A <= 0) continue;
                        RSS = 0;
                        for (int j = 0; j < Ycalc[1].Count; j++)
                            RSS += Math.Pow(Ycalc[1][j] - Yt[1][j], 2);

                        List<double> Row = new List<double>();

                        Row.Add(RSS);
                        Row.Add(A);
                        Row.Add(n);
                        Row.Add(m);
                        Row.Add(p);
                        ModelParam.Add(Row);
                    }

            if (ModelParam.Count == 0) return;
            List<double> MinParam = ModelParam[0];
            for (int i = 0; i < ModelParam.Count; i++)
            {
                if (MinParam[0] > ModelParam[i][0]) MinParam = ModelParam[i];
            }
            A = MinParam[1];
            VyazovkinA = A / 1e10;
            Model_n = MinParam[2];
            Model_m = MinParam[3];
            Model_p = MinParam[4];

            Paint(out Yteor, out A, T1, E0, Tmax, alMax, Model_m, Model_n, Model_p, b[b.Count - 1], tStep);

        }
        void Paint(out List<List<double>> Yt, out double A, double T1, double E0, double Tmax, double alMax, double m, double n, double p, double b, double TStep)    //Рисуем найденную теоретическую функцию и экспериментальную кривую dальфа/dt
        {
            //Расчитываем Yteor
            double Ti, Y05teor, Al, x0, x1, y0, y1;
            List<double> col1 = new List<double>();
            List<double> col2 = new List<double>();

            Yt = new List<List<double>>();

            A = FindA(Tmax, alMax, E0, m, n, p, b);
            if (A < 0) return;
            Ti = T1;
            for (int i = 0; i < Ycalc[0].Count; i++)
            {
                col1.Add(Ycalc[0][i]);
                col2.Add(A * Math.Exp(-E0 / (R * Ti)) * f(m, n, p, Ycalc[0][i]));

                Ti = Ti + TStep;
            }
            Yt.Add(col1);
            Yt.Add(col2);

            Y05teor = 0; Al = 0.5;
            for (int i = 1; i < Yt[0].Count; i++)
            {
                if ((Yt[0][i - 1] <= Al) && (Yt[0][i] > Al))
                {
                    x0 = Yt[0][i - 1]; y0 = Yt[1][i - 1];
                    x1 = Yt[0][i]; y1 = Yt[1][i];
                    Y05teor = ((Al - x0) / (x1 - x0)) * (y1 - y0) + y0;
                    break;
                }
            }
            for (int j = 0; j < Yt[1].Count; j++) Yt[1][j] /= Y05teor;
        }
        double f(double m, double n, double p, double x) // Функция процесса
        {
            // x - альфа
            // m,n,p - степени
            return Math.Pow(x, m) * Math.Pow(1 - x, n) * Math.Pow(-Math.Log(1 - x), p);
        }
        double fD(double m, double n, double p, double x)                      //Первая и последняя производная процесса
        {
            // x - альфа
            // m,n,p - степени
            return Math.Pow(x, m - 1) * Math.Pow(1 - x, n - 1) * Math.Pow(-Math.Log(1 - x), p - 1) * (Math.Log(1 - x) * (m * (x - 1) + n * x) + p * x);
        }
        double FindA(double Tmax, double alMax, double E0, double m, double n, double p, double b) //Нахождение предэкспоненциального множителя A 
        {
            // Tmax - максимальная темпертура на рассматриваемом участке в Кельвинах, 
            // alMax - альфа при максимальной температуре Tmax,
            // E0 - средняя энергия активции, 
            // m,n,p - степени модели,
            // b - коэффициент скорость нагрева
            double R = 8.3144598;
            double f = fD(m, n, p, alMax);
            if (f == 0) return -1;
            return ((-b * E0) / (R * Tmax * Tmax * f)) * Math.Exp(E0 / (R * Tmax));
        }
        double FindAl(List<List<double>> T, List<List<double>> alpha, double al, int Speed)
        {
            // Кусочно-линейная аппроксимция
            if (alpha[Speed][0] > al)
            {
                double x0 = alpha[Speed][0];
                double y0 = T[Speed][0];
                double x1 = alpha[Speed][1];
                double y1 = T[Speed][1];
                return ((al - x0) / (x1 - x0)) * (y1 - y0) + y0;
            }

            for (int i = 1; i < alpha[Speed].Count; i++)
            {
                if ((alpha[Speed][i - 1] <= al) && (alpha[Speed][i] > al))
                {
                    double x0 = alpha[Speed][i - 1];
                    double y0 = T[Speed][i - 1];
                    double x1 = alpha[Speed][i];
                    double y1 = T[Speed][i];
                    return ((al - x0) / (x1 - x0)) * (y1 - y0) + y0;
                }
            }
            return T[Speed][T[Speed].Count - 1];
        }
        double F(double[,] T, double E, List<double> b, int t1)
        {
            //Нахождение функционала Вязовского
            //T - массив температур, E - энергия активации, b - вектор коэффициентов скоростей нагрева, t1 - соотносит с альфа

            double S = 0;
            for (int i = 0; i < b.Count; i++)
                for (int j = 0; j < b.Count; j++)
                {
                    if (i != j)
                    {
                        double T1 = T[t1, i + 1];
                        double T2 = T[t1, j + 1];
                        S = S + (I(T1, E) * b[j]) / (I(T2, E) * b[i]);
                    }
                }
            return S;
        }
        double I(double T, double E)
        {
            //Расчет значения интеграла
            //T и E - температура и энергия активации при заданном значении альфа 
            return E * pp(T, E) / 8.3144598;
        }
        double pp(double T, double E)
        {
            double x = E / (8.3144598 * T);
            return (Math.Exp(-x) * (x * x * x + 18 * x * x + 88 * x + 96)) / (x * (x * x * x * x + 20 * x * x * x + 120 * x * x + 240 * x + 120));
        }
    }

    class AVGModel : SignalObject
    {
        Dictionary<string, int> Keys = new Dictionary<string, int>();

        List<double> bGrid; // правильные индексы для грида

        struct AS
        {
            public Signal[] S;
            public double Betta;
            public double T0;
            public double Tend;
        }
        List<AS> Signals = new List<AS>(); // по скоростям


        int TempToIndex(double T, SimpleSignal SpeedSignal)
        {
            int res = Convert.ToInt32((T - SpeedSignal.T0) / (SpeedSignal.Betta * (SpeedSignal.dT)));
            if (res < 0) res = 0;
            else if (res >= SpeedSignal.GetSignal("t").Length)
                res = SpeedSignal.GetSignal("t").Length - 1;

            return res;
        }

        double[] ConcateArray(List<SimpleSignal> Signals, int[] StartInd, int[] EndInd, string SignalName)
        {
            int Len = 0;
            for (int i = 0; i < StartInd.Length; i++)
            {
                Len += EndInd[i] - StartInd[i] + 1;
            }

            double[] arr = new double[1];

            if (Signals.Count > 0)
            {
                arr = Signals[0].GetSignal(SignalName);
                for (int i = 1; i < Signals.Count; i++)
                {
                    arr = (arr.Concat(Signals[i].GetSignal(SignalName))).ToArray<double>();
                }
            }
            return arr;
        }

        void DTG()
        {
            // посчитать производную от TG по T

            for (int k = 0; k < Signals.Count; k++)
            {
                for (int i = 1; i < Signals[k].S[1].array.Length - 1; i++)
                    Signals[k].S[3].array[i] = (Signals[k].S[1].array[i] - Signals[k].S[1].array[i - 1]);// / (Signals[k].S[0].array[i] - Signals[k].S[0].array[i - 1]) + (Signals[k].S[1].array[i + 1] - Signals[k].S[1].array[i]) / (Signals[k].S[0].array[i + 1] - Signals[k].S[0].array[i])) * 0.5;
            }

        }

        public double[] GetSignal(string str, int speed)
        {
            return Signals[speed].S[Keys[str]].array;
        }


        public double GetBetta(int speed)
        {
            return Signals[speed].Betta;
        }

        public double GetBettaGrid(int speed)
        {
            return bGrid[speed];
        }

        public int CountSpeeds()
        {
            return Signals.Count;
        }

        public AVGModel(Model InpModel, double startT, double endT, int[] DataIndex)
        {
            Keys.Add("T", 0);
            Keys.Add("TG", 1);
            Keys.Add("DTA", 2);
            Keys.Add("DTG", 3);

            for (int i = 0; i < InpModel.Count; i++)
            {
                int Dim = 4;

                AS SignalForSpeed = new AS();
                SignalForSpeed.S = new Signal[Dim];

                List<SimpleSignal> SpeedSignal = InpModel[i];
                int[] StartInd = new int[SpeedSignal.Count];
                int[] EndInd = new int[SpeedSignal.Count];
                for (int j = 0; j < SpeedSignal.Count; j++)
                {
                    StartInd[j] = TempToIndex(startT, SpeedSignal[j]);
                    EndInd[j] = TempToIndex(endT, SpeedSignal[j]);
                }

                double dtemp = (endT - startT) / (EndInd[0] - StartInd[0]);

                double[] x = ConcateArray(SpeedSignal, StartInd, EndInd, "t");
                double[] y = ConcateArray(SpeedSignal, StartInd, EndInd, "T");

                LineParams param = MNK(x, y);

                SignalForSpeed.Betta = param.Betta;
                SignalForSpeed.T0 = param.T0;

                int N = (int)((endT - startT) / dtemp);
                SignalForSpeed.S[0] = new Signal(SpeedSignal[0].GetSignal("T"));

                List<Functions> Mass = new List<Functions>();
                for (int j = 0; j < SpeedSignal.Count; j++)
                    Mass.Add(new Functions(new List<double>(SpeedSignal[j].GetSignal("T")), new List<double>(SpeedSignal[j].GetSignal("Mass"))));
                AVGFunctions AVGMass = new AVGFunctions(Mass, 0); // x возьмем по первым данным, это можно исправить позже как решим                
                SignalForSpeed.S[1] = AVGMass.Get();//;new Signal(SpeedSignal[0].GetSignal("Mass"))

                if (DataIndex[DataIndex.Length - 1] != -1)
                {
                    List<Functions> DSC = new List<Functions>();
                    for (int j = 0; j < SpeedSignal.Count; j++)
                        DSC.Add(new Functions(new List<double>(SpeedSignal[j].GetSignal("T")), new List<double>(SpeedSignal[j].GetSignal("DSC"))));
                    AVGFunctions AVGDSC = new AVGFunctions(DSC, 0); // x возьмем по первым данным, это можно исправить позже как решим                
                    SignalForSpeed.S[2] = AVGDSC.Get(); //new Signal(SpeedSignal[0].GetSignal("DSC"));
                }

            SignalForSpeed.S[3] = new Signal(new double[SpeedSignal[0].GetSignal("T").Length]);

            Signals.Add(SignalForSpeed);
            }

            bGrid = new List<double>();
            
            for (int i = 0; i<Signals.Count; i++)
            {
                bGrid.Add(Signals[i].Betta);
            }


            bool f = true;
            while (f)
            {
                f = false;
                for (int i = 0; i<Signals.Count - 1; i++)
                    if (Signals[i].Betta > Signals[i + 1].Betta)
                    {
                        AS tmp = Signals[i];
                        Signals[i] = Signals[i + 1];
                        Signals[i + 1] = tmp;

                        f = true;
                    }
            }

            DTG();
        }
    }

    class Functions // класс для интерполяции данных
{
    List<double> DataX;
    List<double> DataY;

    public int Len
    {
        get { return DataX.Count; }
    }

    public double GetX(int i)
    {
        return DataX[i];
    }

    public Functions(List<double> _DataX, List<double> _DataY)
    {
        DataX = _DataX;
        DataY = _DataY;
    }

    public double GetLinear(double x)// Кусочно-линейная аппроксимция
    {
        if (DataX[0] > x)
        {
            double x0 = DataX[0];
            double y0 = DataY[0];
            double x1 = DataX[1];
            double y1 = DataY[1];
            return ((x - x0) / (x1 - x0)) * (y1 - y0) + y0;
        }

        for (int i = 1; i < DataX.Count; i++)
        {
            if ((DataX[i - 1] <= x) && (DataX[i] > x))
            {
                double x0 = DataX[i - 1];
                double y0 = DataY[i - 1];
                double x1 = DataX[i];
                double y1 = DataY[i];
                return ((x - x0) / (x1 - x0)) * (y1 - y0) + y0;
            }
        }
        return DataY[DataY.Count - 1];
    }
}


class AVGFunctions // класс для интерполяции данных
{
    List<Functions> Data;
    int finestMesh; // набор данных с наиболее детализированной сеткой по X

    public AVGFunctions(List<Functions> _Data, int refX)
    {
        Data = _Data;
        finestMesh = refX;
    }

    public Signal Get()
    {
        int N = Data[finestMesh].Len;
        double[] Arr = new double[N];

        for (int i = 0; i < N; i++)
        {
            Arr[i] = 0;
            double x = Data[finestMesh].GetX(i);
            for (int j = 0; j < Data.Count; j++)
                Arr[i] += Data[j].GetLinear(x);
            Arr[i] /= Data.Count;
        }

        return new Signal(Arr);
    }
}

}
