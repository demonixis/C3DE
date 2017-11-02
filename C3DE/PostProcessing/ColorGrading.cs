using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.PostProcessing
{
    /// <summary>
    /// 
    ///     /// Version 1.1, 26. April. 2017
    /// 
    ///     Color Grading / Correction Filter, TheKosmonaut (kosmonaut3d@googlemail.com)
    /// 
    /// A post-process effect that changes colors of the image based on a look-up table (LUT). 
    /// For more information check out the github info file / readme.md
    /// You can use Draw() to apply the color grading / color correction to an image and use the returned texture for output.
    /// You can use CreateLUT to create default Look-up tables with unmodified colors.
    /// </summary>
    public class ColorGrading : PostProcessPass, IDisposable
    {
        private Effect _shaderEffect;
        private FullScreenQuadRenderer _fsq;
        private RenderTarget2D _renderTarget;
        private EffectParameter _sizeParam;
        private EffectParameter _sizeRootParam;
        private EffectParameter _inputTextureParam;
        private EffectParameter _lutParam;
        private EffectPass _createLUTPass;
        private EffectPass _applyLUTPass;
        private int _size;
        private Texture2D _inputTexture;
        private Texture2D _lookupTable;

        public enum LUTSizes { Size16, Size32, Size64, Size4 };

        #region properties

        private int Size
        {
            get { return _size; }
            set
            {
                if (value != _size)
                {
                    if (value != 16 && value != 32 && value != 64 && value != 4) throw new NotImplementedException("only 16 and 32 supported right now");
                    _size = value;
                    _sizeParam.SetValue((float)_size);
                    _sizeRootParam.SetValue((float)(_size == 4 ? 2 : _size == 16 ? 4 : 8));
                }
            }
        }

        private Texture2D InputTexture
        {
            get { return _inputTexture; }
            set
            {
                if (value != _inputTexture)
                {
                    _inputTexture = value;
                    _inputTextureParam.SetValue(value);
                }
            }
        }

        private Texture2D LookUpTable
        {
            get { return _lookupTable; }
            set
            {
                if (value != _lookupTable)
                {
                    _lookupTable = value;
                    _lutParam.SetValue(value);
                }
            }
        }
        #endregion

        public override void Dispose()
        {
            _shaderEffect?.Dispose();
            _fsq?.Dispose();
            _renderTarget?.Dispose();
        }

        public override void Initialize(ContentManager content)
        {
            if (_shaderEffect != null)
                return;

            _shaderEffect = content.Load<Effect>("Shaders/PostProcessing/ColorGrading");
            _sizeParam = _shaderEffect.Parameters["Size"];
            _sizeRootParam = _shaderEffect.Parameters["SizeRoot"];
            _inputTextureParam = _shaderEffect.Parameters["InputTexture"];
            _lutParam = _shaderEffect.Parameters["LUT"];

            _applyLUTPass = _shaderEffect.Techniques["ApplyLUT"].Passes[0];
            _createLUTPass = _shaderEffect.Techniques["CreateLUT"].Passes[0];
            _fsq = new FullScreenQuadRenderer(Application.GraphicsDevice);
        }

        /// <summary>
        /// A function to create and save a new lookup-table with unmodified colors. 
        /// Check the github readme for use.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="lutsize">32 or 16. 32 will result in a larger LUT which results in better images but worse performance</param>
        /// <param name="relativeFilePath">for example "Lut16.png". The base directory is where the .exe is started from</param>
        public void CreateLUT(GraphicsDevice graphics, LUTSizes lutsize, string relativeFilePath)
        {
            _renderTarget?.Dispose();

            //_sizeParam.SetValue((float) ( lutsize == LUTSizes.Size16 ? 16 : lutsize == LUTSizes.Size32 ? 32 : 64));
            //_sizeRootParam.SetValue((float) (lutsize == LUTSizes.Size64 ? 8 : 4));
            Size = lutsize == LUTSizes.Size16 ? 16 : lutsize == LUTSizes.Size32 ? 32 : lutsize == LUTSizes.Size64 ? 64 : 4;
            int size = lutsize == LUTSizes.Size16 ? 16 * 4 : lutsize == LUTSizes.Size32 ? 32 * 8 : lutsize == LUTSizes.Size64 ? 64 * 8 : 4 * 2;

            _renderTarget = new RenderTarget2D(graphics, size, size / (lutsize == LUTSizes.Size32 ? 2 : 1), false, SurfaceFormat.Color, DepthFormat.None);

            graphics.SetRenderTarget(_renderTarget);

            _createLUTPass.Apply();
            _fsq.RenderFullscreenQuad(graphics);

            //Save this texture
            Stream stream = File.Create(relativeFilePath);
            _renderTarget.SaveAsPng(stream, _renderTarget.Width, _renderTarget.Height);
            stream.Dispose();
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            var graphics = Application.GraphicsDevice;

            //Set up rendertarget
            if (_renderTarget == null || _renderTarget.Width != renderTarget.Width || _renderTarget.Height != renderTarget.Height)
            {
                _renderTarget?.Dispose();
                _renderTarget = new RenderTarget2D(graphics, renderTarget.Width, renderTarget.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }

            InputTexture = renderTarget;
            //LookUpTable = _lookupTable;
            Size = ((renderTarget.Width == 512) ? 64 : (renderTarget.Width == 256) ? 32 : (renderTarget.Width == 64) ? 16 : 4);

            graphics.SetRenderTarget(_renderTarget);
            graphics.BlendState = BlendState.Opaque;

            _applyLUTPass.Apply();
            _fsq.RenderFullscreenQuad(graphics);
            //return _renderTarget;
        }
    }
}
