using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Localization
{
    public class LocalizationLangWindow : EditorWindow
    {
        public string Language;

        private string path;

        private List<LocalizationElement> elementsList;

        private Vector2 scrollRect;

        private int selectedGroup = 0;
        private bool selectAllKeys = false;

        private LocLayout.SearchField searchField;

        public void GetLanguagePath(string filepath = null)
        {
            path = EditorUtility.OpenFilePanel("Select Language File Path",
                string.IsNullOrEmpty(filepath) ? Application.dataPath : filepath, "txt");
            if(string.IsNullOrEmpty(path)) return;
            Language = Path.GetFileNameWithoutExtension(path);

            Load();
            ShowWindow();
        }

        public void CreateLanguagePath()
        {
            path = EditorUtility.SaveFilePanel("Create Language File", Application.dataPath, "English", "txt");
            if (string.IsNullOrEmpty(path))
                return;
            File.Create(path).Dispose();

            Language = Path.GetFileNameWithoutExtension(path);

            Save();
            Load();
            ShowWindow();
        }

        #region PRIVATE METHODS

        private void ShowWindow()
        {
            minSize = new Vector2(601, 200);
            maxSize = new Vector2(2000, 1000);
            name = "Localization Language";
            Show();
        }
        private void Save() => LocalizationController.SaveData(path, elementsList);
        private void Load() => elementsList = LocalizationController.LoadLangData(path);

        #endregion
        private void OnEnable()
        {
            if (searchField == null) searchField = new LocLayout.SearchField();
        }

        private void OnDestroy()
        {
            LocalizationController.SaveData(path, elementsList);
        }

        private void OnGUI()
        {
            LocLayout.Label($"Language: {Language}");

            LocLayout.Space();

            #region SECTION__UPPER

            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Save"))
                Save();

            LocLayout.Space();

            GUILayout.EndHorizontal();

            LocLayout.Space(5);

            #endregion

            LocLayout.Space(5);

            GUILayout.BeginVertical("Box");

            #region SECTION_GROUPS

            GUILayout.BeginHorizontal("Box");

            EditorGUIUtility.labelWidth -= 70;
            selectAllKeys = EditorGUILayout.ToggleLeft("Show all keys", selectAllKeys);
            if (!selectAllKeys)
                selectedGroup = EditorGUILayout.Popup("Group:", selectedGroup,
                    LocalizationController.LOCALIZATION_GROUPS.ToArray(), GUILayout.MaxWidth(300),
                    GUILayout.MinWidth(50));
            else
                EditorGUILayout.LabelField("Group: BLOCKED", GUILayout.MaxWidth(300),
                    GUILayout.MinWidth(50));

            EditorGUIUtility.labelWidth += 70;

            GUILayout.EndHorizontal();

            #endregion

            LocLayout.Space();

            #region SECTION__LOCALIZATION_ARRAY

            searchField.OnGUI();
            List<LocalizationElement> tempLocalizationArray = null;

            if (elementsList?.Count == 0)
                GUILayout.Label("- - Empty - -", EditorStyles.boldLabel);
            else
            {
                scrollRect = EditorGUILayout.BeginScrollView(scrollRect);


                if (string.IsNullOrEmpty(searchField.SearchString))
                    tempLocalizationArray = elementsList;
                else if (searchField.IsChanged)

                {
                    tempLocalizationArray = new List<LocalizationElement>();
                    foreach (LocalizationElement locA in elementsList)
                    {
                        if (locA.Key.IndexOf(searchField.SearchString, StringComparison.OrdinalIgnoreCase) >= 0)
                            tempLocalizationArray.Add(locA);
                    }
                }


                if (tempLocalizationArray != null)
                {
                    for (var i = 0; i < tempLocalizationArray.Count; i++)
                    {
                        LocalizationElement locEl = tempLocalizationArray[i];
                        if (locEl.Group >= LocalizationController.LOCALIZATION_GROUPS.Count)
                        {
                            locEl.Group = 0;
                            break;
                        }

                        if (!selectAllKeys && LocalizationController.LOCALIZATION_GROUPS[locEl.Group] !=
                            LocalizationController.LOCALIZATION_GROUPS[selectedGroup])
                            continue;

                        EditorGUIUtility.labelWidth -= 100;
                        EditorGUILayout.BeginHorizontal("Box");

                        EditorGUILayout.LabelField(locEl.Key, GUILayout.MinWidth(100), GUILayout.MaxWidth(200));

                        EditorGUILayout.LabelField("Text:", GUILayout.Width(50));
                        locEl.Text = EditorGUILayout.TextField(locEl.Text,
                            GUILayout.MinWidth(100));


                        EditorGUILayout.EndHorizontal();
                        EditorGUIUtility.labelWidth += 100;
                    }

                }
                else
                {
                    GUILayout.Label("- - Empty - -", EditorStyles.boldLabel);
                }

                EditorGUILayout.EndScrollView();
            }

            GUILayout.EndVertical();

            #endregion

            EditorGUI.indentLevel--;
        }
    }
}