using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFL
{
    using static Debugger.DebugLog;

    public partial class WavefrontPropagator
    {
        private EventQueue eq => _builder.EventQueue;
        private uint event_ctr_ = 0;
        private bool finalized = false;

        private double time = 0.0;
        private double last_event_time = 0.0;
        private double increment = 0.0005;
        private int current_component = -1;
        private int last_event_component = -1;
        private GreekFireBuilder _builder;

        public WavefrontPropagator(GreekFireBuilder builder)
        {
            _builder = builder;
        }

        private bool no_more_events()
        {
            Debug.Assert(eq != null);
            return eq.empty() || peak().Type == CollapseType.Never;
        }

        public bool propagation_complete() => finalized;

        public double get_time() => time;

        /// <summary>
        ///  Get the current component we are working on.
        /// This is for display purposes only.
        ///    -1 if we should show all. if >=0 we are currently working on this component.
        /// </summary>
        public int get_current_component() => current_component;

        //public void set_time(double t) { time = t; };
        public void set_increment(double i) => increment = i;

        /// <summary>
        /// Move backwards in time
        /// </summary>

        public void reverse_time()
        { time -= increment; }

        /// <summary>
        /// Move forward in time, but ignore any event that may have happened
        /// </summary>
        public void advance_time_ignore_event()
        { time += increment; }

        public void advance_time_ignore_event(double t)
        { time = t; }

        public void reset_time_to_last_event()
        {
            time = last_event_time;
            current_component = last_event_component;
        }

        public CollapseEvent peak()
        {
            Debug.Assert(eq != null);
            Debug.Assert(!eq.empty());
            return (CollapseEvent)eq.peak().priority;
        }

        public uint event_ctr() => event_ctr_;

        /// <summary>
        /// Move forward in time by the increment, or until the next event and handle it
        /// </summary>
        public void advance_time()
        {
            time += increment;

            if (!propagation_complete())
            {
                if (no_more_events())
                {
                    advance_step();
                }
                else
                {
                    double want_time = time;
                    while (!propagation_complete() && want_time > eq.peak().priority.Time)
                    {
                        advance_step();
                    }
                    time = want_time;
                }
            }
        }

        /// <summary>
        /// Move forward in time to the next event but do not handle it yet.
        /// </summary>
        public void advance_time_next()
        {
            if (!no_more_events())
            {
                var next = eq.peak();
                time = next.priority.time();
                current_component = peak().t.component;
            }
        }

        /// <summary>
        /// Move forward in time to the next event and handle it.
        /// </summary>
        public void advance_step()
        {
            ///DBG_INDENT_LEVEL_STORE;
            ///DBG_FUNC_BEGIN(///DBG_PROP);

            if (!no_more_events())
            {
                CollapseEvent next = eq.peak().priority;
                time = next.Time;
                //if (sk.get_kt().restrict_component() != 0)
                //{
                //    current_component = peak().t.component;
                //}
                //++event_ctr_;
                //VLOG(2) << " event#" << event_ctr_ << " @ " << CGAL::to_double(time);
                _builder.HandleEvent(next);
                ///DBG(///DBG_PROP) << " event#" << event_ctr_ << " handling done.  Processing pending PQ updates.";
                eq.ProcessPendingUpdates(time);
                ///DBG(///DBG_PROP) << " event#" << event_ctr_ << " PQ updates done.  Time is now " << CGAL::to_double(time);
                //LOG(INFO) << " event#" << event_ctr_ << " done.  Time is now " << CGAL::to_double(time);

                last_event_time = time;
                last_event_component = current_component;
            }

            if (no_more_events())
            {
                //LOG(INFO) << "All done.";
                finalize();
            }
            else
            {
                CollapseEvent next_event = eq.peak().priority;
                ///DBG(///DBG_PROP) << " event#" << (event_ctr_+1) << " will be " << next_event;
                Debug.Assert((next_event.t.component == current_component && next_event.time() >= last_event_time) ||
                        next_event.t.component > current_component);
                if (eq.Count >= 2)
                {
                    ///DBG(///DBG_PROP) << "   event child in heap: " << eq.peak(1).get_priority();
                    if (eq.Count >= 3)
                    {
                        ///DBG(///DBG_PROP) << "   event child in heap: " << eq.peak(2).get_priority();
                    }
                }
            }
            ///DBG_FUNC_END(///DBG_PROP);
            ///DBG_INDENT_LEVEL_CHECK;
        }

        /// <summary>
        /// Process events until done.
        /// </summary>
        public void advance_to_end()
        {
            while (!propagation_complete())
            {
                advance_step();
            }
        }

        public void do_initial_skips(bool skip_all, int skip_to, double skip_until_time)
        {
            if (skip_all)
            {
                advance_to_end();
            }
            else
            {
                while (!propagation_complete() &&
                       skip_to > event_ctr() + 1)
                {
                    advance_step();
                };
            }
            if (skip_until_time > 0.0)
            {
                while (!propagation_complete() &&
                       (no_more_events() || skip_until_time > peak().time()))
                {
                    advance_step();
                }
                if (skip_until_time > get_time())
                {
                    advance_time_ignore_event(skip_until_time);
                }
            }
        }

        /// <summary>
        /// finish up and create the dcel and whatever else is necessary once the propagation is done.
        /// </summary>
        public void finalize()
        {
            ///DBG_FUNC_BEGIN(///DBG_PROP);
            Debug.Assert(no_more_events());
            if (!finalized)
            {
                ///DBG(///DBG_PROP) << "Calling create_remaining_skeleton_dcel()";
             //TODO   _builder.create_remaining_skeleton_dcel();
                finalized = true;
                current_component = -1;
                ///DBG(///DBG_PROP) << "Finalized.";

                _builder.update_event_timing_stats(-1);
            }
            ///DBG_FUNC_END(///DBG_PROP);
        }
    }
}