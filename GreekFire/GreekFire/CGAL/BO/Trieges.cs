using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGAL
{
    public class Triedge
    {
        public static Triedge NULL { get; private set; }




        static  Triedge() { NULL = new Triedge(); }
        public Halfedge E0 { get; private set; }
        public Halfedge E1 { get; private set; }
        public Halfedge E2 { get; private set; }


        // Skeleton nodes (offset polygon vertices) have 3 defining contour edges    
        public Triedge(Halfedge? aE0 = null, Halfedge? aE1 = null, Halfedge? aE2 = null)
        {
            E0 = aE0 ?? Halfedge.NULL;
            E1 = aE1 ?? Halfedge.NULL;
            E2 = aE2 ?? Halfedge.NULL;
        }

        internal Halfedge e(int idx) { return idx == 0 ? E0 : idx == 1 ? E1 : E2; }

        internal Halfedge e0() { return E0; }
        internal Halfedge e1() { return E1; }
        internal Halfedge e2() { return E2; }

        internal bool is_valid() { return E0 != Halfedge.NULL && E1 != Halfedge.NULL; }

        internal bool is_contour() { return E2 == Halfedge.NULL; }
        internal bool is_skeleton() { return E2 != Halfedge.NULL; }

        internal bool is_contour_terminal() { return E0 == E1; }

        internal bool is_skeleton_terminal() { return E0 == E1 || E1 == E2; }

        // Returns true if the triedges store the same 3 halfedges (in any order)
        public static bool operator ==(Triedge? x, Triedge? y)
        {
       //     if (x == null && y == null) return true;
            return x?.Equals(y)?? false;
        }
        public static bool operator !=(Triedge? x, Triedge? y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj == null|| !( obj is Triedge other ) ) return false;

            return this.number_of_unique_edges() == other.number_of_unique_edges() && CountInCommon(this, other) == this.number_of_unique_edges();

        }


        public static Triedge operator &(Triedge x, Triedge y)
        {
            return new Triedge(x.E0, x.E1, (x.E0 == y.E0 || x.E1 == y.E0) ? y.E1 : y.E0);
        }

        public override string ToString()
        {

            return "{E" + insert_handle_id(E0) + ",E" + insert_handle_id(E1) + ",E" + insert_handle_id(E2) + "}";

        }

        private string insert_handle_id(Halfedge aH)
        {
            if (aH != Halfedge.NULL)
                return aH.Id.ToString();
            return "#";
        }

        // returns 1 if aE is one of the halfedges stored in this triedge, 0 otherwise.
        private int contains(Halfedge aE)
        {
            return aE == E0 || aE == E1 || aE == E2 ? 1 : 0;
        }

        private int number_of_unique_edges()
        {
            return is_contour() ? (is_contour_terminal() ? 1 : 2) : (is_skeleton_terminal() ? 2 : 3);
        }

        // Returns the number of common halfedges in the two triedges x and y
        private static int CountInCommon(Triedge x, Triedge y)
        {
            Halfedge[] lE = new Halfedge[3];

            int lC = 1;

            lE[0] = y.E0;

            if (y.E0 != y.E1)
                lE[lC++] = y.E1;

            if (y.E0 != y.E2 && y.E1 != y.E2)
                lE[lC++] = y.E2;

            return x.contains(lE[0]) + x.contains(lE[1]) + (lC > 2 ? x.contains(lE[2]) : 0);
        }
    }


}
