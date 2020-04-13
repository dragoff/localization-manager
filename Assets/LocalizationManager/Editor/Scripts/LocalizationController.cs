using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Localization
{
    public static class LocalizationController
    {
        public static string PrefsKey = "LOCATION_MANAGER_LocManagPath";

        public static string FILE_HEADER = "Location_Manager_Main_System";

        public static List<string> LOCALIZATION_GROUPS = new List<string>();


        public static void SaveData(string filepath, List<LocalizationElement> dataList)
        {
            if (!File.Exists(filepath))
            {
                LocLayout.Error($"The file path {filepath} doesn't exist!");
                return;
            }

            string data = "";
            void AddLine(string line) => data += line + "\n";
            AddLine(FILE_HEADER);
            if (dataList != null)
                foreach (string group in LOCALIZATION_GROUPS)
                {
                    if (group != "Default")
                        AddLine(LocalizationManager.DIVISION_CHAR + group);
                    foreach (var t in dataList)
                    {
                        var element = t;
                        if (group != LOCALIZATION_GROUPS[element.Group]) continue;
                        if (string.IsNullOrEmpty(element.Key)) continue;

                        element.Key = element.Key.Replace(LocalizationManager.DIVISION_CHAR, "");
                        AddLine(element.Key + LocalizationManager.DIVISION_CHAR + element.Text);
                    }
                }

            File.WriteAllText(filepath, data);
        }

        public static List<LocalizationElement> LoadManagerData(string filepath)
        {
            var data = ReadFile(filepath);
            if (data?.Length <= 1) return null;

            //PARSING

            LOCALIZATION_GROUPS.Clear();
            LOCALIZATION_GROUPS.Add("Default");

            for (int i = 1; i < data.Length; i++)
            {
                var cur = data[i];
                if (cur.StartsWith(LocalizationManager.DIVISION_CHAR))
                    LOCALIZATION_GROUPS.Add(cur.Replace("=", ""));
            }

            return CreateLocArray(data);
        }

        public static List<LocalizationElement> LoadLangData(string filepath)
        {
            var data = ReadFile(filepath);
            if (data?.Length < 1) return null;

            var list = new List<LocalizationElement>();
            var managerList = LocalizationWindow.ElementsList;
            var langList = CreateLocArray(data);
            for (var i = 0; i < managerList.Count; i++)
            {
                var langText =(langList.Count > i && managerList[i].Key.Equals(langList[i].Key))
                    ? langList[i].Text : "";
                list.Add(new LocalizationElement(managerList[i].Key,
                    langText,
                    managerList[i].Group));
            }

            return list;
        }

        private static string[] ReadFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                LocLayout.Error($"The file path {filepath} doesn't exist!");
                PlayerPrefs.DeleteKey(PrefsKey);
                return null;
            }

            var data = File.ReadAllLines(filepath);

            if (data.Length <= 0 || data[0] != FILE_HEADER)
            {
                LocLayout.Error($"The file path {filepath} is corrupted!");
                PlayerPrefs.DeleteKey(PrefsKey);
                return null;
            }

            return data;
        }

        private static List<LocalizationElement> CreateLocArray(string[] data)
        {
            var list = new List<LocalizationElement>();
            var group = 0;
            for (var i = 1; i < data.Length; i++)
            {
                var cur = data[i];
                if (cur.StartsWith(LocalizationManager.DIVISION_CHAR))
                {
                    group++;
                    continue;
                }

                if (cur.Length <= 1)
                    continue;
                if (cur.IndexOf("=", StringComparison.Ordinal) <= 1)
                    continue;
                var key = cur.Substring(0, cur.IndexOf("=", StringComparison.Ordinal));
                var text = cur.Substring(key.Length + 1, cur.Length - key.Length - 1);
                list.Add(new LocalizationElement(key, text, group));
            }

            return list;
        }
    }

    public class LocalizationElement
    {
        public string Key;
        public string Text;
        public int Group;
        public LocalizationElement(string key="", string text="", int group=0) =>
            (this.Key, this.Text, this.Group) = (key, text, group);
    }
}