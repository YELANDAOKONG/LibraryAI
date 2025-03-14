using System.ComponentModel.DataAnnotations;

namespace LibraryAI.Vector;

public class VectorEntity
{
    [Key]
    public long Id { get; set; }

    public int Status { get; set; } = 0;
    
    [Required]
    public required string VectorId { get; set; }
    
    [Required]
    public required float[] Embedding { get; set; }
    
    [Required]
    public required string Text { get; set; }

    public string? Sources { get; set; }
    public string? Metadata { get; set; }
}