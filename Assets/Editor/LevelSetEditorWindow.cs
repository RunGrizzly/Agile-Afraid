using UnityEditor;
using UnityEngine;

public class LevelSetBatchEditor : EditorWindow
{
    private LevelSet levelSet;
    private bool[] levelSelectionStates;

    private bool applyRackSize;
    private int newRackSize;

    private bool applyColorA;
    private Color newColorA;

    private bool applyColorB;
    private Color newColorB;

    private bool applyMusicClip;
    private AudioClip newMusicClip;

    private bool applyPar;
    private int newPar;

    private bool applyTrophyScore;
    private int newTrophyScore;

    private bool applyRepeatLimit;
    private int newRepeatLimit;

    private bool applyLevelRequirements;
    private LevelRequirements newLevelRequirements;

    private bool applyIsTimed;
    private bool newIsTimed;

    private Vector2 applySettingsScrollPos;

    [MenuItem("Tools/Level Set Batch Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelSetBatchEditor>("Level Set Batch Editor");
    }

    private void OnGUI()
    {
        levelSet = (LevelSet)EditorGUILayout.ObjectField("Level Set", levelSet, typeof(LevelSet), false);

        if (levelSet == null)
        {
            EditorGUILayout.HelpBox("Assign a Level Set asset to begin.", MessageType.Info);
            return;
        }

        if (levelSelectionStates == null || levelSelectionStates.Length != levelSet.Levels.Count)
        {
            levelSelectionStates = new bool[levelSet.Levels.Count];
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Levels in Set", EditorStyles.boldLabel);

        for (int i = 0; i < levelSet.Levels.Count; i++)
        {
            var level = levelSet.Levels[i];

            EditorGUILayout.BeginHorizontal();

            levelSelectionStates[i] = EditorGUILayout.ToggleLeft(
                $"Level {i + 1} (Rack: {level.rackSize}, Par: {level.par}, Requirements: {level.LevelRequirements})",
                levelSelectionStates[i]);

            if (GUILayout.Button("Load", GUILayout.Width(60)))
            {
                newRackSize = level.rackSize;
                newColorA = level.DeadZoneColorA;
                newColorB = level.DeadZoneColorB;
                newMusicClip = level.music != null ? level.music.AudioClip : null;
                newPar = level.par;
                newTrophyScore = level.trophyScore;
                newRepeatLimit = level.repeatLimit;
                newLevelRequirements = level.LevelRequirements;
                newIsTimed = level.IsTimed;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        DrawUILine(Color.gray, 1, 6);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Apply Settings", EditorStyles.boldLabel);

        applySettingsScrollPos = EditorGUILayout.BeginScrollView(applySettingsScrollPos, GUILayout.Height(300));

        DrawSettingField(ref applyRackSize, "Rack Size", () => {
            newRackSize = EditorGUILayout.IntField("New Rack Size", newRackSize);
        });

        DrawSettingField(ref applyColorA, "Dead Zone Color A", () => {
            newColorA = EditorGUILayout.ColorField("New Color A", newColorA);
        });

        DrawSettingField(ref applyColorB, "Dead Zone Color B", () => {
            newColorB = EditorGUILayout.ColorField("New Color B", newColorB);
        });

        DrawSettingField(ref applyMusicClip, "Music Clip", () => {
            newMusicClip = (AudioClip)EditorGUILayout.ObjectField("New Music Clip", newMusicClip, typeof(AudioClip), false);
        });

        DrawSettingField(ref applyPar, "Par", () => {
            newPar = EditorGUILayout.IntField("New Par", newPar);
        });

        DrawSettingField(ref applyTrophyScore, "Trophy Score", () => {
            newTrophyScore = EditorGUILayout.IntField("New Trophy Score", newTrophyScore);
        });

        DrawSettingField(ref applyRepeatLimit, "Repeat Limit", () => {
            newRepeatLimit = EditorGUILayout.IntField("New Repeat Limit", newRepeatLimit);
        });

        DrawSettingField(ref applyLevelRequirements, "Level Requirements", () => {
            newLevelRequirements = (LevelRequirements)EditorGUILayout.EnumFlagsField("New Requirements", newLevelRequirements);
        });

        DrawSettingField(ref applyIsTimed, "Is Timed", () => {
            newIsTimed = EditorGUILayout.Toggle("New IsTimed Value", newIsTimed);
        });

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply Selected Settings to Checked Levels"))
        {
            Undo.RecordObject(levelSet, "Batch Apply Level Settings");

            for (int i = 0; i < levelSet.Levels.Count; i++)
            {
                if (!levelSelectionStates[i]) continue;

                var level = levelSet.Levels[i];

                if (applyRackSize) level.rackSize = newRackSize;
                if (applyColorA) level.DeadZoneColorA = newColorA;
                if (applyColorB) level.DeadZoneColorB = newColorB;
                if (applyMusicClip)
                {
                    if (level.music == null) level.music = new PlayableClip();
                    level.music.AudioClip = newMusicClip;
                }
                if (applyPar) level.par = newPar;
                if (applyTrophyScore) level.trophyScore = newTrophyScore;
                if (applyRepeatLimit) level.repeatLimit = newRepeatLimit;
                if (applyLevelRequirements) level.LevelRequirements = newLevelRequirements;
                if (applyIsTimed) level.IsTimed = newIsTimed;
            }

            EditorUtility.SetDirty(levelSet);
            AssetDatabase.SaveAssets();
        }
    }

    private void DrawSettingField(ref bool toggle, string label, System.Action drawField)
    {
        toggle = EditorGUILayout.BeginToggleGroup(label, toggle);
        EditorGUI.indentLevel++;
        drawField();
        EditorGUI.indentLevel--;
        EditorGUILayout.EndToggleGroup();
    }

    private void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }
}
