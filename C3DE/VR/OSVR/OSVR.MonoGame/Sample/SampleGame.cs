using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OSVR.ClientKit;
using OSVR.MonoGame;
using System;

namespace Sample
{
    enum OrientationMode { Head, Mouselook, RightHand };
    public class SampleGame : Game, IStereoSceneDrawer
    {
        // XNA resources
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont diagnosticFont;
        Texture2D blank;
        Model model;
        Axes axes;
        KeyboardState lastKeyboardState;

        // OSVR resources
        ClientContext context;
        VRHead vrHead;
        OSVR.ClientKit.IInterface<XnaPose> leftHandPose;
        OSVR.ClientKit.IInterface<XnaPose> rightHandPose;
        OSVR.ClientKit.IInterface<XnaPose> headPose;
        OSVR.ClientKit.IInterface<Vector2> leftEye2D;
        OrientationMode orientationMode = OrientationMode.Head;
        MouselookInterface mouselook;

        // Game-related properties
        const float moveSpeed = 5f;
        Vector3 leftHandOffset = new Vector3(0, 0, -1);
        Vector3 rightHandOffset = new Vector3(0, 0, -1);
        Vector3 position = new Vector3(0, 5f, 0f);
        float rotationY = 0f;
        bool firstUpdate = true;

        public SampleGame()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            context = new ClientContext("osvr.monogame.Sample");

            leftHandPose = new XnaPoseInterface(context.GetPoseInterface("/me/hands/left"));
            rightHandPose = new XnaPoseInterface(context.GetPoseInterface("/me/hands/right"));
            leftEye2D = new XnaPosition2DInterface(context.GetEyeTracker2DInterface("/me/eyes/left"));

            // You should always be using "/me/head" for HMD orientation tracking,
            // but we're mocking HMD head tracking with either hand tracking (e.g. Hydra controllers)
            // or with mouselook. You can cycle through them with the O key.
            headPose = new XnaPoseInterface(context.GetPoseInterface("/me/head"));

            // Mouselook emulation of the head-tracking orientation interface
            mouselook = new MouselookInterface(GraphicsDevice.Viewport);

            vrHead = new VRHead(graphics, context, headPose);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            axes = new Axes();
            axes.LoadContent(GraphicsDevice);
            model = Content.Load<Model>("SketchupTest");
            diagnosticFont = Content.Load<SpriteFont>("DiagnosticFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            blank = new Texture2D(GraphicsDevice, 1, 1);
            blank.SetData(new[] { Color.White });
            
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            this.leftHandPose.Dispose();
            this.rightHandPose.Dispose();
            this.headPose.Dispose();
            context.Dispose();
            leftHandPose = null;
            rightHandPose = null;
            headPose = null;
            context = null;
        }

        private void CycleOrientationMode()
        {
            switch (orientationMode)
            {
                case OrientationMode.Head:
                    orientationMode = OrientationMode.Mouselook;
                    vrHead.Pose = mouselook;
                    break;
                case OrientationMode.Mouselook:
                    orientationMode = OrientationMode.RightHand;
                    vrHead.Pose = rightHandPose;
                    break;
                case OrientationMode.RightHand:
                    orientationMode = OrientationMode.Head;
                    vrHead.Pose = headPose;
                    break;
                default:
                    throw new InvalidOperationException("Unknown orientation mode.");
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Exit();
            }
            else
            {
                var t = (float)gameTime.ElapsedGameTime.TotalSeconds;
                var kbs = Keyboard.GetState();

                if(firstUpdate)
                {
                    firstUpdate = false;
                }
                else
                {
                    if(lastKeyboardState.IsKeyUp(Keys.O) && kbs.IsKeyDown(Keys.O))
                    {
                        CycleOrientationMode();
                    }

                    if(lastKeyboardState.IsKeyUp(Keys.C) && kbs.IsKeyDown(Keys.C))
                    {
                        leftHandOffset = Vector3.Negate(leftHandPose.GetState().Value.Position);
                        rightHandOffset = Vector3.Negate(rightHandPose.GetState().Value.Position);
                    }

                    if(lastKeyboardState.IsKeyUp(Keys.F) && kbs.IsKeyDown(Keys.F))
                    {
                        graphics.IsFullScreen = !graphics.IsFullScreen;
                        graphics.ApplyChanges();
                    }
                }
                lastKeyboardState = kbs;

                Vector3 movement = Vector3.Zero;
                // forward/back
                if (kbs.IsKeyDown(Keys.W))
                {
                    movement = Vector3.Forward * moveSpeed * t;
                }
                else if (kbs.IsKeyDown(Keys.S))
                {
                    movement = Vector3.Backward * moveSpeed * t;
                }

                // left/right
                if (kbs.IsKeyDown(Keys.A))
                {
                    movement = Vector3.Left * moveSpeed * t;
                }
                else if (kbs.IsKeyDown(Keys.D))
                {
                    movement = Vector3.Right * moveSpeed * t;
                }

                // kb left/right rotation
                if(kbs.IsKeyDown(Keys.Left))
                {
                    rotationY += moveSpeed * t;
                }
                else if(kbs.IsKeyDown(Keys.Right))
                {
                    rotationY -= moveSpeed * t;
                }

                var kbRotation = Microsoft.Xna.Framework.Quaternion.CreateFromYawPitchRoll(rotationY, 0f, 0f);
                var transformedMovement = Vector3.Transform(movement, kbRotation * vrHead.Pose.GetState().Value.Rotation);
                position = position + transformedMovement;

                // increase/decrease stereo amount
                if (kbs.IsKeyDown(Keys.Q)) { vrHead.IPDInMeters += .01f * t; }
                if (kbs.IsKeyDown(Keys.E)) { vrHead.IPDInMeters -= .01f * t; }

                vrHead.Update();
                if (orientationMode == OrientationMode.Mouselook)
                {
                    mouselook.Update(gameTime);
                }

                // context.Update must be called frequently
                // perhaps more frequently than Update is called?
                context.update();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            vrHead.DrawScene(gameTime, spriteBatch, this);
            base.Draw(gameTime);
        }

        void DrawPose(Color color, OSVR.ClientKit.IInterface<XnaPose> pose, Vector3 calibrationOffset, Matrix view, Matrix projection)
        {
            var poseState = pose.GetState();
            var yRotation = Microsoft.Xna.Framework.Quaternion.CreateFromYawPitchRoll(rotationY, 0, 0);
            var rotation = yRotation * poseState.Value.Rotation;
            var leftHandWorld =
                Matrix.CreateFromQuaternion(rotation)
                * Matrix.CreateTranslation(position + Vector3.Transform(calibrationOffset + poseState.Value.Position, yRotation));

            axes.Draw(
                size: .1f,
                color: color,
                view: view,
                world: leftHandWorld,
                projection: projection,
                graphicsDevice: GraphicsDevice);
        }

        public void DrawScene(GameTime gameTime, Microsoft.Xna.Framework.Graphics.Viewport viewport, Matrix stereoTransform, Matrix view, Matrix projection)
        {
            // TODO: Draw something fancy. Or at the very least visible?
            var translation = Matrix.CreateTranslation(Vector3.Negate(position));
            var kbRotation = Matrix.CreateRotationY(-rotationY);
            var cameraView = stereoTransform * translation * kbRotation * view;

            // Draw the model. A model can have multiple meshes, so loop.
            var modelWorld = Matrix.CreateTranslation(0f, -0f, -0f);
            foreach (ModelMesh mesh in model.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = modelWorld;
                    effect.View = cameraView;
                    effect.Projection = projection;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }

            DrawPose(Color.Red, leftHandPose, leftHandOffset, cameraView, projection);
            if (orientationMode != OrientationMode.RightHand)
            {
                DrawPose(Color.Blue, rightHandPose, rightHandOffset, cameraView, projection);
            }

            var kbstate = Keyboard.GetState();
            if (kbstate.IsKeyDown(Keys.Q) || kbstate.IsKeyDown(Keys.E))
            {
                // display IPD
                spriteBatch.Begin();
                spriteBatch.DrawString(diagnosticFont, "IPD: " + (vrHead.IPDInMeters * 1000).ToString() + "mm",
                    new Vector2((float)viewport.Width / 2f, (float)viewport.Height / 2f), Color.White);

                // display eye tracking 2D location
                var halfXSize = diagnosticFont.MeasureString("X") * .5f;
                var eyeState = leftEye2D.GetState();
                var scaledEyePosition = new Vector2(
                    (eyeState.Value.X * (float)viewport.Width) - halfXSize.X,
                    (eyeState.Value.Y * (float)viewport.Height) - halfXSize.Y);

                spriteBatch.DrawString(diagnosticFont, "X",
                    scaledEyePosition, Color.Red);

                spriteBatch.End();
            }
        }
    }
}
