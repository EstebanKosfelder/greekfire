using GFL.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFL
{
    public partial class GreekFire
    {
        internal int HeIDs = 0;
     
     

        public List<ContourVertex> ContourVertices { get; set; } = new List<ContourVertex>();

        public List<Polygon> Polygons { get; set; } = new List<Polygon> { };



        public GreekFire()
        {
          
        }

        public void EnterValidContour(IEnumerable<Vector2D> values)
        {

            var polygon = new Polygon(Polygons.Count + 1, ContourVertices.Count, values.Count(), Polygons.Count != 0);

            ContourVertices.Capacity = ContourVertices.Count + polygon.Count;
            ContourVertices.AddRange(values.Select((p, i) => new ContourVertex(polygon, i + polygon.StartIndex, p)));
            Console.WriteLine($"polygon {polygon.ID} start at {polygon.StartIndex}");
            for (int i = 0; i < polygon.Count; i++)
            {
                Console.WriteLine($"\t{i}\t{ContourVertices[i + polygon.StartIndex].Point}");
            }

            var last = ContourVertices.Last();
            foreach (var curr in ContourVertices.Skip(polygon.StartIndex))
            {
                last.NextInLAV = curr;
                curr.PrevInLAV = last;
                last = curr;
            }

            Polygons.Add(polygon);

        }


    }
}
