namespace LibAI.Tools;

public class ChunkTools
{
    public Stream InputStream;
    public int ChunkSize;
    public int ChunkOverlap;
    public Action<string>? ChunkCallback;
    
    public ChunkTools(
        Stream input, 
        int chunkSize = 1024, 
        int chunkOverlap = 256,
        Action<string>? chunkCallback = null
    )
    {
        InputStream = input;
        ChunkSize = chunkSize;
        ChunkOverlap = chunkOverlap;
        ChunkCallback = chunkCallback;
    }
    
    public void Chunk()
    {
        // TODO...
    }
    
}