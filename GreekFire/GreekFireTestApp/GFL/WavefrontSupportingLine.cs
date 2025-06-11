

namespace GFL
{
    using GFL.Kernel;
    public class WavefrontSupportingLine
    {
        // private  using Transformation = CGAL.Aff_transformation_2<Kernel>;

        public override string ToString()
        {
            return $"e:  wfsl({l}\tw:{weight}\tnd:{normal_direction}\tnu:{normal_unit}\tn:{normal})";
        }

        public Line2D l;

        public double weight;

        /// <summary>
        /// arbitrary length
        /// </summary>
        public Vector2D normal_direction;

        /// <summary>
        /// unit length
        /// </summary>
        public Vector2D normal_unit;

        /// <summary>
        /// weighted
        /// </summary>
        public Vector2D normal;

        public WavefrontSupportingLine(Vector2D u, Vector2D v, double p_weight = 1) : this(new Line2D(u, v), p_weight)
        {
        }

        public WavefrontSupportingLine(Line2D p_l, double p_weight =1)
        {
            this.l = p_l;
            this.weight = p_weight;
            normal_direction = l.to_vector();
            normal_unit = normal_direction / normal_direction.Len_2D();
            normal = (normal_unit * weight);
        }

        public Line2D line_at_one()
        {

            // Traslada la línea por un vector
            var result = l.Translate(normal);
            return result;
        }



        public Line2D line_at(double now)
        {
            return l.Translate(normal*now);
        }
    };
}