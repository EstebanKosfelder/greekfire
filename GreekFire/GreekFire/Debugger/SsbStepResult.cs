
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFL
{
    public abstract class GfbStepResult
    {
        public GreekFireBuilder Builder { get; private set; }

        public GfbStepResult([NotNull] GreekFireBuilder  skeleton)
        {
            Builder = skeleton;
           
            
        }
       
       public  enum StepType {
            CreateContourBisectors,
            HarmonizeSpeeds,
            CreateInitialEvents,


        }

        //public virtual Rect2D Bounds()
        //{
        //   return  Rect2D.NaN();
        //}
    }

    public class GfbStart : GfbStepResult
    {
        public GfbStart([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }

    public class GfbEnd : GfbStepResult
    {
        public GfbEnd([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }

    public class GfbCanceled : GfbStepResult
    {
        public GfbCanceled([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }


    public class GfbError : GfbStepResult
    {
       public readonly Exception  Exception ;
        public GfbError([NotNull] GreekFireBuilder  skeleton, Exception ex) : base(skeleton)
        {
            Exception = ex;
        }
    }
    public class GfbEnterContor : GfbStepResult
    {
        public GfbEnterContor([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }

    public class GfbTriangulate : GfbStepResult
    {
        public GfbTriangulate([NotNull] GreekFireBuilder skeleton) : base(skeleton)
        {
        }
    }




    public class CreateContourBisectorsGfbStepResult : GfbStepResult
    {
        public CreateContourBisectorsGfbStepResult([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }

        //public override Rect2D Bounds()
        //{
   
        //    return this.Builder.Bounds();
        //}
    }

    public class HarmonizeSpeedsGfbStepResult : GfbStepResult
    {
        public HarmonizeSpeedsGfbStepResult([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }

    public class CreateInitialEventsGfbStepResult : GfbStepResult
    {
        public CreateInitialEventsGfbStepResult([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }

    public class PropagateGfbStepResult : GfbStepResult
    {
        public PropagateGfbStepResult([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {

        }
    }

    public class EnventProcessStepResult : GfbStepResult
    {
        public readonly CollapseEvent Event;
        public EnventProcessStepResult([NotNull] GreekFireBuilder  skeleton, CollapseEvent @event) : base(skeleton)
        {
            Event = @event; 
        }
    }

    public class FinishUpStepResult : GfbStepResult
    {
        public FinishUpStepResult([NotNull] GreekFireBuilder  skeleton): base(skeleton)
        {
            
        }
    }
}
