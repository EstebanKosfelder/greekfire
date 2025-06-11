using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TriangleNet.Topology;

namespace CGAL
{
    using static DebugLog;
    public abstract class FlipEventData : EdgeEventData
        {

    

        public  FlipEventData(Halfedge edge, CollapseType type, double time, bool allowCollinear =false) : base(edge, type, time)
        {
            T = (KineticTriangle)Edge.Face;
            N = (KineticTriangle)Edge.Opposite.Face;
            Debug.Assert(Edge != null);
            Debug.Assert(T != null);
            Debug.Assert(N != null);

            tc = Tbc.Vertex;
            ta = Tca.Vertex;
            tb = Tab.Vertex;
            nc = Nbc.Vertex;

            

            Debug.Assert(Na == Tb);
            Debug.Assert(Nb == Ta);
            Log($"Create=>{this}");
        }
        public void Update()
        {
            Log($"Before=>{this}");
            T = (KineticTriangle)Edge.Face;
            N = (KineticTriangle)Edge.Opposite.Face;
            Debug.Assert(Edge != null);
            Debug.Assert(T != null);
            Debug.Assert(N != null);

            tc = Tbc.Vertex;
            ta = Tca.Vertex;
            tb = Tab.Vertex;
            nc = Nbc.Vertex;



            Debug.Assert(Na == Tb);
            Debug.Assert(Nb == Ta);
            Log($"Update=>{this}");
        }


        public Point2 Pa => Ta.PointAt(Time);
        public Point2 Pb => Tb.PointAt(Time);

        public Point2 Pc => Tc.PointAt(Time);

        public bool AllowCollinear { get; protected set; }


        public Halfedge Tca => Edge.Prev;
        public Halfedge Tab => Edge;
        public Halfedge Tbc => Edge.Next;

        public Halfedge Nca => Nab.Prev;
        public Halfedge Nab => Tab.Opposite;
        public Halfedge Nbc => Nab.Next;

        public override KineticTriangle Triangle => T;

        
        public KineticTriangle T { get; private set; }
        public KineticTriangle N { get; private set; }

        public Vertex Tc => tc;
        public Vertex Ta => ta;
        public Vertex Tb => tb;
        public Vertex Nc => nc;
        public Vertex Na => Tb;
        public Vertex Nb => Ta;

        private Vertex tc;
        private Vertex ta;
        private Vertex tb;
        private Vertex nc;


        /// <summary>
        ///  Flip edge ab of this triangle.
        ///
        /// Let t be ea, ab, be  (where each is ea),
        /// and let our neighbor opposite e be ne , b, a.
        ///
        /// Then after the flipping, this triangle will be e, ne, a,
        /// and the neighbor will be ne, e, b.
        ///
        /// </summary>
        public void Execute()
        {
            Update();



            Debug.Assert(N != T && N != null && N is KineticTriangle);

            var oppT = (KineticTriangle)N;
            Debug.Assert(oppT != null);




            (var a, var b, var c) = (Ta, Tb, Tc);
            (var na, var nb, var nc) = (Na, Nb, Nc);

            (var ca, var ab, var bc) = (Tca,Tab,Tbc);
            (var nab, var nbc, var nca) = ( Nab, Nbc, Nca);

            var ti = T.Id;
            var ni = N.Id;

            Debug.Assert(T != null);
            Debug.Assert(N != null);
            Debug.Assert(T.Halfedges.All(h => h.Prev.Vertex == h.Opposite.Vertex));
            Debug.Assert(N.Halfedges.All(h => h.Prev.Vertex == h.Opposite.Vertex));
            Log($"Flip"); 
            Log($"T{T.Id} N{N.Id} from:{a.Id}-{b.Id} {Edge}  to:{c.Id}-{nc.Id}");
            Log($" T { ab.LogVIds} { bc.LogVIds} { ca.LogVIds}");
            Log($" N {nab.LogVIds} {nbc.LogVIds} {nca.LogVIds}");

            //Log(string.Join("->", T.Halfedges.Select(h => h.ToString())));
            //Log(string.Join("->", N.Halfedges.Select(h => h.ToString())));

            if (T.Halfedge == bc) T.Halfedge = nca.IsConstrain ? nca : nab;
            if (N.Halfedge == nbc) N.Halfedge = ca.IsConstrain ? ca : ab;

            Debug.Assert(ca.AssertVertex());
            Debug.Assert(ab.AssertVertex());
            Debug.Assert(bc.AssertVertex());
            Debug.Assert(ca.AssertFaces());

            Debug.Assert(nbc.AssertVertex());
            Debug.Assert(nab.AssertVertex());
            Debug.Assert(nca.AssertVertex());
            Debug.Assert(nbc.AssertFaces());


            if ( ab.Vertex.Id != a.Id)
            {

            }

            // link V<.O
            ab.CrossLink(ca);
            ca.CrossLink(nbc);
            nbc.CrossLink(ab);
            
            ab.Vertex = c;
            nbc.Face = ab.Face;

            nab.CrossLink(nca);
            nca.CrossLink(bc);
            bc.CrossLink(nab);
            
            nab.Vertex = nc;
            bc.Face = nab.Face;

         
           

            if (bc.IsConstrain)
            {
                bc.WavefrontEdge.set_incident_triangle((KineticTriangle)nab.Face);
            }
            if (nca.IsConstrain)
            {
                nca.WavefrontEdge.set_incident_triangle((KineticTriangle)ab.Face);
            }
            Log($" result ");
            Log($" T { ab.LogVIds} { ca.LogVIds} {nbc.LogVIds}  ");
            Log($" N {nab.LogVIds} {nca.LogVIds} {bc.LogVIds} ");
            Debug.Assert(ca.AssertVertex());
            Debug.Assert(ab.AssertVertex());
            Debug.Assert(bc.AssertVertex());
            Debug.Assert(ca.AssertFaces());

            Debug.Assert(nbc.AssertVertex());
            Debug.Assert(nab.AssertVertex());
            Debug.Assert(nca.AssertVertex());
            Debug.Assert(nbc.AssertFaces());

            //Log(string.Join("->", T.Halfedges.Select(h => h.ToString())));
            //Log(string.Join("->", N.Halfedges.Select(h => h.ToString())));

            //Log($"  end {ab} {t} - {n} ");

            Debug.Assert( T.Id==ti);
            Debug.Assert( N.Id==ni);

            Debug.Assert(nca.AssertFaces());
            Debug.Assert(ca.AssertFaces());

            Debug.Assert(T.Orientation() == OrientationEnum.COUNTERCLOCKWISE);
            Debug.Assert(N.Orientation() == OrientationEnum.COUNTERCLOCKWISE);

            T.InvalidateEvent();
            N.InvalidateEvent();
        }

       

        ///<summary>
        ///actually performs the flip, Debug.Assert the triangulation is consistent before. 
        ///</summary>
        public void Assert(double time)
        {
          
            Debug.Assert(N != null);

            Point2 pos_ve = Pc;
            Point2 pos_va = Pa;
            Point2 pos_vb = Pb;
            Debug.Assert(Mathex.orientation(pos_va, pos_vb, pos_ve) != OrientationEnum.RIGHT_TURN); // v may be on line(v1,v2)

            Point2 pos_oe = Nc.PointAt(time);

            //DBG(//DBG_KT_EVENT2) << " o(v,o,v1):  " << CGAL.orientation(pos_v, pos_o, pos_v1);
            //DBG(//DBG_KT_EVENT2) << " o(v,o,v2):  " << CGAL.orientation(pos_v, pos_o, pos_v2);
            //DBG(//DBG_KT_EVENT2) << " o(v1,v2,o):  " << CGAL.orientation(pos_v1, pos_v2, pos_o);

            if ((AllowCollinear || true))
            {
                Debug.Assert(Mathex.orientation(pos_ve, pos_oe, pos_va) != OrientationEnum.LEFT_TURN);
                Debug.Assert(Mathex.orientation(pos_ve, pos_oe, pos_vb) != OrientationEnum.RIGHT_TURN);
            }
            else
            {
                Debug.Assert(Mathex.orientation(pos_ve, pos_oe, pos_va) == OrientationEnum.RIGHT_TURN);
                Debug.Assert(Mathex.orientation(pos_ve, pos_oe, pos_vb) == OrientationEnum.LEFT_TURN);
            }
            Debug.Assert(Mathex.orientation(pos_va, pos_vb, pos_oe) != OrientationEnum.LEFT_TURN); // The target triangle may be collinear even before.

            // not strictly necessary for flipping purpuses, but we probably
            // should not end up here if this doesn't hold:
            //        Debug.Assert(!t.is_constrained(TriangulationUtils.cw(edge_idx)) || !t.is_constrained(TriangulationUtils.ccw(edge_idx)) || allow_collinear);
            Debug.Assert(Tbc.IsKinetic || Tab.IsKinetic || AllowCollinear);
        }

        ///<summary>
        /// perform a flip, marking t and its neighbor as modified. 
        ///</summary>
        public override void Handle(GreekFireBuilder builder)
        {

            Log($"Handle {this}");
            KineticTriangle n = (KineticTriangle)T;
            KineticTriangle t = (KineticTriangle)N;
            Debug.Assert(t != null);
            Debug.Assert(n != null);

            Execute();
            builder.modified(t, true);
            builder.modified(n);
        }
    }
}