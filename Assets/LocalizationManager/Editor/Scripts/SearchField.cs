#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Localization
{
    [Serializable]
    public class SearchField
    {
        public string SearchString { get; private set; } = "";
        public string PreviousSearch { get; private set; } = "";
        
        public bool IsChanged => SearchString != PreviousSearch;

        UnityEditor.IMGUI.Controls.SearchField searchField;
        public void OnGUI() => Draw();

        void Draw()
        {
            var rect = GUILayoutUtility.GetRect(1, 1, 18, 18, GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            DoSearchField(rect);
            GUILayout.EndHorizontal();
            rect.y += 18;
        }

        void DoSearchField(Rect rect)
        {
            if (searchField == null)
            {
                searchField = new UnityEditor.IMGUI.Controls.SearchField();
            }

            PreviousSearch = SearchString;
            SearchString = searchField.OnGUI(rect, SearchString);

            if (HasSearchbarFocused())
            {
                RepaintFocusedWindow();
            }
        }


        bool HasSearchbarFocused() => GUIUtility.keyboardControl == searchField.searchFieldControlID;

        static void RepaintFocusedWindow()
        {
            if (EditorWindow.focusedWindow != null)
            {
                EditorWindow.focusedWindow.Repaint();
            }
        }
    }
}
#endif