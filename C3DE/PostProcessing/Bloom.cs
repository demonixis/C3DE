using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.PostProcessing
{
    /// <summary>
    /// Version 1.1, 16. Dez. 2016
    /// Bloom / Blur, 2016 TheKosmonaut
    /// </summary>
    public class Bloom : PostProcessPass, IDisposable
    {
        public enum BloomPresets
        {
            Wide,
            Focussed,
            Small,
            SuperWide,
            Cheap,
            One
        };

        //resolution
        private int _width;
        private int _height;

        //RenderTargets
        private RenderTarget2D _bloomRenderTarget2DMip0;
        private RenderTarget2D _bloomRenderTarget2DMip1;
        private RenderTarget2D _bloomRenderTarget2DMip2;
        private RenderTarget2D _bloomRenderTarget2DMip3;
        private RenderTarget2D _bloomRenderTarget2DMip4;
        private RenderTarget2D _bloomRenderTarget2DMip5;
        private SurfaceFormat _renderTargetFormat;

        //Objects
        private GraphicsDevice _graphicsDevice;
        private QuadRenderer _quadRenderer;

        private Effect _bloomEffect;
        private EffectPass _bloomPassExtract;
        private EffectPass _bloomPassExtractLuminance;
        private EffectPass _bloomPassDownsample;
        private EffectPass _bloomPassUpsample;
        private EffectPass _bloomPassUpsampleLuminance;
        private EffectParameter _bloomParameterScreenTexture;
        private EffectParameter _bloomInverseResolutionParameter;
        private EffectParameter _bloomRadiusParameter;
        private EffectParameter _bloomStrengthParameter;
        private EffectParameter _bloomStreakLengthParameter;
        private EffectParameter _bloomThresholdParameter;

        //Preset variables for different mip levels
        private float _bloomRadius1 = 1.0f;
        private float _bloomRadius2 = 1.0f;
        private float _bloomRadius3 = 1.0f;
        private float _bloomRadius4 = 1.0f;
        private float _bloomRadius5 = 1.0f;
        private float _bloomStrength1 = 1.0f;
        private float _bloomStrength2 = 1.0f;
        private float _bloomStrength3 = 1.0f;
        private float _bloomStrength4 = 1.0f;
        private float _bloomStrength5 = 1.0f;
        public float BloomStrengthMultiplier = 1.0f;
        private float _radiusMultiplier = 1.0f;

        // Properties
        private BloomPresets _bloomPreset;
        private Vector2 _bloomInverseResolutionField;
        private float _bloomRadius;
        private float _bloomStrength;
        private float _bloomStreakLength;
        private float _bloomThreshold;

        public bool BloomUseLuminance = true;
        public int BloomDownsamplePasses = 5;

        public Bloom(GraphicsDevice graphics) : base(graphics)
        {
        }

        #region Properties

        public BloomPresets BloomPreset
        {
            get { return _bloomPreset; }
            set
            {
                if (_bloomPreset == value) return;

                _bloomPreset = value;
                SetBloomPreset(_bloomPreset);
            }
        }

        private Texture2D BloomScreenTexture
        {
            set { _bloomParameterScreenTexture.SetValue(value); }
        }

        private Vector2 BloomInverseResolution
        {
            get { return _bloomInverseResolutionField; }
            set
            {
                if (value != _bloomInverseResolutionField)
                {
                    _bloomInverseResolutionField = value;
                    _bloomInverseResolutionParameter.SetValue(_bloomInverseResolutionField);
                }
            }
        }

        private float BloomRadius
        {
            get { return _bloomRadius; }
            set
            {
                if (Math.Abs(_bloomRadius - value) > 0.001f)
                {
                    _bloomRadius = value;
                    _bloomRadiusParameter.SetValue(_bloomRadius * _radiusMultiplier);
                }
            }
        }

        private float BloomStrength
        {
            get { return _bloomStrength; }
            set
            {
                if (Math.Abs(_bloomStrength - value) > 0.001f)
                {
                    _bloomStrength = value;
                    _bloomStrengthParameter.SetValue(_bloomStrength * BloomStrengthMultiplier);
                }

            }
        }

        public float BloomStreakLength
        {
            get { return _bloomStreakLength; }
            set
            {
                if (Math.Abs(_bloomStreakLength - value) > 0.001f)
                {
                    _bloomStreakLength = value;
                    _bloomStreakLengthParameter.SetValue(_bloomStreakLength);
                }
            }
        }

        public float BloomThreshold
        {
            get { return _bloomThreshold; }
            set
            {
                if (Math.Abs(_bloomThreshold - value) > 0.001f)
                {
                    _bloomThreshold = value;
                    _bloomThresholdParameter.SetValue(_bloomThreshold);
                }
            }
        }

        #endregion

        private void SetBloomPreset(BloomPresets preset)
        {
            switch (preset)
            {
                case BloomPresets.Wide:
                    {
                        _bloomStrength1 = 0.5f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 2;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 2;
                        _bloomRadius5 = 4.0f;
                        _bloomRadius4 = 4.0f;
                        _bloomRadius3 = 2.0f;
                        _bloomRadius2 = 2.0f;
                        _bloomRadius1 = 1.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.SuperWide:
                    {
                        _bloomStrength1 = 0.9f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 2;
                        _bloomStrength5 = 6;
                        _bloomRadius5 = 4.0f;
                        _bloomRadius4 = 2.0f;
                        _bloomRadius3 = 2.0f;
                        _bloomRadius2 = 2.0f;
                        _bloomRadius1 = 2.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Focussed:
                    {
                        _bloomStrength1 = 0.8f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 2;
                        _bloomRadius5 = 4.0f;
                        _bloomRadius4 = 2.0f;
                        _bloomRadius3 = 2.0f;
                        _bloomRadius2 = 2.0f;
                        _bloomRadius1 = 2.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Small:
                    {
                        _bloomStrength1 = 0.8f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 1;
                        _bloomRadius5 = 1;
                        _bloomRadius4 = 1;
                        _bloomRadius3 = 1;
                        _bloomRadius2 = 1;
                        _bloomRadius1 = 1;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Cheap:
                    {
                        _bloomStrength1 = 0.8f;
                        _bloomStrength2 = 2;
                        _bloomRadius2 = 2;
                        _bloomRadius1 = 2;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 2;
                        break;
                    }
                case BloomPresets.One:
                    {
                        _bloomStrength1 = 4f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 2;
                        _bloomRadius5 = 1.0f;
                        _bloomRadius4 = 1.0f;
                        _bloomRadius3 = 1.0f;
                        _bloomRadius2 = 1.0f;
                        _bloomRadius1 = 1.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
            }
        }

        private void ChangeBlendState()
        {
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        public void UpdateResolution(int width, int height)
        {
            _width = width;
            _height = height;

            if (_bloomRenderTarget2DMip0 != null)
            {
                Dispose();
            }

            _bloomRenderTarget2DMip0 = new RenderTarget2D(_graphicsDevice,
                (int)(width),
                (int)(height), false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            _bloomRenderTarget2DMip1 = new RenderTarget2D(_graphicsDevice,
                (int)(width / 2),
                (int)(height / 2), false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip2 = new RenderTarget2D(_graphicsDevice,
                (int)(width / 4),
                (int)(height / 4), false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip3 = new RenderTarget2D(_graphicsDevice,
                (int)(width / 8),
                (int)(height / 8), false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip4 = new RenderTarget2D(_graphicsDevice,
                (int)(width / 16),
                (int)(height / 16), false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip5 = new RenderTarget2D(_graphicsDevice,
                (int)(width / 32),
                (int)(height / 32), false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        public override void Dispose()
        {
            _bloomRenderTarget2DMip0.Dispose();
            _bloomRenderTarget2DMip1.Dispose();
            _bloomRenderTarget2DMip2.Dispose();
            _bloomRenderTarget2DMip3.Dispose();
            _bloomRenderTarget2DMip4.Dispose();
            _bloomRenderTarget2DMip5.Dispose();
        }

        public override void Initialize(ContentManager content)
        {
            if (_graphicsDevice != null)
                return;

            _graphicsDevice = Application.GraphicsDevice;
            UpdateResolution(Screen.Width, Screen.Height);

            //if quadRenderer == null -> new, otherwise not
            _quadRenderer = new QuadRenderer(_graphicsDevice);
            _renderTargetFormat = SurfaceFormat.Color;

            //Load the shader parameters and passes for cheap and easy access
            _bloomEffect = content.Load<Effect>("Shaders/PostProcessing/Bloom");
            _bloomInverseResolutionParameter = _bloomEffect.Parameters["InverseResolution"];
            _bloomRadiusParameter = _bloomEffect.Parameters["Radius"];
            _bloomStrengthParameter = _bloomEffect.Parameters["Strength"];
            _bloomStreakLengthParameter = _bloomEffect.Parameters["StreakLength"];
            _bloomThresholdParameter = _bloomEffect.Parameters["Threshold"];

            //For DirectX / Windows
            _bloomParameterScreenTexture = _bloomEffect.Parameters["ScreenTexture"];

            //If we are on OpenGL it's different, load the other one then!
            if (_bloomParameterScreenTexture == null)
            {
                //for OpenGL / CrossPlatform
                _bloomParameterScreenTexture = _bloomEffect.Parameters["LinearSampler+ScreenTexture"];
            }

            _bloomPassExtract = _bloomEffect.Techniques["Extract"].Passes[0];
            _bloomPassExtractLuminance = _bloomEffect.Techniques["ExtractLuminance"].Passes[0];
            _bloomPassDownsample = _bloomEffect.Techniques["Downsample"].Passes[0];
            _bloomPassUpsample = _bloomEffect.Techniques["Upsample"].Passes[0];
            _bloomPassUpsampleLuminance = _bloomEffect.Techniques["UpsampleLuminance"].Passes[0];

            //Default threshold.
            BloomThreshold = 0.2f;
            SetBloomPreset(BloomPreset);
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            //Check if we are initialized
            if (_graphicsDevice == null)
                throw new Exception("Module not yet Loaded / Initialized. Use Load() first");

            var width = Screen.Width;
            var height = Screen.Height;

            //Change renderTarget resolution if different from what we expected. If lower than the inputTexture we gain performance.
            if (width != _width || height != _height)
            {
                UpdateResolution(width, height);

                //Adjust the blur so it looks consistent across diferrent scalings
                _radiusMultiplier = (float)width / renderTarget.Width;

                //Update our variables with the multiplier
                SetBloomPreset(BloomPreset);
            }

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.BlendState = BlendState.Opaque;

            //EXTRACT  //Note: Is setRenderTargets(binding better?)
            //We extract the bright values which are above the Threshold and save them to Mip0
            _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip0);

            BloomScreenTexture = renderTarget;
            BloomInverseResolution = new Vector2(1.0f / _width, 1.0f / _height);

            if (BloomUseLuminance) _bloomPassExtractLuminance.Apply();
            else _bloomPassExtract.Apply();
            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

            //Now downsample to the next lower mip texture
            if (BloomDownsamplePasses > 0)
            {
                //DOWNSAMPLE TO MIP1
                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip1);

                BloomScreenTexture = _bloomRenderTarget2DMip0;
                //Pass
                _bloomPassDownsample.Apply();
                _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

                if (BloomDownsamplePasses > 1)
                {
                    //Our input resolution is halfed, so our inverse 1/res. must be doubled
                    BloomInverseResolution *= 2;

                    //DOWNSAMPLE TO MIP2
                    _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip2);

                    BloomScreenTexture = _bloomRenderTarget2DMip1;
                    //Pass
                    _bloomPassDownsample.Apply();
                    _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

                    if (BloomDownsamplePasses > 2)
                    {
                        BloomInverseResolution *= 2;

                        //DOWNSAMPLE TO MIP3
                        _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip3);

                        BloomScreenTexture = _bloomRenderTarget2DMip2;
                        //Pass
                        _bloomPassDownsample.Apply();
                        _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

                        if (BloomDownsamplePasses > 3)
                        {
                            BloomInverseResolution *= 2;

                            //DOWNSAMPLE TO MIP4
                            _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip4);

                            BloomScreenTexture = _bloomRenderTarget2DMip3;
                            //Pass
                            _bloomPassDownsample.Apply();
                            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

                            if (BloomDownsamplePasses > 4)
                            {
                                BloomInverseResolution *= 2;

                                //DOWNSAMPLE TO MIP5
                                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip5);

                                BloomScreenTexture = _bloomRenderTarget2DMip4;
                                //Pass
                                _bloomPassDownsample.Apply();
                                _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

                                ChangeBlendState();

                                //UPSAMPLE TO MIP4
                                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip4);
                                BloomScreenTexture = _bloomRenderTarget2DMip5;

                                BloomStrength = _bloomStrength5;
                                BloomRadius = _bloomRadius5;
                                if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                                else _bloomPassUpsample.Apply();
                                _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

                                BloomInverseResolution /= 2;
                            }

                            ChangeBlendState();

                            //UPSAMPLE TO MIP3
                            _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip3);
                            BloomScreenTexture = _bloomRenderTarget2DMip4;

                            BloomStrength = _bloomStrength4;
                            BloomRadius = _bloomRadius4;
                            if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                            else _bloomPassUpsample.Apply();
                            _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

                            BloomInverseResolution /= 2;
                        }

                        ChangeBlendState();

                        //UPSAMPLE TO MIP2
                        _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip2);
                        BloomScreenTexture = _bloomRenderTarget2DMip3;

                        BloomStrength = _bloomStrength3;
                        BloomRadius = _bloomRadius3;
                        if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                        else _bloomPassUpsample.Apply();
                        _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

                        BloomInverseResolution /= 2;
                    }

                    ChangeBlendState();

                    //UPSAMPLE TO MIP1
                    _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip1);
                    BloomScreenTexture = _bloomRenderTarget2DMip2;

                    BloomStrength = _bloomStrength2;
                    BloomRadius = _bloomRadius2;
                    if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                    else _bloomPassUpsample.Apply();
                    _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);

                    BloomInverseResolution /= 2;
                }

                ChangeBlendState();

                //UPSAMPLE TO MIP0
                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip0);
                BloomScreenTexture = _bloomRenderTarget2DMip1;

                BloomStrength = _bloomStrength1;
                BloomRadius = _bloomRadius1;

                if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                else _bloomPassUpsample.Apply();
                _quadRenderer.RenderQuad(_graphicsDevice, Vector2.One * -1, Vector2.One);
            }

            //Note the final step could be done as a blend to the final texture.
            //return _bloomRenderTarget2DMip0;

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = _bloomRenderTarget2DMip0;

            var viewport = m_GraphicsDevice.Viewport;
            m_GraphicsDevice.SetRenderTarget(renderTarget);

            DrawFullscreenQuad(spriteBatch, _bloomRenderTarget2DMip0, viewport.Width, viewport.Height, null);
        }
    }
}
