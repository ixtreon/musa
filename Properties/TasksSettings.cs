using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ltd = System.Collections.Generic.List<Musa.TaskData>;

namespace Musa.Properties
{
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        /* 
         * This (part of the) class handles serialization of tasks into XML
         * and their automatic saving when Settings.Save() is called. 
         * Requires manual loading!
         */

        //XML serialization
        static XmlSerializer serializer = new XmlSerializer(typeof(ltd), new[] { typeof(TaskData) });
        static XmlSerializerNamespaces xns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        static XmlWriterSettings xws = new XmlWriterSettings()
        {
            Indent = true,
            OmitXmlDeclaration = true
        };

        /// <summary>
        /// Gets all the saved tasks. 
        /// <para>Empty unless the <see cref="LoadTasks()"/> method is called first. </para>
        /// </summary>
        public ltd Tasks { get; private set; }

        /// <summary>
        /// Loads the tasks from their serialized representation in the settings
        /// </summary>
        public void LoadTasks()
        {
            try
            {
                using (var sr = new System.IO.StringReader(taskString))
                    Tasks = (ltd)serializer.Deserialize(sr);
            }
            catch { }
            if (Tasks == null)
                Tasks = new ltd();
        }

        protected override void OnSettingsSaving(object sender, CancelEventArgs e)
        {
            //serialize the tasks and save them as an XML string
            using (var sw = new System.IO.StringWriter())
                using (var xw = XmlWriter.Create(sw, xws))
                {
                    serializer.Serialize(xw, Tasks, xns);
                    taskString = sw.ToString();
                }
            base.OnSettingsSaving(sender, e);
        }

    }
}
