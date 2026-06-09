using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NonogramApp.Models;

[Table("levels")]
public class Level : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("width")]
    public int Width { get; set; }

    [Column("height")]
    public int Height { get; set; }

    [Column("layers_count")]
    public int LayersCount { get; set; }

    [Column("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    public string DimensionsDisplay => $"{Width}x{Height} (Слоев: {LayersCount})";
    
    public bool IsCompleted { get; set; }
}
