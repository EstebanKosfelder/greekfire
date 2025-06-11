namespace CGAL
{
    public class MultinodeComparer : IComparer<Multinode>
    {
        public int Compare(Multinode? x, Multinode? y)
        {
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            return x.size.CompareTo(y.size);
        }
    }
}