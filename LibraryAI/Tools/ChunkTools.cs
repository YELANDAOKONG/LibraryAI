using System.Text;

namespace LibraryAI.Tools;

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

        if (chunkSize < chunkOverlap)
        {
            throw new ArgumentException("Chunk size must be greater than chunk overlap.");
        }
    }
    
    // public void Chunk()
    // {
    //     using var reader = new StreamReader(InputStream);
    //     var buffer = new StringBuilder();
    //     char[] readBuffer = new char[4096];  // 4KB读取缓冲
    //     int totalRead = 0;
    //
    //     // 流式读取核心逻辑
    //     while ((totalRead = reader.ReadBlock(readBuffer, 0, readBuffer.Length)) > 0)
    //     {
    //         buffer.Append(readBuffer, 0, totalRead);
    //
    //         // 滑动窗口处理
    //         while (buffer.Length >= ChunkSize)
    //         {
    //             // 提取当前块
    //             string chunk = buffer.ToString(0, ChunkSize);
    //             ChunkCallback?.Invoke(chunk);
    //
    //             // 计算保留的重叠部分
    //             int keep = Math.Min(ChunkOverlap, ChunkSize);
    //             string overlap = buffer.ToString(ChunkSize - keep, keep);
    //         
    //             // 重置缓冲区并保留重叠
    //             buffer.Remove(0, ChunkSize - keep);
    //             buffer.Insert(0, overlap);
    //         }
    //     }
    //
    //     // 处理剩余内容（最后不足chunkSize的部分）
    //     if (buffer.Length > 0)
    //     {
    //         ChunkCallback?.Invoke(buffer.ToString());
    //     }
    // }
    
    public void Chunk()
    {
        using var reader = new StreamReader(InputStream);
        var buffer = new StringBuilder();
        char[] readBuffer = new char[4096];  // 4KB读取缓冲
        int totalRead = 0;

        // 流式读取核心逻辑
        while ((totalRead = reader.ReadBlock(readBuffer, 0, readBuffer.Length)) > 0)
        {
            buffer.Append(readBuffer, 0, totalRead);

            // 滑动窗口处理
            while (buffer.Length >= ChunkSize)
            {
                // 提取当前块
                string chunk = buffer.ToString(0, ChunkSize);
                ChunkCallback?.Invoke(chunk);

                // 移除已处理的内容,保留重叠部分
                buffer.Remove(0, ChunkSize - ChunkOverlap);
            }
        }

        // 处理剩余内容（最后不足chunkSize的部分）
        if (buffer.Length > 0)
        {
            ChunkCallback?.Invoke(buffer.ToString());
        }
    }


    
}