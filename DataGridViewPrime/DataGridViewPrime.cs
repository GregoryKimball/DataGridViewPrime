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
using IODataTableNamespace;


namespace DataGridViewPrimeNamespace
{
    
    public class RFunctions
    {


        public RFunctions(REngine engine)
        {

        }


        public static List<double> GetLinearRegression(REngine engine, double[] xdata, double[] ydata)
        {
            if (engine == null)
                throw new Exception("REngine not instantiated.");



            double a = 0, b = 0, c = 0;

            if (xdata.Length > 1 && xdata.Length == ydata.Length)
            {



                NumericVector group1 = engine.CreateNumericVector(xdata);
                NumericVector group2 = engine.CreateNumericVector(ydata);
                engine.SetSymbol("group1", group1);
                engine.SetSymbol("group2", group2);

                GenericVector t;
                t = engine.Evaluate("lm.r <- lm (group2 ~ group1)").AsList();
                t = engine.Evaluate("coef(summary(lm.r))").AsList();

                NumericVector r0 = t[0].AsNumeric();  //intercept estimate
                NumericVector r1 = t[1].AsNumeric();  //slope estimate
                NumericVector r2 = t[2].AsNumeric();  //intercept se
                NumericVector r3 = t[3].AsNumeric();  //slope se
                NumericVector r4 = t[4].AsNumeric();  //intercept tvalue
                NumericVector r5 = t[5].AsNumeric();  //slope t value
                NumericVector r6 = t[6].AsNumeric();  //intercept P>t
                NumericVector r7 = t[7].AsNumeric();  //slope P>t


                a = r1.First();
                b = r3.First();
                c = r0.First() + xdata[0] * r1.First();



            }

            List<double> ld = new List<double> { };
            ld.Add(a);
            ld.Add(b);
            ld.Add(c);

            return ld;
        }





    }//*/

    


    public class ZedGraphControlPrime : ZedGraphControl
    {

        bool showLine = false;
        bool showFitLine = false;
        bool useFilter = true;
        bool colorFilter = false;
        REngine engine;



        DataTable dt_source = new DataTable();
        string x_source = "";
        List<string> y_source = new List<string> { };

        public ZedGraphControlPrime()
        {
            this.ContextMenuBuilder += new ZedGraph.ZedGraphControl.ContextMenuBuilderEventHandler(this.ZedGraphControlPrime_ContextMenuBuilder);
            this.ZoomEvent += new ZedGraph.ZedGraphControl.ZoomEventHandler(this.ZedGraphControlPrime_ZoomScaler);
           

        }

        public void SetEngine(REngine engine_in)
        {
            engine = engine_in;

        }

        #region zoom scaler

        protected void ZedGraphControlPrime_ZoomScaler(ZedGraphControl sender, ZoomState z1, ZoomState z2)
        {
            z2.ApplyState(this.GraphPane);

            double d = this.GraphPane.XAxis.Scale.Max - this.GraphPane.XAxis.Scale.Min;

            this.GraphPane.XAxis.Scale.Format = "yyyy";
            if (d < 1000)
            {
                this.GraphPane.XAxis.Title.Text = "Year";
                this.GraphPane.XAxis.Scale.Format = "yyyy-MM";
            }
            if (d < 150)
            {
                this.GraphPane.XAxis.Title.Text = "Year";
                this.GraphPane.XAxis.Scale.Format = "yyyy-MM-dd";
            }
            if (d < 3)
            {
                this.GraphPane.XAxis.Title.Text = ((XDate)this.GraphPane.XAxis.Scale.Min).DateTime.Date.ToString("yyyy-MM-dd");
                this.GraphPane.XAxis.Scale.Format = "HH";
            }
            if (d < .3)
            {
                this.GraphPane.XAxis.Title.Text = ((XDate)this.GraphPane.XAxis.Scale.Min).DateTime.Date.ToString("yyyy-MM-dd");
                this.GraphPane.XAxis.Scale.Format = "HH:mm";
            }


        }

        #endregion




        #region context menu

        protected void ZedGraphControlPrime_ContextMenuBuilder(ZedGraphControl sender, ContextMenuStrip menuStrip, Point mousePt, ZedGraphControl.ContextMenuObjectState objState)
        {
            var t = this.ContextMenuStrip;

            List<string> ii = new List<string> { };

            List<string> no_thank_you = new List<string> { "page_setup", "print", "show_val" };
            string s;

            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                s = (string)item.Tag;
                ii.Add(s);
                if (no_thank_you.Contains(s))
                {
                    item.Visible = false;

                    //menuStrip.Items.Remove(item);
                }
            }

            ToolStripMenuItem addLine = new ToolStripMenuItem("Show Line");
            addLine.Click += new System.EventHandler(this.ZedGraphControlPrime_ShowLine);
            addLine.Checked = showLine;

            ToolStripMenuItem addFitLine = new ToolStripMenuItem("Show Fit Line");
            addFitLine.Click += new System.EventHandler(this.ZedGraphControlPrime_ShowFitLine);
            addFitLine.Checked = showFitLine;

            ToolStripMenuItem addFilter = new ToolStripMenuItem("Use Filter");
            addFilter.Click += new System.EventHandler(this.ZedGraphControlPrime_UseFilter);
            addFilter.Checked = useFilter;

            ToolStripMenuItem addColorFilter = new ToolStripMenuItem("Color Filter");
            addColorFilter.Click += new System.EventHandler(this.ZedGraphControlPrime_ColorFilter);
            addColorFilter.Checked = colorFilter;


            menuStrip.Items.Insert(0, addLine);
            menuStrip.Items.Insert(1, addFitLine);
            menuStrip.Items.Insert(2, addFilter);
            menuStrip.Items.Insert(3, addColorFilter);


        }


        protected void ZedGraphControlPrime_ShowFitLine(object sender, EventArgs e)
        {
            showFitLine = !showFitLine;

            SetGraph();
        }


        protected void ZedGraphControlPrime_ShowLine(object sender, EventArgs e)
        {

            showLine = !showLine;

            int w = 0;

            if (showLine)
                w = 1;


            foreach (LineItem i in this.GraphPane.CurveList)
            {
                i.Line.Width = w;
                i.Line.IsVisible = showLine;
            }

            this.AxisChange();
            this.Invalidate();
            this.Refresh();

        }

        protected void ZedGraphControlPrime_UseFilter(object sender, EventArgs e)
        {
            useFilter = !useFilter;
            SetGraph();

        }

        protected void ZedGraphControlPrime_ColorFilter(object sender, EventArgs e)
        {
            colorFilter = !colorFilter;
        }


        #endregion




        #region graphing functions

        private Color[] ColorScheme = { Color.Blue, Color.Red, Color.Green, Color.LightBlue, Color.Pink, Color.LightGreen };
        private Color[] FilterColorScheme = { Color.Blue, Color.Red, Color.Orange, Color.LightCoral, Color.Magenta, Color.DarkMagenta, Color.BurlyWood, Color.DarkOrange };


        public void SetGraphValues(DataTable dt, string x_col, List<string> y_col)
        {
            dt_source = dt;
            x_source = x_col;
            y_source = y_col;
        }

        public void SetGraph()
        {
            if (this.GraphPane != null)
                this.GraphPane.CurveList.Clear();

            for (int i = 0; i < y_source.Count; i++)
            {
                this.AddCurveFromDataTable(dt_source, x_source, y_source[i], i);
            }
            this.AxisChange();
            this.Invalidate();
            this.Refresh();

        }


        public void AddCurveFromDataTable(DataTable dt, string x_col, string y_col, int index = 0)
        {
            string group_by = "Filter";

            if (y_col == "dt")
                return;


            Color[] scheme = ColorScheme;

            bool useFil = useFilter;
            if (!dt.Columns.Contains("Filter"))
                useFil = false;

            if (colorFilter)
            {
                this.GraphPane.CurveList.Clear();
                scheme = FilterColorScheme;
            }


            List<string> group_list = new List<string> { };
            if (dt.Columns.Contains(group_by) && colorFilter)
            {
                var distinctIds = dt.AsEnumerable()
                     .Select(s => s.Field<string>("" + group_by + "").ToString())
                     .Distinct().ToList();

                group_list = distinctIds;

                group_list.Remove("none");
                group_list.Insert(0, "none");
            }
            else
                group_list = new List<string> { "none" };



            int lineWidth = 0;

            if (showLine)
                lineWidth = 1;

            List<double> x = new List<double> { };
            List<double> y = new List<double> { };

            double d1 = 0;
            double d2 = 0;
            DateTime da;

            double min = 0, max = 0;
            bool x_ok, y_ok;
            string g = "";


            foreach (string f in group_list)
            {
                g = f;

                foreach (DataRow dr in dt.Rows)
                {

                    x_ok = DateTime.TryParse(dr[x_col].ToString(), out da);
                    y_ok = double.TryParse(dr[y_col].ToString(), out d2);
                    if (useFil || colorFilter)
                        g = dr[group_by].ToString();


                    if (x_ok)
                    {
                        d1 = (double)((XDate)da);

                        if (min == 0)
                            min = d1;

                        max = d1;
                    }

                    if (x_ok && y_ok && g == f)
                    {

                        //XDate xd = new XDate(2000, 1, 2);
                        //double xdd = (double)xd;


                        x.Add(d1);
                        y.Add(d2);
                    }

                }

                string name = y_col;
                float size = 1;

                if (colorFilter)
                {
                    name = y_col + " " + f;
                    size = 2;
                }

                LineItem ci = new LineItem(name, x.ToArray(), y.ToArray(), scheme[index % ColorScheme.Length], SymbolType.Square, lineWidth);



                ci.Symbol.Size = size;
                ci.Symbol.Fill = new Fill(scheme[index % ColorScheme.Length]);
                this.GraphPane.CurveList.Add(ci);




                if (this.showFitLine && x.Count > 0)
                {
                    List<double> dd = RFunctions.GetLinearRegression(engine, x.ToArray(), y.ToArray());


                    int i = this.GraphPane.CurveList.Count;
                    if (dd == null || i > 1)
                        continue;

                    double slope = dd[0];
                    double intercept = dd[2] - x[0] * dd[0];
                    double value_at_t0 = dd[2];

                    List<double> yfit = new List<double> { };

                    foreach (double d in x)
                    {
                        yfit.Add(d * slope + intercept);
                    }



                    name = (slope * 100 / value_at_t0 * 365.24).ToString("f3") + "% relative";
                    LineItem cl = new LineItem(name, x.ToArray(), yfit.ToArray(), Color.Red, SymbolType.None, 3);
                    ci.Symbol.Size = size;
                    ci.Symbol.Fill = new Fill(scheme[index % ColorScheme.Length]);
                    this.GraphPane.CurveList.Add(cl);

                }




                index++;
            }



            this.GraphPane.XAxis.Type = AxisType.Date;
            this.GraphPane.XAxis.Scale.Format = "yyyy-MM-dd";
            this.GraphPane.XAxis.Scale.Min = min - 10;
            this.GraphPane.XAxis.Scale.Max = max + 10;
            this.GraphPane.Title.Text = "";
            this.GraphPane.XAxis.Title.Text = x_col;
            this.GraphPane.YAxis.Title.Text = y_col;


            ZoomState zs = new ZoomState(this.GraphPane, ZoomState.StateType.Pan);
            this.ZedGraphControlPrime_ZoomScaler(this, zs, zs);


        }


        public void SetFitLine(double slope, double intercept)
        {


            List<double> x = new List<double> { };
            List<double> y = new List<double> { };


            double min = this.GraphPane.XAxis.Scale.Min;
            double max = this.GraphPane.XAxis.Scale.Max;

            for (double i = min; i < max; i += (max - min) / 100)
            {

                x.Add(i);
                y.Add(i * slope + intercept - min * slope);
            }


            string c_name = "Fit (" + (slope * 365.24).ToString("P3") + ")";
            CurveItem ci = new LineItem(c_name, x.ToArray(), y.ToArray(), Color.Red, SymbolType.None, 3);


            this.GraphPane.CurveList.Add(ci);

            this.AxisChange();
            this.Invalidate();
            this.Refresh();
        }

        /*
        public void SetDataSource(AnalysisDataTable dt, AnalysisClass ac)
        {


            this.GraphPane.CurveList.Clear();


            this.GraphPane.Title.Text = "";
            this.GraphPane.XAxis.Title.Text = ac.independentVariable;
            this.GraphPane.YAxis.Title.Text = ac.dependentVariable;


            int lineWidth = 0;

            if (showLine)
                lineWidth = 1;



            List<double> x = new List<double> { };
            List<double> y = new List<double> { };

            double d1, d2;
            DataTable data = dt.DataHash[AnalysisDataTable.fifteenMinuteLabel];

            foreach (DataRow dr in data.Rows)
            {

                if (dr["Filter"].ToString() == "none")
                {
                    double.TryParse(dr[ac.independentVariable].ToString(), out d1);
                    double.TryParse(dr[ac.dependentVariable].ToString(), out d2);

                    x.Add(d1);
                    y.Add(d2);
                }
            }


            LineItem ci = new LineItem(dt.deviceName, x.ToArray(), y.ToArray(), Color.Blue, SymbolType.Square, lineWidth);
            ci.Symbol.Size = 1;
            ci.Symbol.Fill = new Fill(Color.Blue);

            this.GraphPane.CurveList.Add(ci);

            this.AxisChange();
            this.Invalidate();
            this.Refresh();


        }
        */

        #endregion


    }

    public class DataGridViewPrime : DataGridView
    {



        protected bool usesAmendments = false;
        protected bool savesLocally = false;
        protected DataTable myAmendments;
        public string project_number = "";

        public static Color CellChanged = Color.LemonChiffon;



        protected string LastDirectory;
        protected bool displayGraph;
        ZedGraphControlPrime zed;
        REngine engine;

        public bool ScrollToBottom = false;


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



        public void SetModeAmendments()
        {
            usesAmendments = true;
            savesLocally = false;
            this.ReadOnly = false;
        }
        public void SetModeSavesLocally()
        {
            usesAmendments = false;
            savesLocally = true;
            this.ReadOnly = false;

        }

        public void SetModeReadonly()
        {
            usesAmendments = false;
            savesLocally = false;
            this.ReadOnly = true;
        }



        protected virtual List<MenuItem> GetMenuItems()
        {
            List<MenuItem> ls = new List<MenuItem> { };

            MenuItem mi;

            mi = new MenuItem("Save");
            mi.Click += new System.EventHandler(this.SaveMenuItem_Click);
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


            if (savesLocally)
            {
                mi = new MenuItem("Save Changes to Local");
                mi.Click += new System.EventHandler(this.SaveToLocalMenuItem_Click);
                ls.Add(mi);
            }

            if (usesAmendments)
            {
                mi = new MenuItem("Save Amendments");
                mi.Click += new System.EventHandler(this.SaveAmendmentsMenuItem_Click);
                ls.Add(mi);
            }


            return ls;
        }


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





            if (this.usesAmendments)
            {
                AddAmendment(this.CurrentCell);
                /*object new_value, inverter_name = null, project_name = null, column_name;

                column_name = this.CurrentCell.OwningColumn.Name;
                new_value = this.CurrentCell.Value;



                if (this.Columns.Contains("inverter_name"))
                    inverter_name = this.CurrentRow.Cells["inverter_name"].Value;
                if (this.Columns.Contains("project_number"))
                    project_name = this.CurrentRow.Cells["project_number"].Value;

                myAmendments.Rows.Add(new object[5] { null, new_value, column_name, inverter_name, project_name });
                */
            }


        }
        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);

            if (displayGraph)
                SetGraph();


        }


        //the amendment function is too specific - needs to be generalized 
        private void AddAmendment(DataGridViewCell dgvc)
        {
            object new_value, inverter_name = null, project_name = null, column_name, combiner_name = null, site_rank = null;

            column_name = dgvc.OwningColumn.Name;
            new_value = dgvc.Value;



            if (this.Columns.Contains("inverter_name"))
                inverter_name = dgvc.OwningRow.Cells["inverter_name"].Value;
            if (this.Columns.Contains("project_number"))
                project_name = dgvc.OwningRow.Cells["project_number"].Value;
            if (this.Columns.Contains("combiner_name"))
                combiner_name = dgvc.OwningRow.Cells["combiner_name"].Value;
            if (this.Columns.Contains("site_rank"))
                site_rank = dgvc.OwningRow.Cells["site_rank"].Value;

            //myAmendments.Rows.Add(new object[7] { null, new_value, column_name, inverter_name, project_name, combiner_name, site_rank });

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


                            AddAmendment(this.Rows[r + i].Cells[c + j]);
                        }
                    }
                }

                if (this.SelectedCells.Count > 1 && cell_data_count == 1)
                {
                    foreach (DataGridViewCell dc in this.SelectedCells)
                    {
                        dc.Value = s.Trim();
                        dc.Style.BackColor = DataGridViewPrime.CellChanged;

                        AddAmendment(dc);
                    }
                }


            }


        }


        #endregion



        #region custom events

        [STAThread]
        private void SaveMenuItem_Click(object sender, EventArgs e)
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



        private void SaveToLocalMenuItem_Click(object sender, EventArgs e)
        {
            if (this.DataSource == null)
                return;

            if (project_number == "")
                return;

            DataTable dt = (DataTable)this.DataSource;

            string username = System.Environment.UserName;
            string dt_utc = DateTime.UtcNow.ToString();

            if (dt.Columns.Contains("username") && dt.Rows.Count > 0)
                dt.Rows[0]["username"] = username;

            if (dt.Columns.Contains("dt_UTC") && dt.Rows.Count > 0)
                dt.Rows[0]["dt_UTC"] = dt_utc;

            /*
            DataManager dm = new DataManager();
            dm.SaveAlignmentToProjectFolder(dt, project_number);
            dm.SaveAlignment(dt, project_number);
            */


            foreach (DataGridViewRow dr in this.Rows)
            {
                foreach (DataGridViewCell dc in dr.Cells)
                {
                    dc.Style.BackColor = Color.White;
                }
            }

        }

        private void SaveAmendmentsMenuItem_Click(object sender, EventArgs e)
        {
            if (this.myAmendments.Rows.Count < 1)
                return;

            if (project_number == "")
                return;


            /*
            DataManager dm = new DataManager();
            dm.AppendMetadataAmendments(myAmendments);
            */


            foreach (DataGridViewRow dr in this.Rows)
            {
                foreach (DataGridViewCell dc in dr.Cells)
                {
                    dc.Style.BackColor = Color.White;
                }
            }
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





        public void SetAmendmentsTemplate(DataTable amendments_in)
        {
            myAmendments = amendments_in;
        }



        public void SetDataSource(DataTable d)
        {
            List<string> selectedColumns = new List<string> { };

            foreach (DataGridViewCell v in this.SelectedCells)
            {
                string s = v.OwningColumn.Name;

                if (!selectedColumns.Contains(s))
                    selectedColumns.Add(s);
            }

            selectedColumns.Sort();
            this.ClearSelection();
            //////////////
            this.DataSource = null;

            if (this.myAmendments != null)
                this.myAmendments.Rows.Clear();

            this.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

            //////////////

            if (d.Columns.Count < 200)
                this.DataSource = d;
            else
            {
                for (int i = d.Columns.Count - 10; i > 200; i--)
                {
                    d.Columns.RemoveAt(i);

                }
                this.DataSource = d;
            }

            if (d.Rows.Count > 1)
            {
                foreach (string s in selectedColumns)
                {

                    List<string> t = new List<string> { };
                    string t2 = s;

                    if (s.Contains("Current DC"))
                    {
                        //t = ManipulateDataTable.FindCurrentDCColumns(d);
                        if (t.Count > 0)
                            t2 = t[0];
                    }

                    if (s.Contains("Voltage DC"))
                    {
                        //t = ManipulateDataTable.FindVoltageDCColumns(d);
                        if (t.Count > 0)
                            t2 = t[0];
                    }
                    //ManipulateDataTable.FindCurrentDCColumns(d);


                    if (this.Columns.Contains(t2))
                        this[t2, 0].Selected = true;
                }
            }



        }

        public void MergeDataSource_dep(DataTable d)
        {
            if (this.DataSource == null)
                this.DataSource = d;
            else
                ((DataTable)this.DataSource).Merge(d);

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


        public void ResetWrapFormat()
        {
            //this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            //this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;           

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






    }
}
