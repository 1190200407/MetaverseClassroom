using System;

public static class AudioCompressor
{
    public static byte[] Compress(float[] audioData)
    {
        byte[] compressedData = new byte[audioData.Length];
        for (int i = 0; i < audioData.Length; i++)
        {
            compressedData[i] = (byte)((audioData[i] + 1f) * 127.5f);
        }
        return compressedData;
    }

    public static float[] Decompress(byte[] compressedData)
    {
        float[] decompressedData = new float[compressedData.Length];
        for (int i = 0; i < compressedData.Length; i++)
        {
            decompressedData[i] = (compressedData[i] / 127.5f) - 1f;
        }
        return decompressedData;
    }
}
