using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAI.Vector;

public class SourcesEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int Status { get; set; } = 0;
    
    public string? Title { get; set; }
    
    [Required]
    public required string SourcesId { get; set; }
    
    public byte[]? Data { get; set; }
}