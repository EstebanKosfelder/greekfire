using System.Diagnostics;
using TriangleNet.Meshing;
using TriangleNet.Topology.DCEL;
using TriangleNet.Topology;
using GFL.Kernel;
using TriangleNet.Geometry;
using TriangleNet;
using System.Text.Json;

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
        public GFL.EventQueue EventQueue { get; set; }
       
        public Rect2D Bounds { get; set; }


      

        public GreekFireBuilder()
        {
            GreekFire = new GreekFire();
            this.Bounds = Rect2D.NaN();

        }


        public IEnumerable<GfbStepResult> DebugBuild(IEnumerable<IEnumerable<Vector2D>> points)
        {
            


            yield return new GfbStart(this);

            foreach (var p in points)
            {
                enter_contour(p, true, 0.05);
            }
            yield return new GfbEnterContor(this);
           

            initialize();
            yield return new GfbTriangulate(this);

            init_event_list();

            yield return new GfbStart(this);
            // TODO 
            yield return new CreateInitialEventsGfbStepResult(this);

        }
        public static void GuardarListaVector2(string rutaArchivo, List<List<Vector2D>> lista)
        {
            var options = new JsonSerializerOptions { WriteIndented = true }; // Formato bonito (legible)
            string json = JsonSerializer.Serialize(lista, options);

            File.WriteAllText(rutaArchivo, json);
        }
        public static List<List<Vector2D>> CargarListaVector2(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("El archivo no existe.", rutaArchivo);

            string json = File.ReadAllText(rutaArchivo);
            return JsonSerializer.Deserialize<List<List<Vector2D>>>(json);
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
             Bounds.Extend(aPoly);
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

        public void BuildKineticTriangles(IMesh mesh ,List<WavefrontVertex> wavefrontVertices)
        {

            KineticTriangles = new List<KineticTriangle>(mesh.Triangles.Count);

            Dictionary<(int a, int b), KineticHalfedge> kineticHalfedges = new Dictionary<(int a, int b), KineticHalfedge>();


            var tt = mesh.Triangles.Where(t => t.GetHashCode() >= 0);

            foreach (var tri in EnumerateTriangules(mesh))
            {
                var meshTri = new KineticTriangle(KineticTriangles.Count);

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

                        IHalfedge opp;
                        if (seg.label <= 0) { opp = new KineticHalfedge(GreekFire.HeIDs++); }
                        else {opp = this.WavefrontEdges[b];  };// new BisectorHalfedge(GreekFire.HeIDs++);

                        
                        khe.Vertex_ = wavefrontVertices[a];
                       
                        if (seg.label > 0)
                        {
                            khe.Vertex_.Halfedge = khe;
                        }
                        opp.Vertex_ = wavefrontVertices[b];
                        khe.Opposite_ = opp;
                        opp.Opposite_ = khe;



                     



                        kineticHalfedges.Add((p, q), khe);
                    }

                    if (khe.Vertex_.ID != a || khe.Opposite_.Vertex_.ID != b)
                    {
                        if (!(khe.Opposite_ is KineticHalfedge opp))
                        {
                            throw new ApplicationException($" segmento ({b},{a})  es de un contorno");
                        }



                        khe = opp;
                    }

                    if (khe.Vertex_.ID != a || khe.Opposite_.Vertex_.ID != b)
                    {

                    }

                    if (khe.Next_ != null && khe.Prev_ != null)
                    {
                        throw new Exception(" I see alive Bugs!!");
                    }

                    return khe;

                }

                KineticHalfedge? ab = getOrCreateKineticEdge(tri[0].Segment, tri[0].isOposite);

                KineticHalfedge? bc = getOrCreateKineticEdge(tri[1].Segment, tri[1].isOposite);
                KineticHalfedge? ca = getOrCreateKineticEdge(tri[2].Segment, tri[2].isOposite);

                void link(IHalfedge a, IHalfedge b) { a.Next_ = b; b.Prev_ = a; }
                bool isFace(IHalfedge a)
                {
                    if (a.Opposite_ is WavefrontEdge)
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




                KineticTriangles.Add(meshTri);
                
                //foreach (var e in meshTri.Halfedge.Circulation(3)) { };
            }
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
                GreekFire.HeIDs++;
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
                we.Vertex_ = wva;
            }
            




            BuildKineticTriangles(mesh, WavefrontVertices);

            WavefrontVertex.MaxDigit = Math.Max(WavefrontVertex.MaxDigit, (this.WavefrontVertices.Count - 1).ToString().Length);
            KineticTriangle.MaxDigit = Math.Max(KineticTriangle.MaxDigit, (this.KineticTriangles.Count - 1).ToString().Length);


            // crear kinetic triangles y asigna wavefronVertices
            foreach (var kineticTriangle in KineticTriangles)
            {
              
            
               
             
                var idx = 0;
                foreach (var v in kineticTriangle.Vertices)
                {   if (v is WavefrontVertex wv)
                    {
                        kineticTriangle.vertices[idx]=wv;
                    }
                    else throw new GFLException($"i:{idx} {v} no es {nameof(WavefrontVertex)}");
                    idx++;
                }
                
                
            }

            // asignar los neighbors
            foreach (var t in KineticTriangles)
            {
                var neighbors = new KineticTriangle?[3];
                var kineticTriangle =  KineticTriangles[t.ID];
                var idx = 0;
                foreach (var he in t.Halfedges)
                {
                    if ( he.Opposite_ !=null && he.Opposite_.Face is KineticTriangle mot)
                    {
                        neighbors[idx] =  KineticTriangles[mot.ID];
                    }
                    idx++;
                }
                kineticTriangle.set_neighbors(neighbors);
            }

            // asignar wavefontedges
            foreach (var kineticTriangle in KineticTriangles)
            {
                var wavefrontegdes = new WavefrontEdge[3];
                for( var i = 0; i < 3; i++) {
                 //   var ccwi = TriangulationUtils.(i);
                    if ( kineticTriangle.neighbor(i) ==null) 
                    {
                        wavefrontegdes[i] = WavefrontEdges[kineticTriangle.vertex(i).ID];
                        wavefrontegdes[i].set_initial_incident_triangle(kineticTriangle);

                    }
;
                }
                kineticTriangle.set_wavefronts(wavefrontegdes);

                var o = MathEx.orientation(kineticTriangle.vertex(0).Point, kineticTriangle.vertex(1).Point, kineticTriangle.vertex(2).Point);
                if (o > 0)
                {

                }
                else if (o < 0)
                {

                }
                else
                {
                }
            }


        }

       


        public void initialize()
        {
            //var l = GreekFire.Polygons.Select(p => p.Vertices(GreekFire.ContourVertices).Select(e => e.Point).ToList()).ToList();
            //GuardarListaVector2("F:\\trabajos\\g_test.json",l );

            //this.Bounds = this.Bounds.Offset(this.Bounds.Size * 0.3);
            var m = Triangulate();
            BuildKineticTriangles(m);
            Debug.Assert(AssertTriangleOrientation());
            RefineTriangulationInitial();
            Debug.Assert(AssertTriangleOrientation());
        }
        public void init_event_list()
        {
            this.EventQueue = new GFL.EventQueue(KineticTriangles);
        }

        public bool AssertTriangleOrientation()
        {
            var list = KineticTriangles.Where(t => t.Area() <= 0);
            return !list.Any();

        }







        public IEnumerable<PropagateGfbStepResult> event_loop(bool pause = false)
        {


            WavefrontPropagator wp = new WavefrontPropagator(this);


            while (!wp.propagation_complete() && !EventQueue.empty())
            {
                wp.advance_step();

                yield return new PropagateGfbStepResult(this);

                //wp.advance_step();

            }


         


           

        }
    }
}

