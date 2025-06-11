using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RT = double;
using static CGAL.Mathex;
using static CGAL.DebuggerInfo;
using static CGAL.Intersection;

namespace CGAL
{

    public class Line_2_Line_2_pair : Intersection
    {

        public Line_2_Line_2_pair(Line2 line1, Line2 line2)
        { _line1 = line1; _line2 = line2; }

        protected Line2 _line1;
        protected Line2 _line2;



        protected override void Build()
        {

            RT nom1, nom2, denom;
            // The non const this pointer is used to cast away const.
            denom = _line1.a() * _line2.b() - _line2.a() * _line1.b();
            if (denom == (RT)0)
            {
                if ((RT)(0) == (_line1.a() * _line2.c() - _line2.a() * _line1.c()) &&
                    (RT)(0) == (_line1.b() * _line2.c() - _line2.b() * _line1.c()))
                    _result = Intersection_results.LINE;
                else
                    _result = Intersection_results.NO_INTERSECTION;

            }
            nom1 = (_line1.b() * _line2.c() - _line2.b() * _line1.c());
            if (!is_finite(nom1))
            {
                _result = Intersection_results.NO_INTERSECTION;
            }
            nom2 = (_line2.a() * _line1.c() - _line1.a() * _line2.c());
            if (!Mathex.is_finite(nom2))
            {
                _result = Intersection_results.NO_INTERSECTION;
            }

            if (!construct_if_finite(out var _intersection_point, nom1, nom2, denom))
            {
                _result = Intersection_results.NO_INTERSECTION;

            }
            Points = new Point2[] { _intersection_point };
            _result = Intersection_results.POINT;

        }


        bool construct_if_finite(out Point2 pt, RT x, RT y, RT w)
        {
            CGAL_kernel_precondition(is_finite(x)
                           && is_finite(y)
                           && w != (RT)0);

            var xw = x / w;
            var yw = y / w;
            if (!is_finite(xw) || !is_finite(yw))
            {
                pt = new Point2(double.NaN, double.NaN);
                return false;
            }
            pt = new Point2(xw, yw);
            return true;
        }
        /*
inline
Boolean
do_intersect(Line_2 l1,
             Line_2 l2
{
  typedef Line_2_Line_2_pair<K> pair_t;
  pair_t pair(&l1, &l2);
  return pair.intersection_type() != pair_t::NO_INTERSECTION;
}

result_type
intersection(Line_line1,
             Line_2 line2,
             K)
{
    Line_2_Line_2_pair is_t;
    is_t = linepair(&line1, &line2);
    switch (linepair.intersection_type()) {
    case is_t::NO_INTERSECTION:
    default:
      return intersection_return<Intersect_2, Line_2, Line_2>();

    case is_t::POINT:
        return intersection_return<Intersect_2, Line_2, Line_2>(linepair.intersection_point());

    case is_t::LINE:
        return intersection_return<Intersect_2, Line_2, Line_2>(line1);
    }
}

template <class R, class POINT, class RT>
bool construct_if_finite(POINT &pt, RT x, RT y, RT w, R &, const Cartesian_tag &)
{
  typename R::Construct_point_2 construct_point;
    typedef typename R::FT FT;
    CGAL_kernel_precondition(CGAL_NTS is_finite(x)
                             && CGAL_NTS is_finite(y)
                             && w != RT(0));

    FT xw = FT(x)/FT(w);
    FT yw = FT(y)/FT(w);
    if (!CGAL_NTS is_finite(xw) || !CGAL_NTS is_finite(yw))
        return false;
    pt = construct_point(xw, yw);
    return true;
}

template <class R, class POINT, class RT>
inline
bool
construct_if_finite(POINT &pt, RT x, RT y, RT w, const R &r)
{
  typedef typename R::Kernel_tag Tag;
  Tag tag;
  return construct_if_finite(pt, x, y, w, r, tag);
}

Point_2 intersection_point()
{
    if (_result == UNKNOWN)
        intersection_type();
    CGAL_kernel_assertion(_result == POINT);
    return _intersection_point;
}

Line_2 intersection_line()
{
    if (_result == UNKNOWN)
        intersection_type();
    CGAL_kernel_assertion(_result == LINE);
    return *_line1;
}
} // namespace internal
} // namespace Intersections
} // namespace CGAL

        */
    }
}