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
    //  BASIC ACCOUNT  —  Unverified wallet
    // ============================================================
    public class BasicAccount : EWalletAccount
    {
        public override decimal WalletLimit => 10_000m;
        public override decimal DailyOutgoingLimit => 5_000m;
        public override decimal DailyIncomingLimit => 5_000m;

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

            AddTransaction(new Transaction(TransactionType.CashOut, amount, Balance, refNo));
            return true;
        }

        // ── Send Money — ──────────────────────────────
        public override bool SendMoney(EWalletAccount receiver, decimal amount, string refNo)
        {
            Console.WriteLine("  [!] Send Money is not available on Basic accounts.");
            Console.WriteLine("      Please verify your account first.");
            return false;
        }
    }
}
