using System;
using System.Linq;
using Xunit;

namespace Lec.Tests
{
    public class IndexOfArrayTests
    {
        [Fact]
        public void index_of_end()
        {
            var bytes = new byte[] { 12,56,43,2,57,12,75,54};
            var subBytes = new byte[] { 75,54};

            Assert.Equal(6, lastIndexOfBytes(subBytes, bytes));
        }
        
        [Fact]
        public void index_of_same()
        {
            var bytes = new byte[] { 12,56,43,2,57,12,75,54};
            var subBytes = new byte[] { 12,56,43,2,57,12,75,54};

            Assert.Equal(0, lastIndexOfBytes(subBytes, bytes));
        }
        
        
        [Fact]
        public void index_of_start()
        {
            var bytes = new byte[] { 12,56,43,2,57,12,75,54};
            var subBytes = new byte[] { 12,56};

            Assert.Equal(0, lastIndexOfBytes(subBytes, bytes));
        }
        
        [Fact]
        public void index_of_middle()
        {
            var bytes = new byte[] { 12,56,43,2,57,12,75,54};
            var subBytes = new byte[] { 43,2 };

            Assert.Equal(2, lastIndexOfBytes(subBytes, bytes));
        }
        
        private long lastIndexOfBytes(byte[] pkHeaderBytes, byte[] bytes)
        {
            if (pkHeaderBytes == null
                || bytes == null
                || pkHeaderBytes.Length == 0 
                || bytes.Length < pkHeaderBytes.Length)
            {
                return -1;
            }

            var curIndex = bytes.Length - pkHeaderBytes.Length;
            do
            {
                var notMatching = pkHeaderBytes.Where((curByte, i) => bytes[curIndex + i] != curByte).Any();
                if (!notMatching)
                {
                    return curIndex;
                }
            } while (--curIndex >= 0);

            return -1;
        }
    }
}
