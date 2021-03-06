﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XnaAndWinforms;
using Microsoft.Xna.Framework;
using System.Windows.Forms;
using RenderingLibrary.Math.Geometry;
using RenderingLibrary.Graphics;
using RenderingLibrary;
using Gum.DataTypes;
using Gum.ToolStates;
using Gum.Input;
using RenderingLibrary.Content;
using Gum.Managers;
using Gum.Plugins.BaseClasses;
using Gum.ToolCommands;
using System.ComponentModel.Composition;
using FlatRedBall.AnimationEditorForms.Controls;
using Gum.Debug;
using Microsoft.Xna.Framework.Graphics;

using WinCursor = System.Windows.Forms.Cursor;

namespace Gum.Wireframe
{
    #region WireframeControlPlugin Class

    [Export(typeof(Gum.Plugins.BaseClasses.PluginBase))]
    public class WireframeControlPlugin : InternalPlugin
    {

        public override void StartUp()
        {
            this.ProjectLoad += new Action<GumProjectSave>(OnProjectLoad);
        }

        void OnProjectLoad(GumProjectSave obj)
        {
            GuiCommands.Self.RefreshWireframeDisplay();
        }
    }

    #endregion

    public class WireframeControl : GraphicsDeviceControl
    {
        #region Fields

        WireframeEditControl mWireframeEditControl;

        LineRectangle mScreenBounds;

        public Color BackgroundColor = Color.DimGray;
        public Color ScreenBoundsColor = Color.LightBlue;

        bool mHasInitialized = false;

        Ruler mTopRuler;
        Ruler mLeftRuler;

        #endregion

        #region Properties

        public LineRectangle ScreenBounds
        {
            get { return mScreenBounds; }
        }

        new InputLibrary.Cursor Cursor
        {
            get
            {
                return InputLibrary.Cursor.Self;
            }
        }

        Camera Camera
        {
            get { return Renderer.Self.Camera; }
        }

        #endregion

        public event EventHandler AfterXnaInitialize;

        #region Event Methods


        void OnKeyDown(object sender, KeyEventArgs e)
        {
            ElementTreeViewManager.Self.HandleCopyCutPaste(e);

            ElementTreeViewManager.Self.HandleDelete(e);

            // I think this is handled in ProcessCmdKey
            //HandleNudge(e);
        }


        void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            int m = 3;

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool handled = false;

            handled = HandleNudge(keyData);

            if (handled)
            {
                return true;
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }
        //protected override bool ProcessKeyEventArgs(ref Message m, Keys keyData)
        //{
        //    if (keyData == Keys.Left)
        //    {
        //        Console.WriteLine("left");
        //        return true;
        //    }
        //    // etc..
        //    return base.ProcessCmdKey(ref msg, keyData);
        //}

        private bool HandleNudge(Keys keyData)
        {

            bool handled = false;

            int nudgeX = 0;
            int nudgeY = 0;

            Keys extracted = keyData;
            if (keyData >= Keys.KeyCode)
            {
                int value = (int)(keyData) + (int)Keys.Modifiers;

                extracted = (Keys)value;
            }


            if (extracted == Keys.Up)
            {
                nudgeY = -1;
            }
            if (extracted == Keys.Down)
            {
                nudgeY = 1;
            }
            if (extracted == Keys.Right)
            {
                nudgeX = 1;
            }
            if (extracted == Keys.Left)
            {
                nudgeX = -1;
            }

            bool shiftDown = (keyData & Keys.Shift) == Keys.Shift;
            if (shiftDown)
            {
                nudgeX *= 5;
                nudgeY *= 5;
            }

            if (nudgeX != 0 || nudgeY != 0)
            {
                EditingManager.Self.MoveSelectedObjectsBy(nudgeX, nudgeY);
                handled = true;


                GumCommands.Self.FileCommands.TryAutoSaveCurrentElement();
            
            }
            return handled;
        }     

        #endregion

        #region Initialize Methods

        public void Initialize(WireframeEditControl wireframeEditControl)
        {
            try
            {
                XnaUpdate += HandleXnaUpdate;
                mWireframeEditControl = wireframeEditControl;

                mWireframeEditControl.ZoomChanged += HandleZoomChanged;

                Renderer.Self.Initialize(GraphicsDevice, null);

                Renderer.Self.SamplerState = SamplerState.PointWrap;

                LoaderManager.Self.ContentLoader = new ContentLoader();

                LoaderManager.Self.Initialize(null, "content/TestFont.fnt", Services, null);
                CameraController.Self.Initialize(Cursor, Camera, mWireframeEditControl, this.Width, this.Height);

                InputLibrary.Cursor.Self.Initialize(this);
                InputLibrary.Keyboard.Self.Initialize(this);

                mScreenBounds = new LineRectangle();
                mScreenBounds.Width = 800;
                mScreenBounds.Height = 600;
                mScreenBounds.Color = ScreenBoundsColor;
                ShapeManager.Self.Add(mScreenBounds, SelectionManager.Self.UiLayer);              

                this.KeyDown += new KeyEventHandler(OnKeyDown);
                this.KeyPress += new KeyPressEventHandler(OnKeyPress);
                this.MouseWheel += new MouseEventHandler(CameraController.Self.HandleMouseWheel);
                this.mTopRuler = new Ruler(this, null, InputLibrary.Cursor.Self);
                mLeftRuler = new Ruler(this, null, InputLibrary.Cursor.Self);
                mLeftRuler.RulerSide = RulerSide.Left;

                if (AfterXnaInitialize != null)
                {
                    AfterXnaInitialize(this, null);
                }

                UpdateWireframeToProject();

                mHasInitialized = true;

                SystemManagers.Default = SystemManagers.CreateFromSingletons();
            }
            catch(Exception exception)
            {
                MessageBox.Show("Error initializing the wireframe control\n\n" + exception);
                int m = 3;

            }
        }

        void HandleZoomChanged(object sender, EventArgs e)
        {

            this.mLeftRuler.ZoomValue = mWireframeEditControl.PercentageValue / 100.0f;
            this.mTopRuler.ZoomValue = mWireframeEditControl.PercentageValue / 100.0f;
        }

        void HandleXnaUpdate()
        {

        }



        #endregion



        bool isInActivity = false;

        void Activity()
        {
            if (!isInActivity)
            {
                isInActivity = true;
#if DEBUG
                try
#endif
                {

                    InputLibrary.Cursor.Self.StartCursorSettingFrameStart();
                    ProjectVerifier.Self.AssertSelectedIpsosArePartOfRenderer();
                    TimeManager.Self.Activity();

                    SpriteManager.Self.Activity(TimeManager.Self.CurrentTime);

                    DragDropManager.Self.Activity();

                    InputLibrary.Cursor.Self.Activity(TimeManager.Self.CurrentTime);
                    InputLibrary.Keyboard.Self.Activity();
                    CameraController.Self.CameraMovementAndZoomActivity();
                    if (Cursor.PrimaryPush)
                    {
                        int m = 3;
                    }

                    bool isOver = this.mTopRuler.HandleXnaUpdate(InputLibrary.Cursor.Self.IsInWindow) ||
                        mLeftRuler.HandleXnaUpdate(InputLibrary.Cursor.Self.IsInWindow);



                    // But we want the selection to update the handles to the selected object
                    // after editing is done.  SelectionManager.LateActivity lets us do that.  LateActivity must
                    // come after EidtingManager.Activity.
                    if (mTopRuler.IsCursorOver == false && mLeftRuler.IsCursorOver == false)
                    {
                        SelectionManager.Self.Activity(this);
                        // EditingManager activity must happen after SelectionManager activity
                        EditingManager.Self.Activity();


                        SelectionManager.Self.LateActivity();
                    }
                    InputLibrary.Cursor.Self.EndCursorSettingFrameStart();

                }
#if DEBUG
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
#endif
            }
            isInActivity = false;
        }


        public void UpdateWireframeToProject()
        {
            if (mScreenBounds != null && ProjectManager.Self.GumProjectSave != null)
            {
                mScreenBounds.Width = ProjectManager.Self.GumProjectSave.DefaultCanvasWidth;
                mScreenBounds.Height = ProjectManager.Self.GumProjectSave.DefaultCanvasHeight;
            }
        }

        protected override void PreDrawUpdate()
        {
            if (mHasInitialized)
            {
                Activity();
            }
        }

        protected override void Draw()
        {
            if (mHasInitialized)
            {
                GraphicsDevice.Clear(BackgroundColor);

                Renderer.Self.Draw(null);
            }
        }
    }
}
