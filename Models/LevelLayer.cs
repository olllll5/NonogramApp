using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Collections.Generic;

namespace NonogramApp.Models;

[Table("level_layers")]
public class LevelLayer : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("level_id")]
    public long LevelId { get; set; }

    [Column("layer_index")]
    public int LayerIndex { get; set; }

    [Column("grid_data")]
    public List<List<int>> GridData { get; set; } = new();
}
