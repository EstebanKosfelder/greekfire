
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GFL
{

    using System.Collections;


    using static Debugger.DebugLog;



    public class HeapItemBase<PriorityType> where PriorityType :  IComparable<PriorityType>

    {
        //        template< A, B> friend class HeapBase;

        public int idx_in_heap;
        public PriorityType priority;

        public HeapItemBase()
        { idx_in_heap = -1; }

        public HeapItemBase(PriorityType p_priority) : this()
        {
            priority = p_priority;
        }

        public override string ToString()
        {
            return $"[{idx_in_heap}]{priority}";
        }
        private PriorityType get_priority()
        { return priority; }
    }

    public class HeapBase<PriorityType, HeapItemBase>:IEnumerable<HeapItemBase<PriorityType>> where PriorityType :  IComparable<PriorityType>
    {
        protected List<HeapItemBase<PriorityType>> v_;
        private int heap_eq_ctr;

        /** given a node's index i, return this node's parent's index.
         *
         * For the root node this operation is not defined. */

        public int parent_idx(int i)
        {
            Debug.Assert(i >= 0 && i < Count);
            return (i - 1) / 2;
        }

        /** given a node's index i, return this node's left child's index.
         *
         * Note that this child might not exist, i.e. the index might be behond
         * the backing array. */

        public int left_child_idx(int i)
        {
            Debug.Assert(i >= 0 && i < Count);
            return i * 2 + 1;
        }

        /** given a node's index i, return this node's right child's index.
         *
         * Note that this child might not exist, i.e. the index might be behond
         * the backing array. */

        public int right_child_idx(int i)
        {
            Debug.Assert(i >= 0 && i < Count);
            return i * 2 + 2;
        }

        /** swap elements at positions a and b
         */

        public void swap_idx(int a, int b)
        {
            Debug.Assert(a >= 0 && a < Count);
            Debug.Assert(b >= 0 && b < Count);
            Debug.Assert(v_[a].idx_in_heap == a);
            Debug.Assert(v_[b].idx_in_heap == b);
            var aux = v_[a];
            v_[a] = v_[b];
            v_[b] = aux;
            //  swap(ref v_[a], ref v_[b]);
            v_[a].idx_in_heap = a;
            v_[b].idx_in_heap = b;
        }

        private void set_from_idx(int idx, int src)
        {
            Debug.Assert(idx >= 0 && idx < Count);
            Debug.Assert(src >= 0 && src < Count);
            //Debug.Assert(v_[src].idx_in_heap == src);

            v_[idx] = v_[src];
            v_[idx].idx_in_heap = idx;
        }

        private void set_from_elem(int idx, HeapItemBase<PriorityType> e)
        {
            Debug.Assert(idx >= 0 && idx < Count);

            v_[idx] = e;
            v_[idx].idx_in_heap = idx;
        }

        /** restore heap property downwards
         *
         * The two subtrees rooted at the children of root_idx already must
         * satisfy the heap property, only the root may potentially be in
         * violation.
         */

        public void sift_down(int root_idx)
        {
            Debug.Assert(root_idx >= 0 && root_idx < Count);

            /*
             * Move the root downwards, swapping it with children, until
             * the entire tree is a heap again.
             */
            HeapItemBase<PriorityType> orig_root = v_[root_idx];
            int new_root_idx = root_idx;

            while (true)
            {
                int left, right, smallest;

                left = left_child_idx(new_root_idx);
                if (left >= Count)
                {
                    break; /* reached the bottom */
                }
                PriorityType lp = v_[left].priority;

                right = right_child_idx(new_root_idx);
                if (right >= Count)
                { /* so, root only has one child. */
                    /*STATS_STMT(*/
                    { if (lp.Equals(orig_root.priority)) ++heap_eq_ctr; }/*)*/;
                    if (lp.CompareTo(orig_root.priority) == -1)
                    {
                        smallest = left;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    PriorityType rp = v_[right].priority;
                    /*STATS_STMT(*/
                    { if (lp.Equals(orig_root.priority)) ++heap_eq_ctr; }/*)*/
                    if (lp.CompareTo(orig_root.priority) == -1)
                    {
                        /* STATS_STMT(*/
                        { if (lp.Equals(rp)) ++heap_eq_ctr; }
                        //);
                        smallest = (lp.CompareTo(rp) <= (int)0)? left : right;
                    }
                    else
                    {
                        /* STATS_STMT(*/
                        { if (rp.CompareTo(orig_root.priority) == 0) ++heap_eq_ctr; }/*);*/
                        if (rp.CompareTo(orig_root.priority) < 0)
                        {
                            smallest = right;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                set_from_idx(new_root_idx, smallest);
                new_root_idx = smallest;
            }
            if (root_idx != new_root_idx)
            {
                set_from_elem(new_root_idx, orig_root);
            }

            //{
            //    Debug.Assert(root_idx >= 0 && root_idx < Count);
            //    HeapItemBase<PriorityType> orig_root = v_[root_idx];
            //    int new_root_idx = root_idx;

            //    while (true)
            //    {
            //        int left = left_child_idx(new_root_idx);
            //        if (left >= Count) break;

            //        PriorityType lp = v_[left].priority;
            //        int smallest = left;
            //        PriorityType smallest_prio = lp;

            //        int right = right_child_idx(new_root_idx);
            //        if (right < Count)
            //        {
            //            PriorityType rp = v_[right].priority;
            //            if (rp.CompareTo(smallest_prio) < 0)
            //            {
            //                smallest = right;
            //                smallest_prio = rp;
            //            }
            //        }

            //        if (smallest_prio.CompareTo(orig_root.priority) < 0)
            //        {
            //            set_from_idx(new_root_idx, smallest);
            //            new_root_idx = smallest;
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }

            //    if (new_root_idx != root_idx)
            //    {
            //        set_from_elem(new_root_idx, orig_root);
            //    }
            //}
        }

        /** restore heap property upwards
         *
         * The entire tree satisfies the heap property, only the
         * element at child_idx may violate it upwards.  Specifically
         * the subtree rooted at child_idx, including child_idx, is
         * already correct too.
         */

        private void sift_up(int child_idx)
        {
            //DBG_FUNC_BEGIN(DBG_HEAP);
            //DBG(DBG_HEAP) << "heapelem#" << child_idx;

            Debug.Assert(child_idx >= 0 && child_idx < Count);

            /*
             * Move the child upwards, swapping it with children, until
             * the entire tree is a heap again.
             */
            HeapItemBase<PriorityType> orig_child = v_[child_idx];
            int new_child_idx = child_idx;

            while (new_child_idx != 0)
            {
                int parent = parent_idx(new_child_idx);
               
                if (v_[parent].priority.CompareTo(orig_child.priority) <= 0)
                {
                    break;
                }
                set_from_idx(new_child_idx, parent);
                new_child_idx = parent;
            }
            if (child_idx != new_child_idx)
            {
                set_from_elem(new_child_idx, orig_child);
            }

            //DBG_FUNC_END(DBG_HEAP);
        }

        /** Fix the heap property with respect to a single element whose key has changed.
         */

        public void fix_idx(int idx)
        {
            Debug.Assert(idx >= 0 && idx < Count);

            if (idx != 0 && (v_[idx].priority.CompareTo(v_[parent_idx(idx)].priority) <= 0))
            {
                /*STATS_STMT(*/
                { if (v_[idx].priority.Equals(v_[parent_idx(idx)].priority)) ++heap_eq_ctr; }/*);*/
                sift_up(idx);
            }
            else
            {
                sift_down(idx);
            }
        }

        /** Set a new priority for an item
         *
         * Set a new priority for the item idx and fix the heap property with
         * respect to that single element.
         */

        public void set_priority(int idx, PriorityType p)
        {
            Debug.Assert(idx >= 0 && idx < Count);

            v_[idx].priority = p;
            fix_idx(idx);
        }
        public static int log2i(int v)
        {
            int r = -1;

            while (v > 0)
            {
                r++;
                v >>= 1;
            };

            return r;
        }

        /** Establish the heap property on currently unstructured data.
         */

        public void heapify()
        {
            /* Consider the heap a tree.  Start in the second lowest level and
             * establish the heap property on all these sub-trees (i.e.  put the
             * lowest element in the parent, the two larger ones into the
             * children).
             *
             * Then, go to the next higher level, and for each element establish
             * the heap property of the subtree starting in that element.  Do that
             * by pushing down the element (i.e. switching it with children) until
             * it is smaller than both its children.
             *
             * When we have reached the top level the heap property holds for the
             * entire tree.
             */
            int start_at;

            if (Count <= 1)
                return;

            start_at = (0x1 << log2i(Count)) - 2;
            for (int i = start_at; i >= 0; --i)
                sift_down(i);

            for (int i = 0; i < Count; ++i)
            {
                v_[i].idx_in_heap = i;
            }
        }

        /** checks whether the heap satisfies the heap property.
         *
         * Runs in linear time!  Useful during debugging as
         * Debug.Assert(is_heap(h)).
         */

        public bool is_heap()
        {
            for (int i = Count - 1; i > 0; --i)
            {
                int parent = parent_idx(i);
                if (v_[parent].priority.CompareTo(v_[i].priority) > 0)
                    return false;
            }
            for (int i = 0; i < Count; ++i)
            {
                if (v_[i].idx_in_heap != i)
                {
                    return false;
                }
            }
            return true;
        }

        public HeapBase(List<HeapItemBase<PriorityType>> v) : base()
        {
            v_ = v;
            heapify();
        }

        public HeapBase() : base()
        {
        }

        public void setArray(List<HeapItemBase<PriorityType>> v)
        {
            v_ = v;
            heapify();
        }

        /** remove element at idx from the heap.
         *
         * Returns the element.
         */

        public HeapItemBase<PriorityType> Remove(int idx)
        {
            /* Switch the element to be removed with the one at the end of the
             * array.  Then re-establish heap property for the just moved
             * element, sifting up or down as necessary.  (The removed element,
             * now at the end of the array, no longer belongs to the heap.)
             */
            Debug.Assert(idx >= 0 && idx < Count);
            HeapItemBase<PriorityType> e = v_[idx];

            swap_idx(idx, Count - 1);
            v_.RemoveAt(v_.Count - 1);

            /* We only need to care about heap property if this wasn't the element
             * at the end anyways */
            if (idx != Count)
            {
                fix_idx(idx);
            }

            e.idx_in_heap = -1;
            return e;
        }

        /** remove smallest element from the heap.
         *
         * Returns the element.
         */

        public virtual HeapItemBase<PriorityType> pop()
        {
            Debug.Assert(Count > 0);
            return Remove(0);
        }

        /** get an element from the heap without removing it.
         */

        public virtual HeapItemBase<PriorityType> peak(int idx)
        {
            Debug.Assert(idx >= 0 && idx < Count);
            return v_[idx];
        }

        /** get the smallest element from the heap without removing it.
         */

        public virtual HeapItemBase<PriorityType> peak()
        {
            Debug.Assert(Count > 0);
            return v_[0];
        }

        public void fix_idx(HeapItemBase<PriorityType> e)
        {
            fix_idx(e.idx_in_heap);
        }

        public HeapItemBase<PriorityType> DropElement(HeapItemBase<PriorityType> e)
        {
            return Remove(e.idx_in_heap);
        }

        public void add_element(HeapItemBase<PriorityType> e)
        {
            v_.Add(e);
            v_[Count - 1].idx_in_heap = Count - 1;
            fix_idx(e.idx_in_heap);
        }

        /** checks whether the heap is empty. */

        public bool empty()
        {
            return Count == 0;
        }

        public int Count => v_.Count;

     

        public IEnumerator<HeapItemBase<PriorityType>> GetEnumerator()
        {
            return ((IEnumerable<HeapItemBase<PriorityType>>)v_).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)v_).GetEnumerator();
        }

        // const ArrayType& get_v() const { return v_; };
    }

    public class EventQueueItem : HeapItemBase<CollapseEvent>
    {
        public EventQueueItem(KineticTriangle t, double now)
            : base(new CollapseEvent(t, now))
        { }

        public void UpdatePriority(double now)
        {
            priority.UpdateCollapse(now);
        }
    };

    public class EventQueue : HeapBase<CollapseEvent, EventQueueItem>
    {
        private List<KineticTriangle> need_update = new List<KineticTriangle>();
        private List<KineticTriangle> need_dropping = new List<KineticTriangle>();
        private bool[] tidx_in_need_dropping;
        private bool[] tidx_in_need_update;

        private HeapItemBase<CollapseEvent>[] tidx_to_qitem_map;
        public double NextTime { get { var r = this.v_?.FirstOrDefault()?.priority.Time; return r.HasValue ? r.Value : 0.0; } }
        public EventQueue(IEnumerable<KineticTriangle> triangles)
        {
            List<HeapItemBase<CollapseEvent>> a = new List<HeapItemBase<CollapseEvent>>();
            var count = triangles.Count();
            tidx_to_qitem_map = new HeapItemBase<CollapseEvent>[triangles.Count()];

            tidx_in_need_dropping = new bool[count];
            tidx_in_need_update = new bool[count];

            foreach (var t in triangles)
            {
                var qi = new EventQueueItem(t, 0.0);
                a.Add(qi);
                tidx_to_qitem_map_add(t, qi);
            }
            setArray(a);
            // DBG(DBG_EVENTQ) << "heap array:";
            for (int i = 0; i < this.Count; ++i)
            {
                // DBG(DBG_EVENTQ) << " item " << i << ": " << peak(i).get_priority();
            }
            //var x = peak();

            //  DBG(DBG_EVENTQ) << "top: " << x.get_priority();
            //#if defined (DEBUG_EXPENSIVE_PREDICATES) && DEBUG_EXPENSIVE_PREDICATES >= 1
            //  if (! is_valid_heap() ) {
            //    LOG(ERROR) << "Heap is not valid!";
            //    exit(EXIT_INIT_INVALID_HEAP);
            //  }
            //#endif
        }

        public void tidx_to_qitem_map_add(KineticTriangle t, HeapItemBase<CollapseEvent> qi)
        {
            int id = t.ID;
            /*
            if (id >= tidx_to_qitem_map.Count) {
              tidx_to_qitem_map.resize(id+1);
            };
            */
            Debug.Assert(id < tidx_to_qitem_map.Count());
            tidx_to_qitem_map[id] = qi;
        }

        public void drop_by_tidx(int tidx)
        {
            // DBG_FUNC_BEGIN(DBG_EVENTQ);
            // DBG(DBG_EVENTQ) << "tidx: " << tidx;

            Debug.Assert(tidx_in_need_dropping[tidx]);
            var qi = tidx_to_qitem_map[tidx];
            Debug.Assert(null != qi);
            DropElement(qi);
            tidx_to_qitem_map[tidx] = null;
            tidx_in_need_dropping[tidx] = false;

            // DBG_FUNC_END(DBG_EVENTQ);
        }

        public void update_by_tidx(int tidx, double now)
        {
            // DBG_FUNC_BEGIN(DBG_EVENTQ);
            //  DBG(DBG_EVENTQ) << "tidx: " << tidx << "; now: " << CGAL::to_double(now);

            Debug.Assert(tidx_in_need_update[tidx]);
            var qi = (EventQueueItem)tidx_to_qitem_map[tidx];
            Debug.Assert(null != qi);
            qi.UpdatePriority(now);
            fix_idx(qi);
            tidx_in_need_update[tidx] = false;

            // DBG_FUNC_END(DBG_EVENTQ);
        }

        public void ProcessPendingUpdates(double now)
        {
            LogIndent();
            foreach (var t in need_dropping)
            {
                Debug.Assert(t != null);
                drop_by_tidx(t.ID);
            }
            need_dropping.Clear();

            foreach (var t in need_update)
            {
                Debug.Assert(t != null);
                update_by_tidx(t.ID, now);
            }
            need_update.Clear();

            assert_no_pending();
            LogUnindent();
        }

        public void assert_no_pending()
        {
            Debug.Assert(!need_update.Any());
            Debug.Assert(!need_dropping.Any());
        }

        /** Mark a triangle as needing an update in the priority queue.
         *
         * In general, this implies its collapse spec has become invalidated,
         * however during event refinement we may actually already have set
         * the new collapse spec and it's valid, in which case we need
         * to pass bool may_have_valid_collapse_spec to appease the assertion.
         */

        public void NeedsUpdate(KineticTriangle t, bool may_have_valid_collapse_spec = false)
        {
          
            Log("");
            Log($"{nameof(NeedsUpdate)} {t} ");
            LogIndent();
            Debug.Assert(tidx_in_need_update.Count() > t.ID);
            Debug.Assert(!t.is_collapse_spec_valid() || may_have_valid_collapse_spec);
            // during refinement, the same triangle may be tagged as needs_update multiple times.
            if (!tidx_in_need_update[t.ID])
            {
                tidx_in_need_update[t.ID] = true;
                need_update.Add(t);
            }

            Debug.Assert(!tidx_in_need_dropping[t.ID]); /* Can't drop and update both */
          
            LogUnindent();
        }

        public void needs_dropping(KineticTriangle t)
        {
            // DBG_FUNC_BEGIN(DBG_EVENTQ);
            //  DBG(DBG_EVENTQ) << "t" << t;

            Debug.Assert(tidx_in_need_dropping.Count() > t.ID);
            Debug.Assert(t.IsDying);
            t.set_dead();

            Debug.Assert(!tidx_in_need_dropping[t.ID]);
            tidx_in_need_dropping[t.ID] = true;

            need_dropping.Add(t);

            Debug.Assert(!tidx_in_need_update[t.ID]); /* Can't drop and update both */
            // DBG_FUNC_END(DBG_EVENTQ);
        }

        public bool in_needs_update(KineticTriangle t)
        {
            return tidx_in_need_update[t.ID];
        }

        public bool in_needs_dropping(KineticTriangle t)
        {
            return tidx_in_need_dropping[t.ID];
        }

        /** checks whether the heap satisfies the heap property.
         *
         * Runs in linear time, so should only be used as debugging tool.
         */

        public bool is_valid_heap()
        {
            //#ifndef NT_USE_DOUBLE
            //  for (int i=Count-1; i > 0; --i) {
            //    int parent = parent_idx(i);
            //    // (v1-v2).Rep().getExactSign()
            //    NT delta = peak(parent).get_priority().time() - peak(i).get_priority().time();
            //    if (delta.Rep().getSign() != delta.Rep().getExactSign())
            //    {
            //        LOG(ERROR) << "Sign mismatch at heap item " << parent << " vs. " << i;
            //        DBG(DBG_EVENTQ) << " item " << parent << ": " << peak(i).get_priority();
            //        DBG(DBG_EVENTQ) << " item " << i << ": " << peak(parent).get_priority();
            //        DBG(DBG_EVENTQ) << " delta is " << delta;
            //        DBG(DBG_EVENTQ) << " sign is " << delta.Rep().getSign();
            //        DBG(DBG_EVENTQ) << " exact sign is " << delta.Rep().getExactSign();
            //        return false;
            //    }
            //    if (peak(parent).get_priority() > peak(i).get_priority())
            //    {
            //        LOG(ERROR) << "Mismatch at heap item " << parent << " vs. " << i;
            //        DBG(DBG_EVENTQ) << " item " << parent << ": " << peak(i).get_priority();
            //        DBG(DBG_EVENTQ) << " item " << i << ": " << peak(parent).get_priority();
            //        return false;
            //    }
            //}
            //#endif
            return base.is_heap();
        }

        //std::ostream &
        //operator <<(std::ostream& os, const Event& e)
        //{
        //    os << "Event in " << e.t.get_name() << " " << CollapseSpec(e);
        //    return os;
        //}

        /* we /could/ make this const, and our heap a mutable
         * object attribute and the vectors also mutable.
         * At which point pretty much everything in here is
         * declared mutable, so let's just not.
         */

        public override HeapItemBase<CollapseEvent> peak()
        {
            assert_no_pending();
            return base.peak();
        }

        public override HeapItemBase<CollapseEvent> peak(int idx)
        {
            assert_no_pending();
            return base.peak(idx);
        }
    }
}