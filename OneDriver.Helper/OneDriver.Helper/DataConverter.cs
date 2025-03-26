using System.Text;
using Serilog;
using DataType = OneDriver.Helper.Definitions.DataType;


namespace OneDriver.Helper
{
    public static class DataConverter
    {
        public enum DataError
        {
            NoError = int.MaxValue - 1000,
            InValidData,
            EmptyData,
            UnsupportedDataType,
            InvalidArrayCount
        }

        public static DataError ToByteArray(string[] values, DataType dataType, 
            int lengthInBits, bool isLittleEndian, out byte[] data, int arrayCount = 1)
        {
            data = Array.Empty<byte>();
            if (values.Length != arrayCount)
                return DataError.InvalidArrayCount;

            var bufferList = new List<byte>();

            foreach (string value in values)
            {
                byte[] bytesBuffer = null;

                switch (dataType)
                {
                    case DataType.UINT:
                        bytesBuffer = GetUIntBytes(value, lengthInBits, isLittleEndian);
                        break;

                    case DataType.INT:
                        bytesBuffer = GetIntBytes(value, lengthInBits, isLittleEndian);
                        break;

                    case DataType.BOOL:
                        bytesBuffer = new byte[] { Convert.ToByte(value) };
                        break;

                    case DataType.CHAR:
                    case DataType.Byte:
                        bytesBuffer = Encoding.UTF8.GetBytes(value);
                        break;

                    default:
                        return DataError.UnsupportedDataType;
                }

                if (bytesBuffer != null)
                    bufferList.AddRange(bytesBuffer);
            }

            data = bufferList.ToArray();
            return DataError.NoError;
        }

        private static byte[] GetUIntBytes(string value, int lengthInBits, bool isLittleEndian)
        {
            byte[] result = lengthInBits switch
            {
                8 => new byte[] { Convert.ToByte(value) },
                16 => BitConverter.GetBytes(Convert.ToUInt16(value)),
                32 => BitConverter.GetBytes(Convert.ToUInt32(value)),
                _ => throw new ArgumentException($"Unsupported lengthInBits for UINT: {lengthInBits}")
            };

            if (isLittleEndian)
                Array.Reverse(result);

            return result;
        }

        private static byte[] GetIntBytes(string value, int lengthInBits, bool isLittleEndian)
        {
            byte[] result = lengthInBits switch
            {
                8 => new byte[] { (byte)Convert.ToSByte(value) },
                16 => BitConverter.GetBytes(Convert.ToInt16(value)),
                32 => BitConverter.GetBytes(Convert.ToInt32(value)),
                _ => throw new ArgumentException($"Unsupported lengthInBits for INT: {lengthInBits}")
            };

            if (isLittleEndian)
                Array.Reverse(result);

            return result;
        }





        public static DataError ToString(byte[] data, DataType dataType, int lengthInBits, bool isLittleEndian,
            out string value)
        {
            List<string> valueList = new List<string>();
            value = null;
            if (data == null)
                return DataError.EmptyData;
            if (data.Length < 1)
                return DataError.EmptyData;
            value = Encoding.UTF8.GetString(Array.ConvertAll(data, x => x));
            return DataError.NoError;
        }

        public static DataError ToNumber(byte[] _data, DataType dataType, int lengthInBits, bool isLittleEndian,
            out string[] values)
        {
            List<string> valueList = new List<string>();
            values = null;
            if (_data == null)
                return DataError.EmptyData;
            if (_data.Length < 1)
                return DataError.EmptyData;

            byte[] data = Array.ConvertAll(_data, x => x);
            byte[] temp;


            switch (dataType)
            {
                case DataType.UINT:
                    switch (lengthInBits)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            temp = new byte[sizeof(byte)];
                            for (int i = 0; i < data.Length; i += sizeof(byte))
                                valueList.Add(data[i].ToString());

                            break;
                        case 16:
                            temp = new byte[sizeof(ushort)];
                            try
                            {
                                for (int i = 0; i < data.Length; i += sizeof(ushort))
                                {
                                    Array.Copy(data, i, temp, 0, sizeof(ushort));
                                    if (isLittleEndian)
                                        Array.Reverse(temp);
                                    valueList.Add(BitConverter.ToUInt16(temp, 0).ToString());
                                }
                            }
                            catch (Exception e)
                            {
                                return DataError.InValidData;
                            }

                            break;

                        case 32:

                            temp = new byte[sizeof(uint)];
                            for (int i = 0; i < data.Length; i += sizeof(uint))
                            {
                                Array.Copy(data, i, temp, 0, sizeof(uint));
                                if (isLittleEndian)
                                    Array.Reverse(temp);
                                valueList.Add(BitConverter.ToUInt32(temp, 0).ToString());
                            }

                            break;
                    }

                    break;
                case DataType.INT:

                    switch (lengthInBits)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            temp = new byte[sizeof(sbyte)];
                            for (int i = 0; i < data.Length; i += sizeof(sbyte))
                                valueList.Add(((sbyte)data[i]).ToString());
                            break;
                        case 16:
                            temp = new byte[sizeof(short)];
                            for (int i = 0; i < data.Length; i += sizeof(short))
                            {
                                Array.Copy(data, i, temp, 0, data.Length);
                                if (!isLittleEndian)
                                    Array.Reverse(temp);
                                valueList.Add(BitConverter.ToInt16(temp, 0).ToString());
                            }

                            break;

                        case 32:

                            temp = new byte[sizeof(int)];
                            for (int i = 0; i < data.Length; i += sizeof(int))
                            {
                                Array.Copy(data, i, temp, 0, data.Length);
                                if (!isLittleEndian)
                                    Array.Reverse(temp);
                                valueList.Add(BitConverter.ToInt32(temp, 0).ToString());
                            }

                            break;
                    }

                    break;
                case DataType.Float32:
                    try
                    {
                        temp = new byte[sizeof(float)];
                        for (int i = 0; i < data.Length; i += sizeof(float))
                        {
                            Array.Copy(data, i, temp, 0, temp.Length);
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(temp);
                            valueList.Add(BitConverter.ToSingle(temp, 0).ToString());
                        }
                    }
                    catch (Exception)
                    {
                        Log.Error("Invalid data");
                        return DataError.InValidData;
                    }

                    break;
                case DataType.BOOL:
                    for (int i = 0; i < data.Length; i += sizeof(bool))
                        valueList.Add(data[i].ToString());
                    break;
            }

            values = valueList.ToArray<string>();
            return DataError.NoError;
        }

        public static string MaskByteArray(byte[] buffer, int offset, int lengthInBits, DataType dataType,
            bool isLittleEndian)
        {
            ToNumber(buffer, dataType, buffer.Length * 8, isLittleEndian, out var vals);
            string value = "";
            int mask = (1 << lengthInBits) - 1 << offset;
            value = ((Convert.ToInt32(vals[0]) & mask) >> offset).ToString();
            return value;
        }
        public static bool ConvertTo<T>(T value, out string result)
        {
            try
            {
                if (value == null)
                {
                    result = string.Empty;
                    return false;
                }

                if (value is uint || value is int || value is float || value is string || value is double)
                {
                    result = value.ToString();
                    return true;
                }
                else if (value is int[] intArray)
                {
                    result = string.Join(";", intArray);
                    return true;
                }
                else if (value is float[] floatArray)
                {
                    result = string.Join(";", floatArray);
                    return true;
                }
                else
                {
                    result = string.Empty;
                    return false;
                }
            }
            catch
            {
                result = string.Empty;
                return false;
            }
        }
        public static bool ConvertTo<T>(string value, out T returnValue)
        {
            try
            {
                if (typeof(T) == typeof(int))
                {
                    returnValue = (T)(object)int.Parse(value);
                    return true;
                }
                else if (typeof(T) == typeof(uint))
                {
                    returnValue = (T)(object)uint.Parse(value);
                    return true;
                }
                else if (typeof(T) == typeof(float))
                {
                    returnValue = (T)(object)float.Parse(value);
                    return true;
                }
                else if (typeof(T) == typeof(double))
                {
                    returnValue = (T)(object)double.Parse(value);
                    return true;
                }
                else if (typeof(T) == typeof(string))
                {
                    returnValue = (T)(object)value;
                    return true;
                }
                else if (typeof(T) == typeof(int[]))
                {
                    returnValue = (T)(object)value.Split(';').Select(int.Parse).ToArray();
                    return true;
                }
                else if (typeof(T) == typeof(float[]))
                {
                    returnValue = (T)(object)value.Split(';').Select(float.Parse).ToArray();
                    return true;
                }
                else
                {
                    returnValue = default;
                    return false;
                }
            }
            catch
            {
                returnValue = default;
                Log.Error("Type is invalid. Actual value: " + value + ", could not be converted to " + typeof(T));
                return false;
            }
        }
    }
}

