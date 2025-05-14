using System.Diagnostics;
using TriangleNet.Meshing;
using TriangleNet.Topology.DCEL;
using TriangleNet.Topology;
using GFL.Kernel;
using TriangleNet.Geometry;
using TriangleNet;

namespace GFL
{

    public partial class GreekFireBuilder
    {
        public int EdgeIDs = 0;
        public int VertexIDs = 0;
        public int FaceIDs = 0;



        public GreekFire GreekFire;

        public List<KineticTriangle> KineticTriangles { get; set; } = new List<KineticTriangle>();
        public List<WavefrontVertex> WavefrontVertices { get; set; } = new List<WavefrontVertex>();

        public List<WavefrontEdge> WavefrontEdges { get; set; } = new List<WavefrontEdge>();
        public EventQueue EventQueue { get; set; }

        [Obsolete]
        public EventQueue eq => this.EventQueue;
        [Obsolete]
        private EventQueue queue => this.EventQueue;

        public GreekFireBuilder()
        {
            GreekFire = new GreekFire();
            ;
        }


        public IEnumerable<GfbStepResult> DebugBuild(IEnumerable<IEnumerable<Vector2D>> points)
        {
            yield return new GfbStart(this);

            foreach (var p in points)
                enter_contour(p, true, 0.05);
            yield return new GfbEnterContor(this);
            var mesh = Triangulate();
            BuildKineticTriangles(mesh);
            initialize();
            yield return new GfbTriangulate(this);

            init_event_list();

            yield return new GfbStart(this);
            // TODO 
            yield return new CreateInitialEventsGfbStepResult(this);

        }



        public GreekFireBuilder enter_contour(IEnumerable<Vector2D> aPoly, bool aCheckValidity = true, double eps = Const.EPSILON)
        {
            if (aCheckValidity)
            {
                List<Vector2D> nPoly = new List<Vector2D>();

                int idx = 0;
                foreach (var p in aPoly)
                {
                    if (nPoly.Count == 0 || !nPoly.Last().AreNear(p, eps))
                    {
                        nPoly.Add(p);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(aPoly), $"point {nPoly[idx]} at index:{idx} is near at previuos point {nPoly[idx - 1]}  ");
                    }
                    idx++;
                }
                if (nPoly.Count > 0 && nPoly.First().AreNear(nPoly.Last(), eps))
                {
                    nPoly.RemoveAt(nPoly.Count - 1);
                }
                if (nPoly.Count < 3)
                    throw new ArgumentOutOfRangeException(nameof(aPoly), "Degenerate contour (less than 3 non-degenerate vertices).");
                aPoly = nPoly;

            };
            GreekFire.EnterValidContour(aPoly);

            return this;
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

        public Mesh Triangulate()
        {
            var polygons = new TriangleNet.Geometry.Polygon();

            foreach (var polygon in GreekFire.Polygons.Select(e => e))
            {


                TriangleNet.Geometry.Vertex toVector(Vector2D v, int i) => new TriangleNet.Geometry.Vertex(v.X, v.Y, i);
                Contour pts = new Contour(polygon.Vertices(GreekFire.ContourVertices)
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

        public void BuildKineticTriangles(IMesh mesh)
        {

            WavefrontVertices.Clear();
            WavefrontEdges.Clear();
            WavefrontVertices.Capacity = GreekFire.ContourVertices.Count();
            WavefrontEdges.Capacity = GreekFire.ContourVertices.Count();

            foreach (var cv in this.GreekFire.ContourVertices)
            {
                Debug.Assert(WavefrontEdges.Count == cv.ID);
                WavefrontEdges.Add(new WavefrontEdge(cv.ID, cv.Point, cv.NextInLAV.Point));
            }

            foreach (var cv in this.GreekFire.ContourVertices)
            {
                var left = WavefrontEdges[cv.PrevInLAV.ID];
                var right = WavefrontEdges[cv.ID];
                Debug.Assert(WavefrontVertices.Count == cv.ID);
                WavefrontVertices.Add(new WavefrontVertex(cv.ID, cv.Point, left, right, true, true));
            }

            foreach (var we in this.WavefrontEdges)
            {
                var cv = this.GreekFire.ContourVertices[we.ID];
                var wva = this.WavefrontVertices[cv.ID];
                var wvb = this.WavefrontVertices[cv.NextInLAV.ID];
                we.set_initial_vertices(wva, wvb);
                we.set_vertices(wva, wvb);
            }




            // initializate

            KineticTriangles = new List<KineticTriangle>(mesh.Triangles.Count);

            Dictionary<(int a, int b), KineticHalfedge> kineticHalfedges = new Dictionary<(int a, int b), KineticHalfedge>();


            var tt = mesh.Triangles.Where(t => t.GetHashCode() >= 0);

            foreach (var tri in EnumerateTriangules(mesh))
            {
                var KTri = new KineticTriangle(KineticTriangles.Count);

                var a = tri[0].Segment.a;
                var b = tri[1].Segment.a;
                var c = tri[2].Segment.a;

                // Tener en cuenta nunca como bisecEdge va a traer un

                KineticHalfedge getOrCreateKineticEdge((int a, int b, int label) seg, bool oppsite)
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



                        khe = new KineticHalfedge(GreekFire.HeIDs++);

                        IHalfedge opp = (seg.label <= 0) ? new KineticHalfedge(GreekFire.HeIDs++)
                                                         : new BisectorHalfedge(GreekFire.HeIDs++);


                        khe.Vertex = WavefrontVertices[a];
                        if (seg.label > 0)
                        {
                            khe.Vertex.Halfedge = khe;
                        }

                        opp.Vertex = WavefrontVertices[b];
                        khe.Opposite = opp;
                        opp.Opposite = khe;

                        if (khe.Vertex.ID != a || khe.Opposite.Vertex.ID != b)
                        {

                        }



                        kineticHalfedges.Add((p, q), khe);
                    }

                    if (khe.Vertex.ID != a || khe.Opposite.Vertex.ID != b)
                    {
                        if (!(khe.Opposite is KineticHalfedge opp))
                        {
                            throw new ApplicationException($" segmento ({b},{a})  es de un contorno");
                        }



                        khe = opp;
                    }

                    if (khe.Vertex.ID != a || khe.Opposite.Vertex.ID != b)
                    {

                    }

                    if (khe.Next != null && khe.Prev != null)
                    {
                        throw new Exception(" I see alive Bugs!!");
                    }

                    return khe;

                }

                KineticHalfedge? ab = getOrCreateKineticEdge(tri[0].Segment, tri[0].isOposite);

                KineticHalfedge? bc = getOrCreateKineticEdge(tri[1].Segment, tri[1].isOposite);
                KineticHalfedge? ca = getOrCreateKineticEdge(tri[2].Segment, tri[2].isOposite);

                void link(IHalfedge a, IHalfedge b) { a.Next = b; b.Prev = a; }
                bool isFace(IHalfedge a)
                {
                    if (a.Opposite is BisectorHalfedge)
                    {
                        KTri.Halfedge = a;
                        return true;
                    }
                    return false;
                }

                ab.Face = bc.Face = ca.Face = KTri;
                link(ab, bc);
                link(bc, ca);
                link(ca, ab);

                if (!isFace(ab))
                    if (!isFace(bc))
                        if (!isFace(ca))
                            KTri.Halfedge = ab;




                KineticTriangles.Add(KTri);
                Console.WriteLine($"{KTri}");
                foreach (var e in KTri.Halfedge.Circulation(3)) { };
            }
        }

        internal void CreateKineticVertices()
        {









            // DBG_FUNC_BEGIN(DBG_KT_SETUP);
            /* total number of wavefront edges during the entire propragation.
             * this is an upper bound.
             *
             * num_t is a really rough upper bound on the number of split events.
             */
            // int num_t = num_initial_triangles + input.get_num_extra_beveling_vertices();
            //  var num_wavefront_edges = input.edges().size() * 2 +  num_t * 2;

            //  this.wavefront_edges.Capacity = num_wavefront_edges;

            foreach (var t in this.KineticTriangles)
            {
                //if (!fit.info().matches_component(restrict_component_)) continue;
                WavefrontEdge3 w = new WavefrontEdge3();

                var edges = t.Halfedges.ToArray();

                Debug.Assert(edges.Length == 3);
                for (int i = 0; i < 3; ++i)
                {
                    var he = edges[TriangulationUtils.ccw(i)];
                    if (edges[TriangulationUtils.ccw(i)].is_constrained())
                    {

                        w[i] = this.WavefrontEdges[he.KVertex.ID];
                    }
                    else
                    {
                        w[i] = null;
                    }
                }

                t.set_wavefronts((WavefrontEdge[])w);

            }
        }


        public void initialize()
        {

        }
        public void init_event_list()
        {
        }
    }
}

