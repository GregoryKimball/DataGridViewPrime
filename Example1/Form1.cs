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
using IODataTableNamespace;


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

            IODataTable iodt = new IODataTable();
            DataTable d = iodt.LoadCSVtoDataTable(s);


            //dataGridViewPrime1.userInputMode = UserInputMode.Readonly;
            dataGridViewPrime1.SetDataSource(d);
        }
    }
}
