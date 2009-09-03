﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using NetGore.IO;
using NetGore.NPCChat;
using NetGore.NPCChat.Conditionals;

namespace NetGore.EditorTools.NPCChat
{
    public delegate void EditorNPCChatDialogItemEventHandler(EditorNPCChatDialogItem dialogItem);

    /// <summary>
    /// Describes a single page of dialog in a NPCChatDialogBase, and the possible responses available for the page.
    /// </summary>
    public class EditorNPCChatDialogItem : NPCChatDialogItemBase
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly List<EditorNPCChatResponse> _responses = new List<EditorNPCChatResponse>();
        readonly List<TreeNode> _treeNodes = new List<TreeNode>();
        NPCChatConditionalCollectionBase _conditionals;
        ushort _index;
        string _text;
        string _title;

        /// <summary>
        /// Notifies listeners when any of the object's property values have changed.
        /// </summary>
        public event EditorNPCChatDialogItemEventHandler OnChange;

        /// <summary>
        /// When overridden in the derived class, gets the NPCChatConditionalCollectionBase that contains the
        /// conditionals used to evaluate if this NPCChatDialogItemBase may be used. If this value is null, it
        /// is assumed that there are no conditionals attached to this NPCChatDialogItemBase, and should be treated
        /// the same way as if the conditionals evaluated to true.
        /// </summary>
        public override NPCChatConditionalCollectionBase Conditionals
        {
            get { return _conditionals; }
        }

        /// <summary>
        /// When overridden in the derived class, gets the page index of this NPCChatDialogItemBase in the
        /// NPCChatDialogBase. This value is unique to each NPCChatDialogItemBase in the NPCChatDialogBase.
        /// </summary>
        public override ushort Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Gets the response list.
        /// </summary>
        public List<EditorNPCChatResponse> ResponseList
        {
            get { return _responses; }
        }

        /// <summary>
        /// When overridden in the derived class, gets an IEnumerable of the EditorNPCChatResponses available
        /// for this page of dialog.
        /// </summary>
        public override IEnumerable<NPCChatResponseBase> Responses
        {
            get { return ResponseList.Cast<NPCChatResponseBase>(); }
        }

        /// <summary>
        /// When overridden in the derived class, gets the main dialog text in this page of dialog.
        /// </summary>
        public override string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// When overridden in the derived class, gets the title for this page of dialog. The title is primarily
        /// used for debugging and development purposes only.
        /// </summary>
        public override string Title
        {
            get { return _title; }
        }

        /// <summary>
        /// Gets the List of TreeNodes that use this EditorNPCChatDialogItem. This should only be used by the
        /// <see cref="EditorNPCChatDialogItem"/>.
        /// </summary>
        internal List<TreeNode> TreeNodes
        {
            get { return _treeNodes; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorNPCChatDialogItem"/> class.
        /// </summary>
        /// <param name="reader">IValueReader to read the values from.</param>
        public EditorNPCChatDialogItem(IValueReader reader) : base(reader)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorNPCChatDialogItem"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        public EditorNPCChatDialogItem(ushort index) : this(index, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorNPCChatDialogItem"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="text">The text.</param>
        public EditorNPCChatDialogItem(ushort index, string text) : this(index, text, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorNPCChatDialogItem"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="text">The text.</param>
        /// <param name="title">The title.</param>
        public EditorNPCChatDialogItem(ushort index, string text, string title)
        {
            SetText(text);
            SetTitle(title);

            _index = index;
        }

        /// <summary>
        /// Adds multiple EditorNPCChatResponses.
        /// </summary>
        /// <param name="responses">The EditorNPCChatResponses to add.</param>
        public void AddResponse(params EditorNPCChatResponse[] responses)
        {
            if (responses == null)
                return;

            foreach (EditorNPCChatResponse response in responses)
            {
                AddResponse(response);
            }
        }

        /// <summary>
        /// Adds a EditorNPCChatResponse.
        /// </summary>
        /// <param name="response">The EditorNPCChatResponse to add.</param>
        public void AddResponse(EditorNPCChatResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            _responses.Add(response);
            int index = _responses.IndexOf(response);
            response.SetValue((byte)index);
        }

        /// <summary>
        /// When overridden in the derived class, creates a NPCChatConditionalCollectionBase.
        /// </summary>
        /// <returns>A new NPCChatConditionalCollectionBase instance, or null if the derived class does not
        /// want to load the conditionals when using Read.</returns>
        protected override NPCChatConditionalCollectionBase CreateConditionalCollection()
        {
            return new EditorNPCChatConditionalCollection();
        }

        /// <summary>
        /// When overridden in the derived class, creates a NPCChatResponseBase using the given IValueReader.
        /// </summary>
        /// <param name="reader">IValueReader to read the values from.</param>
        /// <returns>A NPCChatResponseBase created using the given IValueReader</returns>
        protected override NPCChatResponseBase CreateResponse(IValueReader reader)
        {
            return new EditorNPCChatResponse(reader);
        }

        /// <summary>
        /// When overridden in the derived class, gets the index of the next NPCChatDialogItemBase to use from
        /// the given response.
        /// </summary>
        /// <param name="user">The user that is participating in the chatting.</param>
        /// <param name="npc">The NPC chat is participating in the chatting.</param>
        /// <param name="responseIndex">The index of the response used.</param>
        /// <returns>The index of the NPCChatDialogItemBase to go to based off of the given response.</returns>
        public override ushort GetNextPage(object user, object npc, byte responseIndex)
        {
            return ResponseList[responseIndex].Page;
        }

        /// <summary>
        /// When overridden in the derived class, gets the NPCChatResponseBase of the response with the given
        /// <paramref name="responseIndex"/>.
        /// </summary>
        /// <param name="responseIndex">Index of the response.</param>
        /// <returns>The NPCChatResponseBase for the response at index <paramref name="responseIndex"/>, or null
        /// if the response is invalid or ends the chat dialog.</returns>
        public override NPCChatResponseBase GetResponse(byte responseIndex)
        {
            if (responseIndex >= _responses.Count)
            {
                const string errmsg = "Invalid response index `{0}` for page `{1}`. Max response index is `{2}`.";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, responseIndex, Index, _responses.Count - 1);
                return null;
            }

            return _responses[responseIndex];
        }

        /// <summary>
        /// Sets the Conditionals property.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetConditionals(NPCChatConditionalCollectionBase value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (_conditionals == value)
                return;

            _conditionals = value;

            if (OnChange != null)
                OnChange(this);
        }

        /// <summary>
        /// When overridden in the derived class, sets the values read from the Read method.
        /// </summary>
        /// <param name="page">The index.</param>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        /// <param name="responses">The responses.</param>
        /// <param name="conditionals">The conditionals.</param>
        protected override void SetReadValues(ushort page, string title, string text, IEnumerable<NPCChatResponseBase> responses,
                                              NPCChatConditionalCollectionBase conditionals)
        {
            _index = page;
            SetTitle(title);
            SetText(text);

            _responses.Clear();
            _responses.AddRange(responses.Cast<EditorNPCChatResponse>());

            EditorNPCChatConditionalCollection c = conditionals as EditorNPCChatConditionalCollection;
            _conditionals = c ?? new EditorNPCChatConditionalCollection();
        }

        /// <summary>
        /// Sets the Text.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetText(string value)
        {
            if (_text == value)
                return;

            _text = value;

            if (OnChange != null)
                OnChange(this);
        }

        /// <summary>
        /// Sets the Title.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetTitle(string value)
        {
            if (_title == value)
                return;

            _title = value;

            if (OnChange != null)
                OnChange(this);
        }
    }
}