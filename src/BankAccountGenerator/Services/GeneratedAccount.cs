namespace BankAccountGenerator.Services;

public sealed record GeneratedAccount(string AccountNumber, string Iban, string BankCode, string BankName, string Bic)
{
    /// <summary>Account number in the common printed format, e.g. 1503 12 34567.</summary>
    public string FormattedAccountNumber => $"{AccountNumber[..4]} {AccountNumber[4..6]} {AccountNumber[6..]}";
}
