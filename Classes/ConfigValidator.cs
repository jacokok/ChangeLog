using FluentValidation;
using Microsoft.IdentityModel.Tokens;

namespace ChangeLog.Classes;

public class ConfigValidator : AbstractValidator<Config>
{
    public ConfigValidator()
    {
        When(v => v.ConnectionString.IsNullOrEmpty(), () => RuleFor(v => v.SourceConnectionString).NotEmpty());
        When(v => v.SourceConnectionString.IsNullOrEmpty(), () => RuleFor(v => v.ConnectionString).NotEmpty());
    }
}