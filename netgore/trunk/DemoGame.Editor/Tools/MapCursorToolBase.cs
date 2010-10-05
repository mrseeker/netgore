﻿using System;
using System.Linq;
using NetGore.Editor.EditorTool;

namespace DemoGame.Editor
{
    public abstract class MapCursorToolBase : Tool
    {
        const string _enabledToolsGroup = "Map Cursors";

        /// <summary>
        /// Initializes a new instance of the <see cref="MapCursorToolBase"/> class.
        /// </summary>
        /// <param name="toolManager">The <see cref="ToolManager"/>.</param>
        /// <param name="settings">The <see cref="ToolSettings"/> to use to create this <see cref="Tool"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="toolManager"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/> is null.</exception>
        protected MapCursorToolBase(ToolManager toolManager, ToolSettings settings)
            : base(toolManager, ModifyToolSettings(settings))
        {
        }

        /// <summary>
        /// Modifies the <see cref="ToolSettings"/> as it is passed to the base class constructor.
        /// </summary>
        /// <param name="settings">The <see cref="ToolSettings"/>.</param>
        /// <returns>The <see cref="ToolSettings"/></returns>
        static ToolSettings ModifyToolSettings(ToolSettings settings)
        {
            settings.ToolBarVisibility = ToolBarVisibility.Map;
            settings.EnabledToolsGroup = _enabledToolsGroup;
            return settings;
        }
    }
}