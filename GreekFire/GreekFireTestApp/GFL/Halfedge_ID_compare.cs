namespace CGAL
{
    public class Halfedge_ID_compare : IComparer<Halfedge>
    {
        public int Compare(Halfedge? aA, Halfedge? aB)
        {
            if (aA == null) throw new ArgumentNullException("x");
            if (aB == null) throw new ArgumentNullException("y");

            return -(aA.Id.CompareTo(aB.Id));
        }
    };
}