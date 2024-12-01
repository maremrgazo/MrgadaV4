﻿#pragma warning disable CS8601 // Possible null reference assignment.
using static Mrgada;

public static partial class Mrgada
{
    public static int NearestDivisible(int i, int x)
    {
        // i => Number for with we are seeking next nearest i % x == 0
        if (i % x == 0)
        {
            return i;
        }
        else
        {
            return ((i / x) + 1) * x;
        }
    }
    public class S7Var<T>
    {
        private Mrgada.S7db _s7db;
        private T _cv;
        private byte[] _cvBytes = new byte[0];
        private int _bitOffset;
        private short _bitAlligment;
        private int _bitsInVar;
        public S7Var(Mrgada.S7db s7db)
        {
            _s7db = s7db;
            _cv = default(T);

            if (typeof(T) == typeof(bool))
            {
                _bitsInVar = 8;
            }
            else if (typeof(T) == typeof(Int16))
            {
                _bitsInVar = 16;
            }
            else if (typeof(T) == typeof(Int32))
            {
                _bitsInVar = 32;
            }

            if (typeof(T) != typeof(bool))
            {
                _cvBytes = new byte[(int)(_bitsInVar / 8)];
            }
        }
        public T CV
        {
            get => _cv;
            set
            {
                SetCV(value);
            }
        }
        public void SetCV(T cv)
        {
            // TODO
        }

        public void ParseCVs()
        {
            Array.Copy(_s7db.Bytes, _bitOffset / 8, _cvBytes, 0, _cvBytes.Length);
            if ((typeof(T) != typeof(bool)) && BitConverter.IsLittleEndian) { Array.Reverse(_cvBytes); }

            if (typeof(T) == typeof(bool))
            {
                _cv = (T)(object)((_cvBytes[0] & (1 << (_bitOffset % 8))) != 0);
            }
            else if (typeof(T) == typeof(Int16))
            {
                _cv = (T)(object)BitConverter.ToInt16(_cvBytes, 0);
            }
            else if (typeof(T) == typeof(Int32))
            {
                _cv = (T)(object)BitConverter.ToInt32(_cvBytes, 0);
            }
        }

        public int SetAndIncrementOffset(int bitOffset)
        {

            if (typeof(T) == typeof(bool)) _bitAlligment = 8;
            else if (typeof(T) == typeof(Int16)) _bitAlligment = 16;
            else if (typeof(T) == typeof(Int32)) _bitAlligment = 32;

            _bitOffset = Mrgada.NearestDivisible(bitOffset, _bitAlligment);
            return _bitAlligment + _bitsInVar;
        }
    } 
}