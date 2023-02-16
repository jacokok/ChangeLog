using ChangeLog.Utils;

namespace ChangeLog.Classes;

public static class ConfigHelper
{
    public static Config GetConfig(string filePath, string? connectionString, string? sourceConnectionString)
    {
        if (connectionString?.Length > 0)
        {
            return new Config()
            {
                ConnectionString = connectionString,
                SourceConnectionString = sourceConnectionString ?? ""
            };
        }

        return GetConfigFromYaml(filePath);
    }

    private static Config GetConfigFromYaml(string filePath)
    {
        if (File.Exists(filePath))
        {
            return GetConfigFromFilePath(filePath);
        }
        string newPath = GetNewPathYaml(filePath);
        if (File.Exists(newPath))
        {
            return GetConfigFromFilePath(newPath);
        }
        return new Config();
    }

    private static string GetNewPathYaml(string filePath)
    {
        string extension = Path.GetExtension(filePath);
        string newExtension;
        if (extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase))
        {
            newExtension = ".yml";
        }
        else if (extension.Equals(".yml", StringComparison.OrdinalIgnoreCase))
        {
            newExtension = ".yaml";
        }
        else
        {
            newExtension = extension;
        }
        return Path.ChangeExtension(filePath, newExtension);
    }

    private static Config GetConfigFromFilePath(string filePath)
    {
        string fileText = File.ReadAllText(filePath);
        return Yaml.GetDeserializer().Deserialize<Config>(fileText);
    }
}