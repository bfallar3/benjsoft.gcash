using System;

namespace Benjsoft.Gcash
{
    public partial class DailyReport : DevExpress.XtraEditors.XtraForm
    {
        private readonly Repository repo;

        public DailyReport()
        {
            InitializeComponent();

            this.repo = new Repository();
        }

        private void DailyReport_Load(object sender, EventArgs e)
        {
            this.StartDate.DateTime = DateTime.Now;
            this.EndDate.DateTime = DateTime.Now;
        }

        private void GenerateReportButton_Click(object sender, EventArgs e)
        {
            var result = this.repo.CalculateDailyReport(this.StartDate.DateTime, this.EndDate.DateTime);

            this.InitialAmountEdit.Text = result.TotalInitial.ToString("N2");
            this.TotalCashInEdit.Text = result.TotalCashIn.ToString("N2");
            this.TotalCashOutEdit.Text = result.TotalCashOut.ToString("N2");
            this.TotalBankEdit.Text = result.TotalBankTransfer.ToString("N2");
            this.TotalBillsEdit.Text = result.TotalBillPayments.ToString("N2");
            this.TotalChargesEdit.Text = result.TotalCharges.ToString("N2");
        }
    }
}