using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
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

        public ViewSignal(Chart _Plot, List<string> _Title, int k)  
        {
            Plot = _Plot;
            Title2 = _Title;
             
            Plot.Series.Clear();
            Plot.ChartAreas.Clear();
            Plot.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea(Title2[0]));
            Plot.ChartAreas[0].AxisY.LineWidth = 3;
            Plot.ChartAreas[0].AxisX.LineWidth = 3;

            Plot.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea(Title2[1]));
            Plot.ChartAreas[1].AxisY.LineWidth = 3;
            Plot.ChartAreas[1].AxisX.LineWidth = 3;
        
            Plot.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea(Title2[2]));
            Plot.ChartAreas[2].AxisY.LineWidth = 3;
            Plot.ChartAreas[2].AxisX.LineWidth = 3;

            for (int i = 0; i < 3; i++)
            {
                Plot.ChartAreas[i].Position.Auto = false;
                Plot.ChartAreas[i].Position.Width = 90;
                Plot.ChartAreas[i].Position.Height = 30;

                Plot.ChartAreas[i].AlignWithChartArea = Title2[2];

                Plot.ChartAreas[i].Position.X = 0;
                Plot.ChartAreas[i].Position.Y = 5+30*i;

                Plot.ChartAreas[i].AxisX.LabelStyle.Format = "F2";
            }
            
        }


        public void Add(double[] x, double[] y1, double[] y2, double[] y3)
        {
            string legend = "";

            System.Windows.Forms.DataVisualization.Charting.Series mySeriesOfPoint1 = new System.Windows.Forms.DataVisualization.Charting.Series(legend);
            mySeriesOfPoint1.BorderWidth = 3;
            mySeriesOfPoint1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            mySeriesOfPoint1.ChartArea = (Title2[0]);
            for (int i = 0; i < x.Length; i++)
                mySeriesOfPoint1.Points.AddXY(x[i], y2[i]);
            Plot.Series.Add(mySeriesOfPoint1);

            System.Windows.Forms.DataVisualization.Charting.Series mySeriesOfPoint2 = new System.Windows.Forms.DataVisualization.Charting.Series(legend);
            mySeriesOfPoint2.BorderWidth = 3;
            mySeriesOfPoint2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            mySeriesOfPoint2.ChartArea = (Title2[1]);
            for (int i = 0; i < x.Length; i++)
                mySeriesOfPoint2.Points.AddXY(x[i], y1[i]);
            Plot.Series.Add(mySeriesOfPoint2);
        
            System.Windows.Forms.DataVisualization.Charting.Series mySeriesOfPoint3 = new System.Windows.Forms.DataVisualization.Charting.Series(legend);
            mySeriesOfPoint3.BorderWidth = 3;
            mySeriesOfPoint3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            mySeriesOfPoint3.ChartArea = (Title2[2]);
            for (int i = 0; i < x.Length; i++)
                mySeriesOfPoint3.Points.AddXY(x[i], y3[i]);
            Plot.Series.Add(mySeriesOfPoint3);
        }

    }


}
