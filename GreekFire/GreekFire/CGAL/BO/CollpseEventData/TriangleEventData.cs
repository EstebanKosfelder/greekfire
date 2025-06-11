namespace CGAL

{
    public abstract class TriangleEventData : EventData
    {
        private KineticTriangle _triangle;
        public TriangleEventData(KineticTriangle triangle, CollapseType collapseType,  double time)
          : base(collapseType, time)
        {
            _triangle = triangle;
        }

        public override KineticTriangle Triangle => _triangle;
    }
}