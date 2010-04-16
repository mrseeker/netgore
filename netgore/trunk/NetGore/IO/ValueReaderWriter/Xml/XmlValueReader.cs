using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using log4net;

namespace NetGore.IO
{
    /// <summary>
    /// Implementation of the <see cref="IValueReader"/> using Xml.
    /// </summary>
    public class XmlValueReader : IValueReader
    {
        /// <summary>
        /// XmlReaderSettings used for ReadNodes().
        /// </summary>
        static readonly XmlReaderSettings _readNodesReaderSettings = new XmlReaderSettings
        { ConformanceLevel = ConformanceLevel.Fragment };

        readonly bool _useEnumNames = true;
        readonly Dictionary<string, List<string>> _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlValueReader"/> class.
        /// </summary>
        /// <param name="reader">XmlReader that the values will be read from.</param>
        /// <param name="rootNodeName">Name of the root node that is to be read from.</param>
        public XmlValueReader(XmlReader reader, string rootNodeName) : this(reader, rootNodeName, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlValueReader"/> class.
        /// </summary>
        /// <param name="reader">XmlReader that the values will be read from.</param>
        /// <param name="rootNodeName">Name of the root node that is to be read from.</param>
        /// <param name="useEnumNames">If true, Enums I/O will be done using the Enum's name. If false,
        /// Enum I/O will use the underlying integer value of the Enum.</param>
        public XmlValueReader(XmlReader reader, string rootNodeName, bool useEnumNames)
            : this(reader, rootNodeName, false, useEnumNames)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlValueReader"/> class.
        /// </summary>
        /// <param name="filePath">Path of the file to read.</param>
        /// <param name="rootNodeName">Name of the root node that is to be read from.</param>
        public XmlValueReader(string filePath, string rootNodeName) : this(filePath, rootNodeName, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlValueReader"/> class.
        /// </summary>
        /// <param name="filePath">Path of the file to read.</param>
        /// <param name="rootNodeName">Name of the root node that is to be read from.</param>
        /// <param name="useEnumNames">If true, Enums I/O will be done using the Enum's name. If false,
        /// Enum I/O will use the underlying integer value of the Enum.</param>
        public XmlValueReader(string filePath, string rootNodeName, bool useEnumNames)
        {
            _useEnumNames = useEnumNames;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (XmlReader r = XmlReader.Create(stream))
                {
                    while (r.Read())
                    {
                        if (r.NodeType == XmlNodeType.Element &&
                            string.Equals(r.Name, rootNodeName, StringComparison.OrdinalIgnoreCase))
                            break;
                    }

                    if (r.EOF)
                        throw new XmlException(string.Format("Failed to find the node `{0}` in the file.", rootNodeName));

                    _values = ReadNodesIntoDictionary(r, rootNodeName, true);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlValueReader"/> class.
        /// </summary>
        /// <param name="reader">XmlReader that the values will be read from.</param>
        /// <param name="rootNodeName">Name of the root node that is to be read from.</param>
        /// <param name="readAllContent">If true, the XmlReader is expected to be read to the end.</param>
        /// <param name="useEnumNames">If true, Enums I/O will be done using the Enum's name. If false,
        /// Enum I/O will use the underlying integer value of the Enum.</param>
        XmlValueReader(XmlReader reader, string rootNodeName, bool readAllContent, bool useEnumNames)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            _useEnumNames = useEnumNames;
            _values = ReadNodesIntoDictionary(reader, rootNodeName, readAllContent);
        }

        static ArgumentException CreateDuplicateKeysException(string key)
        {
            const string parameterName = "name";
            const string errmsg =
                "Cannot read the value of key `{0}` since multiple values were found with that key." +
                " This method requires that the key's name is unique.";

            return new ArgumentException(string.Format(errmsg, key), parameterName);
        }

        static ArgumentException CreateKeyNotFoundException(string key)
        {
            const string parameterName = "parameterName";
            const string errmsg = "Cannot read the value of key `{0}` since no key with that name was found.";

            return new ArgumentException(string.Format(errmsg, key), parameterName);
        }

        /// <summary>
        /// Creates a new XmlValueReader from a string of a child Xml node.
        /// </summary>
        /// <param name="name">The name of the root node.</param>
        /// <param name="s">The node contents.</param>
        /// <returns>A new XmlValueReader from a string of a child Xml node.</returns>
        XmlValueReader GetXmlValueReaderFromNodeString(string name, string s)
        {
            string trimmed = s.Trim();
            var bytes = Encoding.UTF8.GetBytes(trimmed);

            XmlValueReader ret;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (XmlReader r = XmlReader.Create(ms, _readNodesReaderSettings))
                {
                    ret = new XmlValueReader(r, name, true, UseEnumNames);
                }
            }

            return ret;
        }

        /// <summary>
        /// Reads the contents of each Element node and places it into a dictionary. Attributes will be ignored and
        /// any Element node containing child Element nodes, not a value, will result in an exception being thrown.
        /// </summary>
        /// <param name="reader">XmlReader to read the values from.</param>
        /// <param name="rootNodeName">Name of the root node that is to be read from. Serves as a means of checking
        /// to ensure the XmlValueReader is at the expected location.</param>
        /// <param name="readAllContent">If true, the XmlReader is expected to be read to the end.</param>
        /// <returns>Dictionary containing the content of each Element node.</returns>
        static Dictionary<string, List<string>> ReadNodesIntoDictionary(XmlReader reader, string rootNodeName, bool readAllContent)
        {
            var ret = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            int expectedEndDepth = reader.Depth + 1;
            bool skipRead = false;

            if (reader.NodeType == XmlNodeType.Element)
            {
                expectedEndDepth--;
                skipRead = true;
            }

            // Read past the first node if it is the root node
            if (string.Equals(reader.Name, rootNodeName, StringComparison.OrdinalIgnoreCase))
                reader.Read();
            else
                expectedEndDepth--;

            // Read all the values
            while (skipRead || reader.Read())
            {
                skipRead = false;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        // Read the name and value of the element
                        string key = reader.Name;
                        string value = reader.ReadInnerXml();

                        List<string> l;
                        if (!ret.TryGetValue(key, out l))
                        {
                            l = new List<string>();
                            ret.Add(key, l);
                        }

                        l.Add(value);

                        break;

                    case XmlNodeType.EndElement:
                        // Check if we hit the end of the nodes
                        if (reader.Depth != expectedEndDepth)
                            continue;

                        if (string.Equals(reader.Name, rootNodeName, StringComparison.OrdinalIgnoreCase))
                            return ret;
                        else
                            throw new Exception(string.Format("Was expecting end of element `{0}`, but found `{1}`.", rootNodeName,
                                                              reader.Name));
                }
            }

            if (!readAllContent)
                throw new Exception("XmlReader was read to the end, but this was not expected to happen.");

            return ret;
        }

        /// <summary>
        /// Unescapes a string, replacing the Xml characters with the proper string characters.
        /// </summary>
        /// <param name="s">The string to unescape.</param>
        /// <returns>The unescaped string.</returns>
        static string UnescapeString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            s = s.Replace("&lt;", "<");
            s = s.Replace("&gt;", ">");
            s = s.Replace("&quot;", "\"");
            s = s.Replace("&apos;", "'");
            s = s.Replace("&amp;", "&");
            s = s.Replace("\n", Environment.NewLine);

            return s;
        }

        #region IValueReader Members

        /// <summary>
        /// Gets if this IValueReader supports using the name field to look up values. If false, values will have to
        /// be read back in the same order they were written and the name field will be ignored.
        /// </summary>
        public bool SupportsNameLookup
        {
            get { return true; }
        }

        /// <summary>
        /// Gets if this IValueReader supports reading nodes. If false, any attempt to use nodes in this IValueReader
        /// will result in a NotSupportedException being thrown.
        /// </summary>
        public bool SupportsNodes
        {
            get { return true; }
        }

        /// <summary>
        /// Gets if Enum I/O will be done with the Enum's name. If true, the name of the Enum value instead of the
        /// underlying integer value will be used. If false, the underlying integer value will be used. This
        /// only to Enum I/O that does not explicitly state which method to use.
        /// </summary>
        public bool UseEnumNames
        {
            get { return _useEnumNames; }
        }

        /// <summary>
        /// Reads a boolean.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public bool ReadBool(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseBool(values[0]);
        }

        /// <summary>
        /// Reads a 8-bit unsigned integer.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public byte ReadByte(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseByte(values[0]);
        }

        /// <summary>
        /// Reads a 64-bit floating-point number.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public double ReadDouble(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseDouble(values[0]);
        }

        /// <summary>
        /// Reads an Enum of type <typeparamref name="T"/>. Whether to use the Enum's underlying integer value or the
        /// name of the Enum value is determined from the <see cref="UseEnumNames"/> property.
        /// </summary>
        /// <typeparam name="T">The Type of Enum.</typeparam>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public T ReadEnum<T>(string name) where T : struct, IComparable, IConvertible, IFormattable
        {
            if (UseEnumNames)
                return ReadEnumName<T>(name);
            else
                return ReadEnumValue<T>(name);
        }

        /// <summary>
        /// Reads an Enum of type <typeparamref name="T"/> using the Enum's name instead of the value.
        /// </summary>
        /// <typeparam name="T">The Type of Enum.</typeparam>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public T ReadEnumName<T>(string name) where T : struct, IComparable, IConvertible, IFormattable
        {
            return EnumHelper<T>.ReadName(this, name);
        }

        /// <summary>
        /// Reads an Enum of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The Type of Enum.</typeparam>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public T ReadEnumValue<T>(string name) where T : struct, IComparable, IConvertible, IFormattable
        {
            return EnumHelper<T>.ReadValue(this, name);
        }

        /// <summary>
        /// Reads a 32-bit floating-point number.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public float ReadFloat(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseFloat(values[0]);
        }

        /// <summary>
        /// Reads a 32-bit signed integer.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public int ReadInt(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseInt(values[0]);
        }

        /// <summary>
        /// Reads a signed integer of up to 32 bits.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <param name="bits">Number of bits to read.</param>
        /// <returns>Value read from the reader.</returns>
        public int ReadInt(string name, int bits)
        {
            return ReadInt(name);
        }

        /// <summary>
        /// Reads a 64-bit signed integer.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public long ReadLong(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseLong(values[0]);
        }

        /// <summary>
        /// Reads multiple values that were written with WriteMany.
        /// </summary>
        /// <typeparam name="T">The Type of value to read.</typeparam>
        /// <param name="nodeName">The name of the node containing the values.</param>
        /// <param name="readHandler">Delegate that reads the values from the IValueReader.</param>
        /// <returns>Array of the values read the IValueReader.</returns>
        public T[] ReadMany<T>(string nodeName, ReadManyHandler<T> readHandler)
        {
            IValueReader nodeReader = ReadNode(nodeName);
            int count = nodeReader.ReadInt(XmlValueHelper.CountValueKey);

            var ret = new T[count];
            for (int i = 0; i < count; i++)
            {
                string key = XmlValueHelper.GetItemKey(i);
                ret[i] = readHandler(nodeReader, key);
            }

            return ret;
        }

        /// <summary>
        /// Reads multiple nodes that were written with WriteMany.
        /// </summary>
        /// <typeparam name="T">The Type of nodes to read.</typeparam>
        /// <param name="nodeName">The name of the root node containing the values.</param>
        /// <param name="readHandler">Delegate that reads the values from the IValueReader.</param>
        /// <returns>Array of the values read the IValueReader.</returns>
        public T[] ReadManyNodes<T>(string nodeName, ReadManyNodesHandler<T> readHandler)
        {
            IValueReader nodeReader = ReadNode(nodeName);
            int count = nodeReader.ReadInt(XmlValueHelper.CountValueKey);

            var ret = new T[count];
            for (int i = 0; i < count; i++)
            {
                string key = XmlValueHelper.GetItemKey(i);
                IValueReader childNodeReader = nodeReader.ReadNode(key);
                ret[i] = readHandler(childNodeReader);
            }

            return ret;
        }

        /// <summary>
        /// Reads multiple nodes that were written with WriteMany.
        /// </summary>
        /// <typeparam name="T">The Type of nodes to read.</typeparam>
        /// <param name="nodeName">The name of the root node containing the values.</param>
        /// <param name="readHandler">Delegate that reads the values from the IValueReader.</param>
        /// <param name="handleMissingKey">Allows for handling when a key is missing or invalid instead of throwing
        /// an <see cref="Exception"/>. This allows nodes to be read even if one or more of the expected
        /// items are missing. The returned array will contain null for these indicies. The int contained in the
        /// <see cref="Action{T}"/> contains the 0-based index of the index that failed. This parameter is only
        /// valid when <see cref="IValueReader.SupportsNameLookup"/> and <see cref="IValueReader.SupportsNodes"/> are true.
        /// Default is null.</param>
        /// <returns>
        /// Array of the values read from the IValueReader.
        /// </returns>
        public T[] ReadManyNodes<T>(string nodeName, ReadManyNodesHandler<T> readHandler, Action<int, Exception> handleMissingKey)
        {
            if (handleMissingKey == null)
                return ReadManyNodes(nodeName, readHandler);

            IValueReader nodeReader = ReadNode(nodeName);
            int count = nodeReader.ReadInt(XmlValueHelper.CountValueKey);

            var ret = new T[count];
            for (int i = 0; i < count; i++)
            {
                string key = XmlValueHelper.GetItemKey(i);

                try
                {
                    IValueReader childNodeReader = nodeReader.ReadNode(key);
                    ret[i] = readHandler(childNodeReader);
                }
                catch (Exception ex)
                {
                    const string errmsg = "Failed to read key `{0}` (index: {1}) from `{2}` when using ReadManyNodes on nodeName `{3}`. handleMissingKey argument was specified, so loading will resume...";
                    if (log.IsWarnEnabled)
                        log.WarnFormat(errmsg, key, i, this, nodeName);

                    handleMissingKey(i, ex);
                    ret[i] = default(T);
                }
            }

            return ret;
        }

        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Reads a single child node, while enforcing the idea that there should only be one node
        /// in the key. If there is more than one node for the given <paramref name="key"/>, an
        /// ArgumentException will be thrown.
        /// </summary>
        /// <param name="key">The key of the child node to read.</param>
        /// <returns>An IValueReader to read the child node.</returns>
        /// <exception cref="ArgumentException">Zero or more than one values found for the given
        /// <paramref name="key"/>.</exception>
        public IValueReader ReadNode(string key)
        {
            if (_values[key].Count != 1)
            {
                const string errmsg = "ReadNode() requires there to be one and only one value for the given key.";
                throw new ArgumentException(errmsg, "key");
            }

            string nodeContents = _values[key][0];
            return GetXmlValueReaderFromNodeString(key, nodeContents);
        }

        /// <summary>
        /// Reads one or more child nodes from the IValueReader.
        /// </summary>
        /// <param name="name">Name of the nodes to read.</param>
        /// <param name="count">The number of nodes to read. Must be greater than 0. An ArgumentOutOfRangeException will
        /// be thrown if this value exceeds the actual number of nodes available.</param>
        /// <returns>An IEnumerable of IValueReaders used to read the nodes.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Count is less than 0.</exception>
        public IEnumerable<IValueReader> ReadNodes(string name, int count)
        {
            if (count == 0)
                return Enumerable.Empty<IValueReader>();

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            var valuesAsList = _values[name];
            IEnumerable<string> values;

            if (valuesAsList.Count() < count)
                throw new ArgumentOutOfRangeException("count", "Value is greater than the actual number of available nodes.");

            if (valuesAsList.Count() > count)
                values = valuesAsList.Take(count);
            else
                values = valuesAsList;

            var ret = new List<IValueReader>(count);

            foreach (string value in values.Take(count))
            {
                XmlValueReader reader = GetXmlValueReaderFromNodeString(name, value);
                ret.Add(reader);
            }

            return ret;
        }

        /// <summary>
        /// Reads a 8-bit signed integer.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public sbyte ReadSByte(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseSByte(values[0]);
        }

        /// <summary>
        /// Reads a 16-bit signed integer.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public short ReadShort(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseShort(values[0]);
        }

        /// <summary>
        /// Reads a variable-length string of up to 65535 characters in length.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>String read from the reader.</returns>
        public string ReadString(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            string ret = values[0];
            ret = UnescapeString(ret);
            return ret;
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public uint ReadUInt(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseUInt(values[0]);
        }

        /// <summary>
        /// Reads an unsigned integer of up to 32 bits.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <param name="bits">Number of bits to read.</param>
        /// <returns>Value read from the reader.</returns>
        public uint ReadUInt(string name, int bits)
        {
            return ReadUInt(name);
        }

        /// <summary>
        /// Reads a 64-bit unsigned integer.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public ulong ReadULong(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseULong(values[0]);
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer.
        /// </summary>
        /// <param name="name">Unique name of the value to read.</param>
        /// <returns>Value read from the reader.</returns>
        public ushort ReadUShort(string name)
        {
            var values = _values[name];

            if (values.Count == 0)
                throw CreateKeyNotFoundException(name);

            if (values.Count > 1)
                throw CreateDuplicateKeysException(name);

            return Parser.Invariant.ParseUShort(values[0]);
        }

        #endregion
    }
}