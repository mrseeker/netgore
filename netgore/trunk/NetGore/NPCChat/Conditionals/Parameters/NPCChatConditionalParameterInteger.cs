using System;
using System.Linq;
using NetGore.IO;

namespace NetGore.NPCChat
{
    /// <summary>
    /// A INPCChatConditionalParameter with a value of type Integer.
    /// </summary>
    class NPCChatConditionalParameterInteger : NPCChatConditionalParameter
    {
        int _value;

        /// <summary>
        /// Gets this parameter's Value as an object.
        /// </summary>
        public override object Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets this parameter's Value as an Integer.
        /// </summary>
        /// <exception cref="MethodAccessException">The ValueType is not equal to
        /// NPCChatConditionalParameterType.Integer.</exception>
        public override int ValueAsInteger
        {
            get { return _value; }
        }

        /// <summary>
        /// The NPCChatConditionalParameterType that describes the native value type of this parameter's Value.
        /// </summary>
        public override NPCChatConditionalParameterType ValueType
        {
            get { return NPCChatConditionalParameterType.Integer; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NPCChatConditionalParameterInteger"/> struct.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        public NPCChatConditionalParameterInteger(int value)
        {
            _value = value;
        }

        /// <summary>
        /// When overridden in the derived class, reads the underlying value from the <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The IValueReader to read from.</param>
        /// <param name="valueName">The name to of the value in the <paramref name="reader"/>.</param>
        protected override void ReadValue(IValueReader reader, string valueName)
        {
            _value = reader.ReadInt(valueName);
        }

        /// <summary>
        /// When overridden in the derived class, writes the underlying value to the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The IValueWriter to write to.</param>
        /// <param name="valueName">The name to of the value to use in the <paramref name="writer"/>.</param>
        protected override void WriteValue(IValueWriter writer, string valueName)
        {
            writer.Write(valueName, _value);
        }
    }
}