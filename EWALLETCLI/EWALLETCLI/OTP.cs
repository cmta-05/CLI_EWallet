using EWALLETCLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWalletCLI
{
    public static class OTP
    {
        private static Random _rng = new Random();

        public static string GenerateOTP()
        {
            return _rng.Next(100000, 999999).ToString();
        }

        public static bool ValidateOTP(string generated)
        {
            int maxAttempts = 3;
            for (int i = 0; i < maxAttempts; i++)
            {
                Console.Write("  Enter OTP: ");
                string input = (Console.ReadLine() ?? "").Trim();

                if (input == generated)
                    return true;

                int remaining = maxAttempts - i - 1;
                if (remaining > 0)
                    Console.WriteLine($"  Incorrect OTP. {remaining} attempt(s) remaining.");
            }
            return false;
        }
    }
}
