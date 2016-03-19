using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataGridViewPrimeNamespace;


namespace Example1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = @"C: \Users\Gregory\Desktop\table1.csv";
            dataGridViewPrime1.SetDataSource(IODataTable.LoadCSVtoDataTable(s));
        }
    }
}
