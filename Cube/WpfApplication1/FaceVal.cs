using System;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable PossibleNullReferenceException

namespace WpfApplication1
{
    public class FaceVal :
        IEquatable<FaceVal>, IComparable<FaceVal>,
        IEquatable<uint>, IComparable<uint>,
        IEquatable<int>, IComparable<int>,
        IEquatable<char>, IComparable<char>
    {
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((FaceVal) obj);
        }
        
        public uint Val { get; }

        public FaceVal()
        {
            Val = 0;
        }

        public FaceVal(int? v)
        {
            if (v.HasValue)
                Val = (uint) v;
            else Val = 0;
        }

        public bool Equals(FaceVal other)
        {
            return Val.Equals(other.Val);
        }

        public static bool operator ==(FaceVal lhs, FaceVal rhs)
        {
            return lhs.Val == rhs.Val;
        }

        public static bool operator !=(FaceVal lhs, FaceVal rhs)
        {
            return lhs.Val != rhs.Val;
        }

        public static implicit operator FaceVal(char v)
        {
            return new FaceVal(v);
        }

        public static implicit operator FaceVal(int v)
        {
            return new FaceVal(v);
        }

        public static implicit operator FaceVal(uint v)
        {
            return new FaceVal((int) v);
        }

        public override int GetHashCode()
        {
            return Val.GetHashCode();
        }

        public bool Equals(uint other)
        {
            return Val.Equals(other);
        }

        public int CompareTo(uint other)
        {
            return Val.CompareTo(other);
        }

        public int CompareTo(int other)
        {
            return Val.CompareTo((uint) other);
        }

        public bool Equals(int other)
        {
            return Val.Equals((uint) other);
        }

        public int CompareTo(FaceVal other)
        {
            return Val.CompareTo(other.Val);
        }

        public bool Equals(char other)
        {
            return Val.Equals(other);
        }

        public int CompareTo(char other)
        {
            return Val.CompareTo(other);
        }

        public override string ToString()
        {
            var str = new StringBuilder();

            var bytes = Marshal.SizeOf(Val);
            var bits = bytes * 8;
            for (var byt = 0; byt < bytes; byt++)
            {
                var mask = (uint) (0xFF << (8 * (bytes - 1 - byt)));
                var shift = bits - 8 - 8 * byt;
                var t = Val & mask;

                if (t == 0) continue;

                var ch = (char) (t >> shift);
                str.Append(ch);
            }

            return str.ToString();
        }
    }
}
