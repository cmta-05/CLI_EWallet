using EWalletCLI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EWalletCLI
{
    public static class Validators
    {
        // ── Valid PH mobile prefixes ─────────────────────────────────────────────
        private static string[] validPrefixes =
        {
            "0905","0906","0907","0908","0909",
            "0910","0911","0912","0913","0914",
            "0915","0916","0917","0918","0919",
            "0920","0921","0926","0927","0928","0929",
            "0930","0935","0936","0937","0938","0939",
            "0945","0946","0947","0948","0949",
            "0950","0951","0953","0954","0955","0956",
            "0960","0961","0962","0963","0965","0966","0967","0968","0969",
            "0975","0976","0977","0978","0979",
            "0981","0989",
            "0991","0992","0993","0994","0995","0996","0997","0998","0999",
            "0895","0896","0897","0898","0899"
        };

        // ── Mobile Number ────────────────────────────────────────────────────────
        public static string ValidateMobileNumber()
        {
            while (true)
            {
                Console.Write("  Enter PH Mobile Number (09XXXXXXXXX): 09");
                string raw = (Console.ReadLine() ?? "").Trim();

                if (raw.Length != 9)
                { Console.WriteLine("  [!] Must be exactly 9 digits after '09'."); continue; }

                if (!raw.All(char.IsDigit))
                { Console.WriteLine("  [!] Numbers only."); continue; }

                if (raw.All(c => c == '0') || raw.Distinct().Count() == 1)
                {Console.WriteLine("  [!] Invalid pattern. Enter a valid mobile number."); continue;  }


                string full = "09" + raw;
                string prefix = full.Substring(0, 4);

                if (!validPrefixes.Contains(prefix))
                { Console.WriteLine($"  [!] Invalid prefix ({prefix}). Use a valid PH number."); continue; }

                return full;
            }
        }

        // ── MPIN ─────────────────────────────────────────────────────────────────
        public static string ValidateMPIN()
        {
            while (true)
            {
                Console.Write("  Create 4-digit MPIN: ");
                string mpin = (Console.ReadLine() ?? "").Trim();

                if (mpin.Length != 4 || !mpin.All(char.IsDigit))
                { Console.WriteLine("  [!] MPIN must be exactly 4 digits."); continue; }

                if (mpin.Distinct().Count() == 1 || mpin == "1234" || mpin == "4321" || mpin == "0000")
                { Console.WriteLine("  [!] MPIN is too weak. Avoid sequences or repeating digits."); continue; }

                Console.Write("  Confirm MPIN: ");
                string confirm = (Console.ReadLine() ?? "").Trim();

                if (confirm != mpin)
                { Console.WriteLine("  [!] MPINs do not match. Try again."); continue; }

                return mpin;
            }
        }

        // ── Name ─────────────────────────────────────────────────────────────────
        public static string ValidateName(string fieldName, bool isOptional = false)
        {
            while (true)
            {
                Console.Write($"  {fieldName}{(isOptional ? " (press Enter to skip)" : "")}: ");
                string name = (Console.ReadLine() ?? "").Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    if (isOptional) return "";
                    Console.WriteLine($"  [!] {fieldName} cannot be empty.");
                    continue;
                }

                if (!name.All(c => char.IsLetter(c) || c == ' ' || c == '-' || c == '\''))
                { Console.WriteLine($"  [!] {fieldName} may only contain letters, spaces, hyphens, or apostrophes."); continue; }

                if (name.Contains("  "))
                { Console.WriteLine("  [!] No double spaces allowed."); continue; }

                return name;
            }
        }

        // ── Email ─────────────────────────────────────────────────────────────────
        public static string ValidateEmail()
        {
            while (true)
            {
                Console.Write("  Email Address: ");
                string email = (Console.ReadLine() ?? "").Trim();

                if (string.IsNullOrWhiteSpace(email))
                { Console.WriteLine("  [!] Email cannot be empty."); continue; }

                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                { Console.WriteLine("  [!] Invalid email format."); continue; }

                return email;
            }
        }

        // ── Date of Birth —────────────────────────────────────────────────────
        public static DateTime ValidateDateOfBirth()
        {
            while (true)
            {
                Console.Write("  Date of Birth (YYYY-MM-DD): ");
                string input = (Console.ReadLine() ?? "").Trim();

                if (!DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime dob))
                { Console.WriteLine("  [!] Invalid format. Use YYYY-MM-DD."); continue; }

                int age = DateTime.Today.Year - dob.Year;
                if (dob.Date > DateTime.Today.AddYears(-age)) age--;

                if (age < 18)
                { Console.WriteLine("  [!] You must be at least 18 years old to register."); continue; }

                if (age > 120)
                { Console.WriteLine("  [!] Invalid date of birth. Age cannot exceed 120 years."); continue; }

                return dob.Date;
            }
        }

        // ── Nationality ───────────────────────────────────────────────────────────
        public static string ValidateNationality()
        {
            while (true)
            {
                Console.Write("  Nationality: ");
                string val = (Console.ReadLine() ?? "").Trim();

                if (string.IsNullOrWhiteSpace(val))
                { Console.WriteLine("  [!] Nationality cannot be empty."); continue; }

                if (!val.All(c => char.IsLetter(c) || c == ' ' || c == '-'))
                { Console.WriteLine("  [!] Letters, spaces, or hyphens only."); continue; }

                return val;
            }
        }

        // ── Address field ─────────────────────────────────────────────────────────
        public static string ValidateAddress(string field)
        {
            while (true)
            {
                Console.Write($"  {field}: ");
                string val = (Console.ReadLine() ?? "").Trim();

                if (string.IsNullOrWhiteSpace(val))
                {
                    Console.WriteLine($"  [!] {field} cannot be empty.");
                    continue;
                }

                switch (field.ToLower())
                {
                    case string f when f.Contains("zip"):
                        if (!ValidateZip(val))
                        {
                            Console.WriteLine($"  [!] {field} must be 4–6 digits.");
                            continue;
                        }
                        break;

                    case string f when f.Contains("street"):
                        if (!ValidateStreet(val))
                        {
                            Console.WriteLine($"  [!] {field} contains invalid characters.");
                            Console.WriteLine("  Only letters, numbers, spaces, #, /, and - are allowed.");
                            continue;
                        }
                        break;

                    case string f when f.Contains("barangay"):
                        if (!ValidateName(val))
                        {
                            Console.WriteLine($"  [!] {field} contains invalid characters.");
                            Console.WriteLine("  Only letters, spaces, and hyphens are allowed.");
                            continue;
                        }
                        break;

                    case string f when f.Contains("city") || f.Contains("municipality") || f.Contains("province"):
                        if (!ValidateName(val))
                        {
                            Console.WriteLine($"  [!] {field} contains invalid characters.");
                            Console.WriteLine("  Only letters, spaces, and hyphens are allowed.");
                            continue;
                        }
                        break;

                    default:
                        Console.WriteLine($"  [!] Unknown field type: {field}");
                        continue;
                }

                return val;
            }
        }

        private static bool ValidateZip(string zip)
        {
            return Regex.IsMatch(zip, @"^\d{4}$");
        }

        private static bool ValidateStreet(string street)
        {
            return Regex.IsMatch(street, @"^[a-zA-Z0-9\s#\/\-]+$");
        }

        private static bool ValidateName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z\s\-]+$");
        }

        // ── Amount ────────────────────────────────────────────────────────────────
        public static decimal ValidateAmount()
        {
            while (true)
            {
                Console.Write("  Amount (PHP): ");
                string input = (Console.ReadLine() ?? "").Trim();

                if (!decimal.TryParse(input, out decimal amount))
                { Console.WriteLine("  [!] Enter a valid positive amount."); continue; }

                else if (input == "0")
                { Console.WriteLine("  [!] 0 Is invalid."); continue; }

                return amount;
            }
        }

        // ── Government ID ──────────────────────────────────────────────────────────
        public static string ValidateGovernmentID()
        {
            while (true)
            {
                Console.Write("  Government ID Number: ");
                string id = (Console.ReadLine() ?? "").Trim();

                if (id.Length < 8 || id.Length > 16)
                {
                    Console.WriteLine("  [!] Government ID must be between 8 and 16 characters.");
                    continue;
                }

                if (!id.All(char.IsLetterOrDigit))
                {
                    Console.WriteLine("  [!] Government ID must contain letters and numbers only.");
                    continue;
                }

                if (id[0] == '0')
                {
                    Console.WriteLine("  [!] Government ID cannot start with 0.");
                    continue;
                }

                if (id.Distinct().Count() == 1)
                {
                    Console.WriteLine("  [!] Government ID cannot contain identical characters only.");
                    continue;
                }

                return id;
            }
        }
    }
}
