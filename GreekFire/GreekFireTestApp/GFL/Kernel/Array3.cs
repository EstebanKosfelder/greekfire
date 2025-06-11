using System.Collections;
using System.Linq;

namespace GFL.Kernel
{
    public class Array3Nulleable<T> : IEnumerable<T> where T : class
    {
        public static implicit operator T?[] (Array3Nulleable<T> value)=>value.array;
        public Array3Nulleable()
        {
            array = new T?[3];
        }

        public Array3Nulleable(params T?[] values)
        {
#if !NO_PRECONDITION
            if (values.Length != 3) throw new ArgumentOutOfRangeException(nameof(values));
#endif
            this.array = values;
        }

        internal T?[] array;

        public T? this[int index]
        {
            get
            {
#if !NO_PRECONDITION
                if (index < 0 && index > 2) throw new ArgumentOutOfRangeException(nameof(index));
#endif
                return array[index];
            }
            set
            {
#if !NO_PRECONDITION
                if (index < 0 && index > 2) throw new ArgumentOutOfRangeException(nameof(index));
#endif
                array[index] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T?>)array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }
    }

    public class Array3<T> : IEnumerable<T> where T : class
    {
        public static implicit operator T[](Array3<T> value) => value.array;
        public Array3()
        {
            array = new T[3];
        }

        public Array3(params T[] values)
        {
#if !NO_PRECONDITION
            if (values.Length != 3) throw new ArgumentOutOfRangeException(nameof(values));
#endif
            this.array = values;
        }

        internal T[] array;

        public T this[int index]
        {
            get
            {
#if !NO_PRECONDITION
                if (index < 0 && index > 2) throw new ArgumentOutOfRangeException(nameof(index));
#endif
                return array[index];
            }
            set
            {
#if !NO_PRECONDITION
                if (index < 0 && index > 2) throw new ArgumentOutOfRangeException(nameof(index));
#endif
                array[index] = value;
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }
    }

    public class WavefrontVertex3 : Array3Nulleable<WavefrontVertex>
    {
        public WavefrontVertex3()
        {
        }

        public WavefrontVertex3(params WavefrontVertex[] values) : base(values)
        {
        }
    }

    public class WavefrontEdge3 : Array3<WavefrontEdge>
    {
        public WavefrontEdge3()
        {
        }

        public WavefrontEdge3(params WavefrontEdge[] values) : base(values)
        {
        }
    }

    public class KineticTriangle3 : Array3Nulleable<KineticTriangle>
    {
        public KineticTriangle3()
        {
        }

        public KineticTriangle3(params KineticTriangle[] values) : base(values)
        {
        }
    }

    public class HalfEdge3 : Array3<KineticHalfedge>
    {
        public HalfEdge3(params KineticHalfedge[] values) : base(values)
        {
        }
    }
}