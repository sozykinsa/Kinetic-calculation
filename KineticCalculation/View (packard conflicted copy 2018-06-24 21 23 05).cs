using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;
using OxyPlot.Series;

namespace Termo
{
    class View
    {
        private System.Windows.Forms.DataGridView DataView;

        public View(System.Windows.Forms.DataGridView _DataView)
        {
            DataView = _DataView;
        }

        public void ShowGraph()
        {
            var myModel = new PlotModel { Title = "Example 1" };
            myModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
            TermoMainForm.plot1.Model = myModel;
        }

        public void NumericResults(List<List<double>> NumRes)
        {

        }
    }
}
