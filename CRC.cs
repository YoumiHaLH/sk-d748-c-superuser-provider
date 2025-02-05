using System.IO.Compression;

namespace sk_d748_c_superuser_provider;

public static class CRC
{
    public static byte[] CfgFileDecryCRC(byte[] data)
    {
        //big endian
        var magic = BitConverter.ToUInt32(new[] { data[3], data[2], data[1], data[0] }, 0);
        var size = BitConverter.ToUInt32(new[] { data[11], data[10], data[9], data[8] }, 0);
        var compSize = BitConverter.ToUInt32(new[] { data[15], data[14], data[13], data[12] }, 0);

        Console.WriteLine($"cfgFileDecryCRC magic=0x{magic:x} size=0x{size:x} comp_size=0x{compSize:x}");

        var offset = 60; //offset
        using (var buffer = new MemoryStream())
        {
            while (true)
            {
                //every 12 bytes are header
                var blockHeader = new byte[12];
                Array.Copy(data, offset, blockHeader, 0, 12);
                //parse data
                var blockSize = BitConverter.ToUInt32(new[]
                {
                    blockHeader[3], blockHeader[2], blockHeader[1], blockHeader[0]
                }, 0);
                var compressedSize = BitConverter.ToUInt32(new[]
                {
                    blockHeader[7], blockHeader[6], blockHeader[5], blockHeader[4]
                }, 0);
                var nextOffset = BitConverter.ToUInt32(new[]
                {
                    blockHeader[11], blockHeader[10], blockHeader[9], blockHeader[8]
                }, 0);

                //get data chunk
                var compressedBlock = new byte[compressedSize];
                Array.Copy(data, offset + 12, compressedBlock, 0, compressedSize);

                //decompress data
                using (var compressedStream = new MemoryStream(compressedBlock))
                {
                    using (var decompressedStream = new MemoryStream())
                    {
                        using (var decompressionStream = new ZLibStream(compressedStream, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(decompressedStream);
                        }

                        buffer.Write(decompressedStream.ToArray(), 0, (int)blockSize);
                    }
                }

                //Check data chunk
                if (nextOffset == 0) return buffer.ToArray();
                offset = (int)nextOffset;
            }
        }
    }
}