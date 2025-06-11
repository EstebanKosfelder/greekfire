using GFL.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace GFL
{





    using static Debugger.DebugLog;


    public partial class WavefrontEdge : HalfedgeBase
    {


       
        public override string ToString() => $" B{base.ToString()}";

      

       
   


        private static int wavefront_edge_ctr;

        

        private bool _isDead = false;        /** stopped propagating */

        /** The left and right wavefront vertex right
                                   *  now.  This changes over the course of the
                                   *  propagation period.
                                   */

        ///<summary>
        /// The left and right wavefront vertex right now.
        /// This changes over the course of the propagation period.
        ///</summary>
                        private WavefrontVertex[] vertices = new WavefrontVertex[2];

        ///<summary>
        ///  The supporting line backing this wavefront vertex.
        ///</summary>
        private WavefrontSupportingLine supporting_line;

        ///<summary>
        /// The triangle incident right now at this wavefront edge.
        ///</summary>
        private KineticTriangle incident_triangle_ => (Opposite_ is KineticHalfedge kh) ? kh.Triangle : throw new NullReferenceException();


        /// <summary>
        /// Is this wavefront edge one that was
        /// created initially as part of the input or
        /// from beveling (true), or is it the result
        /// of a wavefront edge having been split during the
        /// propagation period (false).
        /// </summary>
        public readonly bool is_initial; 

        /// <summary>
        /// Is this wavefront edge the result of beveling,
        /// and thus degenerate at time zero?
        /// </summary>
        public readonly bool is_beveling;


        /// <summary>
        /// The pointers to the left and right straight
        /// skeleton arcs (==kinetic wavefront vertex).
        /// Only for is_initial wavefront edges, so
        /// we can then find the faces nicely. 
        /// </summary>
        private WavefrontVertex[] initial_vertices = new WavefrontVertex[2];
       

        ///<summary>
        /// The straight skeleton face that this edge traces out (at least partially.
        /// With splitting, multiple edges are needed to trace out a single face.
        ///</summary>
      

        private EdgeCollapseSpec _collapseSpec;
        private bool _collapseSpecValid = false;
#if  !SURF_NDEBUG
        private WavefrontVertex[] collapse_spec_computed_with_vertices = new WavefrontVertex[2];
#endif

        /// <summary>
        /// Used when setting up initial wavefront edges for all constraints 
        /// </summary>
        public WavefrontEdge(int id_, Vector2D u, Vector2D v, double weight = 1, KineticTriangle incident_triangle=null) : base(id_, SignEnum.ZERO)

        {

            ID = id_;
            vertices = new WavefrontVertex[] { null, null };
            supporting_line = new WavefrontSupportingLine(u, v, weight);
         //   incident_triangle_ = (incident_triangle);
            is_initial = (true);
            is_beveling = (false);
            initial_vertices = new WavefrontVertex[] { null, null };
            //skeleton_face = p_skeleton_face;
//#if !SURF_NDEBUG
          //  collapse_spec_computed_with_vertices = new WavefrontVertex[] { null, null };
//#endif
            //Debug.Assert((skeleton_face!=null) ^ is_beveling);
        }

        /// <summary>
        /// Used when setting up bevels 
        /// </summary>
//        public WavefrontEdge(WavefrontSupportingLine p_supporting_line)
//        {
//#if !SURF_NDEBUG
//            id = (wavefront_edge_ctr++);
//#endif
//            vertices = new WavefrontVertex[] { null, null };
//            supporting_line = p_supporting_line;
//            incident_triangle_ = null;
//            is_initial = true;
//            is_beveling = true;
//            initial_vertices = new WavefrontVertex[] { null, null };
//            skeleton_face = null;
//# if !SURF_NDEBUG
//            collapse_spec_computed_with_vertices = new WavefrontVertex[] { null, null };
//#endif

//            Debug.Assert(skeleton_face!=null ^ is_beveling);
//        }

        private WavefrontEdge(int id_,WavefrontVertex va,
                          WavefrontVertex vb,
                          WavefrontSupportingLine p_supporting_line,
                          KineticTriangle incident_triangle,
                          /*DcelFace p_skeleton_face,*/
                          bool p_is_beveling) : base(id_, SignEnum.ZERO)
        {

            ID = id_;

            vertices = new WavefrontVertex[]{ va, vb};
            supporting_line = (p_supporting_line);
          //  incident_triangle_ = (incident_triangle);
            is_initial = (false);
            is_beveling = (p_is_beveling);
            initial_vertices =new WavefrontVertex[]{ null, null };
          //  skeleton_face = (p_skeleton_face);
#if !SURF_NDEBUG
            collapse_spec_computed_with_vertices = new WavefrontVertex[] { null, null };
#endif

        //    Debug.Assert((skeleton_face!=null) ^ is_beveling);
        }

        public void set_dead()
        {
            Debug.Assert(!_isDead);
            _isDead = true;
        }
        public WavefrontSupportingLine SupportLine => supporting_line;

        [Obsolete($"usar {nameof(SupportLine)}")]
        public WavefrontSupportingLine l() =>supporting_line; 


        public KineticTriangle incident_triangle()=> incident_triangle_; 

        public bool is_dead()=>_isDead;



        public void set_vertices(params  WavefrontVertex[] vertices)
        {
            this.vertices = vertices;
        }

        public void set_initial_vertices(params WavefrontVertex[] initial_vertices)
        {
            this.initial_vertices = initial_vertices;
        }

        public WavefrontVertex vertex(int i)
        {
            Debug.Assert(!_isDead);
            Debug.Assert(i <= 1);
            return vertices[i];
        }

        public WavefrontVertex initial_vertex(int i)
        {
           Debug.Assert(is_initial);
            Debug.Assert(i <= 1);
            return initial_vertices[i];
        }

        public void set_wavefrontedge_vertex(int i, WavefrontVertex v)
        {
            Debug.Assert(!_isDead);
            Debug.Assert(i <= 1);
            Debug.Assert(v != null);
            vertices[i] = v;
            InvalidateCollapseSpec();
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

       

      
   

        public partial EdgePtrPair split(List<WavefrontEdge> wavefront_edges);

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

//#if !SURF_NDEBUG

      

//#else
//private void assert_edge_sane(int collapsing_edge) {};
//#endif


      
        public void set_initial_incident_triangle(KineticTriangle incident_triangle)
        {
           // incident_triangle_ = incident_triangle;
        }


        public void set_incident_triangle()
        {
            Debug.Assert(!_isDead);
            
            InvalidateCollapseSpec();
        }

        public void set_incident_triangle(KineticTriangle incident_triangle)
        {
            Debug.Assert(!_isDead);
           //TODO Debug.Assert(incident_triangle.Wavefronts.Any(e=>e == this));
           

            //incident_triangle_ = incident_triangle;
            //var he = incident_triangle.Halfedges.Where(e=>e.IsConstrain && e.WavefrontEdge ==this).FirstOrDefault();
            //Debug.Assert(he != null);
            //Debug.Assert(he?.Vertex == vertices[0]);
            //Debug.Assert(he?.Next.Vertex == vertices[1]);
            InvalidateCollapseSpec();
        }

        /** Duplicate this edge in the course of a split event.
         *
         * This is just a simple helper that creates two copies of this edge and marks
         * the original as dead.
         *
         * It is the job of the caller (KineticTriangulation) to then give us new vertices.
         */

        public partial EdgePtrPair split(  List<WavefrontEdge> wavefront_edges)
        {
            Debug.Assert(vertices[0] != null);
            Debug.Assert(vertices[1] != null);
            Debug.Assert(vertices[0].incident_wavefront_edge(1) == this);
            Debug.Assert(vertices[1].incident_wavefront_edge(0) == this);
            set_dead();

            wavefront_edges.Add(new WavefrontEdge(wavefront_edges.Count, vertices[0], null, supporting_line, incident_triangle_, is_beveling));
            var pea = wavefront_edges.Last();
            wavefront_edges.Add(new WavefrontEdge(wavefront_edges.Count, null, vertices[1], supporting_line, incident_triangle_, is_beveling));
            var peb = wavefront_edges.Last();

            return new EdgePtrPair(pea, peb);
        }
    }
}