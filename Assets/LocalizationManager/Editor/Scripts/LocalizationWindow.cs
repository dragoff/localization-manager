#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Localization
{
    public class LocalizationWindow : EditorWindow
    {
        
        public static List<LocalizationElement> ElementsList = new List<LocalizationElement>();
        private List<LocalizationElement> tempElementsList;
        private static string path;
        
        private static Vector2 scrollRect;
        private SearchField searchField;
        
        //TODO
        private static bool managerSelected = true;

        private static int groupSelected = 0;
        private static string grpoup_Name;
        private static bool initialized = false;
        private static bool readySteady = false;
        public static void Init()
        {
            //TODO
            if (initialized)
                return;

            LocalizationWindow win = ScriptableObject.CreateInstance<LocalizationWindow>();

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
            managerSelected = true;
            Load();
        }
        
        private static void Save() => LocalizationController.SaveData(path, ElementsList);
        private static void Load() =>ElementsList = LocalizationController.LoadManagerData(path);

        private void OpenLang()
        {
            Save();
            LocalizationLangWindow win = ScriptableObject.CreateInstance<LocalizationLangWindow>();
            win.minSize = new Vector2(601, 200);
            win.maxSize = new Vector2(601, 1000);
            win.name = "Localization Language";
            win.GetLanguagePath();
            win.Show();
            
        }
        private static void CreateLang()
        {
            Save();
            LocalizationLangWindow win = ScriptableObject.CreateInstance<LocalizationLangWindow>();
            win.minSize = new Vector2(601, 200);
            win.maxSize = new Vector2(601, 1000);
            win.name = "Localization Language";
            win.CreateLanguagePath();
            win.Show();
        }
        private void OnEnable()
        {
            if (searchField == null) searchField = new SearchField();
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
                        "txt");
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
                        "LocalizationManager", "txt");
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
            if (managerSelected)
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

            string lang = "";
            
                managerSelected = true;
                lang = "Language Manager";
            

            GUILayout.BeginHorizontal("Box");
            LocLayout.Label("Selected: " + lang);
            
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
            groupSelected = EditorGUILayout.Popup("Group:", groupSelected,
                LocalizationController.LOCALIZATION_GROUPS.ToArray(), GUILayout.MaxWidth(300), GUILayout.MinWidth(50));
            EditorGUIUtility.labelWidth += 70;
            if (managerSelected)
            {
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
                            LocalizationController.LOCALIZATION_GROUPS.RemoveAt(LocalizationController.LOCALIZATION_GROUPS.Count - 1);
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
            }

            GUILayout.EndHorizontal();

            #endregion

            LocLayout.Space();

            #region SECTION__LOCALIZATION_ARRAY

            searchField.OnGUI();

            GUILayout.BeginHorizontal();
            LocLayout.Label("Localization Keys & Translations");
            if (managerSelected && GUILayout.Button("+"))
                ElementsList.Add(new LocalizationElement() {Group = groupSelected});
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
                            if (locA.Key.IndexOf(searchField.SearchString,StringComparison.OrdinalIgnoreCase)>=0)
                                tempElementsList.Add(locA);
                        }
                    }
                

                for (var i = 0; i < tempElementsList.Count; i++)
                {
                    LocalizationElement locEl = tempElementsList[i];
                    if (locEl.Group >= LocalizationController.LOCALIZATION_GROUPS.Count)
                    {
                        locEl.Group = 0;
                        break;
                    }

                    if (LocalizationController.LOCALIZATION_GROUPS[locEl.Group] !=
                        LocalizationController.LOCALIZATION_GROUPS[groupSelected])
                        continue;

                    EditorGUIUtility.labelWidth -= 100;
                    EditorGUILayout.BeginHorizontal("Box");
                    if (!managerSelected)
                    {
                        EditorGUILayout.LabelField(locEl.Key, GUILayout.Width(100));

                        EditorGUILayout.LabelField("Text:", GUILayout.Width(100));
                        locEl.Text = EditorGUILayout.TextField(locEl.Text, GUILayout.MaxWidth(300),
                            GUILayout.MinWidth(100));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Key:", GUILayout.Width(45));

                        locEl.Key = EditorGUILayout.TextField(locEl.Key, GUILayout.MaxWidth(100), GUILayout.MinWidth(30));
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
#endif