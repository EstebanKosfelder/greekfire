using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CGAL
{
    using FT = double;
    public struct Direction2 : IComparer<Direction2>, IComparable<Direction2>
    {
        public Direction2(FT x, FT y)
        {
            DX = x;
            DY = y;
        }

        public Direction2(Point2 v): this(v.X, v.Y)
        {
        }
        public Direction2(Vector2 v): this(v.X, v.Y)
        {
        }
        public Vector2 to_vector()=>new Vector2(DX, DY);


        public FT DX;


        public FT DY;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT dx()=>DX;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT dy()=>DY;


        public  Point2 ToPoint2()=> new Point2(DX, DY);
        public FT this[int index]
        {
            get
            {
                if (!((index == 0) || (index == 1)))
                {
                    throw new IndexOutOfRangeException();
                }

                return (index == 0) ? DX : DY;
            }
            set
            {
                if (!((index == 0) || (index == 1)))
                {
                    throw new IndexOutOfRangeException();
                }
                if (index == 0)
                    DX = value;
                else
                    DY = value;
            }
        }

        public int Dimension
        {
            get { return 2; }
        }

        public override bool Equals(Object? obj)
        {
            if (  obj is  Direction2 other )
              return DX.Equals(other.DX) && DY.Equals(other.DY);
            return false;
        }

        public static bool operator ==(Direction2? a, Direction2? b)
        {
            if (a == null)
            {
                if (b == null)
                {
                    return true;
                }
                return false;
            }
            else if (b == null)
            {
                return false;
            }
            return a.Equals(b);
        }

        public static bool operator !=(Direction2? a, Direction2? b)
        {
            return !(a == b);
        }

        public static bool operator <(Direction2 a, Direction2 b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator <=(Direction2 a, Direction2 b)
        {
            return a.CompareTo(b) < 1;
        }

        public static bool operator >(Direction2 a, Direction2 b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator >=(Direction2 a, Direction2 b)
        {
            return a.CompareTo(b) > -1;
        }

        public static Direction2 operator -(Direction2 a)
        {
            return new Direction2(-a.DX, -a.DY);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region IComparer implementation

        public int Compare(Direction2 x, Direction2 y)
        {
            return Mathex.compare_angle_with_x_axis(x.DX, x.DY, y.DX, y.DY);
        }

        #endregion IComparer implementation

        #region IComparable implementation

        public int CompareTo(Direction2 other)
        {
            return Mathex.compare_angle_with_x_axis(this.DX, this.DY, other.DX, other.DY);
        }

        #endregion IComparable implementation

      

        public static bool Counterclockwise_in_between(Direction2 p, Direction2 q, Direction2 r)
        {
            if (q < p)
                return (p < r) || (r <= q);
            else
                return (p < r) && (r <= q);
        }

        public static bool Counterclockwise_at_or_in_between_2(Direction2 p, Direction2 q, Direction2 r)
        {
            return p == q || p == r || Counterclockwise_in_between(p, q, r);
        }
    }
};
