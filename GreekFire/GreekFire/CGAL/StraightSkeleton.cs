using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CGAL
{
    using static DebuggerInfo;

    /*
template<  class Traits_
         , class Items_ = Straight_skeleton_items_2
         , class Alloc_ = CGAL_ALLOCATOR(int)
        >
		*/

    public partial class StraightSkeleton : IDisposable //: public CGAL_HALFEDGEDS_DEFAULT <Traits_,Items_,Alloc_>
    {
        public List<Halfedge> Halfedges;
        public List<Vertex> Vertices;
        public List<Face> Faces;
        public List<Vertex> ContourVertices { get; private set; } = new List<Vertex>();
        public List<Polygon> Polygons { get; private set; } = new List<Polygon>();
        public List<Vertex> WavefrontVertices { get; private set; } = new List<Vertex>();
        public StraightSkeleton()
        {
            Halfedges = new List<Halfedge>();
            Vertices = new List<Vertex>();
            Faces = new List<Face>();
        }

        public void Dispose()
        {
            Halfedges.Clear();
            Vertices.Clear();
            Faces.Clear();
        }
        public Point2 TopLeft= new Point2(double.MaxValue,double.MaxValue);
        public Point2 BottomRight = new Point2(double.MinValue, double.MinValue);
       

        public int FacesCount => Faces.Count;

       

        

        // Removal
        //
        // The following operations erase an element referenced by a handle.
        // Halfedges are always deallocated in pairs of opposite halfedges. Erase
        // of single elements is optional. The deletion of all is mandatory.

        private void vertices_erase(Vertex v)
        {
            Vertices.Remove(v);
        }

        internal void edges_erase(Halfedge h)
        {
            Halfedge g = h.Opposite;
            Halfedges.Remove(h);
            Halfedges.Remove(g);
        }

        //void edges_pop_front() { edges_erase(halfedges.begin()); }
        //void edges_pop_back()
        //{
        //    Halfedge_iterator h = halfedges.end();
        //    edges_erase(--h);
        //}

        private void faces_erase(Face f)
        {
            Faces.Remove(f);
        }

        private void vertices_clear()
        { Vertices.Clear(); }

        private void edges_clear()
        {
            Halfedges.Clear();
        }

        private void faces_clear()
        { Faces.Clear(); }

        private void clear()
        {
            vertices_clear();
            edges_clear();
            faces_clear();
        }

        private Halfedge get_edge_node(Halfedge h, Halfedge g)
        {
            h.set_opposite(g);
            g.set_opposite(h);
            return h;
        }

        private string id(Halfedge h) => h.ID;

        private string? id(Vertex v) => v.ToString();

        private string? id(Face f) => f.ToString();

        // Partial skeletons are used when constructing offsets, to avoid building larger-than-needed skeletons.
        // In that case, fictitious vertices can exist, and we need to ignore them.
        public bool IsValid(bool is_partial_skeleton = false)
        {
            //
            // This is a copy of the validity code in Halfedge_const_decorator with a different reporting mechanism
            //
            CGAL_STSKEL_VALIDITY_TRACE("begin Straight_skeleton::is_valid()");

            bool valid = (1 != (this.Halfedges.Count() & 1));

            CGAL_STSKEL_VALIDITY_TRACE_IF(!valid, $"number of halfedges: {this.Halfedges.Count} is odd.");

            int n = 0;
            int nb = 0;
            // All halfedges.

            foreach (Halfedge h in Halfedges)
            {
                if (!valid) break;

                CGAL_STSKEL_VALIDITY_TRACE($"he[{id(h)}]{(h.IsBorder ? " [border]" : "")}");

                // Pointer integrity.
                valid = valid && (h.Next != Halfedge.NULL);
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: he[{id(h)}].Next == null!");
                    break;
                }
                valid = valid && (h.Opposite.IsValid);
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: he[{id(h)}].Opposite == nullptr!");
                    break;
                }
                // opposite integrity.
                valid = valid && (h.Opposite != h);
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: he[{id(h)}].Opposite == he!");
                    break;
                }
                valid = valid && (h.Opposite.Opposite == h);
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: he[{id(h)}].Opposite[{id(h.Opposite)}].Opposite[{id(h.Opposite.Opposite)}] != he!");
                    break;
                }
                // Non degeneracy.
                valid = valid && (h.Vertex != h.Opposite.Vertex);
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: he[{id(h)}] has same source/target: v[{h.Vertex.Id}]");
                    break;
                }
                // previous integrity.
                valid = valid && h.Next.Prev == h;
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: he[{id(h)}].Next[{id(h.Next)}].Prev[{id(h.Next.Prev)}] != he.");
                    break;
                }
                // vertex integrity.
                valid = valid && h.Vertex != Vertex.NULL;
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE("ERROR: he[{id(begin)}].Vertex == nullptr!");
                    break;
                }
                if (!is_partial_skeleton || !h.Vertex.has_infinite_time())
                {
                    valid = valid && (h.Vertex == h.Next.Opposite.Vertex);
                    if (!valid)
                    {
                        CGAL_STSKEL_VALIDITY_TRACE($"ERROR: he[{id(h)}].Vertex[{id(h.Vertex)}] != he.Next[{id(h.Next)}].Opposite[{id(h.Next.Opposite)}].Vertex[{h.Next.Opposite.Vertex}]");
                        break;
                    }
                }
                // face integrity.
                valid = valid && (h.IsBorder || h.Face != Face.NULL);
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: he[{id(h)}].Face == nullptr.");
                    break;
                }
                valid = valid && (h.Face == h.Next.Face);
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: he[{id(h)}].Face[{h.Face}] != he.Next[{h.Next}].Face[{h.Next.Face}].");
                    break;
                }
                ++n;
                if (h.IsBorder)
                    ++nb;
            }
            CGAL_STSKEL_VALIDITY_TRACE($"sum of border halfedges (2*nb) = {2 * nb}");

            bool nvalid = (n == this.Halfedges.Count);

            CGAL_STSKEL_VALIDITY_TRACE_IF(valid && !nvalid
                                         , $"ERROR: counted number of halfedges:{n} mismatch with this.size_of_halfedges():{this.Halfedges.Count}");

            valid = valid && nvalid;

            int v = 0;
            n = 0;

            foreach (Vertex vbegin in Vertices)
            {
                // Pointer integrity.
                valid = valid && vbegin.halfedge() != Halfedge.NULL;
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: v[{id(vbegin)}].halfedge() == nullptr.");
                    break;
                }

                // Time check
                if (!is_partial_skeleton)
                {
                    valid = valid && !vbegin.has_infinite_time();
                    if (!valid)
                    {
                        CGAL_STSKEL_VALIDITY_TRACE($"ERROR: v[{id(vbegin)}] has infinite time in a full skeleton");
                        break;
                    }
                }

                // cycle-around-vertex test.
                if (!is_partial_skeleton || !vbegin.has_infinite_time())
                {
                    valid = valid && vbegin.halfedge().Vertex == vbegin;
                    if (!valid)
                    {
                        CGAL_STSKEL_VALIDITY_TRACE($"ERROR: v[{id(vbegin)}].halfedge()[{id(vbegin.halfedge())}].Vertex[{(vbegin.halfedge().Vertex)}] != v.");
                        break;
                    }

                    CGAL_STSKEL_VALIDITY_TRACE($"Circulating halfedges around v[{id(vbegin)}]");

                    if (vbegin.halfedge() != Halfedge.NULL)
                    {
                        Halfedge g = vbegin.halfedge();
                        Halfedge h = g;
                        do
                        {
                            CGAL_STSKEL_VALIDITY_TRACE($"  v.halfedge(): {id(h)}.Face: {h.Face}, .Next: {h.Next}, .Next.Opposite: {id(h.Next.Opposite)}");
                            ++n;
                            h = h.Next.Opposite;
                            valid = valid && (n <= this.Halfedges.Count && n != 0);
                            CGAL_STSKEL_VALIDITY_TRACE_IF(!valid, $"ERROR: more than {this.Halfedges.Count} halfedges around v[{id(vbegin)}]");
                        } while (valid && (h != g));
                    }
                }
                ++v;
            }

            if (!is_partial_skeleton)
            {
                bool vvalid = (v == this.Vertices.Count);

                CGAL_STSKEL_VALIDITY_TRACE_IF(valid && !vvalid, $"ERROR: counted number of vertices:{v} mismatch with this.size_of_vertices(): {this.Vertices.Count}");

                bool vnvalid = n == this.Halfedges.Count;
                CGAL_STSKEL_VALIDITY_TRACE_IF(valid && !vnvalid, $"ERROR: counted number of halfedges via vertices:{n} mismatch with this.size_of_halfedges():{this.Halfedges.Count}");

                valid = valid && vvalid && vnvalid;
            }

            // All faces.
            int f = 0;
            n = 0;
            var begin = Halfedges[0];
            foreach (var fbegin in Faces)
            {
                valid = valid && (begin.IsBorder || fbegin.halfedge() != Halfedge.NULL);
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: f[{id(fbegin)}].halfedge() == nullptr.");
                    break;
                }

                valid = valid && fbegin.halfedge().Face == fbegin;
                if (!valid)
                {
                    CGAL_STSKEL_VALIDITY_TRACE($"ERROR: f[{id(fbegin)}].halfedge()[{id(fbegin.halfedge())}].Face[{id(fbegin.halfedge().Face)}] != f.");
                    break;
                }
                // cycle-around-face test.
                CGAL_STSKEL_VALIDITY_TRACE("Circulating halfedges around f[{id(fbegin)}]");
                Halfedge h = fbegin.halfedge();

                if (h != Halfedge.NULL)
                {
                    Halfedge g = h;
                    do
                    {
                        CGAL_STSKEL_VALIDITY_TRACE($"  f.halfedge(){id(h)}, .Face: {id(h.Face)}, .Next: {id(h.Next)}");
                        ++n;
                        h = h.Next;
                        valid = valid && (n <= this.Halfedges.Count && n != 0);
                        CGAL_STSKEL_VALIDITY_TRACE_IF(!valid, $"ERROR: more than {this.Halfedges.Count}  halfedges around f[{id(fbegin)}]");
                    } while (valid && (h != g));
                }
                ++f;
            }

            bool fvalid = (f == this.Faces.Count);

            CGAL_STSKEL_VALIDITY_TRACE_IF(valid && !fvalid
                                         , $"ERROR: counted number of faces:{f} mismatch with this.size_of_faces():{this.Faces.Count}");

            ////bool fnvalid = (n + nb == this.halfedges.Count);

            ////CGAL_STSKEL_VALIDITY_TRACE_IF(valid && !fnvalid
            ////                             , $"ERROR: counted number of halfedges via faces:{n} plus counted number of border halfedges: {nb} mismatch with this.size_of_halfedges():{this.halfedges.Count}");

            //valid = valid && fvalid && fnvalid;

            CGAL_STSKEL_VALIDITY_TRACE($"end of Straight_skeleton_2::is_valid():{(valid ? "valid." : "NOT VALID.")}");

            return valid;
        }

        private int size_of_vertices()
        {
            return Vertices.Count;
        }

        private int size_of_faces()
        {
            return Faces.Count;
        }

        private int size_of_halfedges()
        {
            return Halfedges.Count;
        }

    

       internal void CreateNewEdge(out Halfedge heA, out Halfedge heO)
        {
            heA = new Halfedge(Halfedges.Count);
            heO = new Halfedge(Halfedges.Count+1);
            heA.Opposite = heO;
            heO.Opposite = heA;
            Halfedges.Add(heA);
            Halfedges.Add(heO);

        }

        internal Vertex Add(Vertex vertex)
        {
            Vertices.Add(vertex);
            return vertex;
        }
                
        internal Face Add(Face face)
        {
            Faces.Add(face);
            return face;
        }

        internal void Remove(Vertex aNode)
        {
            Vertices.Remove(aNode);
        }

        //public static StraightSkeleton create_interior_straight_skeleton_2(IEnumerable<Point2> aOuterContour, IEnumerable<IEnumerable<Point2>> aHoles)
        //{
        //    GreekFireBuilder ssb = new GreekFireBuilder();

        //    ssb.EnterContour(aOuterContour);

        //    foreach (var hole in aHoles)
        //        ssb.EnterContour(hole);

        //    return ssb.ConstructSkeleton(false);
        //}
    }
}