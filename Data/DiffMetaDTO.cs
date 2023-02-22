namespace ChangeLog.Data;

public class DiffMetaDTO
{
    public List<ChangedDTO> Changed { get; set; } = new();
    public List<MetaDTO> Matched { get; set; } = new();
}