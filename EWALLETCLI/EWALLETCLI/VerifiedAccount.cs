using EWalletCLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWalletCLI
{
    // ============================================================
    //  VERIFIED ACCOUNT  —  Fully verified wallet
    // ============================================================
    public class VerifiedAccount : EWalletAccount
    {
        public string GovernmentID { get; set; }

        public override decimal WalletLimit => 100_000m;
        public override decimal DailyOutgoingLimit => 100_000m;
        public override decimal DailyIncomingLimit => 100_000m;

        // ── Withdraw (Cash Out) ───────────────────────────────────
        public override bool Withdraw(decimal amount, string refNo)
        {
            ResetDailyLimitIfNewDay();

            if (amount > Balance)
            { Console.WriteLine("  [!] Insufficient balance."); return false; }

            if (DailyOutgoingAmount + amount > DailyOutgoingLimit)
            { Console.WriteLine($"  [!] Daily cash-out limit of PHP {DailyOutgoingLimit:N2} exceeded."); return false; }

            Balance -= amount;
            DailyOutgoingAmount += amount;
            LastTransactionDate = DateTime.Today;

            AddTransaction(new Transaction(EWALLETCLI.TransactionType.CashOut, amount, Balance, refNo));
            return true;
        }

        // ── Send Money ────────────────────────────────────────────
        public override bool SendMoney(EWalletAccount receiver, decimal amount, string refNo)
        {
            ResetDailyLimitIfNewDay();

            if (amount > Balance)
            { Console.WriteLine("  [!] Insufficient balance."); return false; }

            if (DailyOutgoingAmount + amount > DailyOutgoingLimit)
            { Console.WriteLine($"  [!] Daily outgoing limit of PHP {DailyOutgoingLimit:N2} exceeded."); return false; }

            if (receiver.Balance + amount > receiver.WalletLimit)
            { Console.WriteLine("  [!] Receiver's wallet limit would be exceeded."); return false; }

            Balance -= amount;
            DailyOutgoingAmount += amount;
            LastTransactionDate = DateTime.Today;

            receiver.Balance += amount;
            receiver.LastTransactionDate = DateTime.Today;

            AddTransaction(new Transaction(EWALLETCLI.TransactionType.SendMoney, amount, Balance, refNo));

            receiver.AddTransaction(new Transaction(EWALLETCLI.TransactionType.ReceiveMoney, amount, receiver.Balance, refNo));

            return true;
        }
    }
}