#pragma warning disable CS8601 // Possible null reference assignment.
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
        private T _cv;
        private byte[] _cvBytes = new byte[0];
        private int _bitOffset;
        private short _bitAlligment;
        private int _bitsInVar;
        private int _dbNum;
        public S7Var(int dbNum)
        {
            _dbNum = dbNum;
            _cv = default(T);

            if (typeof(T) == typeof(bool))
            {
                _bitsInVar = 1;
            }
            else if (typeof(T) == typeof(Int16))
            {
                _bitsInVar = 16;
            }
            else if (typeof(T) == typeof(Int32))
            {
                _bitsInVar = 32;
            }
            else if (typeof(T) == typeof(float))
            {
                _bitsInVar = 32;
            }

            _cvBytes = new byte[(int)(_bitsInVar / 8)];
            if (typeof(T) == typeof(bool)) _cvBytes = new byte[1];
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

        public void ParseCVs(byte[] bytes)
        {
            try { 
            Array.Copy(bytes, _bitOffset / 8, _cvBytes, 0, _cvBytes.Length);
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
            else if (typeof(T) == typeof(float))
            {
                _cv = (T)(object)BitConverter.ToSingle(_cvBytes, 0);
            }
            }
            catch
            {

            }
        }

        public int AlignAndIncrement(int bitOffset)
        {
            _bitAlligment = 8;
            if (typeof(T) == typeof(bool)) _bitAlligment = 1;
            else if (typeof(T) == typeof(Int16)) _bitAlligment = 16;
            else if (typeof(T) == typeof(Int32)) _bitAlligment = 16;
            else if (typeof(T) == typeof(float)) _bitAlligment = 16;

            _bitOffset = Mrgada.NearestDivisible(bitOffset, _bitAlligment);
            return _bitOffset + _bitsInVar;
        }
    } 
}