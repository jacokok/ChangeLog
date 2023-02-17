using ChangeLog.Liquibase;

namespace ChangeLog.Utils;

public static class Procs
{
    public static ProcResults GetProcsFromFiles(string procsFolder)
    {
        var procs = GetProcsInFolder(procsFolder);
        var deleted = GetProcsInFolder(Path.Combine(procsFolder, "deleted"));

        return new ProcResults()
        {
            Procs = procs,
            DeletedProcs = deleted
        };
    }

    public static Dictionary<string, LiquibaseContainer> GetProcsInFolder(string procsFolder)
    {
        string folderPath = Path.GetFullPath(procsFolder);
        Directory.CreateDirectory(folderPath);

        string[] extensions = { ".yaml", ".yml" };

        string[] files = Directory.GetFiles(folderPath, "*.*")
            .Where(f => extensions.Contains(new FileInfo(f).Extension.ToLower()))
            .ToArray();

        Dictionary<string, LiquibaseContainer> procs = new();
        foreach (var file in Directory.GetFiles(folderPath, "*.*"))
        {
            var item = Yaml.GetDeserializer().Deserialize<LiquibaseContainer>(File.ReadAllText(file));
            var name = Path.GetFileNameWithoutExtension(file);
            procs.Add(name, item);
        }
        return procs;
    }
}

public class ProcResults
{
    public Dictionary<string, LiquibaseContainer> Procs { get; set; } = new();
    public Dictionary<string, LiquibaseContainer> DeletedProcs { get; set; } = new();
}