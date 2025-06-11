namespace CGAL

{
    using static DebugLog;
    public class NeverCollapseEventData : TriangleEventData
    {
        public string Message { get; private set; }

        public NeverCollapseEventData(KineticTriangle triangle, string message)
            : this( triangle, 0.0, message) { }
        public NeverCollapseEventData(KineticTriangle triangle , double time,  string message )
            : base(triangle, CollapseType.Never,time)
        {
            Message = message;
        }

        public override void Handle(GreekFireBuilder builder)
        {
            Log($"Handle {this}");
        }
    }
}