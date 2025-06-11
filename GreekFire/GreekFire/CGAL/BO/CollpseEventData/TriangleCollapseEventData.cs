using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace CGAL

{
    using static DebugLog;
    public class TriangleCollapseEventData : TriangleEventData
    {
        public TriangleCollapseEventData(KineticTriangle triangle, double time) : base(triangle, CollapseType.TriangleCollapse, time)
        {
        }

        public override void Handle(GreekFireBuilder builder)
        {
            Log($"Handle {this}");


            double time = Time;

                // int num_constraints = t.is_constrained(0).ToInt() + t.is_constrained(1).ToInt() + t.is_constrained(2).ToInt();
                //DBG(//DBG_KT_EVENT) << "have " << num_constraints << " constraints";

                Triangle.set_dying();

                foreach (var v in Triangle.Vertices)
                {
                    v.stop(time);
                }
                
                var va = Triangle.Vertices.FirstOrDefault()?? throw new NullReferenceException($"{Triangle} no tiene Vertices");
                var pa = va.PointAt(time);
                var pstopa = va.pos_stop();
                foreach ( var vo in Triangle.Vertices.Skip(1))
                {
                    Debug.Assert( pa == vo.PointAt(time));
                    Debug.Assert(pstopa == vo.pos_stop());
                }
                foreach (var he in Triangle.Halfedges)
                {
                    if (he.IsConstrain)
                    {
                        
                        Debug.Assert(he.WavefrontEdge == he.Next.Vertex.incident_wavefront_edge(1));
                        Debug.Assert(he.WavefrontEdge == he.Prev.Vertex.incident_wavefront_edge(0));

                        he.WavefrontEdge.set_dead();

                        // update prev/next for the DCEL that is the wavefront vertices
                        
                        he.Prev.Vertex.set_next_vertex(0, he.Next.Vertex, false);
                    }
                    else
                    {
                        // from the other triangle's point of view, a spoke collapsed.  deal with that.
                        var nhe = he.Opposite;
                        KineticTriangle n = (KineticTriangle) nhe.Face;
                        Debug.Assert(n != null);
                        //int idx_in_n = n.index(t);

                        //t.neighbors[i] = null;
                        //n.neighbors[idx_in_n] = null;
                        he.IsCollapse = true;
                        nhe.IsCollapse = true;   

                        do_spoke_collapse_part2( builder, nhe, time);
                    }
                }
               builder.EventQueue.needs_dropping(Triangle);

              //  assert_valid(t.component, time);

                //DBG_FUNC_END(//DBG_KT_EVENT);
            
        }
    }
}