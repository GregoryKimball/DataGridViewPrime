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
        string dataPath = @"C: \Users\Gregory\Desktop\table1.csv";
        string amendmentsPath = @"C:\Users\Gregory\Desktop\Book2.csv";


        public Form1()
        {
            InitializeComponent();            

            //initializing datagridviewprime for amendments-style edits
            dataGridViewPrime1.CanOpenFiles = false;
            dataGridViewPrime1.SetModeAmendments(new List<string> { "dt" });
            dataGridViewPrime1.SaveReady += this.HandleAmendments;

        }


        private void HandleAmendments(object sender, EventArgs e)
        {
            DataTable dt = dataGridViewPrime1.GetSaveDataTable();

            IODataTable iodt = new IODataTable();
            iodt.SaveDataTabletoCSV(amendmentsPath, dt);
        }

        private void HandleFileSave(object sender, EventArgs e)
        {
            DataTable dt = dataGridViewPrime1.GetSaveDataTable();

            IODataTable iodt = new IODataTable();
            iodt.SaveDataTabletoCSV(dataPath, dt);
        }




        private void button1_Click(object sender, EventArgs e)
        {           
            
            IODataTable iodt = new IODataTable();
            DataTable d = iodt.LoadCSVtoDataTable(dataPath);
            DataTable a = iodt.LoadCSVtoDataTable(amendmentsPath);
            
            dataGridViewPrime1.SetDataSource(d,a);
        }
    }
}
