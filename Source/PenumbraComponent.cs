﻿using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra
{
    /// <summary>
    /// GPU based 2D lighting and shadowing engine with penumbra support. Operates with
    /// <see cref="Light"/>s and shadow <see cref="Hull"/>s, where light is a
    /// colored light source which casts shadows on shadow hulls that are outlines of scene
    /// geometry (polygons) impassable by light.
    /// </summary>
    /// <remarks>
    /// Before rendering scene, ensure to call <c>PenumbraComponent.BeginDraw</c> to swap render target
    /// in order for the component to be able to later apply generated lightmap.
    /// </remarks>
    public class PenumbraComponent : DrawableGameComponent
    {        
        private readonly PenumbraEngine _engine = new PenumbraEngine();

        private bool _initialized;
        private bool _beginDrawCalled;
        private bool _beginNormalMappedCalled;

        /// <summary>
        /// Constructs a new instance of <see cref="PenumbraComponent"/>.
        /// </summary>
        /// <param name="game">Game object to associate the engine with.</param>
        public PenumbraComponent(Game game) 
            : base(game)
        {
            // We only need to draw this component.
            Enabled = false;
        }

        /// <summary>
        /// Gets or sets if debug outlines should be drawn for shadows and light sources and
        /// if logging is enabled.
        /// </summary>
        public bool Debug
        {
            get { return _engine.Debug; }
            set { _engine.Debug = value; }
        }

        /// <summary>
        /// Gets or sets the ambient color of the scene. Note that alpha value will be ignored.
        /// </summary>
        public Color AmbientColor
        {
            get { return _engine.AmbientColor; }
            set { _engine.AmbientColor = value; }
        }

        /// <summary>
        /// Gets or sets the custom transformation matrix used by the component.
        /// </summary>
        public Matrix Transform
        {
            get { return _engine.Camera.Custom; }
            set { _engine.Camera.Custom = value; }
        }

        /// <summary>
        /// Gets or sets if this component is used with <see cref="SpriteBatch"/> and its transform should
        /// be automatically applied. Default value is <c>true</c>.
        /// </summary>
        public bool SpriteBatchTransformEnabled
        {
            get { return _engine.Camera.SpriteBatchTransformEnabled; }
            set { _engine.Camera.SpriteBatchTransformEnabled = value; }
        }

        /// <summary>
        /// Gets or sets if normal-mapped lighting is enabled. With normal-mapped lighting, user is also responsible
        /// of calling BeginNormalMapped() and rendering scene normals.
        /// </summary>
        public bool NormalMappedLightingEnabled
        {
            get { return _engine.NormalMappedLightingEnabled; }
            set { _engine.NormalMappedLightingEnabled = value; }
        }

        /// <summary>
        /// Gets the list of lights registered with the engine.
        /// </summary>
        public ObservableCollection<Light> Lights => _engine.Lights;

        /// <summary>
        /// Gets the list of shadow hulls registered with the engine.
        /// </summary>
        public ObservableCollection<Hull> Hulls => _engine.Hulls;

        /// <summary>
        /// Explicitly initializes the engine. This should only be called if the
        /// component was not added to the game's components list through <c>Components.Add</c>.
        /// </summary>
        public override void Initialize()
        {
            if (_initialized) return;

            base.Initialize();
            var deviceManager = (GraphicsDeviceManager)Game.Services.GetService<IGraphicsDeviceManager>();
            _engine.Load(GraphicsDevice, deviceManager);
            _initialized = true;
        }

        /// <summary>
        /// Sets up the lightmap generation sequence. This should be called before Draw.
        /// </summary>
        public void BeginDraw()
        {
            if (Visible)
            {
                EnsureInitialized();
                _engine.PreRender();
                _beginDrawCalled = true;
            }
        }

        public void BeginNormalMapped()
        {
            if (Visible)
            {
                EnsureInitialized();
                _engine.PreNormalMapped();
                _beginNormalMappedCalled = true;   
            }
        }        

        private void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException(
                    $"{nameof(PenumbraComponent)} is not initialized. Make sure to call {nameof(Initialize)} when setting up a game.");
        }

        /// <summary>
        /// Generates the lightmap, blends it with whatever was drawn to the scene between the
        /// calls to BeginDraw and this and presents the result to the backbuffer.
        /// </summary>        
        /// <param name="gameTime">Time passed since the last call to Draw.</param>
        public override void Draw(GameTime gameTime)
        {
            if (Visible)
            {
                if (!_beginDrawCalled)
                    throw new InvalidOperationException(
                        $"{nameof(BeginDraw)} must be called before rendering a scene to be lit and calling {nameof(Draw)}.");

                _engine.Render();
                _beginDrawCalled = false;
                _beginNormalMappedCalled = false;
            }
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            _engine.Dispose();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                UnloadContent();
        }
    }
}