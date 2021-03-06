﻿using System.IO;

using UnityEngine;
using UnityEditor;

namespace Toolbox.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(DirectoryAttribute))]
    public class DirectoryAttributeDrawer : ToolboxNativePropertyDrawer
    {
        private static bool IsPathValid(string propertyPath, string assetRelativePath)
        {
            var projectRelativePath = Application.dataPath + "/";

            if (!string.IsNullOrEmpty(assetRelativePath))
            {
                projectRelativePath += assetRelativePath + "/";
            }

            return Directory.Exists(projectRelativePath + propertyPath);
        }


        protected override float GetPropertyHeightSafe(SerializedProperty property, GUIContent label)
        {
            //validate property type and serialized path
            if (IsPathValid(property.stringValue, Attribute.RelativePath))
            {
                return base.GetPropertyHeightSafe(property, label);
            }

            //return adjusted height
            return base.GetPropertyHeightSafe(property, label) + Style.boxHeight + Style.spacing * 2;
        }

        protected override void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
        {
            //current stored path validation
            if (!IsPathValid(property.stringValue, Attribute.RelativePath))
            {
                var helpBoxRect = new Rect(position.x, 
                                           position.y, 
                                           position.width, Style.boxHeight);
                EditorGUI.HelpBox(helpBoxRect, "Provided directory does not exist.", MessageType.Warning);
                position.y += Style.boxHeight + Style.spacing + Style.spacing;
            }

            position.height = Style.rowHeight;
            position.width -= Style.directoryButtonWidth + Style.spacing;
            //draw standard string property field
            EditorGUI.PropertyField(position, property, label);
            position.x = position.xMax + Style.spacing;
            position.width = Style.directoryButtonWidth;

            //create additional pick directory button
            if (GUI.Button(position, Style.directoryButtonLabel, Style.directoryButtonStyle))
            {
                var assetRelativePath = Attribute.RelativePath;
                var projectRelativePath = Application.dataPath + "/";

                if (!string.IsNullOrEmpty(assetRelativePath))
                {
                    projectRelativePath += assetRelativePath + "/";
                }

                property.serializedObject.Update();
                property.stringValue = EditorUtility.OpenFolderPanel("Pick directory", "Assets/" + assetRelativePath, "").Replace(projectRelativePath, "");
                property.serializedObject.ApplyModifiedProperties();

                //NOTE: we have to exit GUI since EditorUtility methods will break layouting system
                GUIUtility.ExitGUI();
            }
        }


        public override bool IsPropertyValid(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.String;
        }


        private DirectoryAttribute Attribute => attribute as DirectoryAttribute;


        private static class Style
        {
            internal static readonly float rowHeight = EditorGUIUtility.singleLineHeight;
            internal static readonly float boxHeight = EditorGUIUtility.singleLineHeight * 2.5f;
            internal static readonly float spacing = EditorGUIUtility.standardVerticalSpacing;
            internal static readonly float directoryButtonWidth = 30.0f;

            internal static readonly GUIContent directoryButtonLabel;

            internal static readonly GUIStyle directoryButtonStyle;
 
            static Style()
            {
                directoryButtonStyle = new GUIStyle(EditorStyles.miniButton);
                directoryButtonLabel = new GUIContent(EditorGUIUtility.FindTexture("Folder Icon"), "Pick directory");
            }
        }
    }
}