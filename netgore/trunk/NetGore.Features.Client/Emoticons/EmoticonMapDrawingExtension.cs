﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using NetGore.Graphics;

namespace NetGore.Features.Emoticons
{
    /// <summary>
    /// 
    /// </summary>
    public class EmoticonMapDrawingExtension : MapDrawingExtension
    {
        readonly EmoticonDisplayManager _displayManager;

        public EmoticonDisplayManager DisplayManager { get { return _displayManager; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmoticonMapDrawingExtension"/> class.
        /// </summary>
        /// <param name="displayManager">The <see cref="EmoticonDisplayManager"/>.</param>
        public EmoticonMapDrawingExtension(EmoticonDisplayManager displayManager)
        {
            _displayManager = displayManager;
        }

        /// <summary>
        /// When overridden in the derived class, handles drawing to the map after the given <paramref name="layer"/> is drawn.
        /// </summary>
        /// <param name="map">The map the drawing is taking place on.</param>
        /// <param name="layer">The layer that was just drawn.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to draw to.</param>
        protected override void HandleDrawAfterLayer(IDrawableMap map, MapRenderLayer layer, SpriteBatch spriteBatch)
        {
            if (layer == MapRenderLayer.Chararacter)
                _displayManager.Draw(spriteBatch);
        }
    }
}
