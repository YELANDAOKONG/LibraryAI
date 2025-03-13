using System.ComponentModel.DataAnnotations;

namespace LibraryAI.Vector;

public class VectorEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public required string VectorId { get; set; }
    
    [Required]
    public required float[] Embedding { get; set; }
    
    public string? Metadata { get; set; }
}