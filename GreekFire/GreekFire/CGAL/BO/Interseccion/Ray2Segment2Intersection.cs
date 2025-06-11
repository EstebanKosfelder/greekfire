using static CGAL.DebuggerInfo;

namespace CGAL
{
    public class Ray2Segment2Intersection : Intersection
    {

        public Ray2Segment2Intersection(Ray2 ray, Segment2 seg) { _ray = ray; _seg = seg; }




        Ray2 _ray;
        Segment2 _seg;



        protected override void Build()
        {


            // The non const this pointer is used to cast away const.
            //    if (!do_overlap(_ray.bbox(), _seg.bbox()))
            //        return NO_INTERSECTION;
            Line2 l1 = _ray.supporting_line();
            Line2 l2 = _seg.supporting_line();
            Line_2_Line_2_pair linepair = new Line_2_Line_2_pair(l1, l2);
            switch (linepair.Result)
            {
                case Intersection_results.NO_INTERSECTION:
                    _result = Intersection_results.NO_INTERSECTION;
                    break;
                case Intersection_results.POINT:
                    Points = linepair.Points;
                    _result = (_ray.collinear_has_on(Points[0])
                            && _seg.CollinearHasOn(Points[0]))
                        ? Intersection_results.POINT : Intersection_results.NO_INTERSECTION;
                    break;
                case Intersection_results.LINE:
                    {
                        //typedef RT RT;
                        Point2 start1 = _seg.source();
                        Point2 end1 = _seg.target();
                        Point2 start2 = _ray.source();
                        Point2 minpt, maxpt;
                        var diff1 = end1 - start1;
                        if (Math.Abs(diff1.x()) > Math.Abs(diff1.y())) {

                            if (start1.x() < end1.x())
                            {
                                minpt = start1;
                                maxpt = end1;
                            }
                            else
                            {
                                minpt = end1;
                                maxpt = start1;
                            }
                            if (_ray.direction().to_vector().x() > 0)
                            {
                                if (maxpt.x() < start2.x())
                                {
                                    _result = Intersection_results.NO_INTERSECTION;
                                    break;
                                }
                                if (maxpt.x() == start2.x())
                                {
                                    Points = new[] { maxpt };
                                    _result = Intersection_results.POINT;
                                    break;
                                }
                                if (minpt.x() < start2.x())
                                {
                                    Points = new[] { start2, maxpt };
                                }
                                else
                                {
                                    Points = new[] { _seg.source(), _seg.target() };
                                }
                                _result = Intersection_results.SEGMENT;
                                break;
                            }
                            else
                            {
                                if (minpt.x() > start2.x())
                                {
                                    _result = Intersection_results.NO_INTERSECTION;
                                    break;
                                }
                                if (minpt.x() == start2.x())
                                {
                                    Points = new[] { minpt };
                                    _result = Intersection_results.POINT;
                                    break;
                                }
                                if (maxpt.x() > start2.x())
                                {
                                    Points = new[] { start2, maxpt };
                                }
                                else
                                {
                                    Points = new[] { _seg.source(), _seg.target() };
                                }
                                _result = Intersection_results.SEGMENT;
                                break;
                            }
                        } else
                        {

                            if (start1.y() < end1.y())
                            {
                                minpt = start1;
                                maxpt = end1;
                            }
                            else
                            {
                                minpt = end1;
                                maxpt = start1;
                            }
                            if (_ray.direction().to_vector().y() > 0)
                            {
                                if (maxpt.y() < start2.y())
                                {
                                    _result = Intersection_results.NO_INTERSECTION;
                                    break;
                                }
                                if (maxpt.y() == start2.y())
                                {
                                    Points = new[] { maxpt };
                                    _result = Intersection_results.POINT;
                                    break;
                                }
                                if (minpt.y() < start2.y())
                                {
                                    Points = new[] { start2, maxpt };
                                }
                                else
                                {
                                    Points = new[] { _seg.source(), _seg.target() };
                                }
                                _result = Intersection_results.SEGMENT;
                                break;
                            }
                            else
                            {
                                if (minpt.y() > start2.y())
                                {
                                    _result = Intersection_results.NO_INTERSECTION;
                                    break;
                                }
                                if (minpt.y() == start2.y())
                                {
                                    Points = new[] { minpt };
                                    _result = Intersection_results.POINT;
                                    break;
                                }
                                if (maxpt.y() > start2.y())
                                {
                                    Points = new[] { start2, maxpt };
                                }
                                else
                                {
                                    Points = new[] { _seg.source(), _seg.target() };
                                }
                                _result = Intersection_results.SEGMENT;
                                break;
                            }
                        }
                    }
                default:
                    CGAL_kernel_assertion(false); // should not be reached:
                    break;
            }

        }


    }
}