using ProcessMining.Controls;
using ProcessMining.DataClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace ProcessMining
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Excel files (*.xls)|*.xls|Excel files (*.xlsx)|*.xlsx";
            openFileDialog1.FileName = "";
            DialogResult selected = openFileDialog1.ShowDialog();
            if (selected == DialogResult.OK)
            {
                var file = openFileDialog1.FileName;
                txtFilePath.Text = file;
            }

        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            Loader.Show();
            if (!string.IsNullOrEmpty(txtFilePath.Text))
            {
                Algo Algo = new Algo(txtFilePath.Text);
                Algo.Process();

                lblW.Text = Algo.AllProcess.GetText(Algo.Occurences.ToList());
                lblT_W.Text = Algo.T_W.GetText();
                lblT_I.Text = Algo.T_I.GetText();
                lblT_O.Text = Algo.T_O.GetText();
                lblX_W.Text = Algo.X_W.GetText();
                lblY_W.Text = Algo.Y_W.GetText();
                lblP_W.Text = Algo.GetP_W();
                lblF_W.Text = Algo.GetF_W();
                gridEvents.DataSource = Algo.GetEventsReport();
                gridTransition.DataSource = Algo.GetTransitionReport();
                gridTransition.Columns["TotalTime"].Visible = false;
                gridTransition.Columns["Occurance"].Visible = false;
            }
            else
            {
                MessageBox.Show("Please provide excel file");
            }
            Loader.Hide();
        }

        
    }
}
