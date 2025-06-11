using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FT = double;
namespace CGAL

{

    public static partial class Mathex
    {

        public static Point2 validate(Point2? value)
        {
            if (value != null) return value.Value;
            throw new ArithmeticOverflowException();
        }

        public static T validate<T>(T? v) where T : class
        {
            if (v ==null) throw new NullReferenceException();
            return v;
        }

        public static T validate<T>(T? v) where T : struct
        {
            if (!v.HasValue) throw new ArithmeticOverflowException();
            return v.Value;
        }

        public static bool handle_assigned<T>(T? value)
        {
            return value != null ;
        }

        public static bool handle_assigned(Vertex? value)
        {
            return value != null && value != Vertex.NULL;
        }


        public static bool handle_assigned(Halfedge? value)
        {
            return value != null && value != Halfedge.NULL;
        }

        public static bool handle_assigned(Face? value)
        {
            return value != null && value != Face.NULL;
        }

        

        public static  Halfedge validate(Halfedge? aH)
        {
            if (aH == null )
                throw new Exception("Incomplete straight skeleton");
            return aH;
        }

        public static Vertex validate(Vertex aV)
        {
            if (aV == null)
                throw new Exception("Incomplete straight skeleton");
            return aV;
        }


        public static bool is_negative(FT x) => sign(x) == -1;

        public static bool is_one(FT x) => are_near(x, 1.0);

        public static bool is_zero(this FT value, double eps = EPS)
        {
            return Math.Abs(value) < eps;
        }

       public static Point2 Min(Point2 a, Point2 b)
        {
           return  new Point2( Math.Min(a.X,b.X), Math.Min(a.Y,b.Y));
        }

        public static Point2 Max(Point2 a, Point2 b)
        {
            return new Point2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }
    }
}
