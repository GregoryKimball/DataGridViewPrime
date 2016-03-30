using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RDotNet;

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

    }
}
