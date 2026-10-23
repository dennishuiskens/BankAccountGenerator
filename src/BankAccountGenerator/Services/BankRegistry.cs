using System.Net.Http.Json;

namespace BankAccountGenerator.Services;

/// <summary>Holds the Norwegian bank identifiers loaded from data/banks.json (generated from the bits.no xlsx).</summary>
public sealed class BankRegistry(HttpClient http)
{
    private Dictionary<string, Bank> _byCode = [];

    public IReadOnlyList<Bank> Banks { get; private set; } = [];

    /// <summary>One entry per bank name, with all of its bank codes.</summary>
    public IReadOnlyList<BankGroup> Groups { get; private set; } = [];

    public async Task LoadAsync()
    {
        if (Banks.Count > 0)
        {
            return;
        }

        Banks = await http.GetFromJsonAsync<List<Bank>>("data/banks.json") ?? [];
        _byCode = Banks.GroupBy(b => b.Code).ToDictionary(g => g.Key, g => g.First());
        Groups = Banks
            .GroupBy(b => b.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => new BankGroup(g.Key, g.First().Bic, g.ToList()))
            .OrderBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public Bank? Find(string code) => _byCode.GetValueOrDefault(code);
}

public sealed record BankGroup(string Name, string Bic, IReadOnlyList<Bank> Banks)
{
    public string Display => $"{Name} ({Banks.Count} {(Banks.Count == 1 ? "code" : "codes")})";
}
