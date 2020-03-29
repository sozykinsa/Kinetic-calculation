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
        int[] DataIndex = new int[4];

        double TemperatureRangeLeft;
        double TemperatureRangeRight;

        public TermoMainForm()
        {
            InitializeComponent();
            comboBox3.Text=ConfigurationManager.AppSettings["ColumnTime"];
            comboBox1.Text = ConfigurationManager.AppSettings["ColumnTemperature"];
            comboBox2.Text = ConfigurationManager.AppSettings["ColumnMass"];
            comboBox4.Text = ConfigurationManager.AppSettings["ColumnDSC"];

            string strAlpha_Min = ConfigurationManager.AppSettings["Alpha_Min"];
            string strAlpha_Max = ConfigurationManager.AppSettings["Alpha_Max"];
            double Tmp;

            if (!Double.TryParse(ConfigurationManager.AppSettings["Alpha_Min"], out Tmp))
            {
                if (strAlpha_Min.Contains("."))
                {
                    strAlpha_Min = strAlpha_Min.Replace(".", ",");
                    strAlpha_Max = strAlpha_Max.Replace(".", ",");
                } else
                {
                    strAlpha_Min = strAlpha_Min.Replace(",", ".");
                    strAlpha_Max = strAlpha_Max.Replace(",", ".");
                }

                var configfile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var setting = configfile.AppSettings.Settings;

                setting["Alpha_Min"].Value = strAlpha_Min;
                setting["Alpha_Max"].Value = strAlpha_Max;

                configfile.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }

            textBox_Alpha_min.Text = strAlpha_Min;
            textBox_Alpha_max.Text = strAlpha_Max;            
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
            PgmConfig.GetDataGridInfo(grInitData);  // сохранили Settings
            
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
            Pos = plot1chart.ChartAreas[0].AxisX.PixelPositionToValue(panel2.Location.X);
            //double k = (TGXmax - TGXmin) / (TGPixelmax - TGPixelmin);
            //Pos = k * (panel2.Location.X + panel2.Width / 2 - TGPixelmin) + TGXmin;
            lTemperatureRangeLeft.Text = Math.Round(Pos, 2).ToString();
            TemperatureRangeLeft = Pos;
            //save_temperature_range();
        }


        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            double Pos = 0;
            if (!isRightReperMoove) return;

            if ((panel2.Location.X > panel3.Location.X - 10) && (e.Location.X - (int)X0 < 0))  return;
            if ((panel3.Location.X > TGPixelmax - panel3.Width) && (e.Location.X - (int)X0 > 0)) return;
            if ((panel3.Location.X < TGPixelmin - panel3.Width) && (e.Location.X - (int)X0 < 0))  return;

            panel3.Location = new Point(panel3.Location.X + e.Location.X - (int)X0, panel3.Location.Y);
            Pos = plot1chart.ChartAreas[0].AxisX.PixelPositionToValue(panel3.Location.X);
            //double k = (TGXmax - TGXmin) / (TGPixelmax - TGPixelmin);
            //Pos = k * (panel3.Location.X + panel3.Width / 2 - TGPixelmin) + TGXmin;
            lTemperatureRangeRight.Text = Math.Round(Pos, 2).ToString();
            TemperatureRangeRight = Pos;
            //save_temperature_range();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            double startT = TemperatureRangeLeft;
            double endT = TemperatureRangeRight;

            if (TermoModel == null)
            {
                MessageBox.Show("You need to add data for thermal analysis", "No initial data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                tabControl1.SelectedIndex=0;
                return;
            }

            int iError = TermoModel.Calc(startT, endT);
            if (iError == -1)
            {
                MessageBox.Show("For calculation, you need to more than one heating rate", "One heating rate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                tabControl1.SelectedIndex=0;
                return;
            }

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

            DataIndex[0] = Convert.ToInt32(ConfigurationManager.AppSettings["ColumnTemperature"]) - 1;
            DataIndex[1] = Convert.ToInt32(ConfigurationManager.AppSettings["ColumnTime"]) - 1;             // Задаем формат исходных файлов - порядок столбцов
            DataIndex[2] = Convert.ToInt32(ConfigurationManager.AppSettings["ColumnMass"]) - 1;
            Int32.TryParse(ConfigurationManager.AppSettings["ColumnDSC"], out DataIndex[3]);
            DataIndex[3]--;

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
            if (c == 0) return;

            Dictionary<string, int> keys = new Dictionary<string, int>();
            keys.Add("T", 0);
            keys.Add("t", 1);
            keys.Add("Mass", 2);
            keys.Add("DSC", 3);

            List<string> Titles = new List<string>();
            Titles.Add("DTA");
            Titles.Add("TG");            
            Titles.Add("DTG");

            ViewSignal ViewTG = new ViewSignal(plot1chart, Titles, DataIndex);
            ViewSignal ViewE = new ViewSignal(chartActivationEnergy, "E");
            ViewSignal ViewAlpha = new ViewSignal(chartNormalizedReactionModel, "d_alpha/dt");
            ViewSignal ViewEHidd = new ViewSignal(chart1hidden, "E");
            ViewSignal ViewAlphaHidd = new ViewSignal(chart2hidden, "d_alpha/dt");
            
            TermoModel = new Model(Paths, keys, ViewTG, ViewE, ViewAlpha, ViewEHidd, ViewAlphaHidd, DataIndex);

            List<double> Betta = TermoModel.GetBettaGrid();
            for(int i=0;i<Betta.Count;i++)
                grInitData.Columns[i].HeaderText = Math.Round(Betta[i], 2).ToString() + "\t K/min";

            TermoModel.plotTG_DTG_DTF(DataIndex);
            TemperatureRangeLeft = TermoModel.TminPlot();
            TemperatureRangeRight = TermoModel.TmaxPlot();
            lTemperatureRangeLeft.Text = TermoModel.TminPlot().ToString();
            lTemperatureRangeRight.Text = TermoModel.TmaxPlot().ToString();
            bNext.Enabled = true;
            if (tabPage3.Parent != tabControl1)
                tabControl1.TabPages.Insert(1, tabPage3);
            Application.DoEvents();            
        }
        
        public void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            grInitData.Rows.Clear();
            grInitData.Columns.Clear();
            grInitData.Columns.Add("col0", "... K/min");
            if (TermoModel != null) TermoModel.Close();
            TermoModel = null;
            bNext.Enabled = false;
            tabPage3.Parent = null;
        }

        private void TermoMainForm_Load(object sender, EventArgs e)
        {
            tabPage2.Parent = null;
            tabPage3.Parent = null;
            PgmConfig = new TermoSettings();  //загрузили Settings
            if (PgmConfig.SetDataGridInfo(grInitData))
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

        }

        private void TermoMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            PgmConfig.GetDataGridInfo(grInitData);
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

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearAllToolStripMenuItem_Click(sender, e);
            string[] str = { comboBox3.Text, comboBox1.Text, comboBox2.Text, comboBox4.Text };

            for (int i = 0; i < str.Length; i++)
                if (str[i] == string.Empty) return;
            

            for (int i=0; i<str.Length-1;i++)
                for (int j=i+1;j< str.Length;j++)
                {
                    if (str[i] != str[j] ) continue;
                    System.Windows.Forms.MessageBox.Show("Column numbers must not match!","Warning",MessageBoxButtons.OK);

                    comboBox3.Text = ConfigurationManager.AppSettings["ColumnTime"];
                    comboBox1.Text = ConfigurationManager.AppSettings["ColumnTemperature"];
                    comboBox2.Text = ConfigurationManager.AppSettings["ColumnMass"];
                    comboBox4.Text = ConfigurationManager.AppSettings["ColumnDSC"];
                    return;
                }
            
            var configfile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var setting = configfile.AppSettings.Settings;

            setting["ColumnTime"].Value = comboBox3.Text;
            setting["ColumnTemperature"].Value = comboBox1.Text;
            setting["ColumnMass"].Value = comboBox2.Text;
            setting["ColumnDSC"].Value = comboBox4.Text;
            configfile.Save();
            
            ConfigurationManager.RefreshSection("appSettings");
            LoadData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double Alpha_Min, Alpha_Max;

            if (Double.TryParse(textBox_Alpha_min.Text, out Alpha_Min) && Double.TryParse(textBox_Alpha_max.Text, out Alpha_Max))
            {
              if (Alpha_Min<0 || Alpha_Max>1)
                {
                    System.Windows.Forms.MessageBox.Show("Alpha value must be no less than zero and no more than one!", "Error", MessageBoxButtons.OK);
                    return;
                }
              if (Alpha_Min >= Alpha_Max)
                {
                    System.Windows.Forms.MessageBox.Show("The minimum alpha value must be strictly less than the maximum value!", "Error", MessageBoxButtons.OK);
                    return;
                }

              var configfile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
              var setting = configfile.AppSettings.Settings;
              setting["Alpha_Min"].Value = Alpha_Min.ToString();
              setting["Alpha_Max"].Value = Alpha_Max.ToString();
              configfile.Save();
              ConfigurationManager.RefreshSection("appSettings");
                System.Windows.Forms.MessageBox.Show("Alpha value saved successfully!", "Save", MessageBoxButtons.OK);
            } else System.Windows.Forms.MessageBox.Show("Alpha value is not correct!", "Warning", MessageBoxButtons.OK);
        }

        private void panel3_MouseUp(object sender, MouseEventArgs e)
        {
            isRightReperMoove = false;
        }

        private bool save_temperature_range()
        {
            double tmpTemperatureRangeLeft, tmpTemperatureRangeRight;
            string strTemperatureRangeLeft = lTemperatureRangeLeft.Text;
            string strTemperatureRangeRight = lTemperatureRangeRight.Text;

            if (!(Double.TryParse(strTemperatureRangeLeft, out tmpTemperatureRangeLeft) ))
            {
                if (strTemperatureRangeLeft.Contains("."))
                {
                    strTemperatureRangeLeft = strTemperatureRangeLeft.Replace(".", ",");
                }
                else
                {
                    strTemperatureRangeLeft = strTemperatureRangeLeft.Replace(",", ".");
                }
            }

            if (!(Double.TryParse(strTemperatureRangeRight, out tmpTemperatureRangeRight)))
            {
                if (strTemperatureRangeRight.Contains("."))
                {
                    strTemperatureRangeRight = strTemperatureRangeRight.Replace(".", ",");
                }
                else
                {
                    strTemperatureRangeRight = strTemperatureRangeRight.Replace(",", ".");
                }
            }

            if (Double.TryParse(strTemperatureRangeLeft, out tmpTemperatureRangeLeft) && Double.TryParse(strTemperatureRangeRight, out tmpTemperatureRangeRight))
            {
                TGXmin = TermoModel.TminPlot();
                TGXmax = TermoModel.TmaxPlot();
                if (tmpTemperatureRangeLeft < TGXmin || tmpTemperatureRangeRight > TGXmax)
                {
                    System.Windows.Forms.MessageBox.Show("Temperature value must be in experimental data range!", "Error", MessageBoxButtons.OK);
                    return false;
                }
                if (tmpTemperatureRangeRight <= tmpTemperatureRangeLeft)
                {
                    System.Windows.Forms.MessageBox.Show("The minimum temperature value must be strictly less than the maximum value!", "Error", MessageBoxButtons.OK);
                    double k = (TGXmax - TGXmin) / (TGPixelmax - TGPixelmin);
                    lTemperatureRangeLeft.Text = (plot1chart.ChartAreas[0].AxisX.PixelPositionToValue(panel2.Location.X)).ToString();
                    lTemperatureRangeRight.Text = (plot1chart.ChartAreas[0].AxisX.PixelPositionToValue(panel3.Location.X)).ToString();
                    return false;
                }
                else
                {
                    double k = (TGXmax - TGXmin) / (TGPixelmax - TGPixelmin);
                    panel2.Location = new Point(Convert.ToInt32((Convert.ToDouble(strTemperatureRangeLeft) - TGXmin) / k - panel2.Width / 2 + TGPixelmin), panel2.Location.Y);
                    panel3.Location = new Point(Convert.ToInt32((Convert.ToDouble(strTemperatureRangeRight) - TGXmin) / k - panel3.Width / 2 + TGPixelmin), panel3.Location.Y);

                    TemperatureRangeLeft = tmpTemperatureRangeLeft;
                    TemperatureRangeRight = tmpTemperatureRangeRight;
                }

                lTemperatureRangeLeft.Text = strTemperatureRangeLeft;
                lTemperatureRangeRight.Text = strTemperatureRangeRight;
                return true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Temperature range  is not correct!", "Warning", MessageBoxButtons.OK);
                return false;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (save_temperature_range()) System.Windows.Forms.MessageBox.Show("Temperature range setted successfully!", "Set", MessageBoxButtons.OK);
        }

        private void exportDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TermoModel.SaveDataE(saveDataDialog);
        }

        private void plot1chart_Paint(object sender, PaintEventArgs e)
        {
            if (TermoModel != null)
            {
                TGXmin = TermoModel.TminPlot();
                TGXmax = TermoModel.TmaxPlot();
                TGPixelmin = plot1chart.ChartAreas[0].AxisX.ValueToPixelPosition(Convert.ToDouble(TGXmin));
                TGPixelmax = plot1chart.ChartAreas[0].AxisX.ValueToPixelPosition(Convert.ToDouble(TGXmax));
                String strj = TemperatureRangeLeft.ToString();
                int PosX1 = (int)plot1chart.ChartAreas[0].AxisX.ValueToPixelPosition(TemperatureRangeLeft);
                //panel2.Location = new Point((int)TGPixelmin, panel2.Location.Y);
                panel2.Location = new Point(PosX1, panel2.Location.Y);
                strj = TemperatureRangeRight.ToString();
                int PosX2 = (int)plot1chart.ChartAreas[0].AxisX.ValueToPixelPosition(TemperatureRangeRight);
                //panel3.Location = new Point((int)TGPixelmax, panel3.Location.Y);
                panel3.Location = new Point(PosX2, panel3.Location.Y);
            }
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

