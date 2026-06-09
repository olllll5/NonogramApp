using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace NonogramApp.Models;

[Table("user_progress")]
public class UserProgress : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;
    
    [Column("level_id")]
    public long LevelId { get; set; }
    
    [Column("is_completed")]
    public bool IsCompleted { get; set; }
    
    [Column("last_played")]
    public DateTime? LastPlayed { get; set; }
}
