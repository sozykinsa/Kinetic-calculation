using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Xml;

using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Annotations;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing.Imaging;
using System.IO;
using System.Globalization;

namespace Termo
{
    public partial class TermoMainForm : Form
    {
        Model TermoModel;
        TermoSettings PgmConfig;

        double TGXmin, TGXmax, TGPixelmin, TGPixelmax, X0;


        public TermoMainForm()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.RowIndex < 0) || (e.ColumnIndex < 0)) return;

            for (int i = 0; i < e.RowIndex; i++)
            {
                if (grInitData.Rows[i].Cells[e.ColumnIndex].Value == null) return;
                if (grInitData.Rows[i].Cells[e.ColumnIndex].Value.ToString().Length < 1) return;
            }

            if(openInputFileDialog.ShowDialog() != DialogResult.OK) return;
            
            if (e.RowIndex == grInitData.Rows.Count - 1)
                grInitData.Rows.Add();
            if (e.ColumnIndex == grInitData.Columns.Count - 1)
                grInitData.Columns.Add("col"+ grInitData.Columns.Count.ToString(),"... K/min");

            DataGridViewTextBoxCell txtxCell = (DataGridViewTextBoxCell)grInitData.Rows[e.RowIndex].Cells[e.ColumnIndex];

                txtxCell.Value =  openInputFileDialog.FileName;
                

            LoadData();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PgmConfig.GetDataGridInfo(grInitData, label5, label6);  // сохранили Settings
            
            tabControl1.SelectedIndex++;
        }



        double[,] IntScale = new double[3, 4];
        bool isLeftReperMoove = false;
        bool isRightReperMoove = false;


        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            double Pos = 0;
            if (!isLeftReperMoove) return;

            if ((panel2.Location.X > panel3.Location.X - 10)&&(e.Location.X - (int)X0 > 0))  return;

            if ((panel2.Location.X > TGPixelmax - panel2.Width) && (e.Location.X - (int)X0 > 0)) return;
            if ((panel2.Location.X < TGPixelmin + panel2.Width) && (e.Location.X - (int)X0 < 0)) return;


            panel2.Location = new Point(panel2.Location.X + e.Location.X - (int)X0, panel2.Location.Y);
            double k = (TGXmax - TGXmin) / (TGPixelmax - TGPixelmin);
            Pos = k * (panel2.Location.X + panel2.Width / 2 - TGPixelmin) + TGXmin;
            label5.Text = Math.Round(Pos, 2).ToString();
        }


        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            double Pos = 0;
            if (!isRightReperMoove) return;

            if ((panel2.Location.X > panel3.Location.X - 10) && (e.Location.X - (int)X0 < 0))  return;

            if ((panel3.Location.X > TGPixelmax - panel3.Width) && (e.Location.X - (int)X0 > 0))
                return;
            if((panel3.Location.X < TGPixelmin - panel3.Width) && (e.Location.X - (int)X0 < 0))  return;

            panel3.Location = new Point(panel3.Location.X + e.Location.X - (int)X0, panel3.Location.Y);
            double k = (TGXmax - TGXmin) / (TGPixelmax - TGPixelmin);
            Pos = k * (panel3.Location.X + panel3.Width / 2 - TGPixelmin) + TGXmin;
            label6.Text = Math.Round(Pos, 2).ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            double startT = Convert.ToDouble(label5.Text);
            double endT = Convert.ToDouble(label6.Text); 

            TermoModel.Calc(startT, endT);

            labelVyazovkinE0.Text = TermoModel.GetVyazovkinE0();
            labelVyazovkinA.Text = TermoModel.GetVyazovkinA();
            labelFridman.Text = TermoModel.GetFridmanE0();
            labelModel_n.Text = TermoModel.Getn();
            labelModel_m.Text = TermoModel.Getm();
            labelModel_p.Text = TermoModel.Getp();
            labelErrorModel.Text = TermoModel.GetError();
            labelOzawaE0.Text = TermoModel.GetOzawaE0();

            TermoModel.plotEvsAl();
            TermoModel.plotDAlvsAl();
        }



        private void LoadData()
        {
            if (TermoModel != null)
                TermoModel.Close();

            List<List<string>> Paths = new List<List<string>>();
            int c = 0;
            while (grInitData.Rows[0].Cells[c].FormattedValue.ToString().Length > 0)
            {
                List<string> SpeedFiles = new List<string>();
                int r = 0;
                while (grInitData.Rows[r].Cells[c].FormattedValue.ToString().Length > 0)
                {
                    SpeedFiles.Add(grInitData.Rows[r].Cells[c].FormattedValue.ToString());
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

            List<string> Titles = new List<string>();
            Titles.Add("Mass");
            Titles.Add("DTA");
            Titles.Add("DTG");

            ViewSignal ViewTG = new ViewSignal(plot1chart, Titles,1);

            ViewSignal ViewE = new ViewSignal(chartActivationEnergy, "Activation Energy");
            ViewSignal ViewAlpha = new ViewSignal(chartNormalizedReactionModel, "Normalized Reaction Model");

            ViewSignal ViewEHidd = new ViewSignal(chart1hidden, "Activation Energy");
            ViewSignal ViewAlphaHidd = new ViewSignal(chart2hidden, "Normalized Reaction Model");

            TermoModel = new Model(Paths, keys, ViewTG, ViewE, ViewAlpha, ViewEHidd, ViewAlphaHidd);

            List<double> Betta = TermoModel.GetBettaGrid();
            for(int i=0;i<Betta.Count;i++)
                grInitData.Columns[i].HeaderText = Math.Round(Betta[i], 2).ToString() + "\t K/min";

            TermoModel.plotTG_DTG_DTF();

            Application.DoEvents();
        }
        
        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            grInitData.Rows.Clear();
            grInitData.Columns.Clear();
            grInitData.Columns.Add("col0", "... K/min");
    
        }

        private void TermoMainForm_Load(object sender, EventArgs e)
        {
            PgmConfig = new TermoSettings();  //загрузили Settings
#if Fredman
            labelFridman.Visible = true; label4.Visible = true;
#else
            labelFridman.Visible = false; label4.Visible = false;
#endif
            PgmConfig.LoadCursorSettings(label5, label6);  //загрузили курсоры

            if (PgmConfig.SetDataGridInfo(grInitData))      //загрузили в DataGrid файлы
            {
                try
                {
                    LoadData();
                }
                catch (Exception ex)
                {
                    PgmConfig.ClearDataGrid(grInitData);
                }
            }
            tabPage2.Parent = null;
        }

        private void TermoMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            PgmConfig.GetDataGridInfo(grInitData, label5, label6);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chart1hidden.Series.Clear();
            chart1hidden.ChartAreas.Clear();
            chart1hidden.ChartAreas.Add(new ChartArea("Math functions"));

            //Создаем и настраиваем набор точек для рисования графика, в том
            //не забыв указать имя области на которой хотим отобразить этот
            //набор точек.
            System.Windows.Forms.DataVisualization.Charting.Series mySeriesOfPoint = new System.Windows.Forms.DataVisualization.Charting.Series("Sinus");
            mySeriesOfPoint.ChartType = SeriesChartType.Line;
            mySeriesOfPoint.ChartArea = "Math functions";
            for (double x = -Math.PI; x <= Math.PI; x += Math.PI / 10.0)
            {
                mySeriesOfPoint.Points.AddXY(x, Math.Sin(x));
            }
            //Добавляем созданный набор точек в Chart
            chart1hidden.Series.Add(mySeriesOfPoint);
        }

        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            isLeftReperMoove = false;
            isRightReperMoove = true;
            X0 = e.X;
        }

        private void panel3_MouseHover(object sender, EventArgs e)
        {
            isRightReperMoove = false;
        }

        private void panel2_MouseHover(object sender, EventArgs e)
        {
            isLeftReperMoove = false;
        }

        void BigFigureSettings(Chart chart1)
        {
            int lineWidth = 10;
            int fontSize = 80;
            chart1.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font(this.Font.FontFamily, fontSize, FontStyle.Regular);
            chart1.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font(this.Font.FontFamily, fontSize, FontStyle.Regular);

            chart1.ChartAreas[0].AxisX.LabelStyle.Font = new System.Drawing.Font(this.Font.FontFamily, fontSize, FontStyle.Regular);
            chart1.ChartAreas[0].AxisY.LabelStyle.Font = new System.Drawing.Font(this.Font.FontFamily, fontSize, FontStyle.Regular);

            chart1.Legends[0].Font = new System.Drawing.Font(this.Font.FontFamily, fontSize, FontStyle.Regular);
            chart1.ChartAreas[0].AxisX.LineWidth = lineWidth;
            chart1.ChartAreas[0].AxisY.LineWidth = lineWidth;

            chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = lineWidth / 2;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = lineWidth / 2;

            chart1.Series[0].Font = new System.Drawing.Font(this.Font.FontFamily, fontSize, FontStyle.Regular);
            for(int i=0;i<chart1.Series.Count;i++)
               chart1.Series[i].BorderWidth = lineWidth;
                
            chart1.Legends[0].Position.Auto = true;
            chart1.Legends[0].LegendStyle = LegendStyle.Row;
            chart1.Legends[0].Alignment = StringAlignment.Center;
            chart1.Legends[0].Docking = Docking.Top;
            chart1.Legends[0].Font = new System.Drawing.Font(this.Font.FontFamily, fontSize-5, FontStyle.Regular);
        }

        private void exportGraphToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (saveFigureDialog.ShowDialog() != DialogResult.OK) return;

            BigFigureSettings(chart1hidden); 
            Bitmap bm = new Bitmap(chart1hidden.Size.Width, chart1hidden.Size.Height, PixelFormat.Format32bppArgb);
            bm.SetResolution(600, 600);
            chart1hidden.DrawToBitmap(bm, new Rectangle(0, 0, chart1hidden.Size.Width, chart1hidden.Size.Height));
            bm.Save(saveFigureDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void exportGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFigureDialog.ShowDialog() != DialogResult.OK) return;

            BigFigureSettings(chart2hidden);
            Bitmap bm = new Bitmap(chart2hidden.Size.Width, chart2hidden.Size.Height, PixelFormat.Format32bppArgb);
            bm.SetResolution(600, 600);
            chart2hidden.DrawToBitmap(bm, new Rectangle(0, 0, chart2hidden.Size.Width, chart2hidden.Size.Height));
            bm.Save(saveFigureDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void exportDataToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TermoModel.SaveDataAlpha(saveDataDialog);
        }

        private void exportDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TermoModel.SaveDataE(saveDataDialog);
        }

        private void plot1chart_Paint(object sender, PaintEventArgs e)
        {
            TGXmin = TermoModel.TminPlot(); 
            TGXmax = TermoModel.TmaxPlot();
            TGPixelmin = plot1chart.ChartAreas[0].AxisX.ValueToPixelPosition(Convert.ToDouble(TGXmin));
            TGPixelmax = plot1chart.ChartAreas[0].AxisX.ValueToPixelPosition(Convert.ToDouble(TGXmax));

            int PosX1 = (int)plot1chart.ChartAreas[0].AxisX.ValueToPixelPosition(Convert.ToDouble(label5.Text));
            panel2.Location = new Point(PosX1, panel2.Location.Y);

            int PosX2 = (int)plot1chart.ChartAreas[0].AxisX.ValueToPixelPosition(Convert.ToDouble(label6.Text));
            panel3.Location = new Point(PosX2, panel3.Location.Y);
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            isLeftReperMoove = true;
            isRightReperMoove = false;
            X0 = e.X;
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            isLeftReperMoove = false;
        }
    }
}

