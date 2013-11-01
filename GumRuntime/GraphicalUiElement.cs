﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RenderingLibrary;
using RenderingLibrary.Graphics;
using Gum.DataTypes;


namespace Gum.Managers
{
    public enum PositionUnitType
    {
        PixelsFromLeft,
        PixelsFromTop,
        PercentageWidth,
        PercentageHeight,
        PixelsFromRight,
        PixelsFromBottom,
        PixelsFromCenterX,
        PixelsFromCenterY

    }
}

namespace Gum.Wireframe
{
    #region Enums

    public enum GeneralUnitType
    {
        PixelsFromSmall,
        PixelsFromLarge,
        PixelsFromMiddle,
        Percentage,
        PercentageOfFile
    }





    #endregion


    public class GraphicalUiElement : IRenderable, IPositionedSizedObject, IVisible
    {
        #region Fields

        IRenderable mContainedObjectAsRenderable;
        // to save on casting:
        IPositionedSizedObject mContainedObjectAsIpso;
        IVisible mContainedObjectAsIVisible;

        GraphicalUiElement mWhatContainsThis;

        List<GraphicalUiElement> mWhatThisContains = new List<GraphicalUiElement>();

        GeneralUnitType mXUnits;
        GeneralUnitType mYUnits;
        HorizontalAlignment mXOrigin;
        VerticalAlignment mYOrigin;
        DimensionUnitType mWidthUnit;
        DimensionUnitType mHeightUnit;

        float mX;
        float mY;
        float mWidth;
        float mHeight;

        static float mCanvasWidth = 800;
        static float mCanvasHeight = 600;

        IPositionedSizedObject mParent;


        bool mIsLayoutSuspended = false;

        #endregion

        #region Properties


        public bool Visible
        {
            get
            {
                return mContainedObjectAsIVisible.Visible;
            }
            set
            {
                mContainedObjectAsIVisible.Visible = value;
            }
        }

        public static float CanvasWidth
        {
            get { return mCanvasWidth; }
            set { mCanvasWidth = value; }
        }

        public static float CanvasHeight
        {
            get { return mCanvasHeight; }
            set { mCanvasHeight = value; }
        }


        #region IPSO properties

        float IPositionedSizedObject.X
        {
            get
            {
                return mContainedObjectAsIpso.X;
            }
            set
            {
                mContainedObjectAsIpso.X = value;
            }
        }

        float IPositionedSizedObject.Y
        {
            get
            {
                return mContainedObjectAsIpso.Y;

            }
            set
            {
                mContainedObjectAsIpso.Y = value;
            }
        }

        float IPositionedSizedObject.Z
        {
            get
            {
                return mContainedObjectAsIpso.Z;
            }
            set
            {
                mContainedObjectAsIpso.Z = value;
            }
        }

        float IPositionedSizedObject.Width
        {
            get
            {
                return mContainedObjectAsIpso.Width;
            }
            set
            {
                mContainedObjectAsIpso.Width = value;
            }
        }

        float IPositionedSizedObject.Height
        {
            get
            {
                return mContainedObjectAsIpso.Height;
            }
            set
            {
                mContainedObjectAsIpso.Height = value;
            }
        }

        void IPositionedSizedObject.SetParentDirect(IPositionedSizedObject parent)
        {
            mContainedObjectAsIpso.SetParentDirect(parent);
        }

        #endregion

        #region IRenderable properties


        Microsoft.Xna.Framework.Graphics.BlendState IRenderable.BlendState
        {
            get { return mContainedObjectAsRenderable.BlendState; }
        }

        bool IRenderable.Wrap
        {
            get { return mContainedObjectAsRenderable.Wrap; }
        }

        void IRenderable.Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, SystemManagers managers)
        {
            mContainedObjectAsRenderable.Render(spriteBatch, managers);
        }

        float IRenderable.Z
        {
            get
            {
                return mContainedObjectAsRenderable.Z;
            }
            set
            {
                mContainedObjectAsRenderable.Z = value;
            }
        }


        #endregion

        public GeneralUnitType XUnits
        {
            get { return mXUnits; }
            set { mXUnits = value; UpdateLayout(); }
        }

        public GeneralUnitType YUnits
        {
            get { return mYUnits; }
            set { mYUnits = value; UpdateLayout(); }
        }

        public HorizontalAlignment XOrigin
        {
            get { return mXOrigin; }
            set { mXOrigin = value; UpdateLayout(); }
        }

        public VerticalAlignment YOrigin
        {
            get { return mYOrigin; }
            set { mYOrigin = value; UpdateLayout(); }
        }

        public DimensionUnitType WidthUnit
        {
            get { return WidthUnit; }
            set { mWidthUnit = value; UpdateLayout(); }
        }

        public DimensionUnitType HeightUnit
        {
            get { return mHeightUnit; }
            set { mHeightUnit = value; UpdateLayout(); }
        }

        public float X
        {
            get
            {
                return mX;
            }
            set
            {
                mX = value;
                UpdateLayout();
            }
        }

        public float Y
        {
            get
            {
                return mY;
            }
            set
            {
                mY = value;
                UpdateLayout();
            }
        }

        public float Width
        {
            get { return mWidth; }
            set { mWidth = value; UpdateLayout(); }
        }

        public float Height
        {
            get { return mHeight; }
            set { mHeight = value; UpdateLayout(); }
        }

        public IPositionedSizedObject Parent
        {
            get { return mParent; }
            set
            {
                if (mParent != value)
                {
                    if (mParent != null)
                    {
                        mParent.Children.Remove(this);
                    }
                    mParent = value;
                    if (mParent != null)
                    {
                        mParent.Children.Add(this);
                    }
                    UpdateLayout();

                }
            }
        }

        public IRenderable RenderableComponent
        {
            get
            {
                if (mContainedObjectAsRenderable is GraphicalUiElement)
                {
                    return ((GraphicalUiElement)mContainedObjectAsRenderable).RenderableComponent;
                }
                else
                {
                    return mContainedObjectAsRenderable;
                }

            }
        }

        public IEnumerable<GraphicalUiElement> ContainedElements
        {
            get
            {
                return mWhatThisContains;
            }
        }
        
        public string Name
        {
            get
            {
                return mContainedObjectAsIpso.Name;
            }
            set
            {
                mContainedObjectAsIpso.Name = value;
            }
        }

        public ICollection<IPositionedSizedObject> Children
        {
            get { return mContainedObjectAsIpso.Children; }
        }

        public object Tag
        {
            get
            {
                return mContainedObjectAsIpso.Tag;
            }
            set
            {
                mContainedObjectAsIpso.Tag = value;
            }
        }

        public IPositionedSizedObject Component { get { return mContainedObjectAsIpso; } }


        #endregion

        #region Constructor

        public GraphicalUiElement(IRenderable containedObject, GraphicalUiElement whatContainsThis)
        {
            SetContainedObject(containedObject);

            mWhatContainsThis = whatContainsThis;
            if (mWhatContainsThis != null)
            {
                mWhatContainsThis.mWhatThisContains.Add(this);

                this.Parent = whatContainsThis;
            }
        }

        public void SetContainedObject(IRenderable containedObject)
        {
            if (containedObject == this)
            {
                throw new ArgumentException("The argument containedObject cannot be 'this'");
            }

            mContainedObjectAsRenderable = containedObject;
            mContainedObjectAsIpso = mContainedObjectAsRenderable as IPositionedSizedObject;
            mContainedObjectAsIVisible = mContainedObjectAsRenderable as IVisible;

            UpdateLayout();
        }

        #endregion

        public void UpdateLayout()
        {
            if (!mIsLayoutSuspended && mContainedObjectAsIpso != null)
            {
                float xDock = 0;
                float yDock = 0;
                float parentWidth = CanvasWidth;
                float parentHeight = CanvasHeight;
                float unitOffsetX = this.X;
                float unitOffsetY = this.Y;

                float widthToSet = mWidth;
                float heightToSet = mHeight;

                if (this.Parent != null)
                {
                    parentWidth = Parent.Width;
                    parentHeight = Parent.Height;
                }


                if (mWidthUnit == DimensionUnitType.Percentage)
                {
                    widthToSet = parentWidth * mWidth / 100.0f;
                }
                else if (mWidthUnit == DimensionUnitType.PercentageOfSourceFile)
                {
                    throw new NotImplementedException();
                }
                else if (mWidthUnit == DimensionUnitType.RelativeToContainer)
                {
                    widthToSet = parentWidth + mWidth;
                }

                if (mHeightUnit == DimensionUnitType.Percentage)
                {
                    heightToSet = parentHeight * mHeight / 100.0f;
                }
                else if (mHeightUnit == DimensionUnitType.PercentageOfSourceFile)
                {
                    throw new NotImplementedException();
                }
                else if (mHeightUnit == DimensionUnitType.RelativeToContainer)
                {
                    heightToSet = parentHeight + mHeight;
                }


                mContainedObjectAsIpso.Width = widthToSet;
                mContainedObjectAsIpso.Height = heightToSet;

                if (mContainedObjectAsIpso is Text)
                {
                    ((Text)mContainedObjectAsIpso).UpdateTextureToRender();
                }


                if (mXUnits == GeneralUnitType.Percentage)
                {
                    unitOffsetX = parentWidth * this.X / 100.0f;
                }
                else if (mXUnits == GeneralUnitType.PercentageOfFile)
                {
                    throw new NotImplementedException();
                }
                else if (mXUnits == GeneralUnitType.PixelsFromLarge)
                {
                    unitOffsetX = mX + parentWidth;
                }
                else if (mXUnits == GeneralUnitType.PixelsFromMiddle)
                {
                    unitOffsetX = mX + parentWidth / 2.0f;
                }
                //else if (mXUnits == GeneralUnitType.PixelsFromSmall)
                //{
                //    // no need to do anything
                //}

                if (mYUnits == GeneralUnitType.Percentage)
                {
                    unitOffsetY = parentHeight * this.Y / 100.0f;
                }
                else if (mYUnits == GeneralUnitType.PercentageOfFile)
                {
                    throw new NotImplementedException();
                }
                else if (mYUnits == GeneralUnitType.PixelsFromLarge)
                {
                    unitOffsetY = mY + parentHeight;
                }
                else if (mYUnits == GeneralUnitType.PixelsFromMiddle)
                {
                    unitOffsetY = mY + parentHeight / 2.0f;
                }




                if (mXOrigin == HorizontalAlignment.Center)
                {
                    unitOffsetX -= mContainedObjectAsIpso.Width / 2.0f;
                }
                else if (mXOrigin == HorizontalAlignment.Right)
                {
                    unitOffsetX -= mContainedObjectAsIpso.Width;
                }
                // no need to handle left


                if (mYOrigin == VerticalAlignment.Center)
                {
                    unitOffsetY -= mContainedObjectAsIpso.Height / 2.0f;
                }
                else if (mYOrigin == VerticalAlignment.Bottom)
                {
                    unitOffsetY -= mContainedObjectAsIpso.Height;
                }
                // no need to handle top


                this.mContainedObjectAsIpso.X = xDock + unitOffsetX;
                this.mContainedObjectAsIpso.Y = yDock + unitOffsetY;

                mContainedObjectAsIpso.Parent = mParent;

                foreach (var child in this.Children)
                {
                    if (child is GraphicalUiElement)
                    {
                        (child as GraphicalUiElement).UpdateLayout();
                    }
                }
            }

        }

        public override string ToString()
        {
            return Name;
        }


        public void AddToManagers(SystemManagers mManagers, Layer layer)
        {
            if (mContainedObjectAsRenderable is Sprite)
            {
                mManagers.SpriteManager.Add(mContainedObjectAsRenderable as Sprite, layer);
            }
            else if (mContainedObjectAsRenderable is NineSlice)
            {
                mManagers.SpriteManager.Add(mContainedObjectAsRenderable as NineSlice, layer);
            }
            else if (mContainedObjectAsRenderable is global::RenderingLibrary.Math.Geometry.LineRectangle)
            {
                mManagers.ShapeManager.Add(mContainedObjectAsRenderable as global::RenderingLibrary.Math.Geometry.LineRectangle, layer);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SuspendLayout()
        {
            mIsLayoutSuspended = true;
        }

        public void ResumeLayout()
        {
            mIsLayoutSuspended = false;
            UpdateLayout();
        }

        #region IVisible Implementation


        bool IVisible.AbsoluteVisible
        {
            get { return mContainedObjectAsIVisible.AbsoluteVisible; }
        }

        IVisible IVisible.Parent
        {
            get { return this.Parent as IVisible; }
        }

        #endregion

    }
}