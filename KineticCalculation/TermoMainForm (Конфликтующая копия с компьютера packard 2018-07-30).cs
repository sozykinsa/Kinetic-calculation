using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Annotations;

namespace Termo
{   
    public partial class TermoMainForm : Form
    {
        Model TermoModel;


        public TermoMainForm()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.RowIndex < 0)||(e.ColumnIndex<0))
                return;            

            openInputFileDialog.ShowDialog();
            if (openInputFileDialog.FileName != "")
            {
                if (e.RowIndex == dataGridView1.Rows.Count - 1)
                    dataGridView1.Rows.Add();
                if (e.ColumnIndex == dataGridView1.Columns.Count - 1)
                    dataGridView1.Columns.Add("col"+ dataGridView1.Columns.Count.ToString(),"... K/min");
                DataGridViewTextBoxCell txtxCell = (DataGridViewTextBoxCell)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                txtxCell.Value =  openInputFileDialog.FileName;;
            }

            LoadData();
        }


        private void button1_Click(object sender, EventArgs e)
        {
     //       int c = 0;
     //       while (dataGridView1.Rows[0].Cells[c].FormattedValue.ToString().Length>0)
     //       {
     //           int r = 0;
     //           while (dataGridView1.Rows[r].Cells[c].FormattedValue.ToString().Length > 0)
     //           {
     //               TermoController.ReadTg(dataGridView1.Rows[r].Cells[c].FormattedValue.ToString());
     //               r++;
     //           }
     //           c++;
     //       }
     //       TermoController.ViewTg();
        }

 

       // public static PlotModel SelectRange()
       // {
         //   var model = new PlotModel();
         //   model.Series.Add(new FunctionSeries(Math.Cos, 0, 40, 0.1));
         //
         //   var range = new RectangleAnnotation { Fill = OxyColors.SkyBlue.ChangeIntensity(2), MinimumX = 0, MaximumX = 0 };
         //   model.Annotations.Add(range);
         //
         //   double startx = double.NaN;
         //
         //   model.MouseDown += (s, e) =>
         //   {
         //       if (e.ChangedButton == OxyMouseButton.Left)
         //       {
         //           startx = range.InverseTransform(e.Position).X;
         //           range.MinimumX = startx;
         //           range.MaximumX = startx;
         //          // model.RefreshPlot(true);
         //           e.Handled = true;
         //       }
         //   };
         //   model.MouseMove += (s, e) =>
         //   {
         //       if (e.ChangedButton == OxyMouseButton.Left && !double.IsNaN(startx))
         //       {
         //           var x = range.InverseTransform(e.Position).X;
         //           range.MinimumX = Math.Min(x, startx);
         //           range.MaximumX = Math.Max(x, startx);
         //           range.Text = string.Format("∫ cos(x) dx =  {0:0.00}", Math.Sin(range.MaximumX) - Math.Sin(range.MinimumX));
         //           model.Subtitle = string.Format("Integrating from {0:0.00} to {1:0.00}", range.MinimumX, range.MaximumX);
         //           model.RefreshPlot(true);
         //           e.Handled = true;
         //       }
         //   };
         //
         //   model.MouseUp += (s, e) =>
         //   {
         //       startx = double.NaN;
         //   };
         //
         //   return model;
       // }


        private void button4_Click(object sender, EventArgs e)
        {
            LoadData();
            tabControl1.SelectedTab = tabPage3;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //TermoModel.plotTG();
            //TermoModel.plotDTG();
            //TermoModel.plotDTA();
        }


        private void plot1_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        void c_AxisChanged(object sender, EventArgs e)
        {
            if (((OxyPlot.Axes.LinearAxis)sender).IsHorizontal())
            {
                double xMin = ((OxyPlot.Axes.LinearAxis)sender).ActualMinimum;
                double xMax = ((OxyPlot.Axes.LinearAxis)sender).ActualMaximum;

                if (plot1.Model != null)
                {
                    plot1.Model.Axes[0].AbsoluteMaximum = xMax;
                    plot1.Model.Axes[0].AbsoluteMinimum = xMin;

                   // plot1.Model.Axes[0].MinimumRange = 0;
                   // plot1.Model.Axes[0].MaximumRange = (xMax - xMin) + 200;

                    plot1.Model.InvalidatePlot(true);
                    plot1.Refresh();

                    plot1.Model.Axes[0].AbsoluteMinimum = xMin - 100;
                    plot1.Model.Axes[0].AbsoluteMaximum = xMax + 100;
                }

                if (plot2.Model != null)
                {
                    plot2.Model.Axes[0].AbsoluteMaximum = xMax;
                    plot2.Model.Axes[0].AbsoluteMinimum = xMin;             
                    
                 //   plot2.Model.Axes[0].MinimumRange = 0;
                 //   plot2.Model.Axes[0].MaximumRange = (xMax-xMin)+200;

                    plot2.Model.InvalidatePlot(true);
                    plot2.Refresh();

                    plot2.Model.Axes[0].AbsoluteMinimum = xMin - 100;
                    plot2.Model.Axes[0].AbsoluteMaximum = xMax + 100;
                    
                }

                if (plot3.Model != null)
                {
                    plot3.Model.Axes[0].AbsoluteMaximum = xMax;
                    plot3.Model.Axes[0].AbsoluteMinimum = xMin;

               //     plot3.Model.Axes[0].MinimumRange = 0;
              //      plot3.Model.Axes[0].MaximumRange = (xMax - xMin) + 200;

                    plot3.Model.InvalidatePlot(true);
                    plot3.Refresh();

                    plot3.Model.Axes[0].AbsoluteMinimum = xMin - 100;
                    plot3.Model.Axes[0].AbsoluteMaximum = xMax + 100;
                }

            }

            Application.DoEvents();

        }

        double[,] IntScale = new double[3, 4];
              

        private void button6_Click(object sender, EventArgs e)
        {
            plot1.Model.Series[0].PlotModel.Axes[0].AxisChanged += c_AxisChanged;
            plot2.Model.Series[0].PlotModel.Axes[0].AxisChanged += c_AxisChanged;
            plot3.Model.Series[0].PlotModel.Axes[0].AxisChanged += c_AxisChanged;


            IntScale[0, 0] = plot1.Model.Axes[0].ActualMinimum;
            IntScale[0, 1] = plot1.Model.Axes[0].ActualMaximum;

            IntScale[0, 2] = plot1.Model.Axes[1].ActualMinimum;
            IntScale[0, 3] = plot1.Model.Axes[1].ActualMaximum;

            IntScale[1, 0] = plot2.Model.Axes[0].ActualMinimum;
            IntScale[1, 1] = plot2.Model.Axes[0].ActualMaximum;

            IntScale[1, 2] = plot2.Model.Axes[1].ActualMinimum;
            IntScale[1, 3] = plot2.Model.Axes[1].ActualMaximum;

            IntScale[2, 0] = plot3.Model.Axes[0].ActualMinimum;
            IntScale[2, 1] = plot3.Model.Axes[0].ActualMaximum;

            IntScale[2, 2] = plot3.Model.Axes[1].ActualMinimum;
            IntScale[2, 3] = plot3.Model.Axes[1].ActualMaximum;            

            if (plot1.Model != null)
            {
                plot1.Model.Axes[0].MaximumRange = (plot1.Model.Axes[0].ActualMaximum - plot1.Model.Axes[0].ActualMinimum);
            }

            if (plot2.Model != null)
            {
                plot2.Model.Axes[0].MaximumRange = (plot2.Model.Axes[0].ActualMaximum - plot2.Model.Axes[0].ActualMinimum);
            }

            if (plot3.Model != null)
            {
                plot3.Model.Axes[0].MaximumRange = (plot3.Model.Axes[0].ActualMaximum - plot3.Model.Axes[0].ActualMinimum);
            }
        }

        bool isLeftReperMoove = false;
        bool isRightReperMoove = false;

        private void panel2_DoubleClick(object sender, EventArgs e)
        {
            isLeftReperMoove = !isLeftReperMoove;
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            double Pos = 0;
            if (isLeftReperMoove)
            {
                panel2.Location = new Point(panel2.Location.X + e.Location.X, panel2.Location.Y);
                Pos = plot1.Model.Axes[0].InverseTransform(panel2.Location.X + panel2.Width / 2);
                label5.Text = Math.Round(Pos, 2).ToString();
            }
        }

        private void panel3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            isRightReperMoove = !isRightReperMoove;
        }

        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            double Pos = 0;
            if (isRightReperMoove)
            {
                panel3.Location = new Point(panel3.Location.X + e.Location.X, panel3.Location.Y);
                Pos = plot1.Model.Axes[0].InverseTransform(panel3.Location.X + panel3.Width / 2);
                label6.Text = Math.Round(Pos, 2).ToString();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            plot1.Model.Axes[0].AbsoluteMinimum = IntScale[0, 0];
            plot1.Model.Axes[0].AbsoluteMaximum = IntScale[0, 1];

            plot1.Model.Axes[1].AbsoluteMinimum = IntScale[0, 2];
            plot1.Model.Axes[1].AbsoluteMaximum = IntScale[0, 3];

            plot1.Model.InvalidatePlot(true);
            plot1.Refresh();

            plot2.Model.Axes[0].AbsoluteMinimum = IntScale[1, 0];
            plot2.Model.Axes[0].AbsoluteMaximum = IntScale[1, 1];

            plot2.Model.Axes[1].AbsoluteMinimum = IntScale[1, 2];
            plot2.Model.Axes[1].AbsoluteMaximum = IntScale[1, 3];

            plot2.Model.InvalidatePlot(true);
            plot2.Refresh();

            plot3.Model.Axes[0].AbsoluteMinimum = IntScale[2, 0];
            plot3.Model.Axes[0].AbsoluteMaximum = IntScale[2, 1];

            plot3.Model.Axes[1].AbsoluteMinimum = IntScale[2, 2];
            plot3.Model.Axes[1].AbsoluteMaximum = IntScale[2, 3];

            plot3.Model.InvalidatePlot(true);
            plot3.Refresh();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            double startT = Convert.ToDouble(label5.Text); //plot1.Model.Axes[0].InverseTransform(panel2.Location.X + panel2.Width / 2);
            double endT = Convert.ToDouble(label6.Text); //plot1.Model.Axes[0].InverseTransform(panel3.Location.X + panel3.Width / 2);

            TermoModel.Calc(startT, endT);

            label8.Text = TermoModel.GetE0();
            label9.Text = TermoModel.GetA();
            label11.Text = TermoModel.Getn();
            label13.Text = TermoModel.Getm();
            label15.Text = TermoModel.Getp();
            labelError.Text = TermoModel.GetError();


            TermoModel.plotEvsAl();
            TermoModel.plotDAlvsAl();

            //List<List<double>> DAlvsAlcalc = TermoModel.GetDAlvsAlcalc();//Этот массив и
            //List<List<double>> DAlvsAlteor = TermoModel.GetDAlvsAlteor();//этот массив должны распологаться на одном графике, первый столбец это ось x


            //List<List<double>> EvsAl = TermoModel.GetEvsAl();               // Этот массив должен быть расположен на другом графике (Полученная энергия активации)




        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void TermoMainForm_Load(object sender, EventArgs e)
        {
            //tabPage2.Parent = null;
        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void LoadData()
        {
            if (TermoModel != null)
                TermoModel.Close();

            List<List<string>> Paths = new List<List<string>>();
            int c = 0;
            while (dataGridView1.Rows[0].Cells[c].FormattedValue.ToString().Length > 0)
            {
                List<string> SpeedFiles = new List<string>();
                int r = 0;
                while (dataGridView1.Rows[r].Cells[c].FormattedValue.ToString().Length > 0)
                {
                    SpeedFiles.Add(dataGridView1.Rows[r].Cells[c].FormattedValue.ToString());
                    r++;
                }
                Paths.Add(SpeedFiles);
                c++;
            }

            Dictionary<string, int> keys = new Dictionary<string, int>();
            keys.Add("T", 0);
            keys.Add("t", 1);
            keys.Add("DSC", 2);
            keys.Add("Mass", 3);

            ViewSignal ViewDTA = new ViewSignal(plot1, "DTA");
            ViewSignal ViewTG = new ViewSignal(plot2, "Mass");
            ViewSignal ViewDTG = new ViewSignal(plot3, "DTG");


            ViewSignal ViewE = new ViewSignal(plotView1, "E");
            ViewSignal ViewAlpha = new ViewSignal(plotView3, "Alpha");

            TermoModel = new Model(Paths, keys, ViewTG, ViewDTA, ViewDTG, ViewE, ViewAlpha);

            List<double> Betta = TermoModel.GetBetta();
            for(int i=0;i<Betta.Count;i++)
                dataGridView1.Columns[i].HeaderText = Math.Round(Betta[i], 2).ToString() + "\t K/min";

            var myModel = new PlotModel { Title = "Tg" };

            for (int ii = 0; ii < TermoModel.Count; ii++)
            {
                List<SimpleSignal> Data = TermoModel[ii];

                foreach (SimpleSignal SS in Data)
                {
                    LineSeries lineSeries1 = new LineSeries();
                    for (int i = 0; i < SS.GetSignal("T").Length; i++)
                    {
                        lineSeries1.Points.Add(new DataPoint(SS.GetSignal("T")[i], SS.GetSignal("Mass")[i]));

                    }
                    myModel.Series.Add(lineSeries1);
                }

            }
            this.plotView2.Model = myModel;            

            TermoModel.plotTG();
            TermoModel.plotDTG();
            TermoModel.plotDTA();
        
        }
        

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            LoadData();
        }
    }
}
