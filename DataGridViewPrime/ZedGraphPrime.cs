using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Drawing;

using ZedGraph;
using RDotNet;



namespace DataGridViewPrimeNamespace
{

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
}
