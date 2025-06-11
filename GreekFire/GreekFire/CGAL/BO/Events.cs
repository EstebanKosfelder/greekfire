using CGAL;
using System;
using System.Collections.Generic;

using FT = System.Double;

using Point_2 = CGAL.Point2;
using System.Diagnostics.CodeAnalysis;

namespace CGAL
{
    public abstract class Event
    {
        public static int EventIds=0;

        public enum Type
        { cEdgeEvent, cSplitEvent, cPseudoSplitEvent, cArficialEvent };

        public Event([NotNull] Triedge aTriedge, [NotNull] Trisegment aTrisegment)
        {
            Id = EventIds++;
            mTriedge = aTriedge;
            mTrisegment = aTrisegment;
        }

        public abstract Type type();

        public abstract Vertex seed0();

        public abstract Vertex seed1();

        public Triedge triedge()
        { return mTriedge; }

        public Trisegment trisegment()
        { return mTrisegment; }

        public Point_2 point()
        { return mP; }

        public FT time()
        { return mTime; }

        public void SetTimeAndPoint(FT aTime, in Point_2 aP)
        { mTime = aTime; mP = aP; }

        public override string ToString()
        {
            return $"[{dump()} p=({point().x()},{point().y()}) t={time()}] {trisegment().collinearity()}";
        }

        protected virtual string dump()
        { return mTriedge.ToString(); }

        private Triedge mTriedge;
        private Trisegment mTrisegment;
        private Point_2 mP;
        private FT mTime;

        public int Id { get;private set; }
      
    }

    public class EdgeEvent : Event
    {
        public EdgeEvent(Triedge aTriedge
                       , Trisegment aTrisegment
                       ,  Vertex aLSeed
               ,  Vertex aRSeed
               )
    : base(aTriedge, aTrisegment)
        {
            mLSeed = aLSeed;
            mRSeed = aRSeed;
        }

        public override Type type()
        { return Type.cEdgeEvent; }

        public override Vertex seed0()
        { return mLSeed; }

        public override Vertex seed1()
        { return mRSeed; }

        protected override string dump()
        {
            return $"{base.dump()}(Edge Event, LSeed={mLSeed.Id} RSeed={mRSeed.Id})";
        }

        [NotNull]
        private Vertex mLSeed;

        [NotNull]
        private Vertex mRSeed;
    }

    public class SplitEvent : Event
    {
        public SplitEvent(Triedge aTriedge, Trisegment aTrisegment, Vertex aSeed)
            : base(aTriedge, aTrisegment)
        {
            mSeed = aSeed;
        }

        public override Type type()
        { return Type.cSplitEvent; }

        public override Vertex seed0()
        { return mSeed; }

        public override Vertex seed1()
        { return mSeed; }

        public void set_opposite_rnode(Vertex aOppR)
        { mOppR = aOppR; }

        public Vertex? opposite_rnode()
        { return mOppR; }

        protected override string dump()
        {
            return $"{base.dump()} (Split Event, Seed={mSeed.Id} pos=({mSeed.point()}) OppBorder={this.triedge().e2().Id})";
        }

        private Vertex mSeed;
        private Vertex? mOppR;
    }

    public class PseudoSplitEvent : Event
    {
        public PseudoSplitEvent(Triedge aTriedge
                               , Trisegment aTrisegment
                               , Vertex aSeed0
                               , Vertex aSeed1
                               , bool aOppositeIs0
                               )
            :
              base(aTriedge, aTrisegment)
        {
            mSeed0 = (aSeed0);
            mSeed1 = (aSeed1);
            mOppositeIs0 = (aOppositeIs0);
        }

        public override Type type()
        { return Type.cPseudoSplitEvent; }

        public override Vertex seed0()
        { return mSeed0; }

        public override Vertex seed1()
        { return mSeed1; }

        public bool opposite_node_is_seed_0()
        { return mOppositeIs0; }

        public bool is_at_source_vertex()
        { return opposite_node_is_seed_0(); }

        public Vertex opposite_seed()
        { return opposite_node_is_seed_0() ? seed0() : seed1(); }

        protected override string dump()
        {
            return $"{base.dump()} (Pseudo-split Event, Seed0={mSeed0.Id}{(mOppositeIs0 ? " {Opp} " : " ")} Seed1={mSeed1.Id}{(!mOppositeIs0 ? " {Opp} " : "")})";
        }

        private Vertex mSeed0;
        private Vertex mSeed1;
        private bool mOppositeIs0;
    }

    public class ArtificialEvent : Event
    {
        public ArtificialEvent(Triedge aTriedge,
                               Trisegment aTrisegment,
                               Vertex aSeed)
                : base(aTriedge, aTrisegment)
        { mSeed = aSeed; }

        public override Type type()
        { return Type.cArficialEvent; }

        public override Vertex seed0()
        { return mSeed; }

        public override Vertex seed1()
        { return mSeed; }

        protected override string dump()
        {
            return $"{base.dump()} (Artificial Event, Seed={mSeed.Id})";
        }

        private Vertex mSeed;
    };
}