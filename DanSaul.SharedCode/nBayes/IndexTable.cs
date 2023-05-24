namespace nBayes
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <remarks>Original code found here: http://weblogs.asp.net/pwelter34/archive/2006/05/03/444961.aspx</remarks>
    [XmlRoot("index")]
    public class IndexTable<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable where TKey : notnull
	{
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("entry");

                reader.ReadStartElement("word");
                object? keyObj = keySerializer.Deserialize(reader);
                if (keyObj == null)
                    continue;
                TKey key = (TKey)keyObj;

				reader.ReadEndElement();

                reader.ReadStartElement("count");
                object? valueObj = valueSerializer.Deserialize(reader);
				if (valueObj == null)
					continue;
				TValue value = (TValue)valueObj;

				reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("entry");

                writer.WriteStartElement("word");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("count");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}