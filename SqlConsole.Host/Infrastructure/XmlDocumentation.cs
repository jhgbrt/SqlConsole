using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace SqlConsole.Host.Infrastructure
{
    public static class XmlDocumentation
	{
		internal static System.Collections.Generic.HashSet<Assembly> loadedAssemblies = new System.Collections.Generic.HashSet<Assembly>();
		internal static ConcurrentDictionary<string, string> loadedXmlDocumentation = new ConcurrentDictionary<string, string>();

		private static void Load(Assembly assembly)
		{
			if (loadedAssemblies.Contains(assembly))
			{
				return;
			}
			lock (loadedAssemblies)
			{
				if (loadedAssemblies.Contains(assembly))
				{
					return;
				}
			}
			string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
			string xmlFilePath = Path.Combine(directoryPath, assembly.GetName().Name + ".xml");
			if (File.Exists(xmlFilePath))
			{
				using StreamReader streamReader = new StreamReader(xmlFilePath);
				Load(streamReader);
			}
			// currently marking assembly as loaded even if the XML file was not found
			// may want to adjust in future, but I think this is good for now
			lock (loadedAssemblies)
			{
				loadedAssemblies.Add(assembly);
			}
		}

		/// <summary>Loads the XML code documentation into memory so it can be accessed by extension methods on reflection types.</summary>
		/// <param name="textReader">The text reader to process in an XmlReader.</param>
		private static void Load(TextReader textReader)
		{
			using XmlReader xmlReader = XmlReader.Create(textReader);
			while (xmlReader.Read())
			{
				if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "member")
				{
					string? raw_name = xmlReader["name"];
					if (raw_name != null)
						loadedXmlDocumentation[raw_name] = xmlReader.ReadInnerXml();
				}
			}
		}

		/// <summary>Gets the XML documentation on a property.</summary>
		/// <param name="propertyInfo">The property to get the XML documentation of.</param>
		/// <returns>The XML documentation on the property.</returns>
		/// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
		public static string? GetDocumentation(this PropertyInfo propertyInfo)
		{
			var declaringType = propertyInfo.DeclaringType;
			if (declaringType == null) return null;
			if (declaringType.FullName == null) return null;
			Load(declaringType.Assembly);
			string key = "P:" + XmlDocumentationKeyHelper(declaringType.FullName, propertyInfo.Name);
			loadedXmlDocumentation.TryGetValue(key, out string? documentation);
			return documentation;
		}


		private static string XmlDocumentationKeyHelper(string typeFullNameString, string memberNameString)
		{
			string key = Regex.Replace(typeFullNameString, @"\[.*\]", string.Empty).Replace('+', '.');
			if (!(memberNameString is null))
			{
				key += "." + memberNameString;
			}
			return key;
		}

		public static void Clear()
        {
			lock (loadedAssemblies)
			{
				loadedAssemblies.Clear();
			}
			loadedXmlDocumentation.Clear();
        }
	}

}
