using ChangeLog.Command;
using Spectre.Console.Cli;

var app = new CommandApp();
app.SetDefaultCommand<WelcomeCommand>();
app.Configure(config =>
{
#if DEBUG
    config.PropagateExceptions();
#endif

    config.SetApplicationName("changelog");

    config.AddCommand<WelcomeCommand>("welcome").IsHidden();

    config.AddCommand<GenerateCommand>("generate")
        .WithAlias("gen")
        .WithDescription("Generate changelog for specified types")
        .WithExample(new[] { "generate", "-f", "changeLog.yml" });

    config.AddCommand<ValidateYamlCommand>("validate")
        .WithAlias("val")
        .WithDescription("Validates changeLog.yml config file")
        .WithExample(new[] { "validate", "-f", "changeLog.yml" });

    config.AddCommand<DiffCommand>("diff")
        .WithDescription("Diff")
        .WithExample(new[] { "diff" });

    config.AddCommand<SeedCommand>("seed")
        .WithDescription("Seed")
        .WithExample(new[] { "seed", "-t", "People" });

    config.AddCommand<TestCommand>("test")
        .IsHidden()
        .WithDescription("Test")
        .WithExample(new[] { "test" });

    config.AddCommand<InitCommand>("init")
        .WithDescription("Create example changeLog.yml config file")
        .WithExample(new[] { "init" });
});

app.Run(args);