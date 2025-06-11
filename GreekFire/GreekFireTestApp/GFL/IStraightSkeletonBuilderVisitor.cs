// Copyright (c) 2006-2008 Fernando Luis Cacciola Carballal. All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
// You can redistribute it and/or modify it under the terms of the GNU
// General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// Licensees holding a valid commercial license may use this file in
// accordance with the commercial license agreement provided with the software.
//
// This file is provided AS IS with NO WARRANTY OF ANY KIND, INCLUDING THE
// WARRANTY OF DESIGN, MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//

// $URL$
// $Id$
// SPDX-License-Identifier: GPL-3.0+
//
// Author(s)     : Fernando Cacciola <fernando_cacciola@ciudad.com.ar>
//
//#ifndef CGAL_STRAIGHT_SKELETON_BUILDER_2_H
//#define CGAL_STRAIGHT_SKELETON_BUILDER_2_H 1

//#include <CGAL/license/Straight_skeleton_2.h>

//#include <CGAL/disable_warnings.h>

//#include <list>
//#include <queue>
//#include <exception>
//#include <string>
//#include <map>

//#include <boost/tuple/tuple.hpp>
//#include <boost/intrusive_ptr.hpp>
//#include <boost/shared_ptr.hpp>
//#include <boost/scoped_ptr.hpp>

//#include <CGAL/algorithm.h>
//#include <CGAL/Straight_skeleton_2/Straight_skeleton_aux.h>
//#include <CGAL/Straight_skeleton_2/Straight_skeleton_builder_events_2.h>
//#include <CGAL/Straight_skeleton_2.h>
//#include <CGAL/Straight_skeleton_builder_traits_2.h>
//#include <CGAL/HalfedgeDS_const_decorator.h>
//#include <CGAL/enum.h>

namespace CGAL
{
    //template<class StraightSkeleton>
    public interface IStraightSkeletonBuilderVisitor
    {
        //typedef StraightSkeleton StraightSkeleton ;

        //typedef  StraightSkeleton::Halfedge  Halfedge  ;
        //typedef  StraightSkeleton::Vertex    Vertex  ;

        void on_contour_edge_entered(Halfedge h);

        void on_initialization_started(int size_of_vertices);

        void on_initial_events_collected(Vertex v, bool is_reflex, bool is_degenerate);

        void on_edge_event_created(Vertex lnode, Vertex rnode);

        void on_split_event_created(Vertex vertex);

        void on_pseudo_split_event_created(Vertex lnode, Vertex rnode);

        void on_initialization_finished();

        void on_propagation_started();

        void on_anihiliation_event_processed(Vertex node0, Vertex node1);


        void on_edge_event_processed(Vertex lseed
                              , Vertex rseed
                              , Vertex node
                              );

        void on_split_event_processed(Vertex seed
                               , Vertex node0
                               , Vertex node1
                               );

        void on_pseudo_split_event_processed(Vertex lseed
                                            , Vertex rseed
                                            , Vertex node0
                                            , Vertex node1
                                            );

        void on_vertex_processed(Vertex vertex);
        void on_propagation_finished();

        void on_cleanup_started();

        void on_cleanup_finished();

        void on_algorithm_finished(bool finished_ok);

        void on_error(Exception e );
    };
        }