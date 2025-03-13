namespace LibraryAI.Vector;

public class VectorSerializer
{
    
    public static byte[] Serialize(float[] vectors)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(vectors.Length);
        foreach (var vector in vectors)
        {
            writer.Write(vector);
        }
        return stream.ToArray();
    }
    
    public static float[] Deserialize(byte[] vectorBytes)
    {
        using var stream = new MemoryStream(vectorBytes);
        using var reader = new BinaryReader(stream);
        var length = reader.ReadInt32();
        var vectors = new float[length];
        for (var i = 0; i < length; i++)
        {
            vectors[i] = reader.ReadSingle();
        }
        return vectors;
    }
    
    public static string SerializeString(float[] vectors)
    {
        var data = Serialize(vectors);
        return Convert.ToBase64String(data);
    }
    
    public static float[] DeserializeString(string vectorString)
    {
        var data = Convert.FromBase64String(vectorString);
        return Deserialize(data);
    }
    
}