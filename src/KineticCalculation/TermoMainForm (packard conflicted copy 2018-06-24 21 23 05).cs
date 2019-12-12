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

namespace Termo
{   
    public partial class TermoMainForm : Form
    {
        private Controller TermoController;
        private View TermoView;

        private void TermoMainForm_Load(object sender, EventArgs e)
        {
            TermoView = new View(Results);
            TermoController = new Controller(TermoView);
        }

        public TermoMainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            openInputFileDialog.ShowDialog();
            if (openInputFileDialog.FileName != "")
            {
                if (e.RowIndex == dataGridView1.Rows.Count - 1)
                    dataGridView1.Rows.Add();
                DataGridViewTextBoxCell txtxCell = (DataGridViewTextBoxCell)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                txtxCell.Value =  openInputFileDialog.FileName;;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TermoController.Culc();
            TermoController.ShowNumericalResults();


        }


    }
}
