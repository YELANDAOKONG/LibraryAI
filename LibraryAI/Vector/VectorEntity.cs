using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAI.Vector;

public class VectorEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int Status { get; set; } = 0;
    
    [Required]
    public required string VectorId { get; set; }
    
    [Required]
    public required float[] Embedding { get; set; }
    
    [Required]
    public required string Text { get; set; }

    // Sources
    public string? Sources { get; set; } // The source file name (Just name)
    public int SourcesStatus { get; set; } // Enabled or Disabled
    public string? SourcesId { get; set; } // The Id(GUID) of the source file in the database
    public long? SourcesIndex { get; set; } // Index of chunks in the same file
    public long? SourcesPosition { get; set; } // The position of chunk's start in the source file
    
    public string? Metadata { get; set; }
}