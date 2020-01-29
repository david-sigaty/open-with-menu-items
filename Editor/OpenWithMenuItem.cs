using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/*********************************************************************************
 *   This is the base class for "Open With..." menu items. Inheriting            *
 *   from this will add a new entry to the Open With list. Example inheritance   *
 *   is at the bottom of this script.                                            *
 *********************************************************************************/

namespace TeamCitrus.Editor
{
	public abstract class OpenWithMenuItem
	{
		public const string BaseKey = "TeamCitrus.OpenWithMenuItem.";
		public const string BasePath = "Assets/Open With.../";
		public const string OpenPath = "Assets/Open with ";
		public const string EditPath = "Assets/Edit with ";
		public const int BasePriority = -2000;
		
		// Gets all selected asset paths
		private static List<string> AssetPaths
		{
			get
			{
				string path = null;
				List<string> output = new List<string>();
				Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel | SelectionMode.Assets);
				for (int index = 0; index < selectedObjects.Length; ++index)
				{
					path = AssetDatabase.GetAssetPath(selectedObjects[index]);
					if (false == string.IsNullOrEmpty(path))
					{
						output.Add(path);
					}
				}
				return output;
			}
		}
		
		private static void Open(string applicationName, string applicationPath)
		{
			List<string> paths = AssetPaths;
			bool allDirectories = true;
			foreach (string path in paths)
			{
				FileAttributes attributes = File.GetAttributes(path);
				if ((attributes & FileAttributes.Directory) != FileAttributes.Directory)
				{
					allDirectories = false;
					try
					{
						System.Diagnostics.Process.Start(applicationPath, string.Format("\"{0}\"", Path.GetFullPath(path)));
					}
					catch (System.Exception e)
					{
						Debug.LogException(e);
						Debug.LogError("Could not start " + applicationName + ". Is the path set properly?");
					}
				}
			}
			if (true == allDirectories)
			{
				Debug.LogError("Opening directories isn't supported for " + applicationName + ".");
			}
		}
		
		public static void Execute<T>()
		{
			string name;
			if (false == GetString<T>("Name", out name))
			{
				name = typeof(T).ToString();
			}
			string path = GetPath<T>(true);
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogError("Application path not set for " + name);
				return;
			}
			Open(name, path);
		}
		
		public static bool Validate<T>()
		{
			return false == string.IsNullOrEmpty(GetPath<T>());
		}
		
		public static string GetPath(System.Type type, bool logErrors = false)
		{
			string key;
			if (false == GetString(type, "Key", out key))
			{
				if (true == logErrors)
				{
					Debug.LogError("No Key field of type string found in " + type.ToString());
				}
				return null;
			}
			if (string.IsNullOrEmpty(key))
			{
				if (true == logErrors)
				{
					Debug.LogError("Key field is null or empty in " + type.ToString());
				}
				return null;
			}
			string path;
			if (false == EditorPrefs.HasKey(key))
			{
				if (true == GetString(type, "DefaultPath", out path))
				{
					return path;
				}
				if (true == logErrors)
				{
					Debug.LogError("Missing EditorPrefs key " + key);
				}
				return string.Empty;
			}
			path = EditorPrefs.GetString(key, null);
			if (true == string.IsNullOrEmpty(path))
			{
				GetString(type, "DefaultPath", out path);
			}
			return path;
		}
		
		public static bool SetPath(System.Type type, string path, bool logErrors = false)
		{
			string key;
			if (false == GetString(type, "Key", out key))
			{
				if (true == logErrors)
				{
					Debug.LogError("No Key field of type string found in " + type.ToString());
				}
				return false;
			}
			if (string.IsNullOrEmpty(key))
			{
				if (true == logErrors)
				{
					Debug.LogError("Key field is null or empty in " + type.ToString());
				}
				return false;
			}
			EditorPrefs.SetString(key, path);
			return true;
		}
		
		public static string GetPath<T>(bool logErrors = false)
		{
			return GetPath(typeof(T), logErrors);
		}
		
		public static bool GetString(System.Type type, string fieldName, out string value)
		{
			value = null;
			FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			if (null == field)
				return false;
			if (field.FieldType != typeof(string))
				return false;
			value = (string) field.GetValue(null);
			return true;
		}
		
		public static bool GetString<T>(string fieldName, out string value)
		{
			return GetString(typeof(T), fieldName, out value);
		}
		
		public static string GetName(System.Type type)
		{
			string output;
			if (false == GetString(type, "Name", out output))
			{
				output = type.ToString();
			}
			return output;
		}
		
		[MenuItem(BasePath + "Setup Paths", false, 0)]
		private static void SetupPaths()
		{
			OpenWithMenuItemsPathSetup window = EditorWindow.GetWindow<OpenWithMenuItemsPathSetup>(true, "'Open With...' Path Setup", true);
			Rect position = window.position;
			position.x = 80;
			position.y = 100;
			position.width = 485;
			position.height = 300;
			window.position = position;
			window.minSize = new Vector2(485, 300);
		}
	}
}

/*****************************************************************************************
 *                                     Example Usage                                     *
 *****************************************************************************************

using UnityEditor;
using TeamCitrus.Editor;

public class OpenWithApplication : OpenWithMenuItem
{
	// * REQUIRED * The plain text name of the application
	public const string Name = "[Application Name]";
	// * REQUIRED * The key used to save application path data in EditorPrefs
	public const string Key = BaseKey + Name;
	// * OPTIONAL * The default path if there is no data in EditorPrefs
	public const string DefaultPath = "[Application Path]";
	// Define path for menu item and verification methods
	private const string MenuItemPath = BasePath + Name;
	
	// Set up the MenuItem for opening the selected assets with this application.
	// The int added to BasePriority orders options in the list. An editor restart
	// may be necessary to have proper ordering in the list.
	[MenuItem(MenuItemPath, false, BasePriority + 1)]
	public static void Open() { Execute<OpenWithApplication>(); }

	// Set up the MenuItem for validating whether a path is set for this application.
	[MenuItem(MenuItemPath, true)]
	public static bool OpenValidation() { return Validate<OpenWithApplication>(); }
}

******************************************************************************************/