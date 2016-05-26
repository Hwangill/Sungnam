using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Common.Utility
{
    public static class ByteStreamCovert
    {
        public static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }
        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
        public static UInt64 ReverseBytes(UInt64 value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }

        public static T ByteArrayToObject<T>(byte[] arrBytes) where T : class
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            try
            {
                T obj = (T)binForm.Deserialize(memStream);
                return obj;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static byte[] ToByteArray(object source)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                return stream.ToArray();
            }
        }

        //public static byte[] GetBigEndian(T)
        public static byte[] GetBigEndian(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }
    }
    public static class XmlNodeConvert
    {
        public static T ConvertNodeToObject<T>(XmlNode node) where T : class
        {
            MemoryStream strm = new MemoryStream();
            StreamWriter strw = new StreamWriter(strm);
            strw.Write(node.OuterXml);
            strw.Flush();
            strm.Position = 0;
            XmlSerializer ser = new XmlSerializer(typeof(T));

            T resultobj = ser.Deserialize(strm) as T;
            return resultobj;
        }
    }

    public class SubNodeConvert : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            //return destinationType == typeof(object);
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            /*
            FieldInfo fi = m_EnumType.GetField(Enum.GetName(m_EnumType, value));
            DescriptionAttribute dna = Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (dna != null)
                return dna.Description;
            else
                return value.ToString();
            */
            return base.ConvertTo(context, culture, value, destinationType);
        }

    }

    public class EnumTypeConverter : EnumConverter
    {
        private Type m_EnumType;
        public EnumTypeConverter(Type type)
            : base(type)
        {
            m_EnumType = type;
        }
        public static string GetDescriptionFromEnumValue(Enum value)
        {
            DescriptionAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            FieldInfo fi = m_EnumType.GetField(Enum.GetName(m_EnumType, value));
            DescriptionAttribute dna = Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (dna != null)
                return dna.Description;
            else
                return value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            foreach (FieldInfo fi in m_EnumType.GetFields())
            {
                DescriptionAttribute dna = Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if ((dna != null) &&
                    ((string)value == dna.Description))
                    return Enum.Parse(m_EnumType, fi.Name);
            }
            return Enum.Parse(m_EnumType, value as string);

        }
    }
    
    //////RTU 추가
    public static class ConvertUtil
    {
        public static Object convert(string t_type, Object source, int len)
        {
            Object returnValue = source;
            char[] param = new char[1];
            param[0] = '2';
            string[] typeSplit = (t_type.ToUpper()).Split(param);
            int loopCnt = typeSplit.Length;

            if (returnValue == null)
            {
                return returnValue;
            }

            for (int i = 1; i < loopCnt; i++)
            {
                returnValue = convertType(typeSplit[i - 1], typeSplit[i], returnValue, len);
            }

            return returnValue;
        }

        public static Object convertType(string inData, string outData, Object obj, int len)
        {
            Object returnValue = null;

            if ("B".Equals(inData))
            {
                returnValue = convertTypeByte(outData, obj, len);
            }
            else if ("S".Equals(inData))
            {
                returnValue = convertTypeString(outData, obj, len);
            }
            else if ("HS".Equals(inData))
            {
                returnValue = convertTypeHexString(outData, obj, len);
            }
            else if ("I".Equals(inData))
            {
                returnValue = convertTypeInt(outData, obj, len);
            }
            else if ("COUNT".Equals(inData))
            {
                returnValue = convertTypeCount(outData, obj, len);
            }
            else if ("NUM".Equals(inData))
            {
                returnValue = convertTypeNUM(outData, obj, len);
            }
            else
            {
                returnValue = obj;
            }

            return returnValue;
        }
        public static Object convertTypeByte(string outData, Object obj, int len)
        {
            Object returnValue = null;
            if ("B".Equals(outData))
            {
                returnValue = obj;
            }
            else if ("S".Equals(outData))
            {
                returnValue = byteToString((byte[])obj, len);
            }
            else if ("HS".Equals(outData))
            {
                returnValue = byteToHexString((byte[])obj);
            }
            else if ("I".Equals(outData))
            {
                returnValue = byteToInt((byte[])obj, len);
            }
            else if ("COUNT".Equals(outData))
            {
                returnValue = int.Parse(hexStringToCount(byteToHexString((byte[])obj)));
            }
            else if ("NUM".Equals(outData))
            {
                returnValue = HexIntToInt(byteToHexString((byte[])obj));
            }
            else
            {
                returnValue = obj;
            }
            return returnValue;
        }
        public static string byteToString(byte[] bytes,int len)
        {
	        for(int i = 0; i < len; i++)
	        {
	            if(bytes[i] == 0)
	             {
                    return ASCIIEncoding.Default.GetString(bytes, 0, i);
	             }
	        }
            return ASCIIEncoding.Default.GetString(bytes, 0, len);
        }
        public static string byteToHexString(byte bytes_one)
        {
            byte[] bytes = new byte[1];
            bytes[0] = bytes_one;
            return byteToHexString(bytes);
        }
        public static string byteToHexString(byte[] bytes)
        {
            string ReturnString = "";
            for (int i = 0; i < bytes.Length; i++)
            {

                string strTemp = string.Format("{0:X}", bytes[i]);
                if (strTemp.Length < 2)
                    strTemp = "0" + strTemp;
                if (strTemp.Length > 2)
                    strTemp = strTemp.Substring(strTemp.Length - 2);
                ReturnString += strTemp.ToUpper();
                if (i + 1 != bytes.Length)
                    ReturnString += "";
            }
            return ReturnString;
        }
        public static string byteToInt(byte[] bytes, int len)
        {
            int toInt = 0;
            switch (len)
            {
                case 4:
                    toInt |= bytes[3] << 24 & -16777216;
                    toInt |= bytes[2] << 16 & 16711680;
                    goto case 2;
                case 2:
                    toInt |= bytes[1] << 8 & 65280;
                    goto case 1;
                case 1:
                    toInt |= bytes[0] & 255;
                    break;
            }
            return "" + toInt;
        }
        public static string hexStringToCount(string hex)
        {
            string returnValue = "";
            string tmpStr = "";
            if (hex.Length == 2)
            {
                return hex;
            }

            for (int i = hex.Length; i > 0; i = i - 4)
            {

                tmpStr = hex.Substring(i - 4, 4);
                returnValue = tmpStr.Substring(2, 2) + tmpStr.Substring(0, 2) + returnValue;

            }
            return HexIntToInt(returnValue);
        }
        public static string HexIntToInt(string HexNum)
        {
            int count = HexNum.Length;
            int returnValue = 0;
            int hexMult = 1;
            for (int i = 0; i < count; i++)
            {
                hexMult = 1;
                for (int j = count - 1; j > i; j--)
                {
                    hexMult = hexMult * 16;
                }
                try
                {
                    char[] hexChars = HexNum.ToCharArray();
                    returnValue = returnValue + "0123456789ABCDEF".IndexOf(hexChars[i]) * hexMult;
                }
                catch (Exception e)
                {
                    Log.WriteLog(LogLevel.ERROR, e.ToString());
                    return "0";
                }
            }
            return "" + returnValue;
        }
        public static Object convertTypeString(string outData, Object obj, int len)
        {
            Object returnValue = null;
            if ("B".Equals(outData))
            {
                returnValue = Encoding.ASCII.GetBytes((string)obj);
            }
            else if ("S".Equals(outData))
            {
                returnValue = obj;
            }
            else if ("HS".Equals(outData))
            {
                returnValue = byteToHexString(string2byte((string)obj, len));
            }
            else if ("I".Equals(outData))
            {
                returnValue = obj;
            }
            else if ("COUNT".Equals(outData))
            {
                returnValue = obj;
            }
            else if ("NUM".Equals(outData))
            {
                returnValue = HexIntToInt(byteToHexString(Encoding.ASCII.GetBytes((string)obj)));
            }
            else
            {
                returnValue = obj;
            }
            return returnValue;
        }
        public static Object convertTypeHexString(string outData, Object obj, int len)
        {
            Object returnValue = null;
            if ("B".Equals(outData))
            {
                returnValue = hexStringToByte((string)obj);
            }
            else if ("S".Equals(outData))
            {
                returnValue = hexStringToString((string)obj);
            }
            else if ("HS".Equals(outData))
            {
                for (int i = Encoding.ASCII.GetBytes((string)obj).Length; i < 2 * len; i++)
                {
                    obj += "0";
                }
                returnValue = (string)obj;
            }
            else if ("I".Equals(outData))
            {
                returnValue = byteToInt(hexStringToByte((string)obj), len);
            }
            else if ("COUNT".Equals(outData))
            {
                returnValue = hexStringToCount((string)obj);
            }
            else if ("NUM".Equals(outData))
            {
                returnValue = HexIntToInt((string)obj);
            }
            else
            {
                returnValue = obj;
            }
            return returnValue;
        }
        public static Object convertTypeInt(string outData, Object obj, int len)
        {
            Object returnValue = null;
            if ("B".Equals(outData))
            {
                returnValue = intToByte((string)obj, len);
            }
            else if ("S".Equals(outData))
            {
                returnValue = obj;
            }
            else if ("HS".Equals(outData))
            {
                returnValue = byteToHexString((byte[])intToByte((string)obj, len));
            }
            else if ("I".Equals(outData))
            {
                returnValue = obj;
            }
            else if ("COUNT".Equals(outData))
            {
                returnValue = hexStringToCount((string)byteToHexString((byte[])intToByte((string)obj, len))); ;
            }
            else if ("NUM".Equals(outData))
            {
                returnValue = HexIntToInt(byteToHexString((byte[])intToByte((string)obj, len)));
            }
            else
            {
                returnValue = obj;
            }
            return returnValue;
        }
        public static Object convertTypeCount(string outData, Object obj, int len)
        {
            Object returnValue = null;
            if ("B".Equals(outData))
            {
                returnValue = hexStringToByte((string)countToHexString((string)obj, len));
            }
            else if ("S".Equals(outData))
            {
                returnValue = obj;
            }
            else if ("HS".Equals(outData))
            {
                returnValue = countToHexString((string)obj, len);
            }
            else if ("I".Equals(outData))
            {
                returnValue = byteToInt((byte[])hexStringToByte((string)countToHexString((string)obj, len)), len);
            }
            else if ("COUNT".Equals(outData))
            {
                returnValue = obj;
            }
            else if ("NUM".Equals(outData))
            {
                returnValue = HexIntToInt(countToHexString((string)obj, len));
            }
            else
            {
                returnValue = obj;
            }
            return returnValue;
        }
        public static Object convertTypeNUM(string outData, Object obj, int len)
        {
            Object returnValue = null;

            if ("B".Equals(outData))
            {
                returnValue = hexStringToByte(numToHexString((string)obj, len));
            }
            else if ("S".Equals(outData))
            {
                returnValue = obj;
            }
            else if ("HS".Equals(outData))
            {
                returnValue = numToHexString((string)obj, len);
            }
            else if ("I".Equals(outData))
            {
                returnValue = byteToInt(hexStringToByte(numToHexString((string)obj, len)), len);
            }
            else if ("COUNT".Equals(outData))
            {
                returnValue = hexStringToCount(numToHexString((string)obj, len));
            }
            else if ("NUM".Equals(outData))
            {
                returnValue = obj;
            }
            else
            {
                returnValue = obj;
            }

            return returnValue;
        }
        public static byte[] string2byte(string str, int len)
        {
            byte[] bytes = new byte[len];
            byte[] tmpbytes = Encoding.ASCII.GetBytes(str);
            for (int i = 0; i < bytes.Length; i++)
            {

                if (i < tmpbytes.Length)
                {
                    bytes[i] = tmpbytes[i];
                }
                else
                {
                    bytes[i] = 0x00;
                }

            }
            return bytes;
        }
        public static byte[] hexStringToByte(string hex)
        {

            if (hex.StartsWith("0x"))
                hex = hex.Substring(2);
            hex = hex.ToUpper();
            int len = hex.Length / 2;
            byte[] ret = new byte[len];
            int i = 0;
            char[] hexChars;
            hexChars = hex.ToCharArray();
            for (int n = 0; n < len; n++)
            {
                int hiBit = 0;
                int loBit = 0;
                hiBit = "0123456789ABCDEF".IndexOf(hexChars[i++]);
                loBit = "0123456789ABCDEF".IndexOf(hexChars[i++]);
                if (hiBit != -1 && loBit != -1)
                    ret[n] = (byte)(hiBit << 4 & 240 | loBit & 15);
                else
                    ret[n] = 0;
            }
            return ret;
        }
        public static string hexStringToString(string hex)
        {
            byte[] convertBytes = hexStringToByte(hex);
            return byteToString(convertBytes, convertBytes.Length);
        }
        public static byte[] intToByte(string value, int len)
        {
            int numValue = int.Parse(value);
            byte[] arr = new byte[len];

            switch (len)
            {
                case 4:
                    arr[3] = (byte)((numValue & -16777216) >> 24);
                    arr[2] = (byte)((numValue & 16711680) >> 16);
                    goto case 2;
                case 2:
                    arr[1] = (byte)((numValue & 65280) >> 8);
                    goto case 1;
                case 1:
                    arr[0] = (byte)(numValue & 255);
                    break;
            }
            return arr;
        }
        public static string countToHexString(string no, int len)
        {
            int totlen = len * 2;
            string tmpNo = String.Format("{0:X}", int.Parse(no));
            string returnValue = "";
            if (totlen == 2)
            {
                return fillZero(tmpNo, totlen);
            }
            if (totlen % 4 == 0)
            {
                tmpNo = fillZero(tmpNo, totlen);
            }
            else
            {
                tmpNo = fillZero(tmpNo, totlen + (4 - totlen % 4));
            }
            string tmpStr = "";
            for (int i = tmpNo.Length; i > 0; i = i - 4)
            {
                tmpStr = tmpNo.Substring(i - 4, 4);
                returnValue = tmpStr.Substring(2, 2) + tmpStr.Substring(0, 2) + returnValue;
            }
            return returnValue;
        }
        public static string numToHexString(string no, int len)
        {
            int totlen = len * 2;
            string tmpNo = String.Format("{0:X}", int.Parse(no));

            if (totlen == 2)
            {
                return fillZero(tmpNo, totlen);
            }
            if (totlen % 4 == 0)
            {
                tmpNo = fillZero(tmpNo, totlen);
            }
            else
            {
                tmpNo = fillZero(tmpNo, totlen + (4 - totlen % 4));
            }

            return tmpNo;
        }
        public static string fillZero(string Num, int len)
        {
            return fillZero(Num, len, true);
        }
        public static string fillZero(string Num, int len, bool left)
        {
            string returnValue = Num;
            for (int i = Num.Length; i < len; i++)
            {
                if (left)
                {
                    returnValue = "0" + returnValue;
                }
                else
                {
                    returnValue = returnValue + "0";
                }
            }
            return returnValue;
        }
        public static byte[] subBytes(byte[] bytes, int pos, int len)
        {
            byte[] returnvalue = new byte[len];
            for (int i = 0; i < len; i++)
            {
                returnvalue[i] = bytes[pos + i];
            }
            return returnvalue;
        }
    }
}
