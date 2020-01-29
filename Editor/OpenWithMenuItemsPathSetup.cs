using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/**************************************************************************
 *   This is the Editor Window that's used to specify application paths   *
 *   for the options that exist in the project.                           *
 **************************************************************************/

namespace TeamCitrus.Editor
{
	public class OpenWithMenuItemsPathSetup : EditorWindow
	{
		private const float closeButtonHeight = 30;

		private Vector2 scrollPosition = Vector2.zero;
		private List<System.Type> types = new List<System.Type>();
		
		// On window focus, get all relevant types from all loaded assemblies.
		void OnFocus()
		{
			types.Clear();
			System.Type openWithMenuItemType = typeof(OpenWithMenuItem);
			foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (System.Type type in assembly.GetTypes())
				{
					if (type.IsSubclassOf(openWithMenuItemType))
					{
						types.Add(type);
					}
				}
			}
		}
		
		void OnGUI()
		{
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - closeButtonHeight - EditorGUIUtility.singleLineHeight));
			for (int index = 0; index < types.Count; ++index)
			{
				DrawPathGUI(types[index]);
			}
			EditorGUILayout.EndScrollView();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Done", GUILayout.Width(120), GUILayout.Height(closeButtonHeight)))
			{
				Close();
			}
			GUILayout.EndHorizontal();
		}
		
		private void DrawPathGUI(System.Type type)
		{
			EditorGUILayout.BeginHorizontal();
			string name = OpenWithMenuItem.GetName(type);
			string path = OpenWithMenuItem.GetPath(type);
			GUILayout.Label(name, GUILayout.Width(150));
			string newPath = GUILayout.TextField(path, GUILayout.MaxWidth(position.width - 200));
			if (GUILayout.Button("Browse", GUILayout.Width(60)))
			{
				newPath = EditorUtility.OpenFilePanel("Select Application", path, "");
			}
			if (newPath != path)
			{
				OpenWithMenuItem.SetPath(type, newPath);
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}
