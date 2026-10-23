using System.Numerics;
using System.Text;

namespace BankAccountGenerator.Services;

/// <summary>
/// Generates syntactically valid Norwegian bank account numbers (11 digits, MOD11 check digit) and IBANs.
/// The numbers are for testing only and do not belong to real accounts.
/// </summary>
public static class AccountNumberGenerator
{
    private static readonly int[] Weights = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];

    public static GeneratedAccount Generate(Bank bank, Random random)
    {
        var accountNumber = CreateAccountNumber(bank.Code, random);
        return new GeneratedAccount(accountNumber, CreateIban(accountNumber), bank.Code, bank.Name, bank.Bic);
    }

    public static string CreateAccountNumber(string bankCode, Random random)
    {
        if (bankCode.Length != 4 || !bankCode.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("Bank code must be exactly four digits.", nameof(bankCode));
        }

        while (true)
        {
            // Digits 5-6 are the account type; "00" is avoided as it is reserved for internal accounts.
            var body = $"{bankCode}{random.Next(1, 100):D2}{random.Next(0, 10000):D4}";
            var checkDigit = CalculateCheckDigit(body);

            // A remainder that yields 10 has no valid check digit, so try another number.
            if (checkDigit is not null)
            {
                return body + checkDigit;
            }
        }
    }

    public static int? CalculateCheckDigit(string firstTenDigits)
    {
        var sum = 0;
        for (var i = 0; i < Weights.Length; i++)
        {
            sum += (firstTenDigits[i] - '0') * Weights[i];
        }

        var result = 11 - sum % 11;
        return result switch
        {
            11 => 0,
            10 => null,
            _ => result
        };
    }

    public static bool IsValidAccountNumber(string accountNumber) =>
        accountNumber.Length == 11
        && accountNumber.All(char.IsAsciiDigit)
        && CalculateCheckDigit(accountNumber[..10]) == accountNumber[10] - '0';

    public static string CreateIban(string accountNumber)
    {
        var checkDigits = 98 - (int)(ToNumeric(accountNumber + "NO00") % 97);
        return $"NO{checkDigits:D2}{accountNumber}";
    }

    private static BigInteger ToNumeric(string value)
    {
        var digits = new StringBuilder();
        foreach (var c in value)
        {
            digits.Append(char.IsAsciiLetter(c) ? (char.ToUpperInvariant(c) - 'A' + 10).ToString() : c.ToString());
        }

        return BigInteger.Parse(digits.ToString());
    }
}
