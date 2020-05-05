using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TelegramBot.Services.Interfaces;

namespace TelegramBot.Services
{
    class FeistelCipherClassic : IFeistelSipher
    {
        private enum Direction
        {
            Left,
            Right
        }

        public static byte[] _key;
        readonly int _blockSize;
        public IEnumerable<byte[]> _keys
        {
            get
            {
                for (int i = 0; i < 4; i++)
                {
                    yield return CycleShift(_key, (byte)(i * 7), Direction.Right);
                }
            }
        }
        public IEnumerable<byte[]> _keysReverse
        {
            get
            {
                return _keys.Reverse();
            }
        }

        public FeistelCipherClassic ()
        {
            //_key = GenerateKey(4);
            _blockSize = _key.Length * 2;
        }

        public FeistelCipherClassic(byte[] key)
        {
            if (key.Length % 2 != 0)
            {
                throw new Exception("Key size must be multiple of 2 size!");
            }
            _key = key;
            _blockSize = key.Length * 2;
        }

        /*public FeistelCipherClassic(byte[] key) : this(key.Length)
        {
            if(key.Length % 2 != 0)
            {
                throw new Exception("Key size must be multiple of 2 size!");
            }
            _key = key;
            _blockSize = key.Length * 2;
        }*/

        public string CryptText(string plainText, bool isDecrypt = false)
        {
            /*while (plainText.Length % _blockSize != 0)
                plainText += '\0';*/

            var roundKeys = isDecrypt ? _keysReverse : _keys;

            Encoding currentEncoding = Encoding.GetEncoding(1251);

            List<byte> textBytes = new List<byte>(currentEncoding.GetBytes(plainText));

            while (textBytes.Count % _blockSize != 0)
                textBytes.Add(0);

            byte[] plainTextBytes = textBytes.ToArray();

            StringBuilder cryptText = new StringBuilder();

            var blockPosition = 0;

            while (blockPosition < plainTextBytes.Length)
            {
                var blockBytes = new byte[_blockSize];
                Array.Copy(plainTextBytes, blockPosition, blockBytes, 0, _blockSize);
                byte[] blockBytesCr = CryptBlock(blockBytes, roundKeys);

                cryptText.Append(currentEncoding.GetString(blockBytesCr));
                blockPosition += _blockSize;
            }

            return cryptText.ToString().Trim('\0');
        }

        byte[] CryptBlock(byte[] blockBytes, IEnumerable<byte[]> roundKeys)
        {
            foreach (var key in roundKeys)
            {
                //if (_feistelType == FeistelType.ClassicFeistel)
                blockBytes = PerformEncryptionClassic(blockBytes, key, key.SequenceEqual(roundKeys.Last()));
                //else
                //    blockBytes = PerformEncryptionVariant1(blockBytes, key, cryptType);
            }
            return blockBytes;
        }

        byte[] PerformEncryptionClassic(byte[] blockBytes, byte[] roundKey, bool lastRound)
        {
            byte[] leftBlock = new byte[blockBytes.Length / 2];
            byte[] rightBlock = new byte[blockBytes.Length / 2];

            Array.Copy(blockBytes, leftBlock, leftBlock.Length);
            Array.Copy(blockBytes, leftBlock.Length, rightBlock, 0, rightBlock.Length);

            byte[] rightBlockNew = ApplyRoundFunction(leftBlock, roundKey);

            for (uint rightBlockNo = 0; rightBlockNo < leftBlock.Length; ++rightBlockNo)
            {
                rightBlockNew[rightBlockNo] ^= rightBlock[rightBlockNo];
            }

            byte[] resultBlockBytes = new byte[blockBytes.Length];

            if (!lastRound)
            {
                Array.Copy(rightBlockNew, resultBlockBytes, rightBlockNew.Length);
                Array.Copy(leftBlock, 0, resultBlockBytes, leftBlock.Length, rightBlockNew.Length);
            }
            else
            {
                Array.Copy(leftBlock, resultBlockBytes, leftBlock.Length);
                Array.Copy(rightBlockNew, 0, resultBlockBytes, leftBlock.Length, rightBlockNew.Length);
            }
            return resultBlockBytes;
        }

        byte[] ApplyRoundFunction(byte[] blockBytes, byte[] roundKey)
        {
            return XOR(CycleShift(blockBytes, 9, Direction.Left), Inverse(MultiplicationMod2(CycleShift(roundKey, 11, Direction.Right), blockBytes)));
        }

        byte[] MultiplicationMod2(byte[] array1, byte[] array2)
        {
            var result = new byte[array1.Length];
            for(uint i = 0; i < result.Length; i++)
            {
                result[i] = (byte)((array1[i] + array2[i]) % (Math.Pow(2, _blockSize) + 1));
            }
            return result;
        }

        byte[] XOR(byte[] array1, byte[] array2)
        {
            var result = new byte[array1.Length];
            for(uint i = 0; i < result.Length; i++)
            {
                result[i] = (byte)(array1[i] ^ array2[i]);
            }
            return result;
        }

        byte[] Inverse(byte[] array)
        {
            var result = new byte[array.Length];
            for(uint i = 0; i < result.Length; i++)
            {
                result[i] = (byte)~array[i];
            }
            return result;
        }

        byte[] PerformShift(byte[] array, Direction direction)
        {
            var result = new byte[array.Length];
            Array.Copy(array, result, array.Length);
            if(direction == Direction.Left)
            {
                bool tempByte = false;
                bool newTempByte;
                for(int i = result.Length - 1; i >= 0; i--)
                {
                    newTempByte = GetBit(result[i], 7);
                    result[i] = (byte)(result[i] << 1);
                    SetBit(tempByte, ref result[i], 0);
                    tempByte = newTempByte;
                }
            }
            else
            {
                bool tempByte = false;
                bool newTempByte;
                for(int i = 0; i < result.Length; i++)
                {
                    newTempByte = GetBit(result[i], 0);
                    result[i] = (byte)(result[i] >> 1);
                    SetBit(tempByte, ref result[i], 7);
                    tempByte = newTempByte;
                }
            }
            return result;
        }

        byte[] CycleShift(byte[] array, byte count, Direction direction)
        {
            count %= (byte)(array.Length * 8);
            var result = new byte[array.Length];
            Array.Copy(array, result, array.Length);
            while(count > 8)
            {
                result = PerformCycleShift(result, 8, direction);
                count -= 8;
            }
            result = PerformCycleShift(result, count, direction);
            return result;
        }

        byte[] PerformCycleShift(byte[] array, byte count, Direction direction)
        {
                bool[] tempBits;
                bool[] newTempBits;
                if (direction == Direction.Left)
                {
                    tempBits = SaveBits(array[0], count, direction);
                    for (int i = array.Length - 1; i >= 0; i--)
                    {
                        newTempBits = SaveBits(array[i], count, direction);
                        array[i] = (byte)(array[i] << count);
                        PutBits(ref array[i], tempBits, direction);
                        tempBits = newTempBits;
                    }
                }
                else
                {
                    tempBits = SaveBits(array[array.Length - 1], count, direction);
                    for (int i = 0; i < array.Length; i++)
                    {
                        newTempBits = SaveBits(array[i], count, direction);
                        array[i] = (byte)(array[i] >> count);
                        PutBits(ref array[i], tempBits, direction);
                        tempBits = newTempBits;
                    }
                }
                return array;
        }

        void PutBits(ref byte currentByte, bool[] savedBits, Direction direction)
        {
            if (direction == Direction.Left)
            {
                Array.Reverse(savedBits);
                for (int i = 0; i < savedBits.Length; i++)
                {
                    SetBit(savedBits[i], ref currentByte, i);
                }
            }
            else
            {
                Array.Reverse(savedBits);
                for (int i = savedBits.Length - 1; i >= 0; i--)
                {
                    SetBit(savedBits[i], ref currentByte, 7 - i);
                }
            }
        }

        bool[] SaveBits(byte currentByte, byte count, Direction direction)
        {
            bool[] tempArray = new bool[count];
            for (uint i = 0; i < count; i++)
                tempArray[i] = GetBit(currentByte, (direction == Direction.Left) ? (7 - i) : i);
            return tempArray;
        }
        bool GetBit(byte value, uint index)
        {
            if (index > 8)
                throw new Exception("index must be < 8");
            return ((value & (1 << (int)index)) != 0) ? true : false;
        }

        void SetBit(bool bitValue, ref byte container, int index)
        {
            container = (byte)((bitValue) ?
                container | (1 << index) :
                container & (~(1 << index)));
        }

        public static byte[] GenerateKey(int size)
        {
            var result = new byte[size];
            Random random = new Random();
            random.NextBytes(result);
            return result;
        }
    }
}
