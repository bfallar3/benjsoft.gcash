using System;

namespace Benjsoft.Gcash
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public double Amount { get; set; }
        public double ChargeFee { get; set; }
        public TransTypeEnum Type { get; set; }
        public String Name { get; set; }
        public string Number { get; set; }
        public bool Claimed { get; set; }
        public string Remarks { get; set; }
    }

    public enum TransTypeEnum
    {
        Initial,
        CashIn,
        CashOut,
        Bills,
        BankTransfer
    }
}
