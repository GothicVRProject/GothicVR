using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR.Features;


namespace Unity.XR.OpenXR.Features.PICOSupport
{
    internal class PICOModifyAndroidManifest : OpenXRFeatureBuildHooks
    {
        public override int callbackOrder => 1;
        public override Type featureType => typeof(PICOFeature);
        protected override void OnPreprocessBuildExt(BuildReport report) { }
        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
            var androidManifest = new AndroidManifest(GetManifestPath(path));
            androidManifest.AddPICOMetaData(path);
            androidManifest.Save();
        }
        protected override void OnPostprocessBuildExt(BuildReport report) { }
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
            private readonly XmlElement ManifestElement;
            public AndroidManifest(string path) : base(path)
            {
                ManifestElement = SelectSingleNode("/manifest") as XmlElement;
                ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
            }
            private XmlAttribute CreateOrUpdateAndroidAttribute(string key, string value)
            {
                XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
                attr.Value = value;
                return attr;
            }
            private void CreateOrUpdateAndroidPermissionData(string name)
            {
                XmlNodeList nodeList = ManifestElement.SelectNodes("uses-permission");
                foreach (XmlNode node in nodeList)
                {
                    if (node != null)
                    {
                        // Update existing nodes
                        if (node.Attributes != null && name.Equals(node.Attributes[0].Value))
                        {
                            return;
                        }
                    }
                }

                // Create new node
                var md = ManifestElement.AppendChild(CreateElement("uses-permission"));
                md.Attributes.Append(CreateOrUpdateAndroidAttribute("name", name.ToString()));
            }

            private void DeleteAndroidPermissionData(string name)
            {
                XmlNodeList nodeList = ManifestElement.SelectNodes("uses-permission");
                foreach (XmlNode node in nodeList)
                {
                    if (node != null)
                    {
                        // Delete existing nodes
                        if (node.Attributes != null && name.Equals(node.Attributes[0].Value))
                        {
                            node.ParentNode?.RemoveChild(node);
                            return;
                        }
                    }
                }
            }

            private void CreateOrUpdateAndroidMetaData(string name, string value)
            {
                XmlNodeList nodeList = ApplicationElement.SelectNodes("meta-data");
                foreach (XmlNode node in nodeList)
                {
                    if (node != null)
                    {
                        // Update existing nodes
                        if (node.Attributes != null && name.Equals(node.Attributes[0].Value))
                        {
                            node.Attributes[0].Value = name;
                            node.Attributes[1].Value = value;
                            return;
                        }
                    }
                }

                // Create new node
                var md = ApplicationElement.AppendChild(CreateElement("meta-data"));
                md.Attributes.Append(CreateOrUpdateAndroidAttribute("name", name.ToString()));
                md.Attributes.Append(CreateOrUpdateAndroidAttribute("value", value.ToString()));
            }

            private void DeleteAndroidMetaData(string name)
            {
                XmlNodeList nodeList = ApplicationElement.SelectNodes("meta-data");
                foreach (XmlNode node in nodeList)
                {
                    if (node != null)
                    {
                        // Delete existing nodes
                        if (node.Attributes != null && name.Equals(node.Attributes[0].Value))
                        {
                            node.ParentNode?.RemoveChild(node);
                            return;
                        }
                    }
                }
            }

            internal void AddPICOMetaData(string path)
            {
                CreateOrUpdateAndroidMetaData("pvr.app.type", "vr");
                CreateOrUpdateAndroidMetaData("pvr.sdk.version", PICOFeature.SDKVersion);
                CreateOrUpdateAndroidMetaData("pxr.sdk.version_code", "5800");

                if (PICOProjectSetting.GetProjectConfig().isEyeTracking)
                {
                    CreateOrUpdateAndroidPermissionData("com.picovr.permission.EYE_TRACKING");
                    CreateOrUpdateAndroidMetaData("picovr.software.eye_tracking", "1");
                    CreateOrUpdateAndroidMetaData("eyetracking_calibration", PICOProjectSetting.GetProjectConfig().isEyeTrackingCalibration ? "true" : "false");
                }
                else
                {
                    DeleteAndroidPermissionData("com.picovr.permission.EYE_TRACKING");
                    DeleteAndroidMetaData("picovr.software.eye_tracking");
                    DeleteAndroidMetaData("eyetracking_calibration");
                }

                CreateOrUpdateAndroidMetaData("handtracking", PICOProjectSetting.GetProjectConfig().isHandTracking ? "1" : "0");
                CreateOrUpdateAndroidMetaData("pvr.app.splash", PICOProjectSetting.GetProjectConfig().GetSystemSplashScreen(path));
            }
        }
    }
}