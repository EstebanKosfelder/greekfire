namespace CGAL
{
    public class EventPQueue
    {
        List<Event> list;
        IComparer<Event> _comparer;



        public EventPQueue(IComparer<Event> comparer)
        {
            _comparer = comparer ;
            list = new List<Event>();

        }

        public void Enqueue(Event e)
        {
            var index = (list.BinarySearch(e, _comparer));
            if (index < 0)
            {
                index = ~index;
            }
            list.Insert(index, e);
        }

        public Event Dequeue()
        {

            if (Count > 0)
            {
                var e = list[Count - 1];
                list.RemoveAt(Count - 1);
                return e;
            }
            throw new InvalidOperationException($"no tiene elementos");



        }

        public int Count { get { return list.Count; } }
    }
}