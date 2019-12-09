using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Formatii.Quake1.Wad2
{
    public struct Header
    {
        const int MAGIC_LENGTH  = 4;
        const string WAD2_MAGIC = "wad2";

        public string Magic;
        public int NumEntries;
        public int DirOffset;

        public static Header Read(BinaryReader R)
        {
            var head = new Header()
            {
                Magic = new string(R.ReadChars(MAGIC_LENGTH)),
                NumEntries = R.ReadInt32(),
                DirOffset = R.ReadInt32()
            };

            return head;
        }
    }

    public class Wad2 : IDisposable
    {
        public const int NAME_LEN = 16;

        private BinaryReader Reader = null;

        public Header Header;
        public Entry[] Entries;

        public Wad2(string FilePath)
        {
            Reader = new BinaryReader(File.OpenRead(FilePath));
            Read(Reader);
        }

        public void Dispose()
        {
            if (Reader != null)
            {
                Reader.Close();
                Reader.Dispose();
            }
        }

        public void Read(BinaryReader R)
        {
            Header  = Header.Read(R);
            Entries = new Entry[Header.NumEntries];

            R.BaseStream.Seek(Header.DirOffset, SeekOrigin.Begin);
            for (int i = 0; i < Entries.Length; i++)
            {
                Entries[i] = Entry.Read(R);
            }
        }

        public string GetFileList()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Type            Name             Size ");
            sb.AppendLine("----------------------------------------");
            for (int i = 0; i < Entries.Length; i++)
            {
                var entry = Entries[i];
                sb.AppendLine($"{entry.Type,-15} {entry.Name,-16} {entry.Size:N0}");
            }
            sb.AppendLine("----------------------------------------");

            return sb.ToString();
        }

        public MipTex GetMipTex(Entry Entry)
        {
            Reader.BaseStream.Seek(Entry.Offset, SeekOrigin.Begin);
            var mipTex = MipTex.Read(Reader, Entry);
            return mipTex;
        }

        public enum EntryType : byte
        {
            Palette     = 0x40,
            SBarPic     = 0x42,
            MipsTexture = 0x44,
            ConsolePic  = 0x45
        }

        public struct Entry
        {
            public int Offset;
            public int InWadSize;
            public int Size;
            public EntryType Type;
            public byte Compression;
            public ushort _Unkown;
            public string Name;

            public static Entry Read(BinaryReader R)
            {
                var entry = new Entry()
                {
                    Offset      = R.ReadInt32(),
                    InWadSize   = R.ReadInt32(),
                    Size        = R.ReadInt32(),
                    Type        = (EntryType)R.ReadByte(),
                    Compression = R.ReadByte(),
                    _Unkown     = R.ReadUInt16(),
                    Name        = new string(R.ReadChars(NAME_LEN)).Trim(new char[] { '\0', (char)0 })
                };

                return entry;
            }
        }
    }
}
