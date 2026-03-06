using EWALLETCLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWalletCLI
{
    public class Transaction
    {
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public decimal BalanceAfter { get; set; }
        public string ReferenceNo { get; set; }
        public string Status { get; set; }

        public Transaction(TransactionType type, decimal amount, decimal balanceAfter, string referenceNo)
        {
            Type = type;
            Amount = amount;
            Date = DateTime.Now;
            BalanceAfter = balanceAfter;
            ReferenceNo = referenceNo;
            Status = "Completed";
        }
    }
}

