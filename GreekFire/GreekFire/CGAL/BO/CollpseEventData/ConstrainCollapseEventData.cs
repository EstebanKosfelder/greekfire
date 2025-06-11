using System.Diagnostics;
using System.Reflection;

namespace CGAL
{
    using static DebugLog;
    public class ConstrainCollapseEventData : EdgeEventData
    {
        public ConstrainCollapseEventData(Halfedge edge,  double time) : base(edge, CollapseType.ConstraintCollapse, time)
        {
        }

        public override void Handle(GreekFireBuilder builder)
        {
            Log("");
            Log($"{MethodBase.GetCurrentMethod()?.Name}");
            LogIndent();


            Debug.Assert(Type == CollapseType.ConstraintCollapse);
            KineticTriangle t = Triangle;
            double time = Time;
            var edge = this.Edge;

            Debug.Assert(edge.IsConstrain);
            var a = edge.WavefrontEdge.GetCollapse(time, edge);
            Debug.Assert(this == edge.WavefrontEdge.GetCollapse(time, edge));


            Vertex va = edge.Vertex;
            Vertex vb = edge.Next.Vertex;
            Vertex vc = edge.Prev.Vertex;
            va.stop(time);
            vb.stop(time);
            Log($"va: {va.PointAt(time)}");
            Log($"vb: {vb.PointAt(time)}");
            Log($"vc: {vc.PointAt(time)}");

            Debug.Assert(va.PointAt(time).AreNear(vb.PointAt(time)));

            // update prev/next for the DCEL that is the wavefront vertices
            va.set_next_vertex(1, vb, false);

            do_constraint_collapse_part2(builder, edge, time);

            builder.assert_valid( time);

            LogUnindent();
        }
    }


}