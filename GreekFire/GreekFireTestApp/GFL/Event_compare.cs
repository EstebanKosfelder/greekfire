namespace CGAL
{
    // Event compare for the main queue

    // TODO TOCHECK inverse result;
    public class Event_compare : IComparer<CGAL.Event>
    {
        public Event_compare(StraightSkeletonBuilder aBuilder) { mBuilder = aBuilder; }

        private StraightSkeletonBuilder mBuilder;
        public int Compare(CGAL.Event? x, CGAL.Event? y)
        {
            if (x == null)
            {
                if (y == null) { return 0; }
                else { return -1; }
            }
            if (y == null) { return -1; }

            return (int)mBuilder.CompareEvents(x, y);
        }
    }
}