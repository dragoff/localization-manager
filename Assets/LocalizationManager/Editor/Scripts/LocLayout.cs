using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Localization
{
    public static class LocLayout
    {
        public static readonly GUISkin Skin = AssetDatabase.LoadAssetAtPath<GUISkin>(
            "Assets/LocalizationManager/Editor/Graphics/Skin.guiskin");

        public static readonly Color BACK_COLOR = new Color(0.8f, 0.8f, 0.86f, 1f);
        public static readonly Color EMPTY_COLOR = new Color(1f, 0.7f, 0.7f, 1f);
        public static readonly Color ACTIVE_COLOR = new Color(0.5f, 0.68f, 1f, 1f);

        public static readonly Color FONT_BOLD_COLOR = new Color(0f, 0f, 0f, 1f);
        public static readonly Color FONT_REGULAR_COLOR = new Color(0f, 0f, 0f, 1f);


        public static readonly GUIStyle BACKGROUND = Skin.customStyles.First(x => x.name.Equals("Background"));

        public static readonly GUIStyle BAR_OPENED = Skin.customStyles.First(x => x.name.Equals("BarOpen"));

        public static readonly GUIStyle BAR_CLOSED = Skin.customStyles.First(x => x.name.Equals("BarClose"));

        public static readonly GUIStyle BUTTON = Skin.customStyles.First(x => x.name.Equals("Button"));
        
        public static readonly GUIStyle LABEL = Skin.customStyles.First(x => x.name.Equals("Label"));

        public static readonly GUIStyle HEADER = Skin.customStyles.First(x => x.name.Equals("Header"));

        public static readonly GUIStyle PLUS = Skin.customStyles.First(x => x.name.Equals("IconButtonPlus"));
        public static readonly GUIStyle MINUS = Skin.customStyles.First(x => x.name.Equals("IconButtonMinus"));

        public static readonly GUIStyle TOGGLE_TRUE = Skin.customStyles.First(x => x.name.Equals("ToggleEnabled"));
        public static readonly GUIStyle TOGGLE_FALSE = Skin.customStyles.First(x => x.name.Equals("ToggleDisabled"));

        public static void Property(SerializedProperty p, string text = "", string toolTip = "",
            bool includeChilds = false, bool isColor = false, bool isNullColor = false)
        {
            var color = isNullColor && p.objectReferenceValue == null ? EMPTY_COLOR : BACK_COLOR;
            if (isColor)
            {
                var height = 20;
                Background(color, height);
                GUILayout.Space(-height);
            }

            EditorGUILayout.PropertyField(p, new GUIContent(text, toolTip), includeChilds);
            Space(2);
        }

        public static void Space(float space = 10) => GUILayout.Space(space);
        public static void Error(string text) => EditorUtility.DisplayDialog("Error", text, "OK");
        public static void Message(string text) => EditorUtility.DisplayDialog("Info", text, "OK");

        public static void Label(string text,string tooltip = "", params GUILayoutOption[] options)
        {
            GUIStyle style = LABEL;
            GUILayout.Label(new GUIContent(text,tooltip), style, options);
        }

        public static void List(SerializedProperty p, string header = "")
        {
            GUILayout.BeginVertical();
            {
                int backgroundHeight = 2;
                if (p.arraySize > 0)
                    for (int i = 0; i < p.arraySize; i++)
                    {
                        backgroundHeight += (int) EditorGUI.GetPropertyHeight(p.GetArrayElementAtIndex(i));
                        backgroundHeight += 3;
                    }

                backgroundHeight += 16;
                var backColor = p.arraySize != 0 ? BACK_COLOR : EMPTY_COLOR;
                Background(backColor, backgroundHeight);
                Space(-backgroundHeight);

                GUILayout.BeginHorizontal();
                {
                    Label(header);
                    GUILayout.Space(2);
                    GUILayout.FlexibleSpace();
                    if (IconButton(new Color(0, 0.5f, 0), Icons.PLUS, 16))
                    {
                        p.InsertArrayElementAtIndex(p.arraySize);
                        SerializedProperty element = p.GetArrayElementAtIndex(p.arraySize - 1);
                        switch (element.propertyType)
                        {
                            case SerializedPropertyType.ObjectReference:
                                element.objectReferenceValue = default(Object);
                                break;
                        }
                    }
                }
                GUILayout.EndHorizontal();

                if (p.arraySize != 0)
                {
                    for (int i = 0; i < p.arraySize; i++)
                    {
                        SerializedProperty childProperty = p.GetArrayElementAtIndex(i);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.BeginHorizontal(GUILayout.Width(10));
                            Label(i.ToString());
                            GUILayout.EndHorizontal();

                            EditorGUILayout.PropertyField(childProperty, GUIContent.none, false,
                                GUILayout.ExpandWidth(true));
                            Space();
                            if (IconButton(Color.red, Icons.MINUS, 16))
                                p.DeleteArrayElementAtIndex(i);
                        }
                        GUILayout.EndHorizontal();
                        Space(3);
                    }
                }
            }
            GUILayout.EndVertical();
        }

        public static bool ToggleLabel(SerializedProperty p, string text,string tooltip = "", bool isColor = true, float height = 11)
        {
            bool result = p.boolValue;
            int backgroundHeight = (int) height + 5;
            var color = result ? ACTIVE_COLOR : BACK_COLOR;
            var toggleStyle = result ? TOGGLE_TRUE : TOGGLE_FALSE;

            GUIStyle labelStyle = LABEL;
            GUIContent content = new GUIContent(text);
            Vector2 labelSize = labelStyle.CalcSize(content);
            int totalWidth = (int) (toggleStyle.fixedWidth + 14 + labelSize.x);

            GUILayout.BeginVertical(GUILayout.Height(backgroundHeight), GUILayout.ExpandWidth(true));
            {
                if (isColor)
                {
                    Background(color, backgroundHeight, totalWidth);
                    GUILayout.Space(-backgroundHeight);
                }

                GUILayout.BeginHorizontal(GUILayout.Height(height), GUILayout.ExpandWidth(true));
                {
                    result = Toggle(p, toggleStyle, color, height);
                    Label(text,tooltip);
                    Space();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            return result;
        }

        public static bool Toggle(SerializedProperty p, GUIStyle toggleStyle, Color color, float height)
        {
            Color initialColor = GUI.color;
            GUI.color = color;
            bool value = p.boolValue;
            EditorGUI.BeginChangeCheck();
            value = GUILayout.Toggle(value, GUIContent.none, toggleStyle);
            if (EditorGUI.EndChangeCheck())
            {
                p.boolValue = value;
                GUIUtility.keyboardControl = 0;
            }

            GUI.color = initialColor;
            return value;
        }

        public static bool Bar(string text,ref bool isActive, bool isEmptyColor = false,int height = 30)
        {
            Color temp = GUI.color;
            GUI.color = isActive ? ACTIVE_COLOR : isEmptyColor ? EMPTY_COLOR : BACK_COLOR;
            var style = isActive ?  new GUIStyle(BAR_OPENED) : new GUIStyle(BAR_CLOSED);
            if (height > 0) style.fixedHeight = height;
            if (GUILayout.Button(text, style, GUILayout.ExpandWidth(true)))
                isActive = !isActive;
            GUI.color = temp;
            return isActive;
        }

        public static bool Button(string text,ref bool isActive, int width = -1, int height = 20, Color color=default)
        {
            Color tColor = GUI.color;
            GUI.color = color!= default ? color : ACTIVE_COLOR;

            GUILayout.BeginVertical(GUILayout.Width(width));

            var resizedStyle = new GUIStyle(BUTTON);
            if (width != -1)
                resizedStyle.fixedWidth = width;
            if (height != -1)
                resizedStyle.fixedHeight = height;
            if(GUILayout.Button(text, BUTTON))
                isActive = !isActive;

            GUILayout.EndVertical();
            GUI.color = tColor;
            return isActive;
        }
        public static bool Button(string text, int width = -1, int height = 20,Color color=default)
        {
            Color tColor = GUI.color;
            GUI.color = color!= default ? color : ACTIVE_COLOR;

            GUILayout.BeginVertical(GUILayout.Width(width));

            var resizedStyle = new GUIStyle(BUTTON);
            if (width != -1)
                resizedStyle.fixedWidth = width;
            if (height != -1)
                resizedStyle.fixedHeight = height;
            bool value = (GUILayout.Button(text, BUTTON));

            GUILayout.EndVertical();
            GUI.color = tColor;
            return value;
        }
        public static void Background(Color color, int height, int width = 0)
        {
            Color temp = GUI.color;
            GUI.color = color;
            GUILayout.Label(GUIContent.none, BACKGROUND, GUILayout.Height(height),
                width != 0 ? GUILayout.Width(width) : GUILayout.ExpandWidth(true));
            GUI.color = temp;
        }

        public static bool IconButton(Color iconColor, Icons icon, int height = -1)
        {
            GUIStyle style = null;
            switch (icon)
            {
                case Icons.PLUS:
                    style = PLUS;
                    break;
                case Icons.MINUS:
                    style = MINUS;
                    break;
            }

            Color color = GUI.color;

            GUI.color = iconColor;
            bool buttonClicked = GUILayout.Button(GUIContent.none, style);
            GUI.color = color;
            if (!buttonClicked) return false;
            GUIUtility.keyboardControl = 0;
            Event.current.Use();
            return true;
        }

        public enum Icons
        {
            PLUS,
            MINUS
        }
    }
}