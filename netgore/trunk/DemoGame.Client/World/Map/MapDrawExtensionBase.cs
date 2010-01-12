using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using NetGore.Graphics;

namespace DemoGame.Client
{
    /// <summary>
    /// Base class for a class that extends the Map drawing in some way.
    /// </summary>
    public abstract class MapDrawExtensionBase
    {
        Map _map;

        /// <summary>
        /// Gets or sets if this MapDrawExtensionBase will perform the drawing.
        /// </summary>
        public virtual bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the Map that the drawing is for. Can be null.
        /// </summary>
        public Map Map
        {
            get { return _map; }
            set
            {
                if (_map == value)
                    return;

                // Remove the event hooks from the old map
                if (_map != null)
                    UnhookMap(_map);

                // Set the new map
                _map = value;

                // Add the event hooks to the new map
                if (_map != null)
                    HookMap(_map);
            }
        }

        /// <summary>
        /// When overridden in the derived class, handles additional drawing for a MapRenderLayer after the
        /// Map actually renders the layer.
        /// </summary>
        /// <param name="layer">The MapRenderLayer that was drawn.</param>
        /// <param name="spriteBatch">The SpriteBatch the Map used to draw.</param>
        /// <param name="camera">The camera that the Map used to draw.</param>
        protected abstract void EndDrawLayer(MapRenderLayer layer, SpriteBatch spriteBatch, ICamera2D camera);

        /// <summary>
        /// Handles actions needed in adding a Map to this MapDrawExtensionBase. All event hooks and cached values
        /// for the <paramref name="map"/> should be added here.
        /// </summary>
        /// <param name="map">Map that is to be used for drawing. This value is never null.</param>
        protected virtual void HookMap(Map map)
        {
            map.OnStartDrawLayer += MapStartDrawEventForwarder;
            map.OnEndDrawLayer += MapEndDrawEventForwarder;
        }

        /// <summary>
        /// Forwards the draw events from the Map to the corresponding abstract method in this Class.
        /// </summary>
        /// <param name="map">Map that the drawing is taking place on.</param>
        /// <param name="layer">The layer that the drawing event is related to.</param>
        /// <param name="spriteBatch">The SpriteBatch that was used to do the drawing.</param>
        /// <param name="camera">The camera that was used in the drawing.</param>
        void MapEndDrawEventForwarder(Map map, MapRenderLayer layer, SpriteBatch spriteBatch, ICamera2D camera)
        {
            Debug.Assert(map == Map, "We received a draw event for the wrong map?");
            Debug.Assert(Map != null, "How did we receive a draw event while the Map is null?");

            if (Enabled && Map != null)
                EndDrawLayer(layer, spriteBatch, camera);
        }

        /// <summary>
        /// Forwards the draw events from the Map to the corresponding abstract method in this Class.
        /// </summary>
        /// <param name="map">Map that the drawing is taking place on.</param>
        /// <param name="layer">The layer that the drawing event is related to.</param>
        /// <param name="spriteBatch">The SpriteBatch that was used to do the drawing.</param>
        /// <param name="camera">The camera that was used in the drawing.</param>
        void MapStartDrawEventForwarder(Map map, MapRenderLayer layer, SpriteBatch spriteBatch, ICamera2D camera)
        {
            Debug.Assert(map == Map, "We received a draw event for the wrong map?");
            Debug.Assert(Map != null, "How did we receive a draw event while the Map is null?");

            if (Enabled && Map != null)
                StartDrawLayer(layer, spriteBatch, camera);
        }

        /// <summary>
        /// When overridden in the derived class, handles additional drawing for a MapRenderLayer before the
        /// Map actually renders the layer.
        /// </summary>
        /// <param name="layer">The MapRenderLayer that is to be drawn.</param>
        /// <param name="spriteBatch">The SpriteBatch the Map used to draw.</param>
        /// <param name="camera">The camera that the Map used to draw.</param>
        protected abstract void StartDrawLayer(MapRenderLayer layer, SpriteBatch spriteBatch, ICamera2D camera);

        /// <summary>
        /// Handles actions needed in removing a Map from this MapDrawExtensionBase. All event hooks and cached values
        /// for the <paramref name="map"/> should be removed here.
        /// </summary>
        /// <param name="map">Map that is to stop being used for drawing. This value is never null.</param>
        protected virtual void UnhookMap(Map map)
        {
            map.OnStartDrawLayer -= MapStartDrawEventForwarder;
            map.OnEndDrawLayer -= MapEndDrawEventForwarder;
        }
    }
}