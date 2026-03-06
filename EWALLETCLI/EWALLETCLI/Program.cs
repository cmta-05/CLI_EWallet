using EWalletCLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWallet
{
    class Program
    {
        static EWalletSystem system = new EWalletSystem();

        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("======================================================");
                Console.WriteLine("              WELCOME TO CLI E-WALLET                 ");
                Console.WriteLine("======================================================");
                Console.WriteLine("  [1]  Register");
                Console.WriteLine("  [2]  Login");
                Console.WriteLine("  [3]  Exit");
                Console.WriteLine("------------------------------------------------------");
                Console.Write("  Select option: ");

                switch ((Console.ReadLine() ?? "").Trim())
                {
                    case "1":
                        system.RegisterAccount();
                        break;
                    case "2":
                        var user = system.Login();
                        if (user != null) Dashboard(ref user);
                        break;
                    case "3":
                        Console.Clear();
                        Console.WriteLine("  Thank you for using CLI E-Wallet. Goodbye!");
                        return;
                    default:
                        Console.WriteLine("  [!] Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // ============================================================
        //  DASHBOARD
        // ============================================================
        static void Dashboard(ref EWalletCLI.EWalletAccount user)
        {
            while (true)
            {
                bool isVerified = user is VerifiedAccount;
                Console.Clear();
                Console.WriteLine("======================================================");
                Console.WriteLine($"         WELCOME, {user.FirstName.ToUpper()}!");
                Console.WriteLine("======================================================");
                Console.WriteLine($"  Balance      : PHP {user.Balance:N2}");
                Console.WriteLine($"  Account Type : {(isVerified ? "Verified" : "Basic (Unverified)")}");
                Console.WriteLine("------------------------------------------------------");
                Console.WriteLine("  [1]  Profile");
                Console.WriteLine("  [2]  Cash In");
                Console.WriteLine("  [3]  Cash Out");
                Console.WriteLine(isVerified ? "  [4]  Send Money" : "  [4]  Send Money  [LOCKED — Verify first]");
                Console.WriteLine("  [5]  Transaction History");
                if (!isVerified)
                    Console.WriteLine("  [6]  Verify Account");
                Console.WriteLine("  [7]  Settings");
                Console.WriteLine("  [8]  Logout");
                Console.WriteLine("------------------------------------------------------");
                Console.Write("  Select option: ");

                switch ((Console.ReadLine() ?? "").Trim())
                {
                    case "1": ProfileMenu(ref user); break;
                    case "2": system.CashIn(user); break;
                    case "3": system.CashOut(user); break;
                    case "4": system.TransferMoney(user); break;
                    case "5": ShowTransactionHistory(user); break;
                    case "6":
                        if (!isVerified)
                            user = system.VerifyAccount(user);
                        else
                        { Console.WriteLine("  [!] Invalid option."); Console.ReadKey(); }
                        break;
                    case "7": SettingsMenu(user); break;
                    case "8": return;
                    default:
                        Console.WriteLine("  [!] Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // ============================================================
        //  PROFILE MENU
        // ============================================================
        static void ProfileMenu(ref EWalletAccount user)
        {
            while (true)
            {
                user.DisplayProfile();
                Console.WriteLine("\n  [1]  Update Account Information");
                Console.WriteLine("  [2]  Delete Account");
                Console.WriteLine("  [3]  Back");
                Console.WriteLine("------------------------------------------------------");
                Console.Write("  Select option: ");

                switch ((Console.ReadLine() ?? "").Trim())
                {
                    case "1":
                        system.UpdateAccountInfo(user);
                        break;
                    case "2":
                        if (system.DeleteAccount(user))
                            return;  
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("  [!] Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // ============================================================
        //  TRANSACTION HISTORY
        // ============================================================
        static void ShowTransactionHistory(EWalletAccount user)
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("               TRANSACTION HISTORY                   ");
            Console.WriteLine("======================================================");

            if (!user.TransactionHistory.Any())
            {
                Console.WriteLine("  No transactions found.");
            }
            else
            {
                Console.WriteLine($"  {"Date & Time",-20} {"Type",-14} {"Amount",12}  {"Balance",12}  {"Reference",-22}  Status");
                Console.WriteLine("  " + new string('-', 90));

                foreach (var t in user.TransactionHistory)
                {
                    string typeStr = t.Type.ToString();
                    Console.WriteLine($"  {t.Date:yyyy-MM-dd HH:mm}   {typeStr,-14} PHP {t.Amount,9:N2}  PHP {t.BalanceAfter,9:N2}  {t.ReferenceNo,-22}  {t.Status}");
                }
            }

            Console.WriteLine("\n======================================================");
            Console.WriteLine("  Press any key to return...");
            Console.ReadKey();
        }

        // ============================================================
        //  SETTINGS
        // ============================================================
        static void SettingsMenu(EWalletAccount user)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("======================================================");
                Console.WriteLine("                    SETTINGS                         ");
                Console.WriteLine("======================================================");
                Console.WriteLine("  [1]  Change MPIN");
                Console.WriteLine("  [2]  Back");
                Console.WriteLine("------------------------------------------------------");
                Console.Write("  Select option: ");

                switch ((Console.ReadLine() ?? "").Trim())
                {
                    case "1": system.ResetMPIN(user); break;
                    case "2": return;
                    default:
                        Console.WriteLine("  [!] Invalid option."); Console.ReadKey(); break;
                }
            }
        }
    }
}