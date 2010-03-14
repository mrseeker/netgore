using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetGore.IO;

namespace NetGore.Graphics
{
    /// <summary>
    /// Set of keyframes used to perform an animation
    /// </summary>
    public class SkeletonSet
    {
        /// <summary>
        /// The file suffix used for the SkeletonSet.
        /// </summary>
        public const string FileSuffix = ".skels";

        const string _framesNodeName = "Frames";
        const string _rootNodeName = "SkeletonSet";

        /// <summary>
        /// Keyframes use by the set
        /// </summary>
        SkeletonFrame[] _keyFrames;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkeletonSet"/> class.
        /// </summary>
        /// <param name="keyFrames">Array of frames to use for the keyframes.</param>
        public SkeletonSet(SkeletonFrame[] keyFrames)
        {
            _keyFrames = keyFrames;
        }

        public SkeletonSet(IValueReader reader, ContentPaths contentPath)
        {
            Read(reader, contentPath);
        }

        public SkeletonSet(string skeletonSetName, ContentPaths contentPath)
            : this(new XmlValueReader(GetFilePath(skeletonSetName, contentPath), _rootNodeName), contentPath)
        {
        }

        /// <summary>
        /// Gets the keyframes used by the set
        /// </summary>
        public SkeletonFrame[] KeyFrames
        {
            get { return _keyFrames; }
        }

        /// <summary>
        /// Gets the absolute file path for a SkeletonSet file.
        /// </summary>
        /// <param name="skeletonSetName">The name of the SkeletonSet.</param>
        /// <param name="contentPath">The content path to use.</param>
        /// <returns>The absolute file path for a SkeletonSet file.</returns>
        public static string GetFilePath(string skeletonSetName, ContentPaths contentPath)
        {
            return contentPath.Skeletons.Join(skeletonSetName + FileSuffix);
        }

        /// <summary>
        /// Creates a string to represent the SkeletonSet.
        /// </summary>
        /// <returns>String representing the SkeletonSet.</returns>
        public string GetFramesString()
        {
            StringBuilder sb = new StringBuilder(512);
            foreach (SkeletonFrame frame in _keyFrames)
            {
                sb.Append(frame.FileName);
                sb.Append("/");
                sb.Append(frame.Delay);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void Read(IValueReader reader, ContentPaths contentPath)
        {
            var loadedFrames = reader.ReadManyNodes(_framesNodeName, x => new SkeletonFrame(x, contentPath));
            _keyFrames = loadedFrames;
        }

        public void Write(IValueWriter writer)
        {
            writer.WriteManyNodes(_framesNodeName, KeyFrames, ((w, item) => item.Write(w)));
        }

        /// <summary>
        /// Gets the names of all the <see cref="SkeletonSet"/>s that exist in the specified <see cref="ContentPaths"/>.
        /// </summary>
        /// <param name="contentPath">The <see cref="ContentPaths"/> to search.</param>
        public static IEnumerable<string> GetSetNames(ContentPaths contentPath)
        {
            return Directory.GetFiles(contentPath.Skeletons, "*" + FileSuffix, SearchOption.AllDirectories).Select(x => Path.GetFileNameWithoutExtension(x));
        }

        /// <summary>
        /// Saves the SkeletonSet to a file.
        /// </summary>
        /// <param name="filePath">File to save to.</param>
        public void Write(string filePath)
        {
            using (IValueWriter writer = new XmlValueWriter(filePath, _rootNodeName))
            {
                Write(writer);
            }
        }
    }
}