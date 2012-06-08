#region License and Copyright Notice
// Copyright (c) 2010 Ananth Balasubramaniam
// All rights reserved.
// 
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.
#endregion

using System;
using Graffiti.Core.Brushes;
using Graffiti.Core.Rendering;
using Microsoft.Xna.Framework;

namespace Graffiti.Core.Geometry
{
    public class IndexedMesh<TVertex, TTexcoords> : IIndexedMesh<TVertex, TTexcoords>
        where TVertex : struct, IVertex<TTexcoords>
        where TTexcoords : struct, ITexcoords
    {
        internal IndexedMesh()
        {
            Transform = Matrix.Identity;
        }

        internal IndexedMesh(int vertexCount, int indexCount)
            : this()
        {
            _vertices = new TVertex[vertexCount];
            _indices = new short[indexCount];
        }

        #region IIndexedMesh<TVertex,TTexcoords> Members

        private short[] _indices;
        public short[] Indices
        {
            get { return _indices; }
            internal set { _indices = value; }
        }

        private TVertex[] _vertices;
        public TVertex[] Vertices 
        {
            get { return _vertices; }
            internal set { _vertices = value; }
        }

        #endregion

        #region IRenderable Members

        public virtual void Render(IRenderer renderer, Matrix parentTransform)
        {
            var bucket = (renderer as IRenderer<TVertex, TTexcoords>)[Brush];
            bucket.Add(Transform * parentTransform, Vertices, Indices);
        }

        public IBrush Brush { get; set; }

        #endregion

        #region IPoseable Members

        public Matrix Transform { get; set; }

        #endregion

        internal void ResizeVerticesAndIndices(int newVertexCount, int newIndexCount)
        {
            if (_vertices != null)
                Array.Resize(ref _vertices, newVertexCount);
            _vertices = new TVertex[newVertexCount];
            
            if (_indices != null)
                Array.Resize(ref _vertices, newIndexCount);
            _vertices = new TVertex[newIndexCount];
        }
    }
}