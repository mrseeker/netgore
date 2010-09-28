using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using NetGore;
using NetGore.Content;
using NetGore.EditorTools;
using NetGore.Graphics;
using NetGore.Graphics.ParticleEngine;
using NetGore.IO;
using SFML.Graphics;

namespace DemoGame.ParticleEffectEditor
{
    public partial class ScreenForm : Form, IGetTime
    {
        const string _defaultCategory = "Particle";

        readonly string _defaultTitle;
        readonly Stopwatch _watch = new Stopwatch();

        IContentManager _content;
        ParticleEmitter _emitter;

        /// <summary>
        /// Keeps track of the last emitter name. Used to update the form's title.
        /// </summary>
        string _lastEmitterName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenForm"/> class.
        /// </summary>
        public ScreenForm()
        {
            InitializeComponent();

            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            _defaultTitle = Text;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor

            _watch.Start();
        }

        /// <summary>
        /// Gets or sets the current <see cref="ParticleEmitter"/>.
        /// </summary>
        public ParticleEmitter Emitter
        {
            get { return _emitter; }
            set
            {
                if (_emitter == value)
                    return;

                _emitter = value;

                pgEffect.SelectedObject = Emitter;
                if (Emitter != null)
                cmbEmitter.SelectedItem = Emitter.GetType();
            }
        }

        RenderWindow RenderWindow
        {
            get { return GameScreen.RenderWindow; }
        }

        /// <summary>
        /// Creates the initial <see cref="ParticleEmitter"/> to display.
        /// </summary>
        /// <returns>The initial <see cref="ParticleEmitter"/> to display.</returns>
        ParticleEmitter CreateInitialEmitter()
        {
            var ret = new PointEmitter { Origin = new Vector2(GameScreen.Width, GameScreen.Height) / 2f, ReleaseRate = 35 };
            ret.Sprite.SetGrh(GrhInfo.GetData(new SpriteCategorization(_defaultCategory, "ball")));

            var colorModifier = new ParticleColorModifier
            { ReleaseColor = new Color(0, 255, 0, 255), UltimateColor = new Color(0, 0, 255, 175) };
            ret.ParticleModifiers.Add(colorModifier);

            return ret;
        }

        /// <summary>
        /// Main entry point for all the screen drawing.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        public void DrawGame(ISpriteBatch spriteBatch)
        {
            // Clear the background
            RenderWindow.Clear(Color.Black);
            spriteBatch.Begin();
            Emitter.Draw(spriteBatch);
            spriteBatch.End();
        }

        /// <summary>
        /// Handles the MouseDown event of the GameScreen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        void GameScreen_MouseDown(object sender, MouseEventArgs e)
        {
            GameScreen_MouseMove(sender, e);
        }

        /// <summary>
        /// Handles the MouseMove event of the GameScreen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        void GameScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Emitter.Origin = new Vector2(e.X, e.Y);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GameScreen.ScreenForm = this;

            // Load the content
            _content = ContentManager.Create();
            GrhInfo.Load(ContentPaths.Build, _content);

            // Load the additional UI editors for the property grid
            CustomUITypeEditors.AddEditors();

            // Set the initial emitter
            Emitter = CreateInitialEmitter();
        }

        /// <summary>
        /// Main entry point for all the screen content updating.
        /// </summary>
        public void UpdateGame()
        {
            if (Emitter == null)
                return;

            Emitter.Update(GetTime());

            if (Emitter.Name != _lastEmitterName)
            {
                _lastEmitterName = Emitter.Name;
                Text = _defaultTitle + " - " + _lastEmitterName;
            }

            if (Emitter.IsExpired)
            {
                Emitter.Reset();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnLoad control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void btnLoad_Click(object sender, EventArgs e)
        {
            string filePath;
            ParticleEmitter emitter;
            var wasSuccessful = FileDialogs.TryOpenParticleEffect(out filePath, out emitter);

            if (wasSuccessful)
                Emitter = emitter;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void btnSave_Click(object sender, EventArgs e)
        {
            if (Emitter == null)
                return;

            // Check if using the default name
            if (StringComparer.Ordinal.Equals(ParticleEmitter.DefaultName, Emitter.Name))
            {
                const string changeNameMsg =
                    "You should change the particle emitter's name from the default name before saving.{0}To do so, change the Name property of the emitter.";
                MessageBox.Show(string.Format(changeNameMsg, Environment.NewLine), "Change emitter name");
                return;
            }

            // Check if the emitter already exists
            if (ParticleEmitterFactory.EmitterExists(ContentPaths.Dev, Emitter.Name))
            {
                const string overwriteMsg = "An emitter named `{0}` already exists. Do you wish to overwrite?";
                if (MessageBox.Show(string.Format(overwriteMsg, Emitter.Name), "Overwrite?", MessageBoxButtons.YesNo) ==
                    DialogResult.No)
                {
                    const string savingAbortedMsg = "Saving aborted. No files were altered.";
                    MessageBox.Show(savingAbortedMsg);
                    return;
                }
            }

            // Save
            ParticleEmitterFactory.SaveEmitter(ContentPaths.Dev, Emitter);

            const string savedMsg = "The particle emitter `{0}` was successfully saved.";
            MessageBox.Show(string.Format(savedMsg, Emitter.Name), "Saved", MessageBoxButtons.OK);
        }

        void cmbEmitter_SelectedEmitterChanged(ParticleEmitterComboBox sender, ParticleEmitter emitter)
        {
            // Ensure the selected emitter is valid
            if (_emitter == null || emitter == null)
                return;

            // Ensure the type is actually different (no need to recreate if its the same kind of emitter)
            if (_emitter.GetType() == emitter.GetType())
                return;

            // Copy over the values of this emitter to the new emitter, and use the new emitter
            _emitter.CopyValuesTo(emitter);
            Emitter = emitter;
        }

        #region IGetTime Members

        /// <summary>
        /// Gets the current time in milliseconds.
        /// </summary>
        /// <returns>The current time in milliseconds.</returns>
        public TickCount GetTime()
        {
            return (TickCount)_watch.ElapsedMilliseconds;
        }

        #endregion
    }
}