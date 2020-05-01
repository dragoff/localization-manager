using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Localization
{
    public class LocalizationWindow : EditorWindow
    {
        public static List<LocalizationElement> ElementsList = new List<LocalizationElement>();
        private List<LocalizationElement> tempElementsList;
        private static string path;

        private static Vector2 scrollRect;
        private LocLayout.SearchField searchField;


        private static int groupSelected = 0;
        private static bool selectAllKeys;
        private static string grpoup_Name;
        private static bool initialized = false;
        private static bool readySteady = false;

        public static void Init()
        {
            //TODO
            if (initialized)
                return;
            
            CreateInstance<LocalizationWindow>();
            initialized = true;

            ElementsList.Clear();
            GetManagerPath();
        }

        [MenuItem("Tools/Localization Manager", false, 20)]
        public static void InitWindow()
        {
            LocalizationWindow win = ScriptableObject.CreateInstance<LocalizationWindow>();
            win.minSize = new Vector2(400, 200);
            win.maxSize = new Vector2(601, 1000);
            win.name = "Localization Manager";
            win.Show();

            ElementsList.Clear();

            GetManagerPath();
        }

        private static void GetManagerPath()
        {
            path = PlayerPrefs.GetString(LocalizationController.PrefsKey);
            if (string.IsNullOrEmpty(path))
            {
                readySteady = false;
                return;
            }

            readySteady = true;
            Load();
        }

        private static void Save() => LocalizationController.SaveData(path, ElementsList);
        private static void Load() => ElementsList = LocalizationController.LoadManagerData(path);

        private void OpenLang()
        {
            Save();
            CreateInstance<LocalizationLangWindow>().GetLanguagePath();
        }

        private static void CreateLang()
        {
            Save();
            CreateInstance<LocalizationLangWindow>().CreateLanguagePath();
        }

        private void OnEnable()
        {
            if (searchField == null) searchField = new LocLayout.SearchField();
        }


        private void OnGUI()
        {
            EditorGUI.indentLevel++;
            LocLayout.Space();

            LocLayout.Label("Localization Manager");

            LocLayout.Space();

            //---Not ready:
            if (!readySteady)
            {
                GUILayout.BeginVertical("Box");

                EditorGUILayout.HelpBox(
                    "There is no Localization Manager file. To set up keys structure and language system, select or create a Localization Manager file.",
                    MessageType.Info);
                GUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("Select Localization Manager file", GUILayout.Width(250)))
                {
                    string f = EditorUtility.OpenFilePanel("Select Localization Manager file", Application.dataPath,
                        "loc");
                    if (string.IsNullOrEmpty(f))
                        return;
                    path = f;
                    PlayerPrefs.SetString(LocalizationController.PrefsKey, path);
                    LocLayout.Message("Great! The Localization Manager is now ready.");
                    GetManagerPath();
                    return;
                }

                if (GUILayout.Button("Create Localization Manager file"))
                {
                    string f = EditorUtility.SaveFilePanel("Create Localization Manager file", Application.dataPath,
                        "LocalizationManager", "loc");
                    if (string.IsNullOrEmpty(f))
                        return;
                    File.Create(f).Dispose();
                    path = f;
                    PlayerPrefs.SetString(LocalizationController.PrefsKey, path);
                    LocLayout.Message("Great! The Localization Manager is now ready.");
                    Save();
                    GetManagerPath();
                    return;
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                return;
            }
            //---Else...:

            #region SECTION__UPPER

            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Save System"))
                Save();
            LocLayout.Space();
            if (GUILayout.Button("Reset Manager Path"))
            {
                if (EditorUtility.DisplayDialog("Question",
                    "You are about to reset the Localization Manager path... Are you sure?", "Yes", "No"))
                {
                    PlayerPrefs.DeleteKey(LocalizationController.PrefsKey);
                    this.Close();
                }
            }

            GUILayout.EndHorizontal();

            LocLayout.Space(5);


            GUILayout.BeginHorizontal("Box");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Language"))
            {
                OpenLang();
            }

            if (GUILayout.Button("Create Language"))
            {
                CreateLang();
            }

            GUILayout.EndHorizontal();

            #endregion

            LocLayout.Space(5);

            GUILayout.BeginVertical("Box");

            #region SECTION_GROUPS

            GUILayout.BeginHorizontal("Box");
            EditorGUIUtility.labelWidth -= 70;
            selectAllKeys = EditorGUILayout.ToggleLeft("Show all keys", selectAllKeys);
            if (!selectAllKeys)
                groupSelected = EditorGUILayout.Popup("Group:", groupSelected,
                    LocalizationController.LOCALIZATION_GROUPS.ToArray(), GUILayout.MaxWidth(300),
                    GUILayout.MinWidth(50));
            else
                EditorGUILayout.LabelField("Group: BLOCKED", GUILayout.MaxWidth(300),
                    GUILayout.MinWidth(50));

            EditorGUIUtility.labelWidth += 70;

            LocLayout.Space();
            grpoup_Name = EditorGUILayout.TextField(grpoup_Name);
            if (GUILayout.Button("+ Group"))
            {
                if (string.IsNullOrEmpty(grpoup_Name))
                {
                    LocLayout.Error("Please fill the required field! [Group Name]");
                    return;
                }

                LocalizationController.LOCALIZATION_GROUPS.Add(grpoup_Name);
                grpoup_Name = "";
                GUI.FocusControl("Set");
                return;
            }

            if (GUILayout.Button("- Group") && LocalizationController.LOCALIZATION_GROUPS.Count > 1)
            {
                if (EditorUtility.DisplayDialog("Question", "You are going to remove category... Are you sure?",
                    "Yes", "No"))
                {
                    if (string.IsNullOrEmpty(grpoup_Name))
                    {
                        LocalizationController.LOCALIZATION_GROUPS.RemoveAt(
                            LocalizationController.LOCALIZATION_GROUPS.Count - 1);
                        groupSelected = 0;
                    }
                    else
                    {
                        int cc = 0;
                        bool notfound = true;
                        foreach (string cat in LocalizationController.LOCALIZATION_GROUPS)
                        {
                            if (grpoup_Name == cat)
                            {
                                LocalizationController.LOCALIZATION_GROUPS.RemoveAt(cc);
                                groupSelected = 0;
                                notfound = false;
                                break;
                            }

                            cc++;
                        }

                        if (notfound)
                            LocLayout.Error("The category couldn't be found.");
                        grpoup_Name = "";
                    }

                    return;
                }
            }


            GUILayout.EndHorizontal();

            #endregion

            LocLayout.Space();

            #region SECTION__LOCALIZATION_ARRAY

            searchField.OnGUI();

            GUILayout.BeginHorizontal();
            LocLayout.Label("Localization Keys & Translations");
            if (GUILayout.Button("+"))
                ElementsList.Insert(0,new LocalizationElement() {Group = groupSelected});
            GUILayout.EndHorizontal();

            if (ElementsList.Count == 0)
                GUILayout.Label("- - Empty - -", EditorStyles.boldLabel);
            else
            {
                scrollRect = EditorGUILayout.BeginScrollView(scrollRect);


                if (string.IsNullOrEmpty(searchField.SearchString))
                    tempElementsList = ElementsList;
                else if (searchField.IsChanged)
                {
                    tempElementsList = new List<LocalizationElement>();
                    foreach (LocalizationElement locA in ElementsList)
                    {
                        if (locA.Key.IndexOf(searchField.SearchString, StringComparison.OrdinalIgnoreCase) >= 0)
                            tempElementsList.Add(locA);
                    }
                }
                foreach (var locEl in tempElementsList)
                {
                    if (locEl.Group >= LocalizationController.LOCALIZATION_GROUPS.Count)
                    {
                        locEl.Group = 0;
                        break;
                    }

                    if (!selectAllKeys && LocalizationController.LOCALIZATION_GROUPS[locEl.Group] !=
                        LocalizationController.LOCALIZATION_GROUPS[groupSelected])
                        continue;

                    EditorGUIUtility.labelWidth -= 100;
                    EditorGUILayout.BeginHorizontal("Box");
                    {
                        EditorGUILayout.LabelField("Key:", GUILayout.Width(45));

                        locEl.Key = EditorGUILayout.TextField(locEl.Key, GUILayout.MaxWidth(150),
                            GUILayout.MinWidth(100));
                        EditorGUILayout.LabelField("Group:", GUILayout.Width(75));
                        locEl.Group = EditorGUILayout.Popup(locEl.Group,
                            LocalizationController.LOCALIZATION_GROUPS.ToArray());
                        if (GUILayout.Button("-", GUILayout.Width(30)))
                        {
                            ElementsList.Remove(locEl);
                            return;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUIUtility.labelWidth += 100;
                }

                EditorGUILayout.EndScrollView();
            }

            GUILayout.EndVertical();

            #endregion

            EditorGUI.indentLevel--;
        }
    }
}