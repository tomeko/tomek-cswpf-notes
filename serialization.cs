using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;

namespace tomek_cswpf_notes
{
    public static class serialization
    {
        public enum Endianness
        {
            BigEndian,
            LittleEndian
        }


        public static void WriteToXmlFile<T>(string filePath, T obj, bool append = false) where T : new()
        {
            var dcs = new DataContractSerializer(typeof(T));

            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                XmlWriter xw = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8 });

                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xw))
                {
                    writer.WriteStartDocument();
                    dcs.WriteObject(writer, obj);
                }
            }
        }

        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(File.ReadAllText(filePath))))
            {
                DataContractSerializer formatter0 = new DataContractSerializer(typeof(T));
                return (T)formatter0.ReadObject(reader);
            }

        }

        public static T ReadStruct<T>(string file)
        {

            byte[] dat = File.ReadAllBytes(file);
            GCHandle h = GCHandle.Alloc(dat, GCHandleType.Pinned);
            T t_struct = (T)Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof(T));
            h.Free();
            return t_struct;
        }

        public static void WriteStruct<T>(T t_struct, string file)
        {
            byte[] dat = new byte[Marshal.SizeOf(typeof(T))];
            GCHandle h = GCHandle.Alloc(dat, GCHandleType.Pinned);
            Marshal.StructureToPtr(t_struct, h.AddrOfPinnedObject(), false);
            h.Free();
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(file));
            bw.Write(dat);
            bw.Close();
        }

        private static void MaybeAdjustEndianness(Type type, byte[] data, Endianness endianness, int startOffset = 0)
        {
            if ((BitConverter.IsLittleEndian) == (endianness == Endianness.LittleEndian))
                // nothing to change => return
                return;

            foreach (var field in type.GetFields())
            {
                var fieldType = field.FieldType;
                if (field.IsStatic)
                    // don't process static fields
                    continue;

                if (fieldType == typeof(string))
                    // don't swap bytes for strings
                    continue;

                var offset = Marshal.OffsetOf(type, field.Name).ToInt32();

                // handle enums
                if (fieldType.IsEnum)
                    fieldType = Enum.GetUnderlyingType(fieldType);

                // check for sub-fields to recurse if necessary
                var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();
                var effectiveOffset = startOffset + offset;
                if (subFields.Length == 0)
                    Array.Reverse(data, effectiveOffset, Marshal.SizeOf(fieldType));
                else
                    MaybeAdjustEndianness(fieldType, data, endianness, effectiveOffset);
            }
        }

        internal static T BytesToStruct<T>(byte[] rawData, Endianness endianness) where T : struct
        {
            T result = default(T);

            MaybeAdjustEndianness(typeof(T), rawData, endianness);
            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            try
            {
                IntPtr rawDataPtr = handle.AddrOfPinnedObject();
                result = (T)Marshal.PtrToStructure(rawDataPtr, typeof(T));
            }
            finally
            {
                handle.Free();
            }
            return result;
        }

        internal static byte[] StructToBytes<T>(T data, Endianness endianness) where T : struct
        {
            byte[] rawData = new byte[Marshal.SizeOf(data)];
            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            try
            {
                IntPtr rawDataPtr = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr(data, rawDataPtr, false);
            }
            finally
            {
                handle.Free();
            }

            MaybeAdjustEndianness(typeof(T), rawData, endianness);
            return rawData;
        }

    }

    public static class serialization_extensions
    {
        public static string SerializeBase64(this object value)
        {
            MemoryStream ws = new MemoryStream();
            BinaryFormatter sf = new BinaryFormatter();
            sf.Serialize(ws, value);
            byte[] bytes = ws.GetBuffer();
            return bytes.Length + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
        }

        public static object DeserializeBase64(this string base64)
        {
            int p = base64.IndexOf(':');
            int length = Convert.ToInt32(base64.Substring(0, p));
            byte[] data = Convert.FromBase64String(base64.Substring(p + 1));
            MemoryStream rs = new MemoryStream(data, 0, length);
            BinaryFormatter sf = new BinaryFormatter();
            return sf.Deserialize(rs);
        }
    }
}
