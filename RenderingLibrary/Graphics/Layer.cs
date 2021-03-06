﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace RenderingLibrary.Graphics
{
    public class Layer
    {
        #region Fields

        List<IRenderable> mRenderables = new List<IRenderable>();

        ReadOnlyCollection<IRenderable> mRenderablesReadOnly;

        #endregion

        #region Properties

        public IPositionedSizedObject ScissorIpso { get; set; }

        public LayerCameraSettings LayerCameraSettings
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public ReadOnlyCollection<IRenderable> Renderables
        {
            get
            {
                return mRenderablesReadOnly;
            }
        }

        internal List<IRenderable> RenderablesWriteable
        {
            get
            {
                return mRenderables;
            }
        }

        public Layer ParentLayer
        {
            get;
            set;
        }

        #endregion

        public Layer()
        {
            mRenderablesReadOnly = new ReadOnlyCollection<IRenderable>(mRenderables);
        }

        public void Add(IRenderable renderable)
        {
            lock (mRenderables)
            {
                mRenderables.Add(renderable);
            }
        }

        public void Remove(IRenderable renderable)
        {
            mRenderables.Remove(renderable);
        }

        /// <summary>
        /// This is a stable sort on Z.  It's incredibly fast on already-sorted lists so we'll do this over something like the built-in 
        /// binary sorts that .NET offers.
        /// </summary>
        internal void SortRenderables()
        {
            if (mRenderables.Count == 1 || mRenderables.Count == 0)
                return;

            int whereObjectBelongs;

            for (int i = 1; i < mRenderables.Count; i++)
            {
                if ((mRenderables[i]).Z < (mRenderables[i - 1]).Z)
                {
                    if (i == 1)
                    {
                        mRenderables.Insert(0, mRenderables[i]);
                        mRenderables.RemoveAt(i + 1);
                        continue;
                    }

                    for (whereObjectBelongs = i - 2; whereObjectBelongs > -1; whereObjectBelongs--)
                    {
                        if ((mRenderables[i]).Z >= (mRenderables[whereObjectBelongs]).Z)
                        {
                            mRenderables.Insert(whereObjectBelongs + 1, mRenderables[i]);
                            mRenderables.RemoveAt(i + 1);
                            break;
                        }
                        else if (whereObjectBelongs == 0 && (mRenderables[i]).Z < (mRenderables[0]).Z)
                        {
                            mRenderables.Insert(0, mRenderables[i]);
                            mRenderables.RemoveAt(i + 1);
                            break;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return Name + " : " + mRenderables.Count + " IRenderables";
        }

        public bool ContainsRenderable(IRenderable whatToTest)
        {
            if (this.Renderables.Contains(whatToTest))
            {
                return true;
            }

            foreach (IRenderable renderable in this.Renderables)
            {
                if (renderable is SortableLayer)
                {
                    if (((SortableLayer)renderable).ContainsRenderable(whatToTest))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal Microsoft.Xna.Framework.Rectangle GetScissorRectangleFor(Camera camera)
        {
            var ipso = ScissorIpso;

            if (ipso == null)
            {
                return new Microsoft.Xna.Framework.Rectangle(
                    0,0,
                    camera.ClientWidth,
                    camera.ClientHeight

                    );
            }
            else
            {

                float worldX = ipso.GetAbsoluteLeft();
                float worldY = ipso.GetAbsoluteTop();

                float screenX;
                float screenY;
                camera.WorldToScreen(worldX, worldY, out screenX, out screenY);

                int left = global::RenderingLibrary.Math.MathFunctions.RoundToInt(screenX);
                int top = global::RenderingLibrary.Math.MathFunctions.RoundToInt(screenY);

                worldX = ipso.GetAbsoluteRight();
                worldY = ipso.GetAbsoluteBottom();
                camera.WorldToScreen(worldX, worldY, out screenX, out screenY);

                int right = global::RenderingLibrary.Math.MathFunctions.RoundToInt(screenX);
                int bottom = global::RenderingLibrary.Math.MathFunctions.RoundToInt(screenY);



                left = System.Math.Max(0, left);
                top = System.Math.Max(0, top);
                right = System.Math.Max(0, right);
                bottom = System.Math.Max(0, bottom);

                left = System.Math.Min(left, camera.ClientWidth);
                right = System.Math.Min(right, camera.ClientWidth);

                top = System.Math.Min(top, camera.ClientHeight);
                bottom = System.Math.Min(bottom, camera.ClientHeight);



                if (ParentLayer != null)
                {
                    var parentRectangle = ParentLayer.GetScissorRectangleFor(camera);
                    if(top > parentRectangle.Bottom)
                    {
                        int m = 3;
                    }
                    left = System.Math.Max(left, parentRectangle.Left);
                    right = System.Math.Min(right, parentRectangle.Right);
                    top = System.Math.Max(top, parentRectangle.Top);
                    bottom = System.Math.Min(bottom, parentRectangle.Bottom);
                }

                int width = System.Math.Max(0, right - left);
                int height = System.Math.Max(0, bottom - top);


                Microsoft.Xna.Framework.Rectangle thisRectangle = new Microsoft.Xna.Framework.Rectangle(
                    left,
                    top,
                    width,
                    height);

                return thisRectangle;
            }

        }
    }
}
