using System.Collections.Generic;

namespace CGAL
{
    public class AreVerticesEqual : IComparer<Point2>
    {
        public int Compare(Point2 x, Point2 y)
        {
            return x.CompareXY(y);
        }
    }
};
