using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sk_d748_c_superuser_provider
{
    public static class CRC
    {
        public static byte[] CfgFileDecryCRC(byte[] data)
{
    //big endian
    uint magic = BitConverter.ToUInt32(new byte[] { data[3], data[2], data[1], data[0] }, 0);
    uint size = BitConverter.ToUInt32(new byte[] { data[11], data[10], data[9], data[8] }, 0);
    uint compSize = BitConverter.ToUInt32(new byte[] { data[15], data[14], data[13], data[12] }, 0);
    
    Console.WriteLine($"cfgFileDecryCRC magic=0x{magic:x} size=0x{size:x} comp_size=0x{compSize:x}");

    int offset = 60;  //offset
    using (MemoryStream buffer = new MemoryStream())
    {
        while (true)
        {
            //every 12 bytes are header
            byte[] blockHeader = new byte[12];
            Array.Copy(data, offset, blockHeader, 0, 12);
            //parse data
            uint blockSize = BitConverter.ToUInt32(new byte[] { 
                blockHeader[3], blockHeader[2], blockHeader[1], blockHeader[0] }, 0);
            uint compressedSize = BitConverter.ToUInt32(new byte[] { 
                blockHeader[7], blockHeader[6], blockHeader[5], blockHeader[4] }, 0);
            uint nextOffset = BitConverter.ToUInt32(new byte[] { 
                blockHeader[11], blockHeader[10], blockHeader[9], blockHeader[8] }, 0);

            //get data chunk
            byte[] compressedBlock = new byte[compressedSize];
            Array.Copy(data, offset + 12, compressedBlock, 0, compressedSize);

            //decompress data
            using (MemoryStream compressedStream = new MemoryStream(compressedBlock))
            using (MemoryStream decompressedStream = new MemoryStream())
            {
                using (ZLibStream decompressionStream = new ZLibStream(compressedStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedStream);
                }
                buffer.Write(decompressedStream.ToArray(), 0, (int)blockSize);
            }

            //Check data chunk
            if (nextOffset == 0)
            {
                return buffer.ToArray();
            }
            offset = (int)nextOffset;
        }
    }
}
    }
}
