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
        else if (File.Exists(filePath))
        {
            string fileText = File.ReadAllText(filePath);
            return Yaml.GetDeserializer().Deserialize<Config>(fileText);
        }
        else
        {
            return new Config();
        }
    }
}