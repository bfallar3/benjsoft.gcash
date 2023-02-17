using System.Collections.Generic;
using System.Linq;

namespace Benjsoft.Gcash
{
    public class FeeService
    {
        private static List<FeeCharge> _fees;

        static FeeService()
        {
            double start = 1.00;
            double end = 500.00;
            double charge = 10;
            double increment = 500;

            _fees = new List<FeeCharge>();
            for(int i=0; i<20; i++)
            {
                _fees.Add(new FeeCharge(start, end, charge));
                start += increment;
                end += increment;

                charge += 10;
            }
        }

        public static double GetCharge(double amount)
        {
            var charge = _fees.FirstOrDefault(f => amount >= f.Minumum && amount <= f.Maximum);
            if (charge != null)
                return charge.Charge;
            return 0.00;
        }
    }

    internal class FeeCharge
    {
        public FeeCharge(double minumum, double maximum, double fee)
        {
            Minumum = minumum;
            Maximum = maximum;
            this.Charge = fee;
        }

        public double Minumum { get; set; }
        public double Maximum { get; set; }
        public double Charge { get; set; }
    }
}
