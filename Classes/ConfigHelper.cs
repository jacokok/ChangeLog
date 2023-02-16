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
        else if (YamlExtensionFileExists(filePath))
        {
            string fileText = File.ReadAllText(filePath);
            return Yaml.GetDeserializer().Deserialize<Config>(fileText);
        }
        else
        {
            return new Config();
        }
    }

    private static bool YamlExtensionFileExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            return true;
        }

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

        string newPath = Path.ChangeExtension(filePath, newExtension);
        return File.Exists(newPath);
    }
}