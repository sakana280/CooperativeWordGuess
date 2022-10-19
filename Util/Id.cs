using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace CooperativeWordGuess.Util
{
    public static class Id
    {
        private const string EncodeCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int NumBitsRandomness = 256;

        /// <summary>
        /// Returns a unique and unguessable string.
        /// </summary>
        /// <returns></returns>
        public static string Generate()
        {
            return GenerateUniquePart() + GenerateSecurePart();
        }

        /// <summary>
        /// Return a globally unique string using the <see cref="EncodeCharacters"/>.
        /// </summary>
        /// <returns></returns>
        private static string GenerateUniquePart()
        {
            var uniqueBytes = Guid.NewGuid().ToByteArray();
            return ToBaseX(uniqueBytes);
        }

        public static string ToBaseX(byte[] bytes)
        {
            // Create a BigInteger from the bytes.
            // Appending 0 byte forces a positive number, since the ctor assumes little endian byte ordering.
            bytes = bytes.Append((byte)0).ToArray();
            var number = new BigInteger(bytes);
            Debug.Assert(number.Sign != -1);
            var encoded = new StringBuilder();

            do
            {
                number = BigInteger.DivRem(number, EncodeCharacters.Length, out var index);
                encoded.Append(EncodeCharacters[(int)index]);
            } while (number > 0);

            return encoded.ToString();
        }

        /// <summary>
        /// Return a string with <see cref="NumBitsRandomness"/> bits of randomness using the <see cref="EncodeCharacters"/>.
        /// </summary>
        /// <returns></returns>
        private static string GenerateSecurePart()
        {
            // Number of characters required to achieve the desired amount of randomness.
            var numValidChars = EncodeCharacters.Length;
            var numChars = (int)Math.Ceiling(NumBitsRandomness / Math.Log2(numValidChars));

            // Build string length=numChars of truely random characters.
            var s = new StringBuilder();
            for (int i = 0; i < numChars; i++)
            {
                var n = RandomNumberGenerator.GetInt32(0, numValidChars);
                s.Append(EncodeCharacters[n]);
            }

            return s.ToString();
        }

    }
}
