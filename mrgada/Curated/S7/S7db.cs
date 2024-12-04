public static partial class Mrgada 
{
    public class S7db
    {
        public readonly int Num;
        public readonly int Len;
        private byte[] _bytes;
        public byte[] Bytes => _bytes;
        private byte[] _bytesOld;
        private bool b_broadcastFlag;
        public bool BroadcastFlag => b_broadcastFlag;
        private object o_broadcastFlagLock = new();

        public void ResetBroadcastFlag()
        {
            lock(o_broadcastFlagLock) 
            { 
            b_broadcastFlag = false;
            }
        }

        public S7db(int num, int len)
        {
            Num = num;
            Len = len;
            //_s7Plc = _S7Plc;

            _bytes = new byte[len];
            _bytesOld = new byte[len];

            b_broadcastFlag = true;
        }

        public void SetBytes(byte[] bytes)
        {
            //{
            //        bytes = _s7Plc.ReadBytes(S7.Net.DataType.DataBlock, Num, 0, Len);
            //if (!BytesOld.AsSpan().SequenceEqual(Bytes))
            //{
            //    _Acquisitor.AcquisitorBroadcast(Bytes);
            //}
            lock (o_broadcastFlagLock)
            {
                _bytes = bytes;
                if (!_bytes.SequenceEqual(_bytesOld))
                {
                    b_broadcastFlag = true;
                    //short dbNum = (short)this.Num;
                    //byte[] dbNumByteArray = BitConverter.GetBytes(dbNum);

                    //short BroadcastBytesLength = (short)(dbNumByteArray.Length + _bytes.Length + 2);
                    //byte[] BroadcastBytesLengthByteArray = BitConverter.GetBytes(BroadcastBytesLength);

                    //_Acquisitor.AcquisitorBroadcastBytes.AddRange(BroadcastBytesLengthByteArray);
                    //_Acquisitor.AcquisitorBroadcastBytes.AddRange(dbNumByteArray);
                    //_Acquisitor.AcquisitorBroadcastBytes.AddRange(Bytes);
                }
                _bytesOld = _bytes;
            }
        }

        public void OnClientConnect()
        {
            lock(o_broadcastFlagLock) 
            { 
            b_broadcastFlag = true;
            }
        }

        public virtual void ParseCVs()
        {
        }
    }
}