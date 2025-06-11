namespace CGAL

{
    public class NeverOrInvalidCollapseEventData : NeverCollapseEventData
    {
        public Polynomial1D Determinant { get; private set; }
        public Halfedge? Edge { get; private set; } = null;
        public bool IsValid { get; private set; }
        public NeverOrInvalidCollapseEventData(KineticTriangle triangle, Polynomial1D determinant, double time,string message) : base(triangle,time, message )
        {
            Determinant = determinant;
            IsValid = Validate();
        }

        public NeverOrInvalidCollapseEventData( Halfedge edge , Polynomial1D determinant, double time,string message) : base((edge.Face is KineticTriangle t)?t:throw new InvalidCastException(), time,message)
        {
            Determinant = determinant;
            IsValid = false;
        }

        private bool Validate()
        {
            double collapse_time;
            bool has_collapse = KineticTriangle.GetGenericCollapseTime(Time, Determinant, out collapse_time);
            return !has_collapse;
        }
    }
}