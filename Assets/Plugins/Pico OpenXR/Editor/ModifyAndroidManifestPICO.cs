using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.OpenXR.Features.PICOSupport;

namespace UnityEditor.XR.OpenXR.Features.PICONeoSupport
{
    internal class ModifyAndroidManifestPICO : OpenXRFeatureBuildHooks
    {
        public override int callbackOrder => 1;
        public override Type featureType => typeof(PICOFeature);
        protected override void OnPreprocessBuildExt(BuildReport report)  { }
        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
            var androidManifest = new AndroidManifest(GetManifestPath(path));
            androidManifest.AddPICOMetaData();
            androidManifest.Save();
        }
        protected override void OnPostprocessBuildExt(BuildReport report){ }
        private string _manifestFilePath;
        private string GetManifestPath(string basePath)
        {
            if (!string.IsNullOrEmpty(_manifestFilePath)) return _manifestFilePath;
            var pathBuilder = new StringBuilder(basePath);
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
            _manifestFilePath = pathBuilder.ToString();

            return _manifestFilePath;
        }
        private class AndroidXmlDocument : XmlDocument
        {
            private string m_Path;
            protected XmlNamespaceManager nsMgr;
            public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

            public AndroidXmlDocument(string path)
            {
                m_Path = path;
                using (var reader = new XmlTextReader(m_Path))
                {
                    reader.Read();
                    Load(reader);
                }
                nsMgr = new XmlNamespaceManager(NameTable);
                nsMgr.AddNamespace("android", AndroidXmlNamespace);
            }
            public string Save()
            {
                return SaveAs(m_Path);
            }
            public string SaveAs(string path)
            {
                using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
                {
                    writer.Formatting = Formatting.Indented;
                    Save(writer);
                }
                return path;
            }
        }
        private class AndroidManifest : AndroidXmlDocument
        {
            private readonly XmlElement ApplicationElement;
            public AndroidManifest(string path) : base(path)
            {
                ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
            }
            private XmlAttribute CreateOrUpdateAndroidAttribute(string key, string value)
            {
                XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
                attr.Value = value;
                return attr;
            }

            private void CreateOrUpdateAndroidMetaData(string name, string value)
            {
                XmlNodeList nodeList = ApplicationElement.SelectNodes("meta-data");
                foreach ( XmlNode node in nodeList)
                {
                    if(node != null)
                    {
                        foreach (XmlAttribute attribute in node.Attributes)
                        {
                            if (name.Equals(attribute.Value))
                            {
                                return;
                            }
                        }                     
                    }
                }

                var md = ApplicationElement.AppendChild(CreateElement("meta-data"));
                md.Attributes.Append(CreateOrUpdateAndroidAttribute("name", name.ToString()));
                md.Attributes.Append(CreateOrUpdateAndroidAttribute("value", value.ToString()));
            }

            internal void AddPICOMetaData()
            {
                CreateOrUpdateAndroidMetaData("pvr.app.type", "vr");
                CreateOrUpdateAndroidMetaData("pvr.sdk.version", "1.0.0");
            }
        }
    }
}
