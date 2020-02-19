using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace Termo
{
    class ViewSignal
    {
        public Chart Plot;
        private List<string> Title2;
        private string Title1;

        double ymin, ymax;

        public ViewSignal(Chart _Plot, string _Title)
        {
            Plot = _Plot;
            Title1 = _Title;
            Plot.Series.Clear();
            Plot.ChartAreas.Clear();
            Plot.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea(Title1));

            Plot.ChartAreas[0].Position.Auto = false;
            Plot.ChartAreas[0].Position.Width = 90;
            Plot.ChartAreas[0].Position.Height = 95;


            Plot.ChartAreas[0].AxisX.Title = "alpha";
            Plot.ChartAreas[0].AxisY.Title = Title1;
            Plot.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Far;
            Plot.ChartAreas[0].AxisY.TitleAlignment = StringAlignment.Far;

            Plot.ChartAreas[0].AlignmentOrientation = AreaAlignmentOrientations.Horizontal;
            Plot.ChartAreas[0].AlignmentStyle = AreaAlignmentStyles.PlotPosition;
            
            Plot.ChartAreas[0].Position.X = 0;
            Plot.ChartAreas[0].Position.Y = 5;

            Plot.ChartAreas[0].AxisX.LabelStyle.Format = "F2";
            Plot.ChartAreas[0].AxisY.LabelStyle.Format = "F2";

            ymax = 0;
            ymin = 1000;
        }

        public void Add(double [] x, double [] y, string legend = "")
        {
            System.Windows.Forms.DataVisualization.Charting.Series mySeriesOfPoint = new System.Windows.Forms.DataVisualization.Charting.Series(legend);
            mySeriesOfPoint.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            mySeriesOfPoint.ChartArea = Title1;

            for (int i = 0; i < x.Length; i++)
            {
                mySeriesOfPoint.Points.AddXY(x[i], y[i]);
                if (y[i] < ymin) ymin = y[i];
                if (y[i] > ymax) ymax = y[i];
            }
            Plot.Series.Add(mySeriesOfPoint);

            Plot.ChartAreas[0].AxisY.Minimum = ymin;
            Plot.ChartAreas[0].AxisY.Maximum = ymax;

            Plot.ChartAreas[0].Position.Auto = false;
            Plot.ChartAreas[0].Position.Width = 95;
            Plot.ChartAreas[0].Position.Height = 95;
            Plot.ChartAreas[0].Position.X = 0;
            Plot.ChartAreas[0].Position.Y = 15;
        }

        public void Clear()
        {
            Plot.Series.Clear();
            ymax = 0;
            ymin = 1000;
        }

        public ViewSignal(Chart _Plot, List<string> _Title, int[] _DataIndex) // параметр k не нужен. он лишь позволяет еще один конструктор задать
        {
            Plot = _Plot;
            Title2 = _Title;
            //myModel = new PlotModel { Title = Title1 };
            Plot.Series.Clear();
            Plot.ChartAreas.Clear();

            int iBeg = 0,i=0, iHeight=30;
            if (_DataIndex[3] == -1)
            {
                iBeg++;
                iHeight = 45;
            }

            for (int j = iBeg; j < 3; j++,i++)
            {
                Plot.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea(Title2[j]));
                Plot.ChartAreas[i].AxisX.Title = "T, C";
                Plot.ChartAreas[i].AxisY.Title = Title2[j];
                Plot.ChartAreas[i].AxisX.TitleAlignment = StringAlignment.Far;
                Plot.ChartAreas[i].AxisY.TitleAlignment = StringAlignment.Far;
                Plot.ChartAreas[i].AxisY.LineWidth = 3;
                Plot.ChartAreas[i].AxisX.LineWidth = 3;


                Plot.ChartAreas[i].Position.Auto = false;
                Plot.ChartAreas[i].Position.Width = 90;
                Plot.ChartAreas[i].Position.Height = iHeight; 

                Plot.ChartAreas[i].Position.X = 0;
                Plot.ChartAreas[i].Position.Y = 5+ iHeight * i;

                Plot.ChartAreas[i].AxisX.LabelStyle.Format = "F2";
                
            }
            i = 0;
            for (int j = iBeg; j < 3; j++,i++)
            {
                Plot.ChartAreas[i].AlignWithChartArea = Title2[2];
            }            
        }


        public void Add(double[] x, double[] y1, double[] y2, double[] y3)
        {
            string legend = "";

            Color cl = Color.Red;

            int CountPlot = 3;
            if(y2 == null) CountPlot--;

            if ((Plot.Series.Count / CountPlot) == 0) cl = Color.Red;
            if ((Plot.Series.Count / CountPlot) == 1) cl = Color.Blue;
            if ((Plot.Series.Count / CountPlot) == 2) cl = Color.Green;
            if ((Plot.Series.Count / CountPlot) == 3) cl = Color.Snow;
            if ((Plot.Series.Count / CountPlot) == 4) cl = Color.Yellow;
            if ((Plot.Series.Count / CountPlot) == 5) cl = Color.Brown;
            if ((Plot.Series.Count / CountPlot) == 6) cl = Color.Pink;
            if ((Plot.Series.Count / CountPlot) == 7) cl = Color.OliveDrab;

            if (y2 != null)
            {
                System.Windows.Forms.DataVisualization.Charting.Series mySeriesOfPoint1 = new System.Windows.Forms.DataVisualization.Charting.Series(legend);
                mySeriesOfPoint1.BorderWidth = 3;
                mySeriesOfPoint1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                mySeriesOfPoint1.ChartArea = (Title2[0]);

                mySeriesOfPoint1.Color = cl;

                for (int i = 0; i < x.Length; i++)
                    mySeriesOfPoint1.Points.AddXY(x[i], y2[i]);
                Plot.Series.Add(mySeriesOfPoint1);
            }

            System.Windows.Forms.DataVisualization.Charting.Series mySeriesOfPoint2 = new System.Windows.Forms.DataVisualization.Charting.Series(legend);
            mySeriesOfPoint2.BorderWidth = 3;
            mySeriesOfPoint2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            mySeriesOfPoint2.ChartArea = (Title2[1]);
            mySeriesOfPoint2.Color = cl;


            for (int i = 0; i < x.Length; i++)
                mySeriesOfPoint2.Points.AddXY(x[i], y1[i]);
            Plot.Series.Add(mySeriesOfPoint2);
        
            System.Windows.Forms.DataVisualization.Charting.Series mySeriesOfPoint3 = new System.Windows.Forms.DataVisualization.Charting.Series(legend);
            mySeriesOfPoint3.BorderWidth = 3;
            mySeriesOfPoint3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            mySeriesOfPoint3.ChartArea = (Title2[2]);
            mySeriesOfPoint3.Color = cl;
            for (int i = 0; i < x.Length; i++)
                mySeriesOfPoint3.Points.AddXY(x[i], y3[i]);
            Plot.Series.Add(mySeriesOfPoint3);
        }

    }


}
