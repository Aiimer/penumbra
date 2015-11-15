﻿using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Providers
{
    internal class TextureProvider : GraphicsProvider
    {
        private const int MaxNumberOfRenderTargetBindings = 4;
        
        public RenderTargetBinding[] SceneBindings { get; } = new RenderTargetBinding[1];
        public RenderTarget2D Scene { get; private set; }
        public RenderTargetBinding[] NormalMapBindings { get; } = new RenderTargetBinding[1];
        public RenderTarget2D NormalMap { get; private set; }
        public RenderTargetBinding[] LightMapBindings { get; } = new RenderTargetBinding[1];
        public RenderTargetBinding[] LightMapBindingsNormal { get; } = new RenderTargetBinding[2];
        public RenderTarget2D LightMap { get; private set; }
        public RenderTarget2D LightMapNormal { get; private set; }

        private int _numQueriedBindings;
        private RenderTargetBinding[] _originalBindings;
        private readonly RenderTargetBinding[] _queriedBindings = new RenderTargetBinding[MaxNumberOfRenderTargetBindings];

        public RenderTargetBinding[] GetOriginalRenderTargetBindingsForQuery()
        {
            for (int i = 0; i < MaxNumberOfRenderTargetBindings; i++)
                _queriedBindings[i] = new RenderTargetBinding();
            return _queriedBindings;
        }

        public RenderTargetBinding[] GetOriginalRenderTargetBindings()
        {
            int numQueriedBindingsThisFrame = 0;
            for (int i = 0; i < MaxNumberOfRenderTargetBindings; i++)
            {
                if (_queriedBindings[i].RenderTarget == null)
                    break;
                numQueriedBindingsThisFrame++;
            }            

            if (numQueriedBindingsThisFrame != _numQueriedBindings)
            {
                _originalBindings = new RenderTargetBinding[numQueriedBindingsThisFrame];
                for (int i = 0; i < numQueriedBindingsThisFrame; i++)
                    _originalBindings[i] = _queriedBindings[i];
                _numQueriedBindings = numQueriedBindingsThisFrame;
            }
            
            return _numQueriedBindings == 0 ? null : _originalBindings;
        }

        public override void Load(PenumbraEngine engine)
        {
            base.Load(engine);
            BuildRenderTargets();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DestroyRenderTargets();
            base.Dispose(disposing);
        }

        protected override void OnSizeChanged()
        {
            BuildRenderTargets();
        }

        private void BuildRenderTargets()
        {
            DestroyRenderTargets();

            PresentationParameters pp = Engine.GraphicsDevice.PresentationParameters;
            
            LightMap = new RenderTarget2D(Engine.GraphicsDevice, BackBufferWidth, BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            LightMapBindings[0] = LightMap;
            LightMapNormal = new RenderTarget2D(Engine.GraphicsDevice, BackBufferWidth, BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            LightMapBindingsNormal[0] = LightMap;
            LightMapBindingsNormal[1] = LightMapNormal;

            NormalMap = new RenderTarget2D(Engine.GraphicsDevice, BackBufferWidth, BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            NormalMapBindings[0] = NormalMap;
            Scene = new RenderTarget2D(Engine.GraphicsDevice, BackBufferWidth, BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            SceneBindings[0] = Scene;            

            Logger.Write("New lightmap textures created");
        }

        private void DestroyRenderTargets()
        {
            Scene?.Dispose();
            LightMap?.Dispose();
            NormalMap?.Dispose();
            LightMapNormal?.Dispose();
        }
    }
}
