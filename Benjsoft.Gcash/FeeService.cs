using System.Collections.Generic;
using System.Linq;

namespace Benjsoft.Gcash
{
    public class FeeService
    {
        private static List<FeeCharge> _fees;
        private static Repository _repository = new Repository();

        static FeeService()
        {
            _fees = new List<FeeCharge>();
            var feeCharges = _repository.GetFeeCharges();
            foreach (var charge in feeCharges)
            {
                FeeCharge feeCharge = new FeeCharge();
                feeCharge.Minumum = charge.Minumum;
                feeCharge.Maximum = charge.Maximum;
                feeCharge.Fee = charge.Fee;
                _fees.Add(feeCharge);
            }
        }

        public static double GetCharge(double amount)
        {
            var charge = _fees.FirstOrDefault(f => amount >= f.Minumum && amount <= f.Maximum);
            if (charge != null)
                return charge.Fee;
            return 0.00;
        }
    }
}
