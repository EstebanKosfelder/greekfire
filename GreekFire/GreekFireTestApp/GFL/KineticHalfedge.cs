using GFL.Kernel;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace GFL
{
    public class KineticHalfedge : HalfedgeBase
    {
        public override string ToString() => $"K{base.ToString()} {Vertex_.ID}-{Opposite_?.Vertex_?.ID}";

        public KineticHalfedge(int aID, SignEnum aSlope = SignEnum.ZERO) : base(aID, aSlope)
        {
        }

        public IEnumerable<KineticHalfedge> Halfedges => (this as IHalfedge).Circulation(3).Cast<KineticHalfedge>();

        public bool AssertVertex()
        {
            return (Next.Vertex == Opposite_.Vertex_);
            
        }
        public bool AssertFaces()
        {
            return (Halfedges.Select(h=>h.Face).Distinct().Count()==1);

        }
        public bool IsConstrain=> (Opposite_ is WavefrontEdge biHe) ? biHe != null : false;
        public WavefrontEdge WavefrontEdge
        {
            get
            {
                if (!(Opposite_ is WavefrontEdge biHe))
                     throw new InvalidCastException($"{this} Opposite is not {nameof(WavefrontEdge)}");
                return biHe;
            }
        }
       

        public KineticTriangle Neighbor
        {
            get
            {
                if (Opposite_ is KineticHalfedge he)
                {
                    return he.Triangle;
                }
                else
                {
                    throw new GFLException($"{this} is not constrained");
                }
            }
        }

        internal bool is_constrained()
        {
            bool result = !(this.Next.Opposite_ is KineticHalfedge);

            if (result)
            {
                //var h = this.KNext.Opposite;

                //Debug.WriteLine(h.ToString());
            }
            return result;
        }

        public WavefrontVertex Vertex
        {
            get { Debug.Assert(Vertex_ is WavefrontVertex); return (WavefrontVertex)Vertex_; }
            set { Vertex_ = value; }
        }

        public KineticHalfedge Prev
        {
            get { Debug.Assert(Prev_ is KineticHalfedge); return (KineticHalfedge)Prev_; }
            set { Prev_ = value; }
        }

        public KineticHalfedge Next
        {
            get { Debug.Assert(Next_ is KineticHalfedge); return (KineticHalfedge)Next_; }
            set { Next_ = value; }
        }

        public KineticHalfedge OppositeKineticHalfedge
        {
            get => Opposite_ is KineticHalfedge r ? r : throw new InvalidCastException($"{this} opposite es not {nameof(KineticHalfedge)} ");
        }

        public KineticTriangle Triangle => (KineticTriangle)Face;

        public double collapse_time_edge()
        {
            //    Returns the time when the given 2 kinetic vertices are closest to each
            //    other

            //    If the 2 vertices belong to a wavefront edge there are 3 options:
            //    - they cross each other in the past
            //    - they cross each other in the future
            //    - they run parallel, so they never meet
            //    Note, the distance between the two points is a linear function.

            //    If the 2 vertices do not belong to a wavefront edge,
            //    the distance between the two points can be a quadratic or linear function
            //    - they cross each other in the past
            //    - they cross each other in the future
            //    - they run in parallel, so they never cross
            //    - they never cross, but there exists a point in time when they are closest

            //  var logging.debug("edge collapse time for v1 = {} [{}] and v2 = {} [{}]".format(id(v1), v1.info,id(v2), v2.info))

            var v1 = Vertex;
            var v2 = Next.Vertex;
            var s1 = v1.velocity;
            var s2 = v2.velocity;
            var o1 = v1.Point;
            var o2 = v2.Point;
            var dv = (s1 - s2);

            var denominator = dv.Dot(dv);
            //     logging.debug("denominator for edge collapse time {}".format(denominator))
            if (!MathEx.IsZero(denominator))
            {
                var w0 = o2 - o1;
                var nominator = dv.Dot(w0);
                //         logging.debug("nominator for edge collapse time {}".format(nominator))
                var collapse_time = nominator / denominator;
                // logging.debug("edge collapse time: " + str(collapse_time))
                return collapse_time;
            }
            else
            {
                //throw new ApplicationException($" denominador near 0 {v1} - {v2}");
                //logging.debug("denominator (close to) 0")
                //logging.debug("these two vertices move in parallel:")
                //logging.debug(str(v1) + "|" + str(v2))
                //logging.debug("edge collapse time: None (near) parallel movement")
                // any time will do (we pick a time in the past, before the start of our event simulation)
                return -1.0;
            }

            var o = MathEx.orientation(s1, s2);

            if (o == 0/* !=(int) OrientationEnum.LEFT_TURN*/)
            {
                /* If the two wavefront vertices move away from each other
                 * or in parallel, this edge will never collapse.
                 */
                if (o == (int)ETurn.Right)
                {
                    // DBG(DBG_KT) << "Orientation is right turn";
                    // let's consider two points that are identical right now but moving away from another as not collapsing.
                    return -1.0;//   res = new(int)EdgeCollapseSpec(EdgeCollapseType.PAST);
                }
                else
                {/*
                    //  DBG(DBG_KT) << "Orientation is collinear";
                    assert(o == OrientationEnum.COLLINEAR);

                    // Previously we computed the distance at time_now.  I wonder why.
                    // If we then claim that the edge is always collapsing, then it should
                    // suffice to compute the distance at t=0.
                    NT sqdist = squared_distance(wfv0.pos_zero, wfv1.pos_zero);
                    //DBG(DBG_KT) << "sqdist zero: " << CGAL.to_double(sqdist);

                    {
                        Point2 p0 = (wfv0.p_at(time_now));
                        Point2 p1 = (wfv1.p_at(time_now));
                        NT sqdistnow = squared_distance(p0, p1);
                        //  DBG(DBG_KT) << "sqdist now : " << sqdistnow;
                        if (sqdist == 0.0)
                        {
                            assert_expensive_eq(sqdist, sqdistnow);
                        }
                        else
                        {
                            assert(sqdistnow > 0.0);
                        }
                    }

                    if (sqdist == 0.0)
                    {
                        // DBG(DBG_KT) << "Distance is zero now.";
                        res = new EdgeCollapseSpec(EdgeCollapseType.ALWAYS, time_now);
                    }
                    else
                    {
                        // DBG(DBG_KT) << "Distance is not zero.";
                        res = new EdgeCollapseSpec(EdgeCollapseType.NEVER);
                    }
                    */
                    return -1;
                }
            }
            else
            {
                // DBG(DBG_KT) << "Orientation is left turn";

                /* Note that, by construction, if you start on the wavefront edge, go out v0,
                 * and go back v1, you end up on the wavefrong edge again.  Or, in other words,
                 * v0-v1 is collinear with the direction of the wavefront edge e.
                 *
                 * Now, we want to know when e=AB collapses.  So we can restrict ourselves to
                 * consider the projection of A+t*v0 and B+t*v1 to e itself.  Once they meet,
                 * the edge collapses.  Equivalently, once the length of projected t*v0 and
                 * projected t*v1 equals the length of e, the edge collapses.
                 *
                 * Let d be the vector AB (i.e., B-A).  The dot product v0.d is the length of
                 * the projected v0 times the length of d.  So v0.d/|d| is the length of the
                 * projected v0.  Likewise, v1.d/|d| is the length of the projected v1.
                 * Thus e will collapse at time t := |d| / ( v0.d/|d| - v1.d/|d| ) ==
                 * == |d|^2 / ( v0.d - v1.d ) == |d|^2 / ( (v0 - v1).d) ==
                 * == d.d /  ( (v0 - v1).d).
                 *
                 * Isn't that interesting?  Note how we compare the length of d projected
                 * onto d with the length of (v0 - v1) projected onto d?  Remember that, as
                 * previously mentioned, by construction, (v0 - v1) is in the same direction
                 * as d.  Thus, this quotient will have the same value regardless of
                 * which line we project both d and (v0 - v1) -- as long as the line is not
                 * orthogonal to (v0 - v1).  So we might just as well project onto (1,0) and
                 * only consider their x-coordinates (if d is not exactly vertical).
                 * So t == d_x / (v0_x - v1_x)
                 *
                 * This also makes sense when looking at it another way.  Consider the points
                 * of the projection of A+t*v0 and B+t*v1 onto e.  Since they are on e, they
                 * will become incident when and only when their x-coordinates is the same.
                 */
                double edge_delta;
                double wfvs_delta;
                if (o1.Y == o2.Y)
                {
                    edge_delta = o2.X - o1.X;
                    wfvs_delta = s1.X - s2.X;
                }
                else
                { /* l is vertical, do the same with y-coordinates */
                    edge_delta = o2.Y - o1.Y;

                    wfvs_delta = s1.Y - s2.Y;
                }

                // assert(edge_delta != 0);
                // assert(wfvs_delta != 0);
                double time = edge_delta / wfvs_delta;
                //DBG(DBG_KT) << "future edge collapse: " << CGAL.to_double(time);
                //DBG(DBG_KT) << "time_now            : " << CGAL.to_double(time_now);
                return time;
                //assert_ge(time, time_now);
                //res = new EdgeCollapseSpec(EdgeCollapseType.FUTURE, time);
            }
            //DBG(DBG_KT) << "returning " << res;
            //DBG_FUNC_END(DBG_KT);
            return -1;
        }
    }
}