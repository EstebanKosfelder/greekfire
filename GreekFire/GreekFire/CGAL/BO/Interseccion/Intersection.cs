namespace CGAL
{
    public abstract class Intersection {
        public enum Intersection_results { NO_INTERSECTION, POINT, LINE, SEGMENT, UNKNOWN }

        public Intersection_results Result {
            get
            {
                if (_result == Intersection_results.UNKNOWN) Build();
                return _result;
            }
        }

        protected Intersection_results _result = Intersection_results.UNKNOWN;


        protected abstract void Build();


        public Point2[] Points { get; protected set; } = new Point2[0];




    }
}