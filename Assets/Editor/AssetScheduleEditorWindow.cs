using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class AssetScheduleEditorWindow : EditorWindow
{
    private AssetSchedule tempSchedule;
    private Vector2 scrollPos;
    private string exportPath = "Assets/AssetSchedule.json";

    // Dummy editing storage
    private Dictionary<string, string> keyEdits = new();
    private Dictionary<string, int> startYear = new();
    private Dictionary<string, int> startMonth = new();
    private Dictionary<string, int> startDay = new();

    private Dictionary<string, int> endYear = new();
    private Dictionary<string, int> endMonth = new();
    private Dictionary<string, int> endDay = new();

    [MenuItem("Tools/Asset Schedule Editor")]
    public static void ShowWindow()
    {
        GetWindow<AssetScheduleEditorWindow>("Asset Schedule Editor");
    }

    private void OnEnable()
    {
        tempSchedule = new AssetSchedule();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Asset Schedule Editor", EditorStyles.boldLabel);

        // Export path with file explorer
        EditorGUILayout.BeginHorizontal();
        exportPath = EditorGUILayout.TextField("Export JSON Path", exportPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string folder = Path.GetDirectoryName(exportPath);
            string filename = Path.GetFileName(exportPath);
            string selectedPath = EditorUtility.SaveFilePanel("Select Export Path", folder, filename, "json");
            if (!string.IsNullOrEmpty(selectedPath))
                exportPath = selectedPath;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var key in tempSchedule.schedulePeriods.Keys.ToList())
        {
            var period = tempSchedule.schedulePeriods[key];

            // Initialize dummy fields
            if (!keyEdits.ContainsKey(key)) keyEdits[key] = period.Key;

            if (!startYear.ContainsKey(key)) startYear[key] = period.StartTime.Year;
            if (!startMonth.ContainsKey(key)) startMonth[key] = period.StartTime.Month;
            if (!startDay.ContainsKey(key)) startDay[key] = period.StartTime.Day;

            if (!endYear.ContainsKey(key)) endYear[key] = period.EndTime.Year;
            if (!endMonth.ContainsKey(key)) endMonth[key] = period.EndTime.Month;
            if (!endDay.ContainsKey(key)) endDay[key] = period.EndTime.Day;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Period: {period.Key}", EditorStyles.boldLabel);

            keyEdits[key] = EditorGUILayout.TextField("Key", keyEdits[key]);

            // --- Start Date Inline: Day Slider, Month & Year Dropdowns ---
            EditorGUILayout.LabelField("Start Date (DD/MM/YYYY)");
            EditorGUILayout.BeginHorizontal();

            // Day slider
            startDay[key] = EditorGUILayout.IntSlider(startDay[key], 1, DateTime.DaysInMonth(startYear[key], startMonth[key]), GUILayout.Width(120));

            // Month dropdown
            string[] months = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;
            startMonth[key] = EditorGUILayout.Popup(startMonth[key] - 1, months, GUILayout.Width(120)) + 1;

            // Year dropdown (current + 4)
            int[] years = new int[5];
            int currentYear = DateTime.Now.Year;
            for (int i = 0; i < 5; i++) years[i] = currentYear + i;
            int yearIndex = Array.IndexOf(years, startYear[key]);
            startYear[key] = years[EditorGUILayout.Popup(yearIndex, Array.ConvertAll(years, y => y.ToString()), GUILayout.Width(60))];

            EditorGUILayout.EndHorizontal();

            // --- End Date Inline ---
            EditorGUILayout.LabelField("End Date (DD/MM/YYYY)");
            EditorGUILayout.BeginHorizontal();

            endDay[key] = EditorGUILayout.IntSlider(endDay[key], 1, DateTime.DaysInMonth(endYear[key], endMonth[key]), GUILayout.Width(120));
            endMonth[key] = EditorGUILayout.Popup(endMonth[key] - 1, months, GUILayout.Width(120)) + 1;
            int endYearIndex = Array.IndexOf(years, endYear[key]);
            endYear[key] = years[EditorGUILayout.Popup(endYearIndex, Array.ConvertAll(years, y => y.ToString()), GUILayout.Width(60))];

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Changes"))
            {
                try
                {
                    DateTime newStart = new DateTime(startYear[key], startMonth[key], startDay[key], 0, 0, 0);
                    DateTime newEnd = new DateTime(endYear[key], endMonth[key], endDay[key], 23, 59, 59);
                    string newKey = keyEdits[key];

                    tempSchedule.schedulePeriods.Remove(key);
                    tempSchedule.schedulePeriods[newKey] = new AssetSchedulePeriod(newKey, newStart, newEnd);

                    keyEdits.Remove(key);
                    startYear.Remove(key);
                    startMonth.Remove(key);
                    startDay.Remove(key);

                    endYear.Remove(key);
                    endMonth.Remove(key);
                    endDay.Remove(key);
                }
                catch (Exception e)
                {
                    Debug.LogError("Invalid date: " + e.Message);
                }
            }

            if (GUILayout.Button("Remove"))
            {
                tempSchedule.schedulePeriods.Remove(key);
                keyEdits.Remove(key);
                startYear.Remove(key);
                startMonth.Remove(key);
                startDay.Remove(key);

                endYear.Remove(key);
                endMonth.Remove(key);
                endDay.Remove(key);
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Add New Period"))
        {
            string newKey = "NewPeriod" + (tempSchedule.schedulePeriods.Count + 1);
            var period = new AssetSchedulePeriod(
                newKey,
                DateTime.Today,                      // start at midnight
                DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59) // end at 23:59:59
            );
            tempSchedule.schedulePeriods[newKey] = period;

            keyEdits[newKey] = period.Key;

            startYear[newKey] = period.StartTime.Year;
            startMonth[newKey] = period.StartTime.Month;
            startDay[newKey] = period.StartTime.Day;

            endYear[newKey] = period.EndTime.Year;
            endMonth[newKey] = period.EndTime.Month;
            endDay[newKey] = period.EndTime.Day;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Export to JSON"))
        {
            string json = JsonConvert.SerializeObject(tempSchedule, Formatting.Indented);
            File.WriteAllText(exportPath, json);
            Debug.Log($"✅ AssetSchedule exported to {exportPath}");
        }

        if (GUILayout.Button("Clear Schedule"))
        {
            tempSchedule = new AssetSchedule();
            keyEdits.Clear();
            startYear.Clear();
            startMonth.Clear();
            startDay.Clear();

            endYear.Clear();
            endMonth.Clear();
            endDay.Clear();
        }
    }
}
