using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ZedGraph;
using RDotNet;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Dynamic;



namespace DataGridViewPrimeNamespace
{
   

    



    public class DataGridViewPrime : DataGridView
    {

        ZedGraphControlPrime zed;
        REngine engine;

        public event EventHandler SaveReady;
        protected DataTable myAmendments;
        public List<string> amendmentPrimaryKeys = new List<string> { };
        protected List<DataGridViewCell> editedCells = new List<DataGridViewCell> { };

        protected string LastDirectory;
        protected string FileName;
        


        #region front end constants

        protected static Color CellChanged = Color.LemonChiffon;
        protected static Color AmendmentError = Color.LightPink;
        protected bool displayGraph;
        protected bool ScrollToBottom = false;


        #endregion




        public bool CanOpenFiles = true;
        public bool CanSaveFiles = true;
        public enum UserEditMode { ReadOnly, SaveToFile, SaveToFileEvent, SaveAmendmentEvent};
        public UserEditMode userEditMode = UserEditMode.ReadOnly;

        


        public DataGridViewPrime() : base()
        {
            this.ReadOnly = true;
            this.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            displayGraph = false;

            zed = new ZedGraphControlPrime();
            zed.Location = new System.Drawing.Point(287, 106);
            zed.Name = "zedGraphControlPrime1";
            zed.ScrollGrace = 0D;
            zed.ScrollMaxX = 0D;
            zed.ScrollMaxY = 0D;
            zed.ScrollMaxY2 = 0D;
            zed.ScrollMinX = 0D;
            zed.ScrollMinY = 0D;
            zed.ScrollMinY2 = 0D;
            zed.Size = new System.Drawing.Size(674, 446);
            zed.TabIndex = 13;

            zed.Visible = false;

        }


        public void SetEngine(REngine engine_in)
        {
            engine = engine_in;

            this.zed.SetEngine(engine_in);

        }



        #region saving behavior

        public void SetModeAmendments(List<string> primaryKeys_in)
        {
            if (primaryKeys_in.Count < 1)
                return;

            amendmentPrimaryKeys = primaryKeys_in;

            myAmendments = new DataTable();
            myAmendments.Columns.Add("column_name");
            myAmendments.Columns.Add("new_value");


            for(int i=0; i<primaryKeys_in.Count; i++)
            {
                string s = primaryKeys_in[i];
                myAmendments.Columns.Add("column" + i.ToString());
                myAmendments.Columns.Add("value" + i.ToString());
            }


            userEditMode = UserEditMode.SaveAmendmentEvent;
            this.ReadOnly = false;
            this.CanSaveFiles = true;
        }
        public void SetModeSavesLocal()
        {
            amendmentPrimaryKeys = new List<string> { };
            userEditMode = UserEditMode.SaveToFile;
            this.ReadOnly = false;
            this.CanSaveFiles = true;
        }

        public void SetModeSaveEvent()
        {
            amendmentPrimaryKeys = new List<string> { };
            userEditMode = UserEditMode.SaveToFileEvent;
            this.ReadOnly = false;
            this.CanSaveFiles = true;
        }

        public void SetModeReadonly()
        {
            amendmentPrimaryKeys = new List<string> { };
            userEditMode = UserEditMode.ReadOnly;
            this.ReadOnly = true;
            this.CanSaveFiles = false;
        }

        public DataTable GetSaveDataTable()
        {
            switch (userEditMode)
            {
                case UserEditMode.SaveToFile:
                    return (DataTable)this.DataSource;
                case UserEditMode.SaveToFileEvent:
                    return (DataTable)this.DataSource;
                case UserEditMode.ReadOnly:
                    return new DataTable();
                case UserEditMode.SaveAmendmentEvent:
                    return myAmendments;
                default:
                    return new DataTable();
            }
        }

        private void SaveLocally()
        {
            if (this.DataSource == null)
                return;

            int changes = editedCells.Count;

            DialogResult mb = MessageBox.Show("Changes to " + changes.ToString() + " cell(s) have been registered for this data file. " + Environment.NewLine + "Would you like to save changes to: " + Environment.NewLine + this.FileName, "Save changes?", MessageBoxButtons.YesNoCancel);

            if (mb != DialogResult.Yes)
                return;


            IODataTable iodt = new IODataTable();
            iodt.SaveDataTabletoCSV(this.FileName, (DataTable)this.DataSource);

        }

        protected virtual void SaveLocally(EventArgs e)
        {

            if (this.DataSource == null)
                return;

            int changes = editedCells.Count;

            DialogResult mb = MessageBox.Show("Changes to " + changes.ToString() + " cell(s) have been registered for this data file. " + Environment.NewLine + "Would you like to save changes?", "Save changes?", MessageBoxButtons.YesNoCancel);

            if (mb != DialogResult.Yes)
                return;



            EventHandler handler = SaveReady;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void SaveAmendments(EventArgs e)
        {
            if (this.DataSource == null)
                return;

            int changes = editedCells.Count;

            DialogResult mb = MessageBox.Show("Changes to " + changes.ToString() + " cell(s) have been registered for this data file. " + Environment.NewLine + "Would you like to save amendments?", "Save changes?", MessageBoxButtons.YesNoCancel);

            if (mb != DialogResult.Yes)
                return;

            EventHandler handler = SaveReady;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        #endregion
                

        #region overloaded events

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                List<MenuItem> ls = GetMenuItems();
                ContextMenu = new System.Windows.Forms.ContextMenu();
                ContextMenu.MenuItems.AddRange(ls.ToArray());
                ContextMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
            }
        }

        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            base.OnCellEndEdit(e);


            this.CurrentCell.Style.BackColor = DataGridViewPrime.CellChanged;

            if (this.userEditMode == UserEditMode.SaveAmendmentEvent )
            {
                if (AmendmentKeysOk())
                    AddAmendment(this.CurrentCell);
                else                 
                    this.CurrentCell.Style.BackColor = DataGridViewPrime.AmendmentError;
            }
            else
                editedCells.Add(this.CurrentCell);


        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);

            if (displayGraph)
                SetGraph();


        }
                
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);


            if (e.Control && e.KeyCode == Keys.V)
            {
                string s = Clipboard.GetText();

                int c = this.CurrentCell.OwningColumn.Index;
                int r = this.CurrentCell.OwningRow.Index;
                int cell_data_count = s.Split(new char[2] { '\t', '\n' }).Length;

                int cols = s.Split('\n')[0].Split('\t').Length;
                int rows = s.Split('\n').Length;


                if (this.SelectedCells.Count == 1 && cell_data_count > 1)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            string t = s.Split(new char[2] { '\t', '\n' })[i * cols + j].Trim();
                            this.Rows[r + i].Cells[c + j].Value = t;
                            this.Rows[r + i].Cells[c + j].Style.BackColor = DataGridViewPrime.CellChanged;

                            if (userEditMode == UserEditMode.SaveAmendmentEvent)
                                AddAmendment(this.Rows[r + i].Cells[c + j]);
                            else
                                editedCells.Add(this.Rows[r + i].Cells[c + j]);
                        }
                    }
                }

                if (this.SelectedCells.Count > 1 && cell_data_count == 1)
                {
                    foreach (DataGridViewCell dc in this.SelectedCells)
                    {
                        dc.Value = s.Trim();
                        dc.Style.BackColor = DataGridViewPrime.CellChanged;

                        if (userEditMode == UserEditMode.SaveAmendmentEvent)
                            AddAmendment(dc);
                        else
                            editedCells.Add(dc);
                    }
                }


            }


        }
        
        #endregion
        

        #region context menu

        [STAThread]


        protected virtual List<MenuItem> GetMenuItems()
        {
            List<MenuItem> ls = new List<MenuItem> { };

            MenuItem mi;


            if (this.CanOpenFiles)
            {
                mi = new MenuItem("Open");
                mi.Click += new System.EventHandler(this.OpenMenuItem_Click);
                ls.Add(mi);
            }

            if (this.CanSaveFiles)
            {
                mi = new MenuItem("Save");
                mi.Click += new System.EventHandler(this.SaveMenuItem_Click);
                ls.Add(mi);
            }

            mi = new MenuItem("Save As");
            mi.Click += new System.EventHandler(this.SaveAsMenuItem_Click);
            ls.Add(mi);

            mi = new MenuItem("Copy");
            mi.Click += new System.EventHandler(this.CopyMenuItem_Click);
            ls.Add(mi);

            mi = new MenuItem("Explore");
            mi.Checked = displayGraph;
            mi.Click += new System.EventHandler(this.ExploreMenuItem_Click);
            ls.Add(mi);

            mi = new MenuItem("Clear");
            mi.Click += new System.EventHandler(this.ClearMenuItem_Click);
            ls.Add(mi);





            return ls;
        }


        private void SaveMenuItem_Click(object sender, EventArgs e)
        {

            if (this.DataSource == null)
                return;

            if (userEditMode == UserEditMode.SaveToFile)            
                SaveLocally();

            if (userEditMode == UserEditMode.SaveAmendmentEvent)        
                SaveAmendments(new EventArgs());

            if (userEditMode == UserEditMode.SaveToFileEvent)
                SaveLocally(new EventArgs());


            foreach (DataGridViewCell dc in editedCells)
            {
                
                dc.Style.BackColor = Color.White;
                
            }
            
        }



        private void OpenMenuItem_Click(object sender, EventArgs e)
        {

        



            OpenFileDialog of = new OpenFileDialog();
            of.InitialDirectory = this.LastDirectory;
            of.DefaultExt = ".csv";
            of.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

            of.ShowDialog();

            if (of.FileName == "")
                return;

            this.LastDirectory = Path.GetDirectoryName(of.FileName);
            this.FileName = of.FileName;
            SetModeSavesLocal();

            IODataTable iodt = new IODataTable();
            DataTable dt = iodt.LoadCSVtoDataTable(of.FileName);
            this.SetDataSource(dt);
        }

        private void SaveAsMenuItem_Click(object sender, EventArgs e)
        {

            if (this.DataSource == null)
                return;

            SaveFileDialog sf = new SaveFileDialog();
            sf.InitialDirectory = this.LastDirectory;
            sf.DefaultExt = ".csv";
            sf.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

            sf.ShowDialog();

            if (sf.FileName == "")
                return;

            this.LastDirectory = Path.GetDirectoryName(sf.FileName);



            IODataTable iodt = new IODataTable();
            
            iodt.SaveDataTabletoCSV(sf.FileName, (DataTable)this.DataSource);
        }



       



        private void CopyMenuItem_Click(object sender, EventArgs e)
        {
            if (this.DataSource == null)
                return;

            this.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;

        }

        private void ExploreMenuItem_Click(object sender, EventArgs e)
        {



            displayGraph = !displayGraph;

            if (!this.Parent.Controls.Contains(zed))
                this.Parent.Controls.Add(zed);


            if (displayGraph)
            {
                zed.BringToFront();
                zed.Visible = displayGraph;

                int borderX = this.Size.Width / 5;
                int borderY = this.Size.Height / 5;
                zed.Location = new System.Drawing.Point(this.Location.X + borderX * 3 / 4, this.Location.Y + borderY * 3 / 4);
                zed.Size = new System.Drawing.Size(this.Size.Width - borderX, this.Size.Height - borderY);

                SetGraph();

            }
            else
                this.BringToFront();









        }

        private void ClearMenuItem_Click(object sender, EventArgs e)
        {

            this.DataSource = null;
            displayGraph = false;
            this.BringToFront();
        }

        #endregion
        

        #region amendments

        /*

            Amendments are changes to a datatable.
            
            Each amendment is defined by a list of primary keys and when a datarow matches the primary keys, the value is changed.

            The purpose of amendments is to apply changes to a datatable every time it is loaded without editing the file.

        */

        public void SetAmendments(DataTable amendments_in)
        {
            myAmendments = amendments_in;
        }     

        protected bool AmendmentKeysOk()
        {
            DataTable dt = (DataTable)this.DataSource;
            foreach (string s in amendmentPrimaryKeys)
            {
                if (!dt.Columns.Contains(s))
                    return false;
            }
            return true;

        }




        protected void AddAmendment(DataGridViewCell dgvc)
        {

            List<object> change = new List<object> { };

            change.Add((object)dgvc.OwningColumn.Name);
            change.Add((object)dgvc.Value);

            foreach (string s in amendmentPrimaryKeys)
            {
                change.Add((object)s);
                change.Add((object)dgvc.OwningRow.Cells[s].Value);
            }

            myAmendments.Rows.Add(change.ToArray());
            editedCells.Add(dgvc);
        }

     
        protected void ApplyAmendments()
        {


            

            
            string new_value;
            string column_name;
            DataRow[] row;
            DataTable dt = (DataTable)this.DataSource;

            foreach (DataRow dr in myAmendments.Rows)
            {
                new_value = dr["new_value"].ToString();
                column_name = dr["column_name"].ToString();


                string where_statement = "";
                //column_name new_value   column0 value0


                string col_name = "column0";
                string val_name = "value0";
                string col = "";
                string val = "";
                int num = 0;

                while(myAmendments.Columns.Contains(col_name))
                {
                    col = dr[col_name].ToString();
                    val = dr[val_name].ToString();

                    if (num > 0)
                        where_statement += " and ";

                    if (col != "" && val != "")
                        where_statement += col + " = '" + val + "' ";

                    num++;
                    col_name = "column" + num.ToString();
                    val_name = "value" + num.ToString();
                }
                    

                row = dt.Select(where_statement);

                if (row.Length > 0)
                {
                    row[0][column_name] = new_value;                                        
                }
            }

        }



        #endregion



        private void SetGraph()
        {


            string x_col = "";
            string y_col = "";


            DataTable d = (DataTable)this.DataSource;

            if (d == null)
                return;

            if (d.Columns.Contains("Day"))
                x_col = "Day";
            if (d.Columns.Contains("dt"))
                x_col = "dt";
            if (d.Columns.Contains("inverter_name"))
                x_col = "inverter_name";


            if (x_col == "")
                return;




            List<string> plots = new List<string> { };

            if (zed.GraphPane != null)
                zed.GraphPane.CurveList.Clear();


            foreach (DataGridViewCell cell in this.SelectedCells)
            {
                y_col = cell.OwningColumn.Name;

                if (y_col == "")
                    continue;
                plots.Add(y_col);
            }


            zed.SetGraphValues(d, x_col, plots);
            zed.SetGraph();

        }



        protected List<string> GetSelectedColumns()
        {
            List<string> selectedColumns = new List<string> { };
            foreach (DataGridViewCell v in this.SelectedCells)
            {
                string s = v.OwningColumn.Name;

                if (!selectedColumns.Contains(s))
                    selectedColumns.Add(s);
            }

            selectedColumns.Sort();

            return selectedColumns;
        }
        protected void  SetSelectedColumns(List<string> selectedColumns)
        {
            foreach (string s in selectedColumns)
            {

                List<string> t = new List<string> { };
                string t2 = s;


                if (this.Columns.Contains(t2))
                    this[t2, 0].Selected = true;
            }
        }
        protected void CheckAndCropDataColumns(ref DataTable data)
        {
            if (data.Columns.Count < 200)
                return;
            else
            {
                for (int i = data.Columns.Count - 10; i > 200; i--)
                {
                    data.Columns.RemoveAt(i);

                }               
            }

        }




        public void SetDataSource(DataTable d)
        {
            SetDataSource(d, new DataTable());
        }
        public void SetDataSource(DataTable d, DataTable amendments_in)
        
        {
            List<string> selectedColumns = GetSelectedColumns();
                        
            CheckAndCropDataColumns(ref d);
            this.DataSource = null;
            this.DataSource = d;

            this.myAmendments = amendments_in;
            editedCells = new List<DataGridViewCell> { };
            ApplyAmendments();
            
            SetSelectedColumns(selectedColumns);
        }


        private delegate void MergeDataSourceDelegate(DataTable d);
        public void MergeDataSource(DataTable d)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MergeDataSourceDelegate(this.MergeDataSource), d);
            }
            else
            {
                if (this.DataSource == null)
                    this.DataSource = d;
                else
                {
                    int i = this.FirstDisplayedScrollingRowIndex;
                    ((DataTable)this.DataSource).Merge(d);

                    if (i > -1)
                        this.FirstDisplayedScrollingRowIndex = i;


                    if (ScrollToBottom && this.Rows.Count > 10)
                    {
                        this.FirstDisplayedScrollingRowIndex = this.Rows.Count - 10;
                    }

                }

            }
        }




        #region format datagridview

        public void SetNumberFormat(string columnName, string format)
        {
            if (!this.Columns.Contains(columnName))
                return;

            DataGridViewCellStyle cs = new DataGridViewCellStyle();
            cs.Format = format;

            this.Columns[columnName].DefaultCellStyle = cs;
        }
        public void SetPercentFormat()
        {

            string format = "p3";

            DataGridViewCellStyle cs = new DataGridViewCellStyle();
            cs.Format = format;


            foreach (DataGridViewColumn dc in this.Columns)
            {
                if (dc.Name.Contains("%"))
                    this.Columns[dc.Name].DefaultCellStyle = cs;

            }
        }
        public void SetWrapFormat()
        {

            //this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            //this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedHeaders;
            //this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;



            foreach (DataGridViewColumn dc in this.Columns)
            {


                List<int> len = new List<int> { };
                int l = 0;
                int i = 0;


                foreach (DataGridViewRow dr in this.Rows)
                {
                    if (dr.Cells[dc.Name].Value == null)
                        continue;

                    l = dr.Cells[dc.Name].Value.ToString().Length;
                    i++;

                    if (i > 30)
                        break;

                    if (l < 1)
                        continue;
                    if (len.Count > 20)
                        break;

                    len.Add(l);
                }



                int width = 100;

                if (len.Count > 0)
                    width = (int)(len.Average() * 8);

                dc.Width = width;

                if (dc.Width > 500)
                    dc.Width = 500;

                if (dc.Width < 30)
                    dc.Width = 30;


                dc.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
            }

        }

        #endregion




    }
}
