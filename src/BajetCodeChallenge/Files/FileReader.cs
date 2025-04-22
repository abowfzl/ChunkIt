namespace BajetCodeChallenge.Files;

public static class FileReader
{
    public static Dictionary<int, Chunk> GetFileChunks(string filePath, int chunkCount)
    {
        var fileInfo = new FileInfo(filePath);
        var fileSize = fileInfo.Length;

        var chunks = new Dictionary<int, Chunk>();

        // Handle where file is smaller than chunk
        if (fileSize < chunkCount)
        {
             chunks.TryAdd(0, new Chunk(0, fileSize));

            return chunks;
        }

        var chunkSize = fileSize / chunkCount;

        for (int i = 0; i < chunkCount; i++)
        {
            long start = i * chunkSize;
            long end = i == chunkCount - 1 ? fileSize : (i + 1) * chunkSize;

            chunks.TryAdd(i, new Chunk(start, end));
        }

        return chunks;
    }
}
