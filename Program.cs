using ChangeLog.Command;
using Spectre.Console.Cli;

var app = new CommandApp();
app.SetDefaultCommand<WelcomeCommand>();
app.Configure(config =>
{
#if DEBUG
    config.PropagateExceptions();
#endif

    config.SetApplicationName("ChangeLog");

    config.AddCommand<WelcomeCommand>("welcome").IsHidden();

    config.AddCommand<GenerateCommand>("generate")
        .WithAlias("gen")
        .WithDescription("Generate changelog from changeLog.yml config file")
        .WithExample(new[] { "generate", "-f", "changeLog.yml" });

    config.AddCommand<ValidateYamlCommand>("validate")
        .WithAlias("val")
        .WithDescription("Validates changeLog.yml config file")
        .WithExample(new[] { "validate", "-f", "changeLog.yml" });

    config.AddCommand<DiffCommand>("diff")
        .WithDescription("Diff")
        .WithExample(new[] { "diff" });

    config.AddCommand<TestCommand>("test")
        .WithDescription("Test")
        .WithExample(new[] { "test" });
});

app.Run(args);