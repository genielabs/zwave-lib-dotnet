using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ZWaveLib
{

    /// <summary>
    /// Z wave node config.
    /// </summary>
    [Serializable]
    public class ZWaveNodeConfig
    {
        /// <summary>
        /// Gets or sets the node identifier.
        /// </summary>
        /// <value>The node identifier.</value>
        public byte NodeId { get; internal set; }

        /// <summary>
        /// Gets or sets the node information frame.
        /// </summary>
        /// <value>The node information frame.</value>
        public byte[] NodeInformationFrame { get; internal set; }

        /// <summary>
        /// Gets or sets the secured node information frame.
        /// </summary>
        /// <value>The secured node information frame.</value>
        public byte[] SecuredNodeInformationFrame { get; internal set; }

        /// <summary>
        /// Gets or sets the device private network key.
        /// </summary>
        /// <value>The device private network key.</value>
        public byte[] DevicePrivateNetworkKey { get; internal set; }

        /// <summary>
        /// Gets or sets the command class versions.
        /// </summary>
        /// <value>The command class versions.</value>
        [XmlIgnore]
        public Dictionary<CommandClass, byte> CommandClassVersions { get; internal set; }

        // Get around the fact that Dictionaries aren't supported by XmlSerialize
        // Solution found in: http://stackoverflow.com/questions/495647/serialize-class-containing-dictionary-member
        public class SerializeableKeyValue<T1,T2>
        {
            public T1 Key { get; set; }

            public T2 Value { get; set; }
        }

        public SerializeableKeyValue<CommandClass, byte>[] CommandClassSerializable
        {
            get
            {
                var list = new List<SerializeableKeyValue<CommandClass, byte>>();
                if (CommandClassVersions != null)
                {
                    list.AddRange(CommandClassVersions.Keys.Select(key => new SerializeableKeyValue<CommandClass, byte>() { Key = key, Value = CommandClassVersions[key] }));
                }
                return list.ToArray();
            }
            set
            {
                CommandClassVersions = new Dictionary<CommandClass, byte>();
                foreach (var item in value)
                {
                    CommandClassVersions.Add(item.Key, item.Value);
                }
            }
        }
    }

}

