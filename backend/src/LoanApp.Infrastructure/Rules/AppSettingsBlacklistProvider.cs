using LoanApp.Domain.Rules;
using Microsoft.Extensions.Configuration;

namespace LoanApp.Infrastructure.Rules;

public class AppSettingsBlacklistProvider : IBlacklistProvider
{
    private readonly HashSet<string> _blacklisted;

    public AppSettingsBlacklistProvider(IConfiguration configuration)
    {
        _blacklisted = configuration.GetSection("BlacklistedSsns")
            .Get<string[]>()?.ToHashSet() ?? new HashSet<string>();
    }

    public bool IsBlacklisted(string ssn) => _blacklisted.Contains(ssn);
}