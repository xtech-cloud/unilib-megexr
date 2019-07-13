/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

namespace XTC.MegeXR.Core
{
    public static class Convert
    {
        public static byte[] IntToBytes(int _value)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)_value;
            bytes[1] = (byte)(_value >> 8);
            bytes[2] = (byte)(_value >> 16);
            bytes[3] = (byte)(_value >> 24);
            return bytes;
        }

        public static void CopyInt(int _value, byte[] _byte, int _offset)
        {
            _byte[_offset + 0] = (byte)_value;
            _byte[_offset + 1] = (byte)(_value >> 8);
            _byte[_offset + 2] = (byte)(_value >> 16);
            _byte[_offset + 3] = (byte)(_value >> 24);
        }

        public static int BytesToInt(byte[] _bytes, int _start)
        {
            return bytesToInt(_bytes, _start);
        }

        public static string BytesToString(byte[] _bytes, int _start, int _length)
        {
            return bytesToString(_bytes, _start, _length);
        }

        private static int bytesToInt(byte[] _bytes, int _start)
        {
            if (_bytes.Length < _start + 4)
                throw new System.IndexOutOfRangeException();

            int value = 0;
            value |= _bytes[_start];
            value |= _bytes[_start + 1] << 8;
            value |= _bytes[_start + 2] << 16;
            value |= _bytes[_start + 3] << 24;
            return value;
        }

        private static string bytesToString(byte[] _bytes, int _start, int _length)
        {
            if (_bytes.Length < _start + _length)
                throw new System.IndexOutOfRangeException();

            return System.Text.UTF8Encoding.UTF8.GetString(_bytes, _start, _length);
        }
    }
}