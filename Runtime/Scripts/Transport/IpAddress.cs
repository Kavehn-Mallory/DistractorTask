using System;

namespace DistractorTask.Transport
{
    [Serializable]
    public struct IpAddress
    {
        public int p0;
        public int p1;
        public int p2;
        public int p3;

        public IpAddress(int p0, int p1, int p2, int p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
        

        public override string ToString()
        {
            return $"{p0}.{p1}.{p2}.{p3}";
        }
    }
}