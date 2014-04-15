using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace ToDo
{
    /// <summary>
    /// Contains all the serializable data of a task
    /// </summary>
    [Serializable]
    public class TaskData : IXmlSerializable
    {
        /// <summary>
        /// The default Comparer for this class. Uses shano methods to determine the order. 
        /// </summary>
        protected class TaskComparer : IComparer<TaskData>
        {
            public int Compare(TaskData x, TaskData y)
            {
                //if only one was completed it must go down
                var cComp = x.CompletionDate.HasValue.CompareTo(y.CompletionDate.HasValue);
                if (cComp != 0)
                    return cComp;

                //if both have completion date, sort by it
                if (x.CompletionDate.HasValue)
                    return x.CompletionDate.Value.CompareTo(y.CompletionDate.Value);

                //if only one has expiration value it must go above
                var eComp = x.ExpirationDate.HasValue.CompareTo(y.ExpirationDate.HasValue);
                if (eComp != 0)
                    return -eComp;

                //if both have expiration date, sort by it
                if (x.ExpirationDate.HasValue)
                    return x.ExpirationDate.Value.CompareTo(y.ExpirationDate.Value);

                //otherwise sort by name
                return x.Text.CompareTo(y.Text);
            }
        }

        public static readonly IComparer<TaskData> DefaultComparer = new TaskComparer();


        public string Text;
        public string Description;

        public DateTime CreationDate;
        public DateTime? ExpirationDate;
        public DateTime? CompletionDate;


        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();

            Text = reader.GetAttribute("Text");
            Description = reader.GetAttribute("Description");
            CreationDate = Convert.ToDateTime(reader.GetAttribute("CreatedAt"), CultureInfo.InvariantCulture);

            var exp = reader.GetAttribute("ExpiresAt");
            if (!string.IsNullOrEmpty(exp))
                ExpirationDate = Convert.ToDateTime(exp, CultureInfo.InvariantCulture);

            var comp = reader.GetAttribute("CompletedAt");
            if (!string.IsNullOrEmpty(comp))
                CompletionDate = Convert.ToDateTime(comp, CultureInfo.InvariantCulture);

            reader.Read();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Text", Text);
            writer.WriteAttributeString("Description", Description);
            writer.WriteAttributeString("CreatedAt", CreationDate.ToString(CultureInfo.InvariantCulture));
            if(ExpirationDate.HasValue)
                writer.WriteAttributeString("ExpiresAt", ExpirationDate.Value.ToString(CultureInfo.InvariantCulture));
            if (CompletionDate.HasValue)
                writer.WriteAttributeString("CompletedAt", CompletionDate.Value.ToString(CultureInfo.InvariantCulture));
        }

        public override string ToString()
        {
            return Text;
        }

    }
}
