using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Benjsoft.Gcash
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private readonly Repository repo;
        private IEnumerable<Transaction> transactions;
        private CurrencyManager currencyManager;
        private AutoCompleteStringCollection fullNames;

        public Form1()
        {
            InitializeComponent();

            this.repo = new Repository();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TransactionDateTime.DateTime = DateTime.Now;
            this.TransType.EditValue = TransTypeEnum.CashIn.ToString();

            this.BindContacts();
            this.BindData();            

            this.RemoveTransaction.Enabled = this.transactions.Count() > 0;            
        }

        private void BindContacts()
        {
            var contacts = this.repo.GetContacts();
            this.fullNames = new AutoCompleteStringCollection();
            this.fullNames.AddRange(contacts.Select(c => c.Name).ToArray());
            this.FullName.Properties.DataSource = this.fullNames;
        }

        private void BindData()
        {
            transactions = repo.GetTransactions();
            this.bsTransactions.DataSource = transactions;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void AddTransaction_Click(object sender, EventArgs e)
        {
            // validation
            if (String.IsNullOrWhiteSpace(this.Amount.Text))
            {
                this.Amount.ErrorText = "Please provide a valid amount value!";
                this.Amount.Focus();
                return;
            }
            if (String.IsNullOrWhiteSpace(this.TransType.Text))
            {
                this.TransType.ErrorText = "Please provide a valid type!";
                this.TransType.Focus();
                return;
            }
            if (String.IsNullOrWhiteSpace(this.MobileNumber.Text))
            {
                this.MobileNumber.ErrorText = "Please provide a valid mobile number!";
                this.MobileNumber.Focus();
                return;
            }

            /// Prepare transactions
            Transaction newTransaction = new Transaction
            {
                TransactionDate = this.TransactionDateTime.DateTime,
                Amount = double.Parse(this.Amount.Text),
                Type = (TransTypeEnum)Enum.Parse(typeof(TransTypeEnum), this.TransType.Text),
                Name = this.FullName.Text,
                Number = this.MobileNumber.Text,
                Claimed = this.IsClaimed.Checked,
                Remarks = this.Remarks.Text,
                ChargeFee = double.Parse(this.ChargeOrFee.Text)
            };

            // Add transaction
            var result = repo.AddTransaction(newTransaction);
            if (result == 1)
            {
                repo.UpsertContact(this.FullName.Text, this.MobileNumber.Text);

                this.BindContacts();
                this.BindData();
                this.Reset();

                this.RemoveTransaction.Enabled = this.transactions.Count() > 0;
            }
        }

        private void Reset()
        {
            this.TransactionDateTime.DateTime = DateTime.Now;
            this.TransType.EditValue = TransTypeEnum.CashIn.ToString();
            this.Amount.Text = "0.00";
            this.FullName.Text = String.Empty;
            this.Remarks.Text = String.Empty;
            this.MobileNumber.Text = String.Empty;
            this.ChargeOrFee.Text = "0.00";
            this.IsClaimed.Checked = false;
            this.TransType.Focus();
        }

        private void Amount_Leave(object sender, EventArgs e)
        {
            double amount;
            if (double.TryParse(this.Amount.Text, out amount))
            {
                this.Amount.Text = amount.ToString("N2");
            }
            else
            {
                this.Amount.ErrorText = "Please provide valid amount!";
                this.Amount.Text = "0.00";
                this.Amount.Focus();
            }

            if (!String.IsNullOrEmpty(this.Amount.Text) && !String.IsNullOrEmpty(this.TransType.Text))
            {
                var fee = this.CalculateFee(double.Parse(this.Amount.Text), (TransTypeEnum)Enum.Parse(typeof(TransTypeEnum), this.TransType.Text));
                this.ChargeOrFee.Text = fee.ToString("N2");
            }
        }
       
        private int CalculateFee(double amount, TransTypeEnum type)
        {
            int baseAmount = 1000;
            int multipler = 10;
            if (type == TransTypeEnum.CashIn)
            {
                var fee = Math.Ceiling((amount / baseAmount)) * multipler;
                return (int)fee;
            }
            else if (type == TransTypeEnum.CashOut)
            {
                multipler = 20;
                var fee = Math.Ceiling((amount / baseAmount)) * multipler;
                return (int)fee;
            }
            return 0;
        }

        private void ChargeOrFee_Enter(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.Amount.Text) && !String.IsNullOrEmpty(this.TransType.Text))
            {
                var fee = this.CalculateFee(double.Parse(this.Amount.Text), (TransTypeEnum)Enum.Parse(typeof(TransTypeEnum), this.TransType.Text));
                this.ChargeOrFee.Text = fee.ToString("N2");
            }
        }

        private void ChargeOrFee_Leave(object sender, EventArgs e)
        {
            double amount;
            if (double.TryParse(this.ChargeOrFee.Text, out amount))
            {
                this.ChargeOrFee.Text = amount.ToString("N2");
            }
            else
            {
                this.ChargeOrFee.ErrorText = "Please provide valid fee or charge!";
                this.ChargeOrFee.Text = "0.00";
                this.ChargeOrFee.Focus();
            }
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            this.currencyManager = this.bsTransactions.CurrencyManager;
        }

        private void RemoveTransaction_Click(object sender, EventArgs e)
        {
            var currentItem = this.currencyManager.Current as Transaction;
            if (currentItem != null)
            {
                var response = MessageBox.Show($"Are you sure to delete the selected transaction for {currentItem.Number}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (response == DialogResult.Yes)
                {
                    repo.DeleteTransaction(currentItem.Id);
                    this.BindData();

                    this.RemoveTransaction.Enabled = this.transactions.Count() > 0;
                }
            }
        }

        private void IsClaimed_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            DailyReport report = new DailyReport();
            report.ShowDialog();
        }

        private void FullName_Enter(object sender, EventArgs e)
        {
            
        }

        private void FullName_Leave(object sender, EventArgs e)
        {
            // Populate Mobile number based on selected / found name.
            this.MobileNumber.Text = this.repo.GetMobileByName(this.FullName.Text);
        }

        private void ClaimToggleButton_Click(object sender, EventArgs e)
        {
            var currentItem = this.currencyManager.Current as Transaction;
            if (currentItem != null)
            {
                repo.CashoutClaimed(currentItem.Id, !currentItem.Claimed);
                this.BindData();
            }
        }
    }
}
