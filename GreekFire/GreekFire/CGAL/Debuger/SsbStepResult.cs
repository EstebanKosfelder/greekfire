
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGAL
{
    public abstract class SsbStepResult
    {

        public SsbStepResult([NotNull] GreekFireBuilder  skeleton)
        {
            Skeleton = skeleton;
           
            
        }
        public readonly GreekFireBuilder Skeleton;
       
       public  enum StepType {
            CreateContourBisectors,
            HarmonizeSpeeds,
            CreateInitialEvents,


        }

       
    }

    public class SsbStart : SsbStepResult
    {
        public SsbStart([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }

    public class SsbEnd : SsbStepResult
    {
        public SsbEnd([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }

    public class SsbCanceled : SsbStepResult
    {
        public SsbCanceled([NotNull]  GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }


    public class SsbError : SsbStepResult
    {
       public readonly Exception  Exception ;
        public SsbError([NotNull] GreekFireBuilder  skeleton, Exception ex) : base(skeleton)
        {
            Exception = ex;
        }
    }
    public class SsbEnterContor : SsbStepResult
    {
        public SsbEnterContor([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }

    public class CreateContourBisectorsSsbStepResult : SsbStepResult
    {
        public CreateContourBisectorsSsbStepResult([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }

       
    }

    public class HarmonizeSpeedsSsbStepResult : SsbStepResult
    {
        public HarmonizeSpeedsSsbStepResult([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }

    public class CreateInitialEventsSsbStepResult : SsbStepResult
    {
        public CreateInitialEventsSsbStepResult([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {
        }
    }

    public class PropagateSsbStepResult : SsbStepResult
    {
        public PropagateSsbStepResult([NotNull] GreekFireBuilder  skeleton) : base(skeleton)
        {

        }
    }
    public class SsbTriangulate : SsbStepResult
    {
        public SsbTriangulate([NotNull] GreekFireBuilder skeleton) : base(skeleton)
        {
        }
    }
    public class EnventProcessStepResult : SsbStepResult
    {
        public readonly Event Event;
        public EnventProcessStepResult([NotNull] GreekFireBuilder  skeleton, Event @event) : base(skeleton)
        {
            Event = @event; 
        }
    }

    public class FinishUpStepResult : SsbStepResult
    {
        public FinishUpStepResult([NotNull] GreekFireBuilder  skeleton): base(skeleton)
        {
            
        }
    }
}
