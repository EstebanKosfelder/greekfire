using CGAL;
using System;
using System.Diagnostics.CodeAnalysis;
using FT = System.Double;
using static CGAL.Mathex;
using System.Globalization;

namespace CGAL
{
    public class Segment2
    {
        public static Segment2 NULL { get; private set; }

        static Segment2()
        {
            NULL = new Segment2(Point2.NAN, Point2.NAN, -1);
        }
        public Segment2(Point2 source, Point2 target,int id =-1) 
        {
            Source = source;
            Target = target;
            Id = id;
        }
        public int Id {  get; private set; }
       
        public Point2 Source { get; set; }
        public Point2 Target { get; set; }

#if USING_CACHE
        public Line2? CacheCoeff { get; set; }
        public void ResetCache()
        {
            CacheCoeff = null;
        }
#endif

        internal Point2 source() => Source;

        internal Point2 target() => Target;




        //public static bool AreEdgesCollinear(Segment2 e0, Segment2 e1)
        //{
        //    return Point2.Collinear(e0.source(), e0.target(), e1.source()) && Point2.Collinear(e0.source(), e0.target(), e1.target());
        //}

        //public static bool AreParallelEdgesEquallyOriented(Segment2 e0, Segment2 e1)
        //{
        //    return (oriented(e0.target() - e0.source()) * oriented(e1.target() - e1.source())) == 1;
        //}

        //public static bool AreEdgesOrderlyCollinear(Segment2 e0, Segment2 e1)
        //{
        //    return Segment2.AreEdgesCollinear(e0, e1) && Segment2.AreParallelEdgesEquallyOriented(e0, e1);
        //}

        //public bool AreSimilar(Segment2 other, FT eps = Extensions.EPS)
        //{
        //    return Source.AreNear(other.Source, eps) && Target.AreNear(other.Target, eps);
        //}
        //public bool are_edges_parallel( Segment2 e1)
        //{
        //   var  p0 = Target - Source;
        //   var  p1 = e1.Target - e1.Source;
        //   var a01 = p0.X * p1.Y;
        //   var a10 = p1.Y * p1.Y; 

        //    if ( is_finite( a01) && is_finite(a10))
        //    {
        //        return are_near(a01,a10);
        //    }

        //    throw new IndeterminateValueException();
        //}


     
        public static bool operator == (Segment2 a , Segment2 b)=> a.Equals(b);
        public static bool operator !=(Segment2 a, Segment2 b) => a.Equals(b);

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is Segment2  seg)
                return seg.Source == Source && seg.Target == Target ;    
            return false;
        }
        public override int GetHashCode()
        {
            var code = new HashCode();
            code.Add(Source);
            code.Add(Target);
                return code.ToHashCode();
        }
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "[ {0} {1}]", this.Source,this.Target);
        }

        public Vector2 to_vector()=>new Vector2(Source,Target);










        public Direction2  direction() =>new Direction2(to_vector());
 

    public Line2 supporting_line() => new Line2(this);


        public Segment2 opposite() => new Segment2(Target, Source);

  //Segment_2 transform(const Aff_transformation_2 &t) const
  //{
  //  return Segment_2(t.transform(source()), t.transform(target()));
  //}

        public bool is_horizontal() => are_near( Source.Y,Target.Y );
        public bool is_vertical()=> are_near(Source.X, Target.X);

        public bool has_on(Point2 p) => Mathex.are_ordered_along_line(Source,p,Target);
        public bool CollinearHasOn(Point2 p)
        {
            // Paso 1: Verificar colinealidad
            var ab = new Vector2(Source, Target);
            var ap = new Vector2(Source,p);

            if (!ab.IsCollinear(ap))
                return false;

            // Paso 2: Verificar si p está entre Source y Target
            // Proyectamos sobre X o Y dependiendo de qué eje tenga mayor longitud
            if (Math.Abs(ab.X) > Math.Abs(ab.Y))
            {
                // Proyección en X
                double minX = Math.Min(Source.X, Target.X);
                double maxX = Math.Max(Source.X, Target.X);
                return p.X >= minX - Mathex.EPS && p.X <= maxX + Mathex.EPS;
            }
            else
            {
                // Proyección en Y
                double minY = Math.Min(Source.Y, Target.Y);
                double maxY = Math.Max(Source.Y, Target.Y);
                return p.Y >= minY - Mathex.EPS && p.Y <= maxY + Mathex.EPS;
            }
        }

    }
    
} //namespace CGAL