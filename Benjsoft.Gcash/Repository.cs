using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Benjsoft.Gcash
{
    public class Repository
    {
        public IEnumerable<Transaction> GetTransactions()
        {
            using (var cn = DAO.LiteDbConnection())
            {
                cn.Open();
                string query = @"SELECT * FROM [Transaction]";
                return cn.Query<Transaction>(query);
            }
        }

        public long CountTransactions()
        {
            using (var cn = DAO.LiteDbConnection())
            {
                cn.Open();
                string query = @"SELECT COUNT(*) FROM [Transaction]";
                return (long)cn.ExecuteScalar(query);
            }
        }

        public int AddTransaction(Transaction transaction)
        {
            using (var cn = DAO.LiteDbConnection())
            {
                cn.Open();
                using (var trans = cn.BeginTransaction())
                {
                    string insertQuery = @"INSERT INTO [Transaction] (TransactionDate, Amount, [Type], Name, [Number], Claimed, Remarks, ChargeFee)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?);";

                    var result = cn.Execute(insertQuery, new
                    {
                        transaction.TransactionDate,
                        transaction.Amount,
                        transaction.Type,
                        transaction.Name,
                        transaction.Number,
                        transaction.Claimed,
                        transaction.Remarks,
                        transaction.ChargeFee
                    });

                    trans.Commit();
                    return result;
                }
            }
        }

        public int DeleteTransaction(int id)
        {
            using (var cn = DAO.LiteDbConnection())
            {
                cn.Open();
                using (var trans = cn.BeginTransaction())
                {
                    string insertQuery = @"DELETE FROM [Transaction] WHERE Id = ?";

                    var result = cn.Execute(insertQuery, new
                    {
                        id
                    });

                    trans.Commit();
                    return result;
                }
            }
        }

        public int CashoutClaimed(int id, bool claimed)
        {
            using (var cn = DAO.LiteDbConnection())
            {
                cn.Open();
                using (var trans = cn.BeginTransaction())
                {
                    string insertQuery = @"UPDATE [Transaction] SET Claimed = ? WHERE Id = ?";

                    var result = cn.Execute(insertQuery, new
                    {
                        claimed,
                        id
                    });

                    trans.Commit();
                    return result;
                }
            }
        }

        public IEnumerable<Contact> GetContacts()
        {
            using (var cn = DAO.LiteDbConnection())
            {
                cn.Open();
                string query = @"SELECT * FROM [Contacts]";
                return cn.Query<Contact>(query);
            }
        }

        public void UpsertContact(string name, string mobile)
        {
            using (var cn = DAO.LiteDbConnection())
            {
                cn.Open();
                using (var trans = cn.BeginTransaction())
                {
                    string query = @"SELECT * FROM [Contacts] WHERE Mobile = ?";

                    var result = cn.Query<Contact>(query, new { mobile });
                    if (result.Count() == 0)
                    {
                        // Insert contact
                        cn.Execute("INSERT INTO [Contacts] (Name, Mobile) VALUES (?, ?);", new { name, mobile });
                    }
                    else
                    {
                        // Update name
                        cn.Execute("UPDATE [Contacts] SET Name = ? WHERE Mobile = ?", new { name, mobile });
                    }

                    trans.Commit();
                }
            }
        }

        public string GetMobileByName(string name)
        {
            using (var cn = DAO.LiteDbConnection())
            {
                cn.Open();
                string query = @"SELECT * FROM [Contacts] WHERE Name = ?";
                return cn.QueryFirstOrDefault<Contact>(query, new { name })?.Mobile;

            }
        }

        public Calculation CalculateDailyReport(DateTime start, DateTime end)
        {
            Calculation response = new Calculation();
            using (var cn = DAO.LiteDbConnection())
            {
                cn.Open();
                string query = @"SELECT * FROM [Transaction] t 
                        WHERE DATE(t.TransactionDate) BETWEEN DATE(?) AND DATE(?)";
                var transactions = cn.Query<Transaction>(query, new { start, end });

                response.TotalCashIn = transactions.Where(c => c.Type == TransTypeEnum.CashIn).Sum(c => c.Amount);
                response.TotalCashOut = transactions.Where(c => c.Type == TransTypeEnum.CashOut).Sum(c => c.Amount);
                response.TotalBankTransfer = transactions.Where(c => c.Type == TransTypeEnum.BankTransfer).Sum(c => c.Amount);
                response.TotalBillPayments = transactions.Where(c => c.Type == TransTypeEnum.Bills).Sum(c => c.Amount);
                response.TotalInitial = transactions.Where(c => c.Type == TransTypeEnum.Initial).Sum(c => c.Amount);
                response.TotalCharges = transactions.Sum(c => c.ChargeFee);
            }
            return response;
        }
    }
}
