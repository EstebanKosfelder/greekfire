// Copyright (c) 1997  
// Utrecht University (The Netherlands),
// ETH Zurich (Switzerland),
// INRIA Sophia-Antipolis (France),
// Max-Planck-Institute Saarbruecken (Germany),
// and Tel-Aviv University (Israel).  All rights reserved. 
//
// This file is part of CGAL (www.cgal.org); you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation; either version 3 of the License,
// or (at your option) any later version.
//
// Licensees holding a valid commercial license may use this file in
// accordance with the commercial license agreement provided with the software.
//
// This file is provided AS IS with NO WARRANTY OF ANY KIND, INCLUDING THE
// WARRANTY OF DESIGN, MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//
// $URL$
// $Id$
// SPDX-License-Identifier: LGPL-3.0+
// 
//

using System;

namespace CGAL {

    public class Face {

        public static Face NULL { get; private set; } 
        
        static Face()
        {
            NULL = new Face(-1);
        }

        public Face( int id,int outSide = -1)
        {
            Id = id;
            Outside = outSide;
            Halfedge = Halfedge.NULL;

        }
       public  Halfedge Halfedge { get; set; }
        public virtual IEnumerable<Halfedge>Halfedges  => Halfedge.Halfedges;
        public int Degree => Halfedge.Vertex.Degree; 
        public int Id { get; private set; }
        public int Outside { get; private set; }

        internal Halfedge halfedge() => Halfedge;
       internal void set_halfedge(Halfedge aHE) { Halfedge = aHE; }

        internal void reset_id(int aID) { Id = aID; }

        public override string ToString()
        {
            return $"F{Id} Os:{Outside} H:{Id}";
        }

    }
}