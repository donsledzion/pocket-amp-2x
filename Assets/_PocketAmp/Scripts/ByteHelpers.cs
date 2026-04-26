using System;

namespace SoftAware.PocketAmp
{
    public static class ByteHelpers
    {
        /// <summary>
        /// Converts sbyte[] onto byte[], keeping consistent bit values
        /// </summary>
        public static byte[] ToByteArray(sbyte[] sbytes)
        {
            if (sbytes == null) return null;
            var bytes = new byte[sbytes.Length];
            Buffer.BlockCopy(sbytes, 0, bytes, 0, sbytes.Length);
            return bytes;
        }
    }    
}

