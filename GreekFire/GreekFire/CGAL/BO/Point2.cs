using CGAL;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using FT = System.Double;
using static CGAL.Mathex;
using System.Globalization;
using System.Diagnostics;
using System.Text.Json.Serialization;


namespace CGAL
{
    public struct Point2
    {

        public static explicit operator Point2(Vector2 pt)=>new Point2 (pt.X, pt.Y);
        public static Point2 NAN = new Point2(FT.NaN, FT.NaN);

        public static Point2 ORIGIN = new Point2(0, 0);

        [JsonInclude]
        public FT X;
        [JsonInclude]
        public FT Y;
        public bool IsNaN=> X== FT.NaN ||Y== FT.NaN;

        public double this[int index]
        {
            get {
                Debug.Assert(index >= 0 && index<=1);
                return index==0? X : Y;
            }
            set
            {
                Debug.Assert(index >= 0 && index <= 1);
                if (index == 0) X = value; else  Y = value  ;
            }

        }
        public Point2(FT x, FT y)
        {
            X = x; Y = y;
        }

        public static bool Collinear(Point2 p, Point2 q, Point2 r)
        {
            return are_near((q.X - p.X) * (r.Y - p.Y), (r.X - p.Y) * (q.Y - p.Y));
        }

      
        public static Point2 operator -(Point2 a, Vector2 b)
        {
            return new Point2() { X = a.X - b.X, Y = a.Y - b.Y };
        }

        public static Point2 operator -(Point2 a, Point2 b)
        {
            return new Point2() { X = a.X - b.X, Y = a.Y - b.Y };
        }

        public static Point2 operator -(Point2 a)
        {
            return new Point2() { X = -a.X, Y = -a.Y };
        }

        public static bool operator !=(Point2 a, Point2 b) => a.Equals(b);

        public static Point2 operator *(Point2 a, FT b)
        {
            return new Point2() { X = a.X * b, Y = a.Y * b };
        }

        public static Point2 operator /(Point2 a, FT b)
        {
            return new Point2() { X = a.X / b, Y = a.Y / b };
        }

        public static Point2 operator *(Point2 a, Point2 b)
        {
            return new Point2() { X = a.X * b.X, Y = a.Y * b.Y };
        }
        public static Point2 operator /(Point2 a, Point2 b)
        {
            return new Point2() { X = a.X / b.X, Y = a.Y / b.Y };
        }

        public static Point2 operator +(Point2 a, Point2 b)
        {
            return new Point2() { X = a.X + b.X, Y = a.Y + b.Y };
        }

        public static Point2 operator +(Point2 a, Vector2 b)
        {
            return new Point2() { X = a.X + b.X, Y = a.Y + b.Y };
        }

        public static bool operator ==(Point2 a, Point2 b) => a.Equals(b);

        public static int Oriented(Point2 p)
        {
            return oriented(p.X) * oriented(p.Y);
        }

        public bool AreNear(Point2 other, FT eps = Mathex.EPS)
        {
            return are_near(X, other.X, eps) && are_near(Y, other.Y, eps);
        }

        public int CompareXY(Point2 other, double eps = Mathex.EPS)
        {
            int r = compare(X, other.X);
            if (r != 0)
            {
                r = compare(Y, other.Y);
            }
            return r;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is Point2 seg)
                return seg.X == X && seg.Y == Y;
            return false;
        }

        public override int GetHashCode()
        {
            var code = new HashCode();
            code.Add(X);
            code.Add(Y);
            return code.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT hw() => 1.0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT hx() => X;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT hy() => Y;

        public bool IsFinite()
        {
            return is_finite(X) && is_finite(Y);
        }

        public FT squared_distance()
        {
            return X * X + Y * Y;
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "({0:F9} {1:F9})", X, Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT x() => X;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FT y() => Y;
    }


};
