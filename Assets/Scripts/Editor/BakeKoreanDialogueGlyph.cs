using TMPro;
using UnityEditor;
using UnityEngine;

public static class BakeKoreanDialogueGlyph
{
    [MenuItem("Tools/Localization/Bake KR Dialogue Missing Glyph")]
    public static void Bake()
    {
        const string assetPath = "Assets/Resources/Fonts & Materials/KR/NotoSansKR-Regular SDF Dialogue.asset";
        const string glyphs = "\uC3D8"; // 쏘

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (fontAsset == null)
        {
            Debug.LogError($"Font asset not found: {assetPath}");
            return;
        }

        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fontAsset.isMultiAtlasTexturesEnabled = true;

        bool success = fontAsset.TryAddCharacters(glyphs, out string missingCharacters);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!success || !string.IsNullOrEmpty(missingCharacters))
            Debug.LogError($"Failed to bake glyph(s): {missingCharacters}");
        else
            Debug.Log("Baked Korean dialogue glyph: U+C3D8 쏘");
    }
}