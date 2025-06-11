namespace CGAL
{
    public class Polygon
    {
        public int ID { get; private set; }
        public bool Hole { get; private set; }
        public int StartIndex { get; private set; }
        public int Count { get; private set; }

        public Polygon(int id, int startIdx, int count, bool hole)
        {
            ID = id; StartIndex = startIdx; Count = count; Hole = hole;
        }

        public IEnumerable<Vertex> Vertices(List<Vertex> vector) => vector.Skip(StartIndex).Take(Count);
    }
}
