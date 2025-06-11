using System.Diagnostics;

namespace CGAL
{
    using static DebugLog;
    public class SpokeCollapseEventData : EdgeEventData
    {
        public SpokeCollapseEventData(Halfedge edge, double time, bool allow_collinear = false) : base(edge, CollapseType.SpokeCollapse, time)
        {
        }

        public override void Handle(GreekFireBuilder builder)
        {
            Log($"Handle {this}");
            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            //DBG(//DBG_KT_EVENT) << evnt;

            Debug.Assert(Type == CollapseType.SpokeCollapse);
            KineticTriangle t = Triangle;
            double time = Time;

            var he = Edge;
            if (he == null) throw new GFLException($"{this} has no relevantEdge ");

            Debug.Assert(!he.IsConstrain);

            Vertex va = he.Vertex;// t.vertices[TriangulationUtils.ccw(edge_idx)];
            Vertex vb = he.Next.Vertex;// evnt.RelevantEdge.t.vertices[TriangulationUtils.cw(edge_idx)];
            va.stop(time);
            vb.stop(time);
            Debug.Assert(va.pos_stop() == vb.pos_stop());

            //KineticTriangle n = he.Neighbor;//.neighbors[edge_idx];
            //Debug.Assert(n != null);
            //    int idx_in_n = n.index(t);
            var nhe = he.Opposite;

            //TODO
            //t.neighbors[edge_idx] = null;
            //n.neighbors[idx_in_n] = null;

            do_spoke_collapse_part2(builder, he, time);
            do_spoke_collapse_part2(builder, nhe, time);

            // update prev/next for the DCEL that is the wavefront vertices
            /* actually, nothing to do here, the two do_spoke_collapse_part2 calls did everything. */
            //LOG(WARNING) << __FILE__ << ":" << __LINE__ << " " << "untested code path: DECL linking.";

            builder.assert_valid( time);

            //DBG_FUNC_END(//DBG_KT_EVENT);
        }

     
    }
}