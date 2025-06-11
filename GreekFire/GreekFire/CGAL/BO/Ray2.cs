using static CGAL.Mathex;
using static CGAL.DebuggerInfo;
using FT = double;

namespace CGAL
{
    public class Ray2
    {
        public Ray2(in Point2 sp, in Point2 secondp)
        {
            Source = sp;
            SecondPoint = secondp;
        }

        public  Ray2(in Point2 sp, in Vector2 vector) : this(sp, new Point2(sp.X + vector.X, sp.Y + vector.Y)) { }

        public  Ray2(in Point2 sp, in Direction2 d) : this(sp, d.to_vector()) { }

        public Point2 Source;
        public Point2 SecondPoint;

        public Point2 source() => Source;
        public Point2 second_point() => SecondPoint;

        public bool is_degenerate() => Source == SecondPoint;

        //        translate
        //        Point_2
        //   operator()( const Point_2& p, const Vector_2& v) const
        //    {
        //      Construct_point_2 construct_point_2;
        //      return construct_point_2(p.x() + v.x(), p.y() + v.y());
        //    }

        //    Point_2
        //    operator()( const Origin& , const Vector_2& v) const
        //    {
        //      Construct_point_2 construct_point_2;
        //      return construct_point_2(v.x(), v.y());
        //}

        public Ray2(Point2 p, Vector2 v) : this(p, new Point2(p.X + v.X, p.Y + v.Y)) { }

        public Ray2(Point2 p, Direction2 d) : this(p, d.to_vector()) { }

        public Ray2(Point2 p, Line2 l) : this(p, l.to_vector()) { }

        public Ray2(in Ray2 r) : this(r.Source, r.SecondPoint) { }

        /*
        : RRay_2(typename R::Construct_ray_2()(Return_base_tag(), sp, secondp)) {}

      Ray_2(const Point_2 &sp, const Direction_2 &d)
        : RRay_2(typename R::Construct_ray_2()(Return_base_tag(), sp, d)) {}

      Ray_2(const Point_2 &sp, const Vector_2 &v)
        : RRay_2(typename R::Construct_ray_2()(Return_base_tag(), sp, v)) {}

      Ray_2(const Point_2 &sp, const Linel)
        : RRay_2(typename R::Construct_ray_2()(Return_base_tag(), sp, l)) {}
        */

        /*
  decltype(auto)
 */

        public Point2 point(FT i)
        {
            CGAL_kernel_precondition(i >= 0);

            if (i == (0.0)) return source();
            if (i == (1.0)) return second_point();

            return Source + (SecondPoint - Source) * i;
        }
        public Point2 start() => source();

        public bool is_horizontal() => are_near(Source.Y, SecondPoint.Y);
        public bool is_vertical() => are_near(Source.Y, SecondPoint.Y);

        public Direction2 direction() => new Direction2(to_vector());

        public Vector2 to_vector() => new Vector2(SecondPoint.X - Source.X, SecondPoint.Y - Source.Y);

        // public  bool has_on(Point_2 p)
        //  {
        //return are_near( p, source()) || collinear2(source(), p, second_point()) &&
        //       Direction_2(construct_vector(source(), p)) == direction());
        //  }

        public bool collinear_has_on(Point2 p)
        {
            Point2 source = Source;
            Point2 second = SecondPoint;
            switch ((CompareResultEnum)compare_x(source, second))
            {
                case CompareResultEnum.SMALLER:
                    return (CompareResultEnum)compare_x(Source, p) != CompareResultEnum.LARGER;

                case CompareResultEnum.LARGER:
                    return (CompareResultEnum)compare_x(p, Source) != CompareResultEnum.LARGER;

                default:
                    switch ((CompareResultEnum)compare_y(source, second))
                    {
                        case CompareResultEnum.SMALLER:
                            return (CompareResultEnum)compare_y(source, p) != CompareResultEnum.LARGER;

                        case CompareResultEnum.LARGER:
                            return (CompareResultEnum)compare_y(p, source) != CompareResultEnum.LARGER;

                        default:
                            return true; // p == source
                    }
            } // switch
        }

        //Ray_2
        //opposite() const
        //{
        //  return Ray_2( source(), - direction() );
        //}

        public Line2 supporting_line()
        {
            return new Line2(source(), second_point());
        }
        /*
  bool
  operator ==(const Ray_2& r) const
  {
    return R().equal_2_object()(*this, r);
  }

  bool
  operator !=(const Ray_2& r) const
  {
    return !(*this == r);
  }

  Ray_2
  transform(const Aff_transformation_2 &t) const
  {
    return Ray_2(t.transform(source()), t.transform(second_point()));
  }

        */
    }
};