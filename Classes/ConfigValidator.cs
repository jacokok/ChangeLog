using FluentValidation;
namespace ChangeLog.Classes;

public class ConfigValidator : AbstractValidator<Config>
{
    public ConfigValidator()
    {
        RuleFor(v => v.ConnectionString).NotEmpty();
    }
}