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

namespace Graffiti.Math
{
    public abstract class Axes
    {
        protected internal abstract int Axis
        {
            get;
        }

        public sealed class X : Axes
        {
            protected internal override int Axis
            {
                get
                {
                    return 0;
                }
            }
        }

        public sealed class Y : Axes
        {
            protected internal override int Axis
            {
                get
                {
                    return 1;
                }
            }
        }

        public sealed class Z : Axes
        {
            protected internal override int Axis
            {
                get
                {
                    return 2;
                }
            }
        }
    }
}