
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TriangleNet.Geometry;

namespace CGAL
{
    using static DebugLog;
    public partial class KineticTriangle
    {

        List<EventData> Events = new List<EventData>();
        public EventData? CurrentEvent
        {
            get
            {
                Debug.Assert(_eventValid);
                return Events.LastOrDefault();
            }
        }

      

        private EventData _event;
        private bool _eventValid = false;


        public void SetInitialEvent(EventData eventData)
        {
            if( Events.Any() || _eventValid ) throw new GFLException("Ya tiene un evento inicial");
            if (!_eventValid)
            {
                _event = eventData;
                Events.Add(_event);
                _eventValid = true;
            }
           
        }
        public EventData GetCollapse(double time_now)
        {
            Debug.Assert(!IsDead);
            Debug.Assert(!IsDying);
            if (!_eventValid)
            {
                _event = computeCollapse(time_now);
                Events.Add(_event);
                _eventValid = true;

            };
            return getCachedCollapse();
        }

        private EventData getCachedCollapse()
        {
            Debug.Assert(!IsDying);
            Debug.Assert(_eventValid);
            return _event;
        }


        internal EventData RefineCollapseSpec(EdgeEventData c)
        {
            Debug.Assert(_eventValid);
            Debug.Assert(c.AllowsRefinementTo());
            

            _event = c;
            Events.Add(_event);

            return _event;
        }

       

        internal void InvalidateEvent()
        {
            Debug.Assert(!IsDead);
            _eventValid = false;
        }

        private EventData computeCollapse(double time_now)
        {
            Log("");
            Log($"{MethodBase.GetCurrentMethod()?.Name}");
            LogIndent();
            Log($"{this}");
            Log($"time:{time_now}");
            
            var result = ComputeCollapseBounded(time_now);

            LogUnindent();
            return result;
        }
        private EventData ComputeCollapseBounded(double time_now)
        {

            EventData result;

            /** See notes on classification from 20180731 */
            int num_wavefronts = this.WavefrontsCount;
           
            switch (num_wavefronts)
            {
                case 3:
                    result = ComputeCollapseBoundedConstrained3(time_now);
                    break;

                case 2:
                    result = ComputeCollapseBoundedConstrained2(time_now);
                    break;

                case 1:
                    result = ComputeCollapseBoundedConstrained1(time_now);
                    break;

                case 0:
                    result = ComputeCollapseBoundedConstrained0(time_now);
                    break;

                default:
                    throw new Exception($"Invalid number of constrained edges: {num_wavefronts}");
            }
            Debug.Assert(result.Type != CollapseType.Undefined);

            return result;
        } 




        ///<summary>
        /// Compute the collapse spec for a bounded triangle with 0 contraints.
        /// Such a triangle can either see a "meet" event, where two non-incident
        /// vertices become incident, or it can see a flip event where a reflex
        /// vertex moves over a triangluation spoke.
        /// XXX do meet events.
        ///</summary>
        private EventData ComputeCollapseBoundedConstrained0(double time_now)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
          //DBG(//DBG_TRIANGLE) << this;

            Polynomial1D determinant = ComputeDeterminantFromVertices(this.Vertices.ToArray());
            var result = ComputeFlipEvent(time_now, determinant);

            //DBG(//DBG_TRIANGLE) << "returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}

        private EventData ComputeFlipEvent(double time_now, Polynomial1D determinant)
        { 

            Debug.Assert(this.WavefrontsCount == 0);
            EventData result;

            bool could_flip = Vertices.Any(v => v.IsReflexOrStraight);
            if (!could_flip)
            {
                result = new NeverOrInvalidCollapseEventData(this,determinant,time_now,"Todos los vertices son convexos");
            }
            else
            {
                result = GetGenericCollapse(time_now, determinant);
                switch (result.Type)
                {
                    case CollapseType.Never:
                        break;

                    case CollapseType.TriangleCollapse:
                    case CollapseType.SpokeCollapse:
                        //LOG(INFO) << "compute_flip_event() found a triangle/spoke collapse: " << result;
                        break;

                    case CollapseType.VertexMovesOverSpoke:
                      
                        
                        break;

                    //  case CollapseType.FaceHasInfinitelyFastOpposing:
                    //  case CollapseType.FaceHasInfinitelyFastVertexWeighted:
                    //  case CollapseType.CcwVertexLeavesCh:
                    case CollapseType.SplitOrFlipRefine:
                    case CollapseType.ConstraintCollapse:
                    case CollapseType.Undefined:
                    case CollapseType.InvalidEvent:
                    default:
                        {
                            throw new GFLException($"Unexpected result from {nameof(GetGenericCollapse)}: {result}");
                        }
                }
            }

            return result;
        } 

        ///<summary>
        /// Compute the collapse spec for a bounded triangle with 1 contraint.
        ///</summary>
        private EventData ComputeCollapseBoundedConstrained1(double time_now)
        {
            Log("");
            Log($"{MethodBase.GetCurrentMethod()?.Name}");
            LogIndent();

           EventData result ;

            // XXX only compute the determinant if we need it
            //
            Polynomial1D determinant = ComputeDeterminantFromVertices(this.Vertices.ToArray());
            
            Log($"det(time): {determinant.evaluate( time_now)}");
            Debug.Assert(determinant.evaluate(time_now) >= 0.0);

           // var he = this.Halfedges.Where(he => he.IsConstrain).First();
            // int c_idx = wavefronts[0] != null ? 0 : wavefronts[1] != null ? 1 : 2;

            Debug.Assert(Halfedges.Where(he => he.IsConstrain).Count() == 1);

            Halfedge wfHe = Halfedges.Where(he => he.IsConstrain).First();


            WavefrontEdge wf = wfHe.WavefrontEdge;

            if (wf.ParallelEndpoints(time_now))
            {
                EdgeCollapseSpec edge_collapse = wf.GetEdgeCollapse(time_now);

                Log($"Edge endpoints are parrallel;  collapse is {edge_collapse} det degree is {determinant.Degree}");
                
                if (edge_collapse.Type == EdgeCollapseType.Always)
                {
                    /* Edge collapses right now */
                    result =  EventData.CreateEventData(edge_collapse, wfHe);
                }
                else if (determinant.Degree == 1)
                {
                    result = ComputeSplitOrFlipEventBoundedConstrained1(time_now, wfHe, determinant);
                }
                else
                {
                    result = new NeverCollapseEventData(this,"is paralell wf, not allway determinant deg !=1"  );
                }
            }
            else
            {
                var candidate = wf.GetCollapse(time_now, wfHe);
                Log($"Edge collapse is {candidate}; determinant degree is {determinant.Degree}");
                Debug.Assert(candidate.Type == CollapseType.ConstraintCollapse || candidate.Type == CollapseType.Never);

                if (determinant.Degree == 2)
                { // The edge could collapse, or we could flip/split
                    bool have_collapse = (candidate.Type == CollapseType.ConstraintCollapse)
                                        && AcceptCollapseBoundedConstrained1(candidate.Time, determinant, true);

                    if (have_collapse)
                    {
                        Log($"We like the edge collapse.");
                        result = candidate;
                    }
                    else
                    {
                        Log($"We did not like the edge collapse.  Hunt for the real event.");
                        result = ComputeSplitOrFlipEventBoundedConstrained1(time_now, wfHe, determinant);
                    }
                }
                else
                {
                    Debug.Assert(determinant.Degree <= 1);
                    if (candidate.Type == CollapseType.Never)
                    {
                        Log($"Determinant: {determinant}");
                        Log($"Determinant degree < 2 and non-parallel endpoints of the constraint which will never collapse (so would have collapsed in the past).");
                        result = ComputeSplitOrFlipEventBoundedConstrained1(time_now, wfHe, determinant);
                    }
                    else
                    {
                        Debug.Assert(candidate.Type == CollapseType.ConstraintCollapse);
                        Log($"Determinant degree < 2 and non-parallel endpoints of the constraint which will collapse.  We will use the constraint collapse.");
                        result = candidate;
                    }
                }
            }
            Log($" result: {result}");
            LogUnindent();
            return result; 
        } 

        /** find potential split or flip_event.
         *
         * We have a triangle with exactly one constraint, e, and opposite vertex v.
         * We don't like the collapse of e.  This can happen if e has
         * two parallel endpoints (so it does not collapse) or it witnesses
         * the wrong root/zero of the determinant polynomial of degree two.
         *
         * So check
         *  - if v crashes into e, or
         *  - if a vertex incident to e moves over a spoke (as v moves over the
         *    supporting line of e).
         */

        private EventData ComputeSplitOrFlipEventBoundedConstrained1(double time_now, Halfedge wfHe, Polynomial1D determinant)
        {

            Log("");
            Log($"{MethodBase.GetCurrentMethod()?.Name}");
            LogIndent();

            EventData result  ;

            Debug.Assert(wfHe.IsConstrain);
            Debug.Assert(this.WavefrontsCount == 1);


            /* If all of the vertices are convex, this can't happen. */

            if (this.Vertices.All(v => v.angle ==   EAngle.Convex))
            {
                Log($"{this} all convex vertices.  Will never see an event.", ConsoleColor.White,ConsoleColor.Red);
                result = new  NeverOrInvalidCollapseEventData(this, determinant,  time_now, "{this} all convex vertices.  Will never see an event.");
            }
            else

            {
                var wf = wfHe.WavefrontEdge;
                WavefrontSupportingLine e = wf.SupportLine;
                Vertex v = wfHe.Vertex;
              

                (var collapse_time, var vertex_on_line_type) = GetTimeVertexOnSupportingLine(v, e);
  
                switch (vertex_on_line_type)
                {
                    case VertexOnSupportingLineType.Once:
                        if (collapse_time > time_now)
                        {
                            Log($" v will hit supporting line of e at time {collapse_time}");
                            result = new SplitOrFlipRefineEventData(wfHe, collapse_time);
                        }
                        else if (collapse_time == time_now)
                        {
                            Log($" v is on the supporting line of e right now {collapse_time}");
                            if (determinant.Degree == 2)
                            {
                                Log($" determinant degree 2");
                                if (AcceptCollapseBoundedConstrained1(collapse_time, determinant, false))
                                {
                                    Log($" Will want to handle this.");
                                    result = new SplitOrFlipRefineEventData(wfHe, collapse_time);
                                }
                                else
                                {
                                    Log($" But the triangle is growing.",ConsoleColor.Yellow);
                                    result = new NeverCollapseEventData(this," But the triangle is growing.");
                                }
                            }
                            else
                            {
                                Debug.Assert(determinant.Degree == 1);
                                var sign = (ESign)determinant.sign();

                                Log($" determinant degree 1, sign {sign}");
                                if (sign == ESign.Negative)
                                {
                                    Log($"  Will want to handle this.");
                                    result = new SplitOrFlipRefineEventData(wfHe, collapse_time);
                                }
                                else
                                {
                                    Log($"Untested code path.",ConsoleColor.Red);
                                    result = new NeverCollapseEventData(this, "Untested code path.");
                                }
                            }
                        }
                        else
                        {
                            Log($" v will not hit supporting line of e.",ConsoleColor.White,ConsoleColor.Red);
                            result = new  NeverOrInvalidCollapseEventData(this, determinant, time_now, " v will not hit supporting line of e.");
                            //     Debug.Assert(result.type() == CollapseType.NEVER); // XXX if this holds, we can drop the event_that_will_not_happen thing
                        }
                        break;

                    case VertexOnSupportingLineType.Never:
                        Log($"v will never hit supporting line of e as they have the same speed", ConsoleColor.Red);
                        result = new NeverCollapseEventData(this, "v will never hit supporting line of e as they have the same speed");
                        break;

                    case VertexOnSupportingLineType.Always:
                        Log($" v is on the supporting line of e and just as fast.  Event now.");
                        result = new SplitOrFlipRefineEventData(wfHe, time_now);
                            
                        break;
                    default:
                        result = new NeverOrInvalidCollapseEventData(this,determinant,time_now, $"{vertex_on_line_type} no manejado.");
                        break;
                }
            }
            
            LogUnindent();
            return result;
        }

        /** check if we like a specific collapse as an event.
         *
         * This checkfs whether the collapse at time is really the next instance
         * that the triangle collapses.
         *
         * The area of the triangle as a function of time is proportional to the
         * determinant and is a quadratic in time.  Together with the sign of the
         * leading coefficient of the determinant we can evaluate the derivative of the
         * determinant at time t to see whether this is the next time the determinant
         * vanishes.
         *
         * Unchecked precondition: right *now* (where we try to find the next event time),
         * the determinant of the triangle is not negative.  That is, it's a valid (or
         * degenerate) triangle.
         *
         * If the leading coefficient of det is
         *  - negative, then this means one triangle collapse was in the past already,
         *    and any event we found must be a real one.
         *  - positive, then we are either before or after the time when the area is
         *    negative.
         *    In such cases, we never want the second time (since it'd mean we came
         *    from an invalid triangulation to begin with), only the first.  So, to
         *    verify if the collapse time is the first or the second event, we
         *    look at the sign of the determinant's derivative evaluated at t.
         *    If it's negative, the collapse is the first instance of the triangle
         *    collapsing, otherwise, if the derivative at t is positive, the collapse
         *    is the second instance that the triangle collapses and we'll have
         *    to look for a real event prior.
         *    If the derivative is zero, then this is the only event this triangle
         *    will ever see.  Handle it.
         */

        private static bool AcceptCollapseBoundedConstrained1(double collapse_time, Polynomial1D determinant, bool collapse_time_is_edge_collapse)
        {
            Log("");
            Log($"{MethodBase.GetCurrentMethod()?.Name}");
            LogIndent();

            bool result;

            Debug.Assert(determinant.Degree == 2);
            var determinant_sign = (ESign)(determinant.sign());
            Debug.Assert(determinant_sign != ESign.Zero);

            if (determinant_sign == ESign.Negative)
            {
                Log("Sign is negative, event must be good.  (One collapse was in the past, there only is one more.)");
                result = true;
            }
            else
            {
                Debug.Assert(determinant_sign == ESign.Positive);
                Log("Sign is positive, checking if we got the first or second event.");

                Polynomial1D derivative = determinant.differentiate();
                double derivative_at_collapse = derivative.evaluate(collapse_time);
                Log($"derivative(t): {derivative_at_collapse}");

                switch ((ESign)(Mathex.sign(derivative_at_collapse)))
                {
                    case ESign.Zero:
                        Log("Derivative is zero.  If an edge collapses right now, then either the triangle collapses entirely, or the 3rd vertex moves over our supporting line right now.  Of course it could also just be that the vertices are collinear exactly once.");
                        if (collapse_time_is_edge_collapse)
                        {
                            Log("At any rate, this is an edge collapse and the only event the triangle will ever see.  Handle it.");
                        }
                        else
                        {
                            Log("At any rate, since the sign of the determinant is positive, the triangle has positive area after this event, and this is not an edge collapse: we do not need to do anything here.");
                        }
                        result = collapse_time_is_edge_collapse;
                        break;

                    case ESign.Negative:
                        Log("Derivative is negative.  This is the first time the triangle collapses.  We want it.");
                        result = true;
                        break;

                    case ESign.Positive:
                        Log("Derivative is positive.  This is the second time the triangle collapses.  This triangle MUST change before the first time it collapses.");
                        result = false;
                        break;

                    default:
                        throw new Exception("Fell through switch which should cover all cases.");
                }
            }

            
            LogUnindent();
            return result;
        } // }}}



        ///<summary>
        /// Compute the collapse spec for a bounded triangle with 2 contraints.
        ///
        /// Each (constrained) edge collapse witnesses the vanishing of the triangle
        /// and thus one of the roots of the triangle's determinant.
        ///
        /// If they collapse at the same time, this is a triangle collapse.  If not,
        /// then this is an edge event.
        ///</summary>
        private EventData ComputeCollapseBoundedConstrained2(double time_now)
        { 
            EventData result ;

            Halfedge[] wavefrontsEdges = this.Halfedges.Where(h => h.IsConstrain).ToArray();
            Debug.Assert(wavefrontsEdges.Length == 2);

            //int c1_idx = wavefronts[0] != null ? 0 : 1;
            //int c2_idx = wavefronts[2] != null ? 2 : 1;


            
            Debug.Assert(wavefrontsEdges[0].IsConstrain);
            Debug.Assert(wavefrontsEdges[1].IsConstrain);

            //DBG(//DBG_TRIANGLE) << "v0: " << vertices[0].details();
            //DBG(//DBG_TRIANGLE) << "v1: " << vertices[1].details();
            //DBG(//DBG_TRIANGLE) << "v2: " << vertices[2].details();
            //DBG(//DBG_TRIANGLE) << "wavefront idx 1: " << c1_idx;
            //DBG(//DBG_TRIANGLE) << "wavefront idx 2: " << c2_idx;
            //DBG(//DBG_TRIANGLE) << "wavefront 1: " << *wavefronts[c1_idx];
            //DBG(//DBG_TRIANGLE) << "wavefront 2: " << *wavefronts[c2_idx];
            EventData c1 = (wavefrontsEdges[0].WavefrontEdge.GetCollapse(time_now, wavefrontsEdges[0]));
            EventData c2 = (wavefrontsEdges[1].WavefrontEdge.GetCollapse(time_now, wavefrontsEdges[1]));
            Debug.Assert(c1.Type == CollapseType.ConstraintCollapse || c1.Type == CollapseType.Never);
            Debug.Assert(c2.Type == CollapseType.ConstraintCollapse || c2.Type == CollapseType.Never);
            //DBG(//DBG_TRIANGLE) << "constraint collapse 1: " << c1;
            //DBG(//DBG_TRIANGLE) << "constraint collapse 2: " << c2;
            if (c1.Type == CollapseType.Never)
            {
                result = c2;
            }
            else if (c1.Type == CollapseType.Never)
            {
                result = c1;
            }
            else if (c1 == c2)
            { /* both constraints collapse at this time. */
                result = new TriangleCollapseEventData(this , c1.Time);
            }
            else
            {
                result = c1.CompareTo(c2) < 1 ? c1 : c2;
            }
           

            //DBG(//DBG_TRIANGLE) << "returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
          
            return result;
        } // }}}





        ///<summary>
        /// Compute the collapse spec for a bounded triangle with 3 contraints.
        ///
        /// Since all 3 edges are constrained, this can only be a triangle
        /// collapse.  This happens when all 3 edges collapse at the same time.
        ///</summary>
        private EventData ComputeCollapseBoundedConstrained3(double time_now)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
          //DBG(//DBG_TRIANGLE) << this;

            Debug.Assert(this.WavefrontsCount == 3);
            var hes = this.Halfedges.ToArray();

            EventData candidate = (hes[0].WavefrontEdge.GetCollapse(time_now, hes[0]));
            for (int i = 1; i < 3; ++i)
            {
                Debug.Assert(candidate == hes[i].WavefrontEdge.GetCollapse(time_now, hes[i]));
            };
            Debug.Assert(candidate.Type == CollapseType.ConstraintCollapse);
            var result = new  TriangleCollapseEventData(this, candidate.Time);

            //DBG(//DBG_TRIANGLE) << "returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}


    }
}
