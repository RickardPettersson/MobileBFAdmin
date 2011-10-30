using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using BFAdmin.Models;

namespace BFAdmin.Helpers
{
    public class SocketTools
    {
        private static int clientSequenceNr = 0;

        public static string generatePasswordHash(string salt, string password)
        {
            MD5 md5Hasher = MD5.Create();
            List<Byte> x = new List<byte>();
            Byte[] passBytes = Encoding.Default.GetBytes(password);

            for (int i = 0; i < salt.Length; i += 2)
            {
                x.Add(Convert.ToByte(salt.Substring(i, 2), 16));
            }

            foreach (Byte y in passBytes)
            {
                x.Add(y);
            }

            Byte[] data = md5Hasher.ComputeHash(x.ToArray());

            string str = string.Empty;

            for (int i = 0; i < data.Length; i++)
            {
                str += data[i].ToString("x2");
            }

            return str.ToUpper();
        }

        public static Byte[] EncodeClientRequest(string[] words)
        {
            Byte[] packet = EncodePacket(false, false, clientSequenceNr, words);
            clientSequenceNr = (clientSequenceNr + 1) & 0x3FFFFFFF;
            return packet;
        }

        public static Byte[] EncodePacket(bool isFromServer, bool isResponse, int sequence, string[] words)
        {
            Byte[] encodedHeader = EncodeHeader(isFromServer, isResponse, sequence);
            Byte[] encodedNumWords = EncodeInt32((uint)words.Length);
            Object[] x = EncodeWords(words);
            Byte[] encodedSize = EncodeInt32(Convert.ToUInt32(x[0]) + 12);
            List<Byte> temp = new List<Byte>();
            temp.AddRange(encodedHeader);
            temp.AddRange(encodedSize);
            temp.AddRange(encodedNumWords);
            temp.AddRange((Byte[])x[1]);

            return temp.ToArray();
        }


        public static Object[] EncodeWords(string[] words)
        {
            int size = 0;
            List<Byte> encodedWords = new List<byte>();
            string strWord = string.Empty;

            foreach (string word in words)
            {
                strWord = Convert.ToString(word);
                encodedWords.AddRange(EncodeInt32((uint)strWord.Length));

                foreach (Char c in strWord)
                {
                    encodedWords.Add((Byte)Convert.ToInt32(c));
                }

                encodedWords.Add(0);
                size += strWord.Length + 5;
            }

            Object[] result = new Object[] { size, encodedWords.ToArray() };

            return result;
        }

        public static Byte[] EncodeHeader(bool isFromServer, bool isResponse, int sequence)
        {
            uint header = (uint)sequence & 0x3FFFFFFF;
            if (isFromServer)
            {
                header += 0x80000000;
            }
            if (isResponse)
            {
                header += 0x40000000;
            }
            return pack(header);
        }

        public static Byte[] EncodeInt32(uint number)
        {
            return pack(number);
        }

        public static Byte[] pack(uint num)
        {
            return BitConverter.GetBytes(num);
        }

        public static List<string> DecodeWords(int size, byte[] data)
        {
            List<string> words = new List<string>();

            int numWords = (int)DecodeInt32(data);
            int offset = 0;
            uint wordlen;

            while (offset < size)
            {
                string tempStr = string.Empty;
                Byte[] byteArray = new Byte[] { data[offset], data[offset + 1], data[offset + 2], data[offset + 3] };
                wordlen = DecodeInt32(byteArray);

                for (int i = offset + 4; i < (offset + wordlen + 4); i++)
                {
                    tempStr += Chr(data[i]);
                }
                words.Add(tempStr);
                offset += (int)wordlen + 5;
            }

            return words;
        }

        public static char Chr(int n)
        {
            return (char)n;
        }

        public static uint[] DecodeHeader(Byte[] bytes)
        {
            uint x = unpack(bytes);
            uint[] result = new uint[] { x & 0x80000000, x & 0x40000000, x & 0x3FFFFFFF };
            return result;
        }

        public static packetData DecodePacket(Byte[] data)
        {
            uint[] x = DecodeHeader(data);
            Byte[] byteArray = new Byte[] { data[4], data[5], data[6], data[7] };
            int wordSize = (int)DecodeInt32(byteArray) - 12;

            List<Byte> temp = new List<Byte>();

            for (int i = 12; i < data.Length; i++)
            {
                temp.Add(data[i]);
            }

            List<string> words = DecodeWords(wordSize, temp.ToArray());

            return new packetData()
            {
                isFromServer = Convert.ToBoolean(x[0]),
                isResponse = Convert.ToBoolean(x[1]),
                sequence = x[2],
                Words = words
            };
        }


        public static bool containsCompletePacket(List<Byte> data)
        {
            if (data.Count < 8)
            {
                return false;
            }

            Byte[] byteArray = new Byte[] { data[4], data[5], data[6], data[7] };

            if (data.Count < DecodeInt32(byteArray))
            {
                return false;
            }

            return true;
        }

        public static uint DecodeInt32(Byte[] b)
        {
            Byte[] byteArray = new Byte[] { b[0], b[1], b[2], b[3] };
            return unpack(byteArray);
        }

        public static uint unpack(Byte[] bytes)
        {
            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}
