namespace CGAL
{
    public class RefineFlipEventData : FlipEventData
    {
        public RefineFlipEventData(Halfedge edge, double time) : base(edge, CollapseType.RefineFlip, time)
        {
        }
    }
}