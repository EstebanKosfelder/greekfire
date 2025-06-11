using GFL.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace CGAL
{





   


    public partial class WavefrontEdge
    {



        /// <summary>
        /// Is this wavefront edge the result of beveling,
        /// and thus degenerate at time zero?
        /// </summary>
        public bool is_beveling { get; private set; }

        /// <summary>
        /// Is this wavefront edge one that was
        /// created initially as part of the input or
        /// from beveling (true), or is it the result
        /// of a wavefront edge having been split during the
        /// propagation period (false).
        /// </summary>
        public bool is_initial { get; private set; }

        private static int wavefront_edge_ctr;

        private EdgeCollapseSpec _collapseSpec;

        ///<summary>
        ///The straight skeleton face that this edge traces out (at least partially.
        ///With splitting, multiple edges are needed to trace out a single face.
        ///</summary>
        private bool _collapseSpecValid = false;

        private bool _isDead = false;

        ///<summary>
        ///The triangle incident right now at this wavefront edge.
        ///</summary>
        public KineticTriangle IncidentTriangle { get; private set; }

        /// <summary>
        ///The pointers to the left and right straight
        ///skeleton arcs (==kinetic wavefront vertex).
        ///Only for is_initial wavefront edges, so
        ///we can then find the faces nicely. 
        ///</summary>
        private Vertex[] initial_vertices = new Vertex[2];

        ///<summary>
        ///  The supporting line backing this wavefront vertex.
        ///</summary>
        private WavefrontSupportingLine supporting_line;

        ///<summary>
        /// The left and right wavefront vertex right now.
        /// This changes over the course of the propagation period.
        ///</summary>
        private Vertex[] vertices = new Vertex[2];

        public int ID { get; private set; }

        public WavefrontSupportingLine SupportLine => supporting_line;

        /// <summary>
        /// Used when setting up initial wavefront edges for all constraints 
        /// </summary>
        public WavefrontEdge(int id_, Vertex u, Vertex v, double weight = 1, KineticTriangle incident_triangle = null)

        {
            Debug.Assert(u != null && u != Vertex.NULL);
            Debug.Assert(v != null && v != Vertex.NULL);


            ID = id_;
            vertices = new Vertex[] { u , v };
            supporting_line = new WavefrontSupportingLine(u.Point, v.Point, weight);
            //   incident_triangle_ = (incident_triangle);
            is_initial = (true);
            is_beveling = (false);
            initial_vertices = new Vertex[] { u, v };
        }

        
        
        /// <summary>
        /// Used when setting up bevels 
        /// </summary>
        //public WavefrontEdge(WavefrontSupportingLine p_supporting_line)
        //{
        //    vertices = new Vertex[] { null, null };
        //    supporting_line = p_supporting_line;
        //    incident_triangle_ = null;
        //    is_initial = true;
        //    is_beveling = true;
        //    initial_vertices = new Vertex[] { null, null };
        //}

        private WavefrontEdge(int id_, Vertex va,
                          Vertex vb,
                          WavefrontSupportingLine p_supporting_line,
                          KineticTriangle incident_triangle,
                          /*DcelFace p_skeleton_face,*/
                          bool p_is_beveling)
        {

            ID = id_;

            vertices = new Vertex[] { va, vb };
            supporting_line = (p_supporting_line);
            //  incident_triangle_ = (incident_triangle);
            is_initial = (false);
            is_beveling = (p_is_beveling);
            initial_vertices = new Vertex[] { null, null };
            //  skeleton_face = (p_skeleton_face);
            //#if !SURF_NDEBUG
            //            collapse_spec_computed_with_vertices = new Vertex[] { null, null };
            //#endif

            //    Debug.Assert((skeleton_face!=null) ^ is_beveling);
        }

        public Vertex initial_vertex(int i)
        {
            Debug.Assert(is_initial);
            Debug.Assert(i <= 1);
            return initial_vertices[i];
        }

        public bool is_dead() => _isDead;

        public bool ParallelEndpoints(double time_now)
        {
            EdgeCollapseSpec e = GetEdgeCollapse(time_now);
            switch (e.Type)
            {
                case EdgeCollapseType.Undefine:
                    Debug.Assert(false);
                    return false;

                case EdgeCollapseType.Past:
                case EdgeCollapseType.Future:
                    return false;

                case EdgeCollapseType.Always:
                case EdgeCollapseType.Never:
                    return true;
            }
            Debug.Assert(false);
            return false;
        }

        public void set_dead()
        {
            Debug.Assert(!_isDead);
            _isDead = true;
        }

        public void set_incident_triangle()
        {
            Debug.Assert(!_isDead);

            invalidate_collapse_spec();
        }

        public void set_incident_triangle(KineticTriangle incident_triangle)
        {
            Debug.Assert(!_isDead);
            //TODO Debug.Assert(incident_triangle.Wavefronts.Any(e=>e == this));


            IncidentTriangle = incident_triangle;
            //var he = incident_triangle.Halfedges.Where(e=>e.IsConstrain && e.WavefrontEdge ==this).FirstOrDefault();
            //Debug.Assert(he != null);
            //Debug.Assert(he?.Vertex == vertices[0]);
            //Debug.Assert(he?.Next.Vertex == vertices[1]);
            invalidate_collapse_spec();
        }

        public void set_initial_incident_triangle(KineticTriangle incident_triangle)
        {
            IncidentTriangle = incident_triangle;
        }

        public void set_initial_vertices(params Vertex[] initial_vertices)
        {
            this.initial_vertices = initial_vertices;
        }

        public void set_vertices(params Vertex[] vertices)
        {
            this.vertices = vertices;
        }

        public void set_wavefrontedge_vertex(int i, Vertex v)
        {
            Debug.Assert(!_isDead);
            Debug.Assert(i <= 1);
            Debug.Assert(v != null);
            vertices[i] = v;
            invalidate_collapse_spec();
        }

        public EdgePtrPair Split(List<WavefrontEdge> wavefront_edges)
        {
            Debug.Assert(vertices[0] != null);
            Debug.Assert(vertices[1] != null);
            Debug.Assert(vertices[0].incident_wavefront_edge(1) == this);
            Debug.Assert(vertices[1].incident_wavefront_edge(0) == this);
            set_dead();

            wavefront_edges.Add(new WavefrontEdge(wavefront_edges.Count, vertices[0], Vertex.NULL, supporting_line,this.IncidentTriangle, is_beveling));
            var pea = wavefront_edges.Last();
            wavefront_edges.Add(new WavefrontEdge(wavefront_edges.Count, Vertex.NULL, vertices[1], supporting_line, this.IncidentTriangle, is_beveling));
            var peb = wavefront_edges.Last();

            return new EdgePtrPair(pea, peb);
        }

        public override string ToString() => $" B{base.ToString()}";
        /** stopped propagating */

        /** The left and right wavefront vertex right
                                   *  now.  This changes over the course of the
                                   *  propagation period.
                                   */
        public Vertex vertex(int i)
        {
            Debug.Assert(!_isDead);
            Debug.Assert(i <= 1);
            return vertices[i];
        }
        internal void set_initial_vertices()
        {
            Debug.Assert(is_initial);
            for (int i = 0; i <= 1; ++i)
            {
                Debug.Assert(vertices[i] != null);
                Debug.Assert(initial_vertices[i] == null);
                initial_vertices[i] = vertices[i];
            };
        }

    }
}