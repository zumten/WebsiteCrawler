using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking
{
    public class SocketWriter
    {
        private readonly Stream _stream;

        public SocketWriter(Stream stream)
        {
            _stream = stream;
        }

        public void Write(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Lit du contenu du stream jusqu'à ce que le caractère LineFeed soit
        /// rencontré.
        /// </summary>
        /// <returns>Contenu extrait sous forme d'une chaîne de caractères.</returns>
        public void WriteLine(string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text + "\r\n");
            _stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Lit du contenu du stream jusqu'à ce que le caractère LineFeed soit
        /// rencontré.
        /// </summary>
        /// <returns>Contenu extrait sous forme d'une chaîne de caractères.</returns>
        public string ReadLine()
        {
            byte[] bytes = new byte[1024];
            int offset = 0;
            for (int val = _stream.ReadByte(); val != -1; val = _stream.ReadByte())
            {
                bytes[offset++] = (byte)val;
                if (val == '\n')
                    break;
            }

            return Encoding.ASCII.GetString(bytes, 0, offset).Trim('\r', '\n');
        }

        /// <summary>
        /// Lit le stream en utilisant la syntaxe Chunked transfer encoding.
        /// Voir: http://en.wikipedia.org/wiki/Chunked_transfer_encoding
        /// </summary>
        /// <returns>Contenu extrait du stream.</returns>
        public byte[] ReadByChunks()
        {
            List<byte[]> listResults = new List<byte[]>();

            int nBytes = 0, nTotalBytes = 0;
            string lengthLine = ReadLine();
            int length = Convert.ToInt32(lengthLine.Trim(), 16);

            while (length > 0)
            {
                byte[] buffer = Read(length);
                nTotalBytes += length;
                listResults.Add(buffer);

                ReadLine();
                lengthLine = ReadLine();
                length = Convert.ToInt32(lengthLine.Trim('\r'), 16);
            }

            ReadLine();

            byte[] finalBuffer = new byte[nTotalBytes];
            foreach (byte[] result in listResults)
            {
                Array.Copy(result, 0, finalBuffer, nBytes, result.Length);
                nBytes += result.Length;
            }

            return finalBuffer;
        }

        /// <summary>
        /// Lit le stream jusqu'à ce qu'un paquet de taille 0 soit reçu. Toutes les données sont ensuite
        /// combinées à l'intérieur d'un seul et même byte array.
        /// </summary>
        /// <returns>Contenu extrait du stream.</returns>
        public byte[] ReadToEnd()
        {
            List<byte[]> listResults = new List<byte[]>();

            byte[] buffer = new byte[10240];
            int nBytes, nTotalBytes = 0;

            while ((nBytes = _stream.Read(buffer, 0, 10240)) != 0)
            {
                nTotalBytes += nBytes;
                byte[] bytes = new byte[nBytes];
                Array.Copy(buffer, bytes, nBytes);
                listResults.Add(bytes);
            }

            buffer = new byte[nTotalBytes];
            nBytes = 0;
            foreach (byte[] result in listResults)
            {
                Array.Copy(result, 0, buffer, nBytes, result.Length);
                nBytes += result.Length;
            }

            return buffer;
        }

        /// <summary>
        /// Lit le stream pour une taille prédéterminée
        /// </summary>
        /// <param name="length">Longueur à lire</param>
        /// <returns>Contenu extrait du stream</returns>
        public byte[] Read(int length)
        {
            byte[] bytes = new byte[length];
            int nBytes, nTotalBytes = 0;

            StringBuilder sb = new StringBuilder();
            while (nTotalBytes < length && (nBytes = _stream.Read(bytes, nTotalBytes, Math.Min(10240, length - nTotalBytes))) > 0)
            {
                nTotalBytes += nBytes;
                sb.Append(Encoding.ASCII.GetString(bytes, 0, nBytes));
            }

            return bytes;
        }
    }
}
