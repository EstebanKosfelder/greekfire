using System.Diagnostics;

namespace CGAL
{
    using static DebugLog;
    public class VertexMovesOverSpokeEventData : FlipEventData
    {
        public VertexMovesOverSpokeEventData(Halfedge edge,double longest_spoke, double time) : base(edge, CollapseType.VertexMovesOverSpoke, time)
        {
            Debug.Assert(!edge.IsConstrain);
            LongestSpoke = longest_spoke;
        }
        public double LongestSpoke {  get; private set; }

        public override int CompareTo(EventData? other)
        {
            var result = base.CompareTo(other);
            if (result == 0 )
                if (other is VertexMovesOverSpokeEventData vmosEd) 
            {
                    result = this.LongestSpoke.CompareTo(vmosEd.LongestSpoke);
            }
            return result;
        }

        public override void Handle(GreekFireBuilder builder)
        {
          
                LogIndent();
                Log($"Handle {this}");
            base.Handle(builder);

                LogUnindent();
            
        }


    }
}