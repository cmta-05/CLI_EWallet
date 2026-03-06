using EWALLETCLI;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWalletCLI
{
    // ============================================================
    //  ABSTRACT BASE CLASS  —  EWalletAccount
    // ============================================================
    public abstract class EWalletAccount
    {
        // ── Identity ─────────────────────────────────────────────
        public string MobileNumber { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Nationality { get; set; }
        public string Email { get; set; }

        // ── Address ───────────────────────────────────────────────
        public string Street { get; set; }
        public string Barangay { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string ZipCode { get; set; }

        // ── Security ─────────────────────────────────────────────────
        private string _mpin;
        public string MPIN
        {
            get => _mpin;
            set => _mpin = value;
        }

        public bool IsLocked { get; set; } = false;
        public int LoginAttempts { get; set; } = 0;

        // ── Financial ─────────────────────────────────────────────
        private decimal _balance = 0;
        public decimal Balance
        {
            get => _balance;
            set => _balance = value;
        }

        public decimal DailyOutgoingAmount { get; set; } = 0;
        public DateTime LastTransactionDate { get; set; } = DateTime.MinValue;

        // ── Limits ────────────────────────────────────────────────
        public abstract decimal WalletLimit { get; }
        public abstract decimal DailyOutgoingLimit { get; }
        public abstract decimal DailyIncomingLimit { get; }

        // ── Transactions ─────────────────────────────────────────
        public List<Transaction> TransactionHistory { get; set; } = new List<Transaction>();

        // ── Methods ──────────────────────────────────────────────

        public bool ValidatePIN(string input) => input == _mpin;

        public void Deposit(decimal amount)
        {
            _balance += amount;
        }

        public void ShowBalance()
        {
            Console.WriteLine($"  Current Balance : PHP {_balance:N2}");
            Console.WriteLine($"  Wallet Limit    : PHP {WalletLimit:N2}");
        }

        public void AddTransaction(Transaction t) => TransactionHistory.Add(t);

        public void ResetDailyLimitIfNewDay()
        {
            if (LastTransactionDate.Date < DateTime.Today)
            {
                DailyOutgoingAmount = 0;
            }
        }

        public abstract bool Withdraw(decimal amount, string refNo);
        public abstract bool SendMoney(EWalletAccount receiver, decimal amount, string refNo);

        public void DisplayProfile()
        {
            string fullName = $"{FirstName} {(string.IsNullOrEmpty(MiddleName) ? "" : MiddleName + " ")}{LastName}";
            string address = $"{Street}, {Barangay}, {City}, {Province} {ZipCode}";

            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("                   ACCOUNT PROFILE                   ");
            Console.WriteLine("======================================================");
            Console.WriteLine($"  Mobile Number  : {MobileNumber}");
            Console.WriteLine($"  Full Name      : {fullName}");
            Console.WriteLine($"  Date of Birth  : {DateOfBirth:yyyy-MM-dd}");
            Console.WriteLine($"  Nationality    : {Nationality}");
            Console.WriteLine($"  Email          : {Email}");
            Console.WriteLine($"  Address        : {address}");
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine($"  Account Type   : {(this is VerifiedAccount ? "Verified" : "Basic (Unverified)")}");
            Console.WriteLine($"  Balance        : PHP {Balance:N2}");
            Console.WriteLine($"  Wallet Limit   : PHP {WalletLimit:N2}");
            Console.WriteLine($"  Daily Cash-Out : PHP {DailyOutgoingLimit:N2}");
            Console.WriteLine("======================================================");
        }
    }
}
