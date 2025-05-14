using GFL.Kernel;
using TriangleNet.Topology.DCEL;

namespace GFL
{

    public class Vertex
    {
        public Vector2D Point { get; set; }
        public int ID { get; set; }

        public Vertex(int id, Vector2D point)
        {
            ID = id;
            Point = point;
        }
    }

#pragma warning disable CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.
        public class ContourVertex : Vertex
    {
            public readonly Polygon Polygon;
            public ContourVertex(Polygon polygon, int id, Vector2D point) : base(id, point)
            {
            Polygon = polygon;
        }

        public ContourVertex NextInLAV { get; set; }
        public ContourVertex PrevInLAV { get; set; }

        public override string ToString() => $"C{base.ToString()}";
            #pragma warning restore CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.

    }
#pragma warning restore CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.

    }
