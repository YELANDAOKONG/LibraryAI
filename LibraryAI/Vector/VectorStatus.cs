namespace LibraryAI.Vector;

public enum VectorStatus
{
    Incomplete = 0,     // Deleted
    Normal = 1,         // Ready
    Unprocessed  = 2,   // Chunked Only (No Embedding)
}