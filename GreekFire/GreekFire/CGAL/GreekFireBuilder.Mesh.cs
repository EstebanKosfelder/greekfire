using System.Diagnostics;
using TriangleNet.Meshing;
using TriangleNet.Topology.DCEL;
using TriangleNet.Topology;
using GFL.Kernel;
using TriangleNet.Geometry;
using TriangleNet;
using System.Text.Json;

namespace CGAL
{

    public partial class GreekFireBuilder
    {
        public int EdgeIDs = 0;
        public int VertexIDs = 0;
        public int FaceIDs = 0;



      

     

       
       
        public Rect2D Bounds { get; set; }


      


     
        public static void GuardarListaVector2(string rutaArchivo, List<List<Point2>> lista)
        {
            var options = new JsonSerializerOptions { WriteIndented = true }; // Formato bonito (legible)
            string json = JsonSerializer.Serialize(lista, options);

            File.WriteAllText(rutaArchivo, json);
        }
        public static List<List<Point2>> CargarListaVector2(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("El archivo no existe.", rutaArchivo);

            string json = File.ReadAllText(rutaArchivo);
            return JsonSerializer.Deserialize<List<List<Point2>>>(json);
        }


        public void Initialize()
        {
            
            var m = Triangulate();
            BuildKineticTriangles(m);
            CreateContourBisectors();

            Debug.Assert(AssertTriangleOrientation());
            RefineTriangulationInitial();
            //Debug.Assert(AssertTriangleOrientation());
        }
       
        public bool AssertTriangleOrientation()
        {
            var list = KineticTriangles.Where(t => t.Area()<0 );
            if (list.Any())
            {

            }
            return !list.Any();

        }







     
    }
}

