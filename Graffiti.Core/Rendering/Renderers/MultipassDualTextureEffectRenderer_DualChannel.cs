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

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Graffiti.Core.Brushes;
using Graffiti.Core.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Graffiti.Core.Rendering.Renderers
{
    internal sealed class MultipassDualTextureEffectRenderer_DualChannel : IRenderer
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct VertexPositionColorTexture1Texture2: IVertexType
        {
            private static readonly VertexDeclaration _vertexDeclaration =
                new VertexDeclaration(new[] 
                { 
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                    new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                    new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
                }) { Name = "VertexPositionColorTexture1Texture2.VertexDeclaration" };
            
            #region IVertexType Members

            public VertexDeclaration VertexDeclaration
            {
                get 
                {
                    return _vertexDeclaration;
                }
            }

            #endregion

            public Vector3 Position;
            public Color Color;
            public Vector2 Texcoord1;
            public Vector2 Texcoord2;

            public VertexPositionColorTexture1Texture2(Vector3 position, Color color, Vector2 texcoord1, Vector2 texcoord2)
            {
                Position = position;
                Color = color;
                Texcoord1 = texcoord1;
                Texcoord2 = texcoord2;
            }
        }
        
        private readonly GraphicsDevice _device;
        private readonly BasicEffect _basicEffect;
        private readonly DualTextureEffect _dualTextureEffect;
        private readonly Dictionary<IBrush, IRenderBucket> _renderBuckets =
            new Dictionary<IBrush, IRenderBucket>();
        private readonly ArrayList<VertexPositionColorTexture1Texture2> _bucketVertices_dual = new ArrayList<VertexPositionColorTexture1Texture2>();
        private readonly ArrayList<VertexPositionColorTexture> _bucketVertices_single = new ArrayList<VertexPositionColorTexture>();

        public MultipassDualTextureEffectRenderer_DualChannel(GraphicsDevice device)
        {
            _device = device;
            _basicEffect = new BasicEffect(_device)
            {
                LightingEnabled = false,
                VertexColorEnabled = true,
                TextureEnabled = true
            };
            _dualTextureEffect = new DualTextureEffect(_device)
            {
                VertexColorEnabled = true,
            };
        }

        public IRenderBucket this[IBrush brush]
        {
            get 
            {
                IRenderBucket bucket;
                var exists = _renderBuckets.TryGetValue(brush, out bucket);
                if (!exists)
                {
                    bucket = new PreTransformedRenderBucket();
                    _renderBuckets.Add(brush, bucket);
                }

                return bucket;
            }
        }

        public void Flush()
        {
            foreach (var kvp in _renderBuckets)
            {
                var bucket = kvp.Value;
                var brush = kvp.Key;

                foreach (var layer in brush)
                {
                    var dualLayer = layer as IDualLayer;
                    if (dualLayer != null)
                    {
                        _bucketVertices_dual.Clear();

                        var layer1 = dualLayer.Layer1;
                        var layer2 = dualLayer.Layer2;
                        
                        _dualTextureEffect.Projection = Projection;
                        _dualTextureEffect.World = World;
                        _dualTextureEffect.View = View;
                        
                        _dualTextureEffect.Texture = layer1.Texture;
                        _dualTextureEffect.Texture2 = layer2.Texture;

                        _device.SamplerStates[0] = new SamplerState
                        {
                            AddressU = layer1.AddressU,
                            AddressV = layer1.AddressV
                        };
                        _device.SamplerStates[1] = new SamplerState
                        {
                            AddressU = layer2.AddressU,
                            AddressV = layer2.AddressV
                        };

                        var transform1 = dualLayer.Layer1.CurrentLayerTransform;
                        var transform2 = dualLayer.Layer2.CurrentLayerTransform;
                        for (int i = 0; i < kvp.Value.Vertices.Count; i++)
                        {
                            var incomingVertex = bucket.Vertices[i];
                            var vertex = new VertexPositionColorTexture1Texture2(
                                incomingVertex.Position,
                                new Color(
                                    layer1.Color.R * incomingVertex.Color.R,
                                    layer1.Color.G * incomingVertex.Color.G,
                                    layer1.Color.B * incomingVertex.Color.B,
                                    layer1.Color.A * incomingVertex.Color.A),
                                    Vector2.Transform(incomingVertex.Texcoords[layer1.TexCoordChannel], transform1),
                                    Vector2.Transform(incomingVertex.Texcoords[layer2.TexCoordChannel], transform2));
                            _bucketVertices_dual.Add(vertex);
                        }

                        foreach (var pass in _dualTextureEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            _device.BlendState = layer1.BlendState;

                            _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                                              _bucketVertices_dual.GetArray(), 0,
                                                              kvp.Value.Vertices.Count,
                                                              (kvp.Value.Indices as ArrayList<short>).GetArray(), 0,
                                                              kvp.Value.Indices.Count / 3);
                        }
                    }
                    else
                    {
                        _bucketVertices_single.Clear();

                        var transform = layer.CurrentLayerTransform;

                        _basicEffect.Texture = layer.Texture;

                        _device.SamplerStates[0] = new SamplerState
                        {
                            AddressU = layer.AddressU,
                            AddressV = layer.AddressV
                        };
                        for (int i = 0; i < kvp.Value.Vertices.Count; i++)
                        {
                            var incomingVertex = bucket.Vertices[i];
                            var vertex = new VertexPositionColorTexture(incomingVertex.Position,
                                new Color(layer.Color.R * incomingVertex.Color.R,
                                    layer.Color.G * incomingVertex.Color.G,
                                    layer.Color.B * incomingVertex.Color.B,
                                    layer.Color.A * incomingVertex.Color.A),
                                    Vector2.Transform(incomingVertex.Texcoords[layer.TexCoordChannel], transform));
                            _bucketVertices_single.Add(vertex);
                        }

                        foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            _device.BlendState = layer.BlendState;

                            _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                                              _bucketVertices_single.GetArray(), 0,
                                                              kvp.Value.Vertices.Count,
                                                              (kvp.Value.Indices as ArrayList<short>).GetArray(), 0,
                                                              kvp.Value.Indices.Count / 3);
                        }
                    }
                }
                
                bucket.Clear();
            }

            _renderBuckets.Clear();
        }

        public Matrix Projection { get; set; }
        public Matrix View { get; set; }
        public Matrix World { get; set; }
    }
}