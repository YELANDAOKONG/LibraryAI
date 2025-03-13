namespace LibraryAI.Vector;

public static class VectorComparer
{
    public static float CosineSimilarity(float[] vector1, float[] vector2)
    {
        ValidateVectors(vector1, vector2);
        
        float dotProduct = 0f;
        float magnitude1 = 0f;
        float magnitude2 = 0f;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = MathF.Sqrt(magnitude1);
        magnitude2 = MathF.Sqrt(magnitude2);

        return magnitude1 == 0 || magnitude2 == 0 
            ? 0 
            : dotProduct / (magnitude1 * magnitude2);
    }

    public static float EuclideanDistance(float[] vector1, float[] vector2)
    {
        ValidateVectors(vector1, vector2);
        
        float sum = 0f;
        for (int i = 0; i < vector1.Length; i++)
        {
            float diff = vector1[i] - vector2[i];
            sum += diff * diff;
        }
        return MathF.Sqrt(sum);
    }

    public static float ManhattanDistance(float[] vector1, float[] vector2)
    {
        ValidateVectors(vector1, vector2);
        
        float sum = 0f;
        for (int i = 0; i < vector1.Length; i++)
        {
            sum += MathF.Abs(vector1[i] - vector2[i]);
        }
        return sum;
    }

    public static float DotProductSimilarity(float[] vector1, float[] vector2)
    {
        ValidateVectors(vector1, vector2);
        
        float sum = 0f;
        for (int i = 0; i < vector1.Length; i++)
        {
            sum += vector1[i] * vector2[i];
        }
        return sum;
    }

    public static float PearsonCorrelation(float[] vector1, float[] vector2)
    {
        ValidateVectors(vector1, vector2);
        
        float sum1 = 0f, sum2 = 0f;
        float sum1Sq = 0f, sum2Sq = 0f;
        float pSum = 0f;

        for (int i = 0; i < vector1.Length; i++)
        {
            sum1 += vector1[i];
            sum2 += vector2[i];
            sum1Sq += vector1[i] * vector1[i];
            sum2Sq += vector2[i] * vector2[i];
            pSum += vector1[i] * vector2[i];
        }

        float num = pSum - (sum1 * sum2 / vector1.Length);
        float den = MathF.Sqrt(
            (sum1Sq - sum1 * sum1 / vector1.Length) * 
            (sum2Sq - sum2 * sum2 / vector1.Length));

        return den == 0 ? 0 : num / den;
    }

    private static void ValidateVectors(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
        {
            throw new ArgumentException("Vectors must have the same dimension");
        }
    }
}
