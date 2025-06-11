using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static CGAL.DebuggerInfo;
using static CGAL.Mathex;

using System.Security.Policy;
using System.Xml.Linq;
using FT = double;

using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

using TriangleNet;
using TriangleNet.Topology;


namespace CGAL
{
    using Point2 = CGAL.Point2;
    using FT = System.Double;
    public partial class GreekFireBuilder
    {
      

      

        public List<Face> OutsideContourFaces = new List<Face>();

        

        public StraightSkeleton mSSkel = new StraightSkeleton();
     

        public EventQueue EventQueue = null;

        public List<KineticTriangle> KineticTriangles = new List<KineticTriangle>();

        public List<Vertex> WavefrontVertices = new List<Vertex>();

        public List<WavefrontEdge> WavefrontEdges = new List<WavefrontEdge>();
        private List<Halfedge> mContourHalfedges = new List<Halfedge>();

        void Link(Halfedge aH, CGAL.Face aF) => aH.set_face(aF);

        void Link(Halfedge aH, CGAL.Vertex aV)
        {
            aH.Vertex = aV;
        }

        void Link(CGAL.Vertex aV, Halfedge aH) => aV.Halfedge = aH;

        void CrossLinkFwd(Halfedge aPrev, Halfedge aNext)
        {
            aPrev.Next = aNext;
            aNext.Prev = aPrev;
        }

        void CrossLink(Halfedge aH, CGAL.Vertex aV)
        {
            Link(aH, aV);
            Link(aV, aH);
        }
        internal void CreateNewEdge(out Halfedge heA, out Halfedge heO)
        {
            mSSkel.CreateNewEdge(out heA, out heO);
        }


        public void init_event_list()
        {
            this.EventQueue = new EventQueue(KineticTriangles);
        }

      



        public GreekFireBuilder EnterContour(IEnumerable<CGAL.Point2> aPoly, bool aCheckValidity = true)
        {
            if (aCheckValidity)
            {
                List<CGAL.Point2> nPoly = new List<CGAL.Point2>();
                foreach (var p in aPoly)
                {
                    if (nPoly.Count == 0 || !are_near(nPoly.Last(), p, 0.05))
                    {
                        nPoly.Add(p);
                    }
                }
                if (nPoly.Count > 0 && are_near(nPoly.First(), nPoly.Last(), 0.05))
                {
                    nPoly.RemoveAt(nPoly.Count - 1);
                }
                if (nPoly.Count < 3)
                    throw new NotValidPoligonException("Degenerate contour (less than 3 non-degenerate vertices).");
                aPoly = nPoly;
            };
            EnterValidContour(aPoly);

            return this;
        }

        private void EnterValidContour(IEnumerable<CGAL.Point2> points)
        {
            var polygon = new Polygon(mSSkel.Polygons.Count + 1, mSSkel.ContourVertices.Count, points.Count(), mSSkel.Polygons.Count != 0);

            mSSkel.ContourVertices.Capacity = mSSkel.ContourVertices.Count + polygon.Count;
            mSSkel.ContourVertices.AddRange(points.Select((p, i) => new Vertex(i + polygon.StartIndex, p, VertexType.Contour)));

            CGAL_STSKEL_BUILDER_TRACE(0, "Inserting Connected Component of the Boundary....");

            Halfedge lFirstCCWBorder = Halfedge.NULL;
            Halfedge lPrevCCWBorder = Halfedge.NULL;
            Halfedge lNextCWBorder = Halfedge.NULL;
            Vertex lFirstVertex = Vertex.NULL;
            Vertex lPrevVertex = Vertex.NULL;
            var outsideContourFace = new Face(-1, OutsideContourFaces.Count);
            OutsideContourFaces.Add(outsideContourFace);
            // InputPointIterator lPrev = aBegin ;

            int c = 0;

            var last = mSSkel.ContourVertices.Last();
            foreach (var curr in mSSkel.ContourVertices.Skip(polygon.StartIndex))
            {
                mSSkel.TopLeft = Mathex.Min(mSSkel.TopLeft, curr.Point);

                mSSkel.BottomRight = Mathex.Max(mSSkel.BottomRight, curr.Point);

                CreateNewEdge(out var lCCWBorder, out var lCWBorder);

                if (outsideContourFace.Halfedge == null || outsideContourFace.Halfedge == Halfedge.NULL)
                {
                    outsideContourFace.Halfedge = lCWBorder;
                }

                mContourHalfedges.Add(lCCWBorder);

                mSSkel.Add(curr);
                var lVertex = curr;
                CGAL_STSKEL_BUILDER_TRACE(1, $"Vertex: V{lVertex.Id} at {lVertex.point()}");
             

                CGAL.Face lFace = mSSkel.Add(new CGAL.Face(mSSkel.Faces.Count+1));

                lCWBorder.Face = outsideContourFace;

                lCCWBorder.Face = lFace;
                lFace.Halfedge = lCCWBorder;

                lVertex.Halfedge = lCCWBorder;
                lCCWBorder.Vertex = lVertex;

                if (c == 0)
                {
                    lFirstVertex = lVertex;
                    lFirstCCWBorder = lCCWBorder;
                }
                else
                {
                    lVertex.PrevInLAV = lPrevVertex;
                    lPrevVertex.NextInLAV = lVertex;

                   // SetVertexTriedge(lPrevVertex, new Triedge(lPrevCCWBorder, lCCWBorder));

                    lCWBorder.Vertex = lPrevVertex;

                    lCCWBorder.Prev = lPrevCCWBorder;
                    lPrevCCWBorder.Next = lCCWBorder;

                    lNextCWBorder.Prev = lCWBorder;
                    lCWBorder.Next = lNextCWBorder;

                    CGAL_STSKEL_BUILDER_TRACE(1, $"CCW Border: E{lCCWBorder.Id} {lPrevVertex.point()} . {lVertex.point()}");
                    CGAL_STSKEL_BUILDER_TRACE(1, $"CW  Border: E{lCWBorder.Id} {lVertex.point()} . {lPrevVertex.point()}");

                   
                }
                ++c;

                // lPrev = lCurr ;

                lPrevVertex = lVertex;
                lPrevCCWBorder = lCCWBorder;
                lNextCWBorder = lCWBorder;
            }
            this.mSSkel.Polygons.Add(polygon);

            lFirstVertex.PrevInLAV = lPrevVertex;
            lPrevVertex.NextInLAV = lFirstVertex;

          //  SetVertexTriedge(lPrevVertex, new Triedge(lPrevCCWBorder, lFirstCCWBorder));

            lFirstCCWBorder.Opposite.Vertex = lPrevVertex;

            lFirstCCWBorder.Prev = lPrevCCWBorder;
            lPrevCCWBorder.Next = lFirstCCWBorder;

            lPrevCCWBorder.Opposite.set_prev(lFirstCCWBorder.Opposite);
            lFirstCCWBorder.Opposite.set_next(lPrevCCWBorder.Opposite);

            DebuggerInfo.CGAL_precondition(c >= 3, "The contour must have at least 3 _distinct_ vertices");

            CGAL_STSKEL_BUILDER_TRACE(1, $"CCW Border: E{lFirstCCWBorder.Id} {lPrevVertex.point()} . {lFirstVertex.point()}",
                                         $"CW  Border: E{lFirstCCWBorder.Opposite.Id} {lFirstVertex.point()} . {lPrevVertex.point()}"
                                     );

            DebugLog.Log(string.Join("->", lFirstCCWBorder.Halfedges.Select(v => $"{v.Opposite.Vertex.Id}-{v.Vertex.Id.ToString()}")));
         
        }
        GreekFireBuilder EnterContourWeights(IEnumerable<FT> aWeights)
        {
            var lWeightsN = aWeights.Count();
            var lFacesN = mSSkel.FacesCount;
            CGAL_assertion(lWeightsN <= lFacesN);

            var faceidx = mSSkel.FacesCount - lWeightsN;

            foreach (var lWeight in aWeights)
            {
                CGAL.Face face = mSSkel.Faces[faceidx++];
                Halfedge lBorder = face.halfedge();
                CGAL_assertion(lBorder.Opposite?.IsBorder ?? false);
                CGAL_STSKEL_BUILDER_TRACE(4, $"Assign {lWeight} to E{lBorder.Id}");
                lBorder.Weight = lWeight;
            }

            return this;
        }

        public Mesh Triangulate()
        {
            var polygons = new TriangleNet.Geometry.Polygon();

            foreach (var polygon in mSSkel.Polygons.Select(e => e))
            {
                TriangleNet.Geometry.Vertex toVector(CGAL.Point2 v, int i) => new TriangleNet.Geometry.Vertex(v.X, v.Y, i);
                Contour pts = new Contour(polygon.Vertices(this.mSSkel.ContourVertices)
                                                .Select((p, i) => toVector(p.Point, i + polygon.StartIndex)), polygon.ID);
                bool hole = polygon.Hole == true;
                polygons.Add(pts, hole);
            }

            var options = new ConstraintOptions() { Convex = false };

            var quality = new QualityOptions() { };

            // Generate mesh using the polygons Triangulate extension method.

            Mesh mesh = (Mesh)polygons.Triangulate(options, quality);

            return mesh;
        }
        IEnumerable<((int a, int b, int label) Segment, bool isOposite)[]> EnumerateTriangules(IMesh mesh)
        {
            Otri tri = default;
            Otri neighbor = default;
            Osub sub = default;

            TriangleNet.Geometry.Vertex p1, p2;

            int ii = 0;
            foreach (var t in mesh.Triangles)
            {
                ii++;
                tri.tri = t;
                tri.orient = 0;
                Console.Write($"Tri\t {ii} \t{tri.tri.id}\t");
                ((int a, int b, int label), bool isOposite)[] segments = new ((int a, int b, int label), bool)[3];
                for (int i = 0; i < 3; i++)
                {
                    tri.Sym(ref neighbor);

                    int nid = neighbor.tri.id;

                    if (true/* (nid != -1)*/)
                    {
                        var backColor = Console.ForegroundColor;
                        if ((nid == -1))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                        }
                        else if ((tri.tri.id >= nid))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        }
                        p1 = tri.Org();
                        p2 = tri.Dest();

                        tri.Pivot(ref sub);

                        if (sub.seg.hash == -1)
                        {
                            Console.Write($" [{i}]{p1.ID}-{p2.ID} -1\t");
                            segments[i] = ((p1.ID, p2.ID, -1), (tri.tri.id < nid));
                        }
                        else /*if (segments)*/
                        {
                            // Segments might be processed separately, so only
                            // include them if requested.
                            Console.Write($" [{i}]{p1.ID}-{p2.ID} {sub.Segment.Label} \t");
                            segments[i] = ((p1.ID, p2.ID, sub.Segment.Label), (tri.tri.id < nid));
                        }

                        Console.ForegroundColor = backColor;
                    }

                    tri.orient++;
                }
                Console.WriteLine();
                yield return segments;
            }
        }

        public void BuildKineticTriangles(IMesh mesh, List<Vertex> wavefrontVertices)
        {
            KineticTriangles = new List<KineticTriangle>(mesh.Triangles.Count);
            

            Dictionary<(int a, int b), Halfedge> kineticHalfedges = new Dictionary<(int a, int b), Halfedge>();

            var tt = mesh.Triangles.Where(t => t.GetHashCode() >= 0);

            foreach (var tri in EnumerateTriangules(mesh))
            {
                var meshTri = new KineticTriangle(KineticTriangles.Count);

                var a = tri[0].Segment.a;
                var b = tri[1].Segment.a;
                var c = tri[2].Segment.a;

                // Tener en cuenta nunca como bisecEdge va a traer un

                Halfedge getOrCreateKineticEdge((int a, int b, int label) seg, bool oppsite)
                {
                    int a = seg.a;
                    int b = seg.b;

                    int p, q;
                    if (a < b)
                    {
                        p = a; q = b;
                    }
                    else
                    {
                        p = b; q = a;
                    }

                    if (!kineticHalfedges.TryGetValue((p, q), out var khe))
                    {
                        khe = new Halfedge(this.mSSkel.Halfedges.Count, true);

                        Halfedge opp;
                        if (seg.label <= 0)
                        {
                            opp = new Halfedge(this.mSSkel.Halfedges.Count + 1, true);
                        }
                        else
                        {
                            opp = new Halfedge(this.mSSkel.Halfedges.Count + 1, false);
                        };// new BisectorHalfedge(GreekFire.HeIDs++);

                        khe.Vertex = wavefrontVertices[b];

                        if (seg.label > 0)
                        {
                            khe.Vertex.Halfedge = khe;
                        }
                        opp.Vertex = wavefrontVertices[a];
                        khe.Opposite = opp;
                        opp.Opposite = khe;

                        kineticHalfedges.Add((p, q), khe);
                    }

                    if (khe.Vertex.Id != b || khe.Opposite.Vertex.Id != a)
                    {
                        if (!(khe.Opposite.IsKinetic))
                        {
                            throw new ApplicationException($" segmento ({b},{a})  es de un contorno");
                        }

                        khe = khe.Opposite;
                    }

                    //if (khe.Next != null && khe.Prev != null)
                    //{
                    //    throw new Exception(" I see alive Bugs!!");
                    //}

                    
                    return khe;
                }

                Halfedge? ab = getOrCreateKineticEdge(tri[0].Segment, tri[0].isOposite);

                Halfedge? bc = getOrCreateKineticEdge(tri[1].Segment, tri[1].isOposite);
                Halfedge? ca = getOrCreateKineticEdge(tri[2].Segment, tri[2].isOposite);

                void link(Halfedge a, Halfedge b) { a.Next = b; b.Prev = a; }
                bool isFace(Halfedge a)
                {
                    if (a.IsConstrain)
                    {
                        meshTri.Halfedge = a;
                        return true;
                    }
                    return false;
                }

                ab.Face = bc.Face = ca.Face = meshTri;
                link(ab, bc);
                link(bc, ca);
                link(ca, ab);

                if (!isFace(ab))
                    if (!isFace(bc))
                        if (!isFace(ca))
                            meshTri.Halfedge = ab;

                foreach (var h in meshTri.Halfedges)
                {
                    if (h.IsConstrain)
                    {
                        h.WavefrontEdge = WavefrontEdges[h.Vertex.Id];
                        Debug.Assert(h.Opposite.Vertex.Id == h.WavefrontEdge.vertex(0).Id && h.Vertex.Id == h.WavefrontEdge.vertex(1).Id);
                        
                        h.WavefrontEdge.set_initial_incident_triangle(meshTri);
                    }
                }

                KineticTriangles.Add(meshTri);

                //foreach (var e in meshTri.Halfedge.Circulation(3)) { };
            }
        }

        public void BuildKineticTriangles(IMesh mesh)
        {
            WavefrontVertices.Clear();
            WavefrontEdges.Clear();
            WavefrontVertices.Capacity = this.mSSkel.ContourVertices.Count();
            WavefrontEdges.Capacity = this.mSSkel.ContourVertices.Count();

            foreach (var cv in this.mSSkel.ContourVertices)
            {
                Debug.Assert(WavefrontEdges.Count == cv.Id);
                WavefrontEdges.Add(new WavefrontEdge(cv.Id, cv.PrevInLAV, cv));
            }

            foreach (var cv in this.mSSkel.ContourVertices)
            {
                var left = WavefrontEdges[cv.Id];
                var right = WavefrontEdges[cv.NextInLAV.Id];
                Debug.Assert(WavefrontVertices.Count == cv.Id);
                WavefrontVertices.Add(new Vertex(cv.Id, cv.Point, left, right, true, true));
            }

            foreach (var we in this.WavefrontEdges)
            {
                var cv = this.mSSkel.ContourVertices[we.ID];
                var wva = this.WavefrontVertices[cv.PrevInLAV.Id];
                var wvb = this.WavefrontVertices[cv.Id];
                we.set_initial_vertices(wva, wvb);
                we.set_vertices(wva, wvb);
            }

            BuildKineticTriangles(mesh, WavefrontVertices);

            KineticTriangle.MaxDigit = Math.Max(KineticTriangle.MaxDigit, (this.KineticTriangles.Count - 1).ToString().Length);

            // asignar wavefontedges
        }

        public void CreateContourBisectors()
        {
            CGAL_STSKEL_BUILDER_TRACE(0, "Creating contour bisectors...");

            Stopwatch wt = new Stopwatch();
            wt.Start();

            foreach (Vertex contourVertex in mSSkel.ContourVertices)
            {
                var contourHe = contourVertex.Halfedge;
                var oppContourVertex = contourHe.Opposite.Vertex;

                Vertex wavefrontVertex = this.WavefrontVertices[contourVertex.Id];

                var wavefrontHe = wavefrontVertex.Halfedge;
                var oppWavefrontHe = wavefrontVertex.Halfedge.Opposite;
                var oppWavefrontVertex = oppWavefrontHe.Vertex;

                Debug.Assert(wavefrontVertex.Id == contourVertex.Id);
                Debug.Assert(oppWavefrontVertex.Id == oppContourVertex.Id);

                Debug.Assert(wavefrontVertex.Point == contourVertex.Point);
                Debug.Assert(oppWavefrontVertex.Point == oppContourVertex.Point);
                Debug.Assert(wavefrontHe.IsConstrain);
                Debug.Assert(!oppWavefrontHe.IsKinetic);

                Vertex lPrev = contourVertex.PrevInLAV;
                Vertex lNext = contourVertex.NextInLAV;

                var lOrientation = orientation(lPrev.point(), contourVertex.point(), lNext.point());

#if MYDEBUG
                v.Bisector = ((lPrev.point() - v.point()).Normal() + (lNext.point() - v.point()).Normal()) / 2.0;
#endif

                if (lOrientation == OrientationEnum.COLLINEAR)
                {
                    wavefrontVertex.IsStraight = contourVertex.IsStraight = true;
                    CGAL_STSKEL_BUILDER_TRACE(1, $"COLLINEAR vertex: N{contourVertex.Id} {lPrev.point()},{contourVertex.point()},{lNext.point()} ");
                }
                else if (lOrientation == OrientationEnum.RIGHT_TURN)
                {
                 
                    wavefrontVertex.IsReflex = contourVertex.IsReflex = true;
                    CGAL_STSKEL_BUILDER_TRACE(1, $"Reflex vertex: N{contourVertex.Id}");
                }

                CreateNewEdge(out var lOBisector, out var lIBisector);

                lOBisector.Face = contourVertex.Halfedge.Face;
                lIBisector.Face = contourVertex.Halfedge.Face;
                lIBisector.Vertex = contourVertex;

                Halfedge lIBorder = contourVertex.Halfedge;
                Halfedge lOBorder = contourVertex.Halfedge.Next;
                lIBorder.Next = lOBisector;
                lOBisector.Prev = lIBorder;
                lOBorder.Prev = lIBisector;
                lIBisector.Next = lOBorder;
                CGAL_STSKEL_BUILDER_TRACE(3
                                         , $"Adding Contour Bisector at {contourVertex}"
                                         , $" B{lOBisector.Id} (Out)"
                                         , $" B{lIBisector.Id}(In)"
                                         );
            }

            foreach (Face fit in mSSkel.Faces)
            {
                Halfedge lBorder = fit.Halfedge;
                Halfedge lLBisector = lBorder.Prev;
                Halfedge lRBisector = lBorder.Next;

                var cHe = lBorder;
                var oppContourVertex = cHe.Opposite.Vertex;

                Vertex wV = this.WavefrontVertices[lBorder.Vertex.Id];

                var wfHe = wV.Halfedge;
                var owfHe = wV.Halfedge.Opposite;
                var owV = owfHe.Vertex;

                Debug.Assert(wV.Id == lBorder.Vertex.Id);
                Debug.Assert(owV.Id == oppContourVertex.Id);

                Debug.Assert(wV.Point == lBorder.Vertex.Point);
                Debug.Assert(owV.Point == oppContourVertex.Point);
                Debug.Assert(wfHe.IsKinetic);
                Debug.Assert(!owfHe.IsKinetic);

                this.CrossLinkFwd(lRBisector, owfHe);
                lRBisector.Vertex = wV;
                owfHe.Face = lRBisector.Face;
                this.CrossLinkFwd(owfHe, lLBisector);
                lLBisector.Opposite.Vertex = owV;

                lLBisector.Face = lRBisector.Face;

                Debug.Assert(lBorder.Halfedges.Take(5).Count() == 4);
                Debug.Assert(lBorder.Halfedges.Last() == lLBisector);

                SetBisectorSlope(lRBisector, SignEnum.POSITIVE);
                SetBisectorSlope(lLBisector, SignEnum.NEGATIVE);

                CGAL_STSKEL_BUILDER_TRACE(3, $"Closing face of {lBorder} with a fictitious vertex. B{lRBisector}.N{owfHe}.B{lLBisector}");
            }
            foreach (Face fit in mSSkel.Faces)
            {
                Halfedge lBorder = fit.Halfedge;
                CGAL_STSKEL_BUILDER_TRACE(3, $"Closing face {fit} {string.Join(" => ", lBorder.Circulation())}");
            }

            wt.Stop();
        }
        void SetBisectorSlope(Halfedge aBisector, SignEnum aSlope)
        {
            aBisector.Slope = aSlope;
        }
        void SetBisectorSlope(CGAL.Vertex aA, CGAL.Vertex aB)
        {
            Halfedge lOBisector = aA.primary_bisector();
            Halfedge lIBisector = lOBisector.Opposite;

            CGAL_precondition(!aA.IsContour() || !aB.IsContour());

            if (aA.IsContour())
            {
                SetBisectorSlope(lOBisector, SignEnum.POSITIVE);
                SetBisectorSlope(lIBisector, SignEnum.NEGATIVE);
            }
            else if (aB.IsContour())
            {
                SetBisectorSlope(lOBisector, SignEnum.NEGATIVE);
                SetBisectorSlope(lIBisector, SignEnum.POSITIVE);
            }
            else
            {
                if (aA.has_infinite_time())
                {
                    CGAL_precondition(!aB.has_infinite_time());

                    SetBisectorSlope(lOBisector, SignEnum.NEGATIVE);
                    SetBisectorSlope(lIBisector, SignEnum.POSITIVE);
                }
                else if (aB.has_infinite_time())
                {
                    CGAL_precondition(!aA.has_infinite_time());

                    SetBisectorSlope(lOBisector, SignEnum.NEGATIVE);
                    SetBisectorSlope(lIBisector, SignEnum.POSITIVE);
                }
                else
                {
                    CGAL_precondition(!aA.has_infinite_time());
                    CGAL_precondition(!aB.has_infinite_time());

           //         SignEnum lSlope = (SignEnum)(int)CompareEvents(GetTrisegment(aB), GetTrisegment(aA));
                    //SetBisectorSlope(lOBisector, lSlope);
                    //SetBisectorSlope(lIBisector, (SignEnum)(-(int)(lSlope)));
                }
            }
        }
    }
}