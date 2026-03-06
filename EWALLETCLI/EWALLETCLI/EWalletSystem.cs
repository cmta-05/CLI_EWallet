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
    //  SYSTEM CONTROLLER
    // ============================================================
    public class EWalletSystem
    {
        public List<EWalletAccount> Accounts = new List<EWalletAccount>();

        // ── Reference Number Generator ────────────────────────────
        public string GenerateReferenceNumber()
        {
            return "REF" + DateTime.Now.ToString("yyyyMMddHHmmss") +
                   new Random().Next(100, 999).ToString();
        }

        // ── Find Account ──────────────────────────────────────────
        public EWalletAccount FindAccountByMobile(string mobile)
            => Accounts.FirstOrDefault(a => a.MobileNumber == mobile);

        // ── MPIN Confirmation Helper ──────────────────────────────
        private bool ConfirmMPIN(EWalletAccount user)
        {
            Console.WriteLine("------------------------------------------------------");
            Console.Write("  Confirm MPIN to proceed: ");
            string input = (Console.ReadLine() ?? "").Trim();

            if (!user.ValidatePIN(input))
            {
                Console.WriteLine("  [!] Incorrect MPIN. Transaction cancelled.");
                Console.WriteLine("  Press any key to return...");
                Console.ReadKey();
                return false;
            }
            return true;
        }

        // ────────────────────────────────────────────────────────
        //  REGISTER
        // ────────────────────────────────────────────────────────
        public void RegisterAccount()
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("               ACCOUNT REGISTRATION                  ");
            Console.WriteLine("======================================================");

            // Step 1 — Mobile Number
            Console.WriteLine("\n  STEP 1 — Mobile Number");
            Console.WriteLine("  ______________________");
            string mobile = Validators.ValidateMobileNumber();

            if (Accounts.Any(a => a.MobileNumber == mobile))
            {
                Console.WriteLine("  [!] Mobile number is already registered.");
                Console.WriteLine("\n  Press any key to return...");
                Console.ReadKey();
                return;
            }

            // Step 2 — OTP
            Console.WriteLine("\n  STEP 2 — OTP Verification");
            Console.WriteLine("  _________________________");
            string otp = OTP.GenerateOTP();
            Console.WriteLine($"  [System] Your OTP is: {otp}");

            if (!OTP.ValidateOTP(otp))
            {
                Console.WriteLine("  [!] OTP verification failed. Registration cancelled.");
                Console.WriteLine("\n  Press any key to return...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("  OTP Verified!");

            // Step 3 — MPIN
            Console.WriteLine("\n  STEP 3 — Create MPIN");
            Console.WriteLine("  ____________________");
            string mpin = Validators.ValidateMPIN();

            // Step 4 — Personal Info
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("  STEP 4 — Personal Information");
            Console.WriteLine("  ______________________________");
            string firstName = Validators.ValidateName("First Name");
            string middleName = Validators.ValidateName("Middle Name", isOptional: true);
            string lastName = Validators.ValidateName("Last Name");
            DateTime dob = Validators.ValidateDateOfBirth();   // 
            string nationality = Validators.ValidateNationality();
            string email = Validators.ValidateEmail();

            // Step 5 — Address
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("  STEP 5 — Address Information");
            Console.WriteLine("  ____________________________");
            string street = Validators.ValidateAddress("House / Street");
            string barangay = Validators.ValidateAddress("Barangay");
            string city = Validators.ValidateAddress("City / Municipality");
            string province = Validators.ValidateAddress("Province");
            string zip = Validators.ValidateAddress("ZIP Code");

            // Step 6 — Summary & Confirm
            string fullName = $"{firstName} {(string.IsNullOrEmpty(middleName) ? "" : middleName + " ")}{lastName}";
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("  STEP 6 — Account Summary");
            Console.WriteLine("======================================================");
            Console.WriteLine($"  Mobile Number  : {mobile}");
            Console.WriteLine($"  Full Name      : {fullName}");
            Console.WriteLine($"  Date of Birth  : {dob:yyyy-MM-dd}");
            Console.WriteLine($"  Nationality    : {nationality}");
            Console.WriteLine($"  Email          : {email}");
            Console.WriteLine($"  Address        : {street}, {barangay}, {city}, {province} {zip}");
            Console.WriteLine($"  MPIN           : ****");
            Console.WriteLine($"  Account Type   : Basic (Unverified)");
            Console.WriteLine($"  Balance        : PHP 0.00");
            Console.WriteLine("------------------------------------------------------");
            Console.Write("  Confirm registration? (Y/N): ");
            string confirm = (Console.ReadLine() ?? "").Trim().ToUpper();

            if (confirm != "Y")
            {
                Console.WriteLine("  Registration cancelled.");
                Console.WriteLine("\n  Press any key to return...");
                Console.ReadKey();
                return;
            }

            // Step 7 — Create Account
            BasicAccount newAccount = new BasicAccount
            {
                MobileNumber = mobile,
                MPIN = mpin,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dob,
                Nationality = nationality,
                Email = email,
                Street = street,
                Barangay = barangay,
                City = city,
                Province = province,
                ZipCode = zip
            };

            Accounts.Add(newAccount);

            Console.WriteLine("\n======================================================");
            Console.WriteLine("      Account Created Successfully! Welcome!");
            Console.WriteLine("======================================================");
            Console.WriteLine("\n  Press any key to return to main menu and login your account...");
            Console.ReadKey();
        }

        // ────────────────────────────────────────────────────────
        //  LOGIN
        // ────────────────────────────────────────────────────────
        public EWalletAccount Login()
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("                      LOGIN                          ");
            Console.WriteLine("======================================================");
            Console.Write("\n  Enter PH Mobile Number (09XXXXXXXXX): 09");
            string raw = (Console.ReadLine() ?? "").Trim();
            string digits = new string(raw.Where(char.IsDigit).ToArray());
            string mobile = (digits.StartsWith("09") && digits.Length == 11) ? digits : "09" + raw;

            var user = FindAccountByMobile(mobile);

            if (user == null)
            {
                Console.WriteLine("\n  [!] Account not found.");
                Console.WriteLine("  Press any key to return...");
                Console.ReadKey();
                return null;
            }

            if (user.IsLocked)
            {
                Console.WriteLine("\n  [!] This account is LOCKED due to too many failed attempts.");
                Console.WriteLine("  Press any key to return...");
                Console.ReadKey();
                return null;
            }

            const int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                Console.Write("  Enter MPIN: ");
                string input = (Console.ReadLine() ?? "").Trim();

                if (user.ValidatePIN(input))
                {
                    user.LoginAttempts = 0;
                    Console.WriteLine("\n  Login successful! Press any key to continue...");
                    Console.ReadKey();
                    return user;
                }

                int remaining = maxAttempts - attempt;
                Console.WriteLine(remaining > 0
                    ? $"  [!] Incorrect MPIN. {remaining} attempt(s) remaining."
                    : "  [!] Incorrect MPIN.");
            }

            user.IsLocked = true;
            Console.WriteLine("\n  [!] Account LOCKED due to too many failed attempts.");
            Console.WriteLine("  Press any key to return...");
            Console.ReadKey();
            return null;
        }

        // ────────────────────────────────────────────────────────
        //  CASH IN
        // ────────────────────────────────────────────────────────
        public void CashIn(EWalletAccount user)
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("                      CASH IN                        ");
            Console.WriteLine("======================================================");
            Console.WriteLine("  Select source:");
            Console.WriteLine("  [1] Bank Transfer");
            Console.WriteLine("  [2] Over the Counter");
            Console.WriteLine("  [0] Back");
            Console.WriteLine("------------------------------------------------------");
            Console.Write("  Choice: ");
            string src = (Console.ReadLine() ?? "").Trim();

            if (src == "0") return;

            if (src != "1" && src != "2")
            { Console.WriteLine("  [!] Invalid option."); Console.ReadKey(); return; }

            Console.WriteLine();
            decimal amount = Validators.ValidateAmount();

            if (!(amount <= 50_000))
            { Console.WriteLine("  [!] Maximum per transaction is PHP 50,000."); Console.ReadKey(); return; }

            if (user.Balance + amount > user.WalletLimit)
            { Console.WriteLine($"  [!] This would exceed your wallet limit of PHP {user.WalletLimit:N2}."); Console.ReadKey(); return; }

            // ── MPIN confirmation before completing transaction ──
            if (!ConfirmMPIN(user)) return;

            string refNo = GenerateReferenceNumber();
            user.Deposit(amount);
            user.LastTransactionDate = DateTime.Today;
            user.AddTransaction(new Transaction(TransactionType.CashIn, amount, user.Balance, refNo));

            Console.WriteLine("\n======================================================");
            Console.WriteLine("  Cash In Successful!");
            Console.WriteLine($"  Amount          : PHP {amount:N2}");
            Console.WriteLine($"  New Balance     : PHP {user.Balance:N2}");
            Console.WriteLine($"  Reference No.   : {refNo}");
            Console.WriteLine("======================================================");
            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey();
        }

        // ────────────────────────────────────────────────────────
        //  CASH OUT
        // ────────────────────────────────────────────────────────
        public void CashOut(EWalletAccount user)
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("                     CASH OUT                        ");
            Console.WriteLine("======================================================");
            Console.WriteLine($"  Available Balance : PHP {user.Balance:N2}");
            user.ResetDailyLimitIfNewDay();
            Console.WriteLine($"  Daily Limit Used  : PHP {user.DailyOutgoingAmount:N2} / PHP {user.DailyOutgoingLimit:N2}");
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine();

            decimal amount = Validators.ValidateAmount();

            if (!ConfirmMPIN(user)) return;

            string refNo = GenerateReferenceNumber();

            if (user.Withdraw(amount, refNo))
            {
                Console.WriteLine("\n======================================================");
                Console.WriteLine("  Cash Out Successful!");
                Console.WriteLine($"  Amount          : PHP {amount:N2}");
                Console.WriteLine($"  New Balance     : PHP {user.Balance:N2}");
                Console.WriteLine($"  Reference No.   : {refNo}");
                Console.WriteLine("======================================================");
            }

            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey();
        }

        // ────────────────────────────────────────────────────────
        //  SEND MONEY
        // ────────────────────────────────────────────────────────
        public void TransferMoney(EWalletAccount sender)
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("                    SEND MONEY                       ");
            Console.WriteLine("======================================================");

            if (sender is BasicAccount)
            {
                sender.SendMoney(null, 0, null);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("  Enter [0] to go Back.");
            Console.Write("  Receiver Mobile Number (09XXXXXXXXX): 09");
            string raw = (Console.ReadLine() ?? "").Trim();

            if (raw == "0") return;

            string recvMob = "09" + raw;
            var receiver = FindAccountByMobile(recvMob);

            if (receiver == null)
            { Console.WriteLine("  [!] Receiver not found."); Console.ReadKey(); return; }

            if (receiver.MobileNumber == sender.MobileNumber)
            { Console.WriteLine("  [!] Cannot send money to yourself."); Console.ReadKey(); return; }

            Console.WriteLine($"\n  Receiver : {receiver.FirstName} {receiver.LastName}");
            Console.WriteLine("------------------------------------------------------");

            decimal amount = Validators.ValidateAmount();

            if (amount > 20_000)
            {
                string otp = OTP.GenerateOTP();
                Console.WriteLine($"\n  [Security] Large transfer detected. OTP sent: {otp}");
                if (!OTP.ValidateOTP(otp))
                { Console.WriteLine("  [!] OTP failed. Transfer cancelled."); Console.ReadKey(); return; }
            }

            if (!ConfirmMPIN(sender)) return;

            string refNo = GenerateReferenceNumber();

            if (sender.SendMoney(receiver, amount, refNo))
            {
                Console.WriteLine("\n======================================================");
                Console.WriteLine("  Transfer Successful!");
                Console.WriteLine($"  Sent To         : {receiver.FirstName} {receiver.LastName}");
                Console.WriteLine($"  Amount          : PHP {amount:N2}");
                Console.WriteLine($"  New Balance     : PHP {sender.Balance:N2}");
                Console.WriteLine($"  Reference No.   : {refNo}");
                Console.WriteLine("======================================================");
            }

            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey();
        }

        // ────────────────────────────────────────────────────────
        //  VERIFY ACCOUNT
        // ────────────────────────────────────────────────────────
        public EWalletAccount VerifyAccount(EWalletAccount user)
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("               ACCOUNT VERIFICATION                  ");
            Console.WriteLine("======================================================");

            if (user is VerifiedAccount)
            {
                Console.WriteLine("  Your account is already Verified.");
                Console.WriteLine("\n  Press any key to return...");
                Console.ReadKey();
                return user;
            }

            Console.WriteLine("  Requirements:");
            Console.WriteLine("  - 8–16 characters");
            Console.WriteLine("  - Letters and numbers only");
            Console.WriteLine("  - Cannot start with 0");
            Console.WriteLine("  - Cannot contain identical characters");
            Console.WriteLine();

            string govID = Validators.ValidateGovernmentID();

            if (!ConfirmMPIN(user)) return user;
            VerifiedAccount verified = new VerifiedAccount
            {
                MobileNumber = user.MobileNumber,
                MPIN = user.MPIN,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Nationality = user.Nationality,
                Email = user.Email,
                Street = user.Street,
                Barangay = user.Barangay,
                City = user.City,
                Province = user.Province,
                ZipCode = user.ZipCode,
                Balance = user.Balance,
                DailyOutgoingAmount = user.DailyOutgoingAmount,
                LastTransactionDate = user.LastTransactionDate,
                TransactionHistory = user.TransactionHistory,
                GovernmentID = govID
            };

            string refNo = GenerateReferenceNumber();
            verified.AddTransaction(new Transaction(TransactionType.Verification, 0, verified.Balance, refNo));

            Accounts.Remove(user);
            Accounts.Add(verified);

            Console.WriteLine("\n  Account Verified Successfully!");
            Console.WriteLine("  You now have access to Send Money and higher limits.");
            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey();
            return verified;
        }

        // ────────────────────────────────────────────────────────
        //  DELETE ACCOUNT
        // ────────────────────────────────────────────────────────
        public bool DeleteAccount(EWalletAccount user)
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("                  DELETE ACCOUNT                     ");
            Console.WriteLine("======================================================");
            Console.WriteLine("  WARNING: This action is irreversible.");
            Console.Write("  Enter MPIN to confirm: ");
            string pin = (Console.ReadLine() ?? "").Trim();

            if (!user.ValidatePIN(pin))
            { Console.WriteLine("  [!] Incorrect MPIN. Deletion cancelled."); Console.ReadKey(); return false; }

            Console.Write("  Are you sure? (YES to confirm): ");
            string confirm = (Console.ReadLine() ?? "").Trim().ToUpper();

            if (confirm != "YES")
            { Console.WriteLine("  Deletion cancelled."); Console.ReadKey(); return false; }

            Accounts.Remove(user);
            Console.WriteLine("\n  Account deleted successfully.");
            Console.WriteLine("  Log out now to finish the deletion.");
            Console.WriteLine("  Press any key to continue..");
            Console.ReadKey();
            return true;
        }

        // ────────────────────────────────────────────────────────
        //  RESET MPIN
        // ────────────────────────────────────────────────────────
        public void ResetMPIN(EWalletAccount user)
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("                   RESET MPIN                        ");
            Console.WriteLine("======================================================");
            Console.Write("  Enter current MPIN: ");
            string current = (Console.ReadLine() ?? "").Trim();

            if (!user.ValidatePIN(current))
            { Console.WriteLine("  [!] Incorrect MPIN."); return; }

            Console.WriteLine();
            string newMpin = Validators.ValidateMPIN();
            if (newMpin == current)
            { Console.WriteLine("  [!] You entered your current MPIN."); Console.ReadKey(); return; }

            user.MPIN = newMpin;

            Console.WriteLine("\n  MPIN updated successfully!");
            Console.ReadKey();
        }

        // ────────────────────────────────────────────────────────
        //  UPDATE ACCOUNT INFO
        // ────────────────────────────────────────────────────────
        public void UpdateAccountInfo(EWalletAccount user)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("======================================================");
                Console.WriteLine("              UPDATE ACCOUNT INFORMATION             ");
                Console.WriteLine("======================================================");
                Console.WriteLine("  What would you like to update?");
                Console.WriteLine();
                Console.WriteLine("  [1]  First Name");
                Console.WriteLine("  [2]  Middle Name");
                Console.WriteLine("  [3]  Last Name");
                Console.WriteLine("  [4]  Date of Birth");
                Console.WriteLine("  [5]  Nationality");
                Console.WriteLine("  [6]  Email Address");
                Console.WriteLine("  [7]  House / Street");
                Console.WriteLine("  [8]  Barangay");
                Console.WriteLine("  [9]  City / Municipality");
                Console.WriteLine("  [10] Province");
                Console.WriteLine("  [11] ZIP Code");
                Console.WriteLine("  [0]  Back");
                Console.WriteLine("------------------------------------------------------");
                Console.Write("  Select field to update: ");
                string choice = (Console.ReadLine() ?? "").Trim();

                if (choice == "0") return;

                string updated;
                switch (choice)
                {
                    case "1":
                        updated = Validators.ValidateName("First Name");
                        if (!ConfirmMPIN(user)) return;
                        user.FirstName = updated;
                        break;
                    case "2":
                        updated = Validators.ValidateName("Middle Name", isOptional: true);
                        if (!ConfirmMPIN(user)) return;
                        user.MiddleName = updated;
                        break;
                    case "3":
                        updated = Validators.ValidateName("Last Name");
                        if (!ConfirmMPIN(user)) return;
                        user.LastName = updated;
                        break;
                    case "4":
                        DateTime newDob = Validators.ValidateDateOfBirth();
                        if (!ConfirmMPIN(user)) return;
                        user.DateOfBirth = newDob;
                        Console.WriteLine("  Date of Birth updated!");
                        Console.ReadKey();
                        continue;
                    case "5":
                        updated = Validators.ValidateNationality();
                        if (!ConfirmMPIN(user)) return;
                        user.Nationality = updated;
                        break;
                    case "6":
                        updated = Validators.ValidateEmail();
                        if (!ConfirmMPIN(user)) return;
                        user.Email = updated;
                        break;
                    case "7":
                        updated = Validators.ValidateAddress("House / Street");
                        if (!ConfirmMPIN(user)) return;
                        user.Street = updated;
                        break;
                    case "8":
                        updated = Validators.ValidateAddress("Barangay");
                        if (!ConfirmMPIN(user)) return;
                        user.Barangay = updated;
                        break;
                    case "9":
                        updated = Validators.ValidateAddress("City / Municipality");
                        if (!ConfirmMPIN(user)) return;
                        user.City = updated;
                        break;
                    case "10":
                        updated = Validators.ValidateAddress("Province");
                        if (!ConfirmMPIN(user)) return;
                        user.Province = updated;
                        break;
                    case "11":
                        updated = Validators.ValidateAddress("ZIP Code");
                        if (!ConfirmMPIN(user)) return;
                        user.ZipCode = updated;
                        break;
                    default:
                        Console.WriteLine("  [!] Invalid option.");
                        Console.ReadKey();
                        continue;
                }

                Console.WriteLine("  Information updated successfully!");
                Console.ReadKey();
            }
        }
    }
}