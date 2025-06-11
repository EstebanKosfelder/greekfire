using CGAL;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FT = System.Double;

namespace CGAL
{
    public struct Vector2
    {

        public FT X;
        public FT Y;
        public double Length()=>Mathex.sqrt(this.squared_length());

        public Vector2(FT ahx, FT ahy)
        {
            X = ahx; Y = ahy;
        }

        public Vector2(FT ahx, FT ahy, FT ahw)
        {
            if (ahw == 1.0)
            {
                X = ahx; Y = ahy;
            }
            else
            {
                X = ahx / ahw; Y = ahy / ahy;
            }
        }

        public Vector2(Point2 p, Point2 q)
        {
            X = q.x() - p.x();
            Y = q.y() - p.y();
        }
        public Double Dot(Vector2 other)
        {
            // dot = cos(a) * this.Len * other.Len
            return this.X * other.X + this.Y * other.Y;
        }

        public Vector2 DotCross(Vector2 v1, Vector2 v2)
        {
            var d1 = v1 - this;
            var d2 = v2 - this;
            return new Vector2(d1.Dot(d2), d1.Cross(d2));
        }
        public Double Cross(Vector2 other)
        {
            // cross = sin(a) * this.Len * other.Len
            return this.X * other.Y - this.Y * other.X;
        }

        public bool IsCollinear(Vector2 other)
        {
            return Mathex.IsZero(Cross(other),Mathex.EPS2) ;
        }
        public Vector2(Vector2 v) : this(v.X, v.Y) { }

        public Vector2(in Direction2 d) : this(d.dx(), d.dy()) { }

        //  public Vector2(in Point2 p, in Origin o) : this(-p.x(), -p.y()) { } 
        public Vector2(in Segment2 s) : this(s.to_vector()) { }

        //public Vector2(in  Origin o, in Point_2 q) :this(q.x(), q.y()){ }
        public Vector2(in Line2 l) : this(l.to_vector()) { }

        public static Vector2 NaN => new Vector2(FT.NaN, FT.NaN);

        public static Vector2 ORIGIN => new Vector2(0, 0);

        public static Point2 Zero => new Point2(0, 0);
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2() { X = a.X - b.X, Y = a.Y - b.Y };
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2() { X = -a.X, Y = -a.Y };
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static Vector2 operator *(Vector2 a, FT b)
        {
            return new Vector2() { X = a.X * b, Y = a.Y * b };
        }

        public static Vector2 operator /(Vector2 a, FT b)
        {
            return new Vector2() { X = a.X / b, Y = a.Y / b };
        }

        public static Vector2 operator *(Vector2 a, Point2 b)
        {
            return new Vector2() { X = a.X * b.X, Y = a.Y * b.Y };
        }
        public static Vector2 operator /(Vector2 a, Point2 b)
        {
            return new Vector2() { X = a.X / b.X, Y = a.Y / b.Y };
        }
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2() { X = a.X + b.X, Y = a.Y + b.Y };
        }

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj != null && obj is Vector2 other)
            {
              return   other.X == X &&  other.Y == Y;
            }
            return base.Equals(obj);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT hw() => 1.0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT hx() => X;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT hy() => Y;

        public Vector2 Normal()
        {
            FT length = Math.Sqrt(X * X + Y * Y);
            return new Vector2(X / length, Y / length);
        }

        public Vector2 perpendicular(OrientationEnum o)
        {


            if (o == OrientationEnum.CLOCKWISE)
            {
                return new Vector2(Y, -X);
            }
            else
            {
                return new Vector2(-Y, X);
            }

        }

        public double squared_length() => X * X + Y * Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT x() => X;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT y() => Y;
    }
}