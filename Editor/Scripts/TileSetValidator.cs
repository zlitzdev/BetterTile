using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    [InitializeOnLoad]
    internal static class TileSetValidator
    {
        public static event Action<TileSet> onTileSetValidated;

        static TileSetValidator()
        {
            AssetEvent.onAssetDeleted += OnAssetDeleted;

            AssetEvent.onTextureImported += OnTextureImport;
            AssetEvent.onTileSetImported += OnTileSetImport;
        }

        private static void OnAssetDeleted()
        {
            foreach (TileSet tileSet in tileSets)
            {
                SerializedObject serializedObject = new SerializedObject(tileSet);
                if (RemoveNullTextures(serializedObject))
                {
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    onTileSetValidated?.Invoke(tileSet);
                }
            }
        }

        private static void OnTextureImport(string path)
        {
            
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Sprite[]  sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

            foreach (TileSet tileSet in tileSets)
            {
                SerializedObject serializedObject = new SerializedObject(tileSet);
                if (UpdateSprites(serializedObject, texture, sprites))
                {
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    onTileSetValidated?.Invoke(tileSet);
                }
            }
        }

        private static void OnTileSetImport(string path)
        {
            TileSet tileSet = AssetDatabase.LoadAssetAtPath<TileSet>(path);
            if (tileSet == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(tileSet);
            bool changed = false;

            if (RemoveNullTextures(serializedObject))
            {
                changed = true;
            }

            if (UpdateSprites(serializedObject))
            {
                changed = true;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                onTileSetValidated?.Invoke(tileSet);
            }
        }

        private static bool RemoveNullTextures(SerializedObject serializedObject)
        {
            bool changed = false;

            SerializedProperty ruleGroupsProperty = serializedObject.FindProperty("m_ruleGroups");
            for (int i = ruleGroupsProperty.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty ruleGroupProperty = ruleGroupsProperty.GetArrayElementAtIndex(i);

                SerializedProperty textureProperty = ruleGroupProperty.FindPropertyRelative("m_texture");
                if (textureProperty.objectReferenceValue == null)
                {
                    ruleGroupsProperty.DeleteArrayElementAtIndex(i);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool UpdateSprites(SerializedObject serializedObject)
        {
            bool changed = false;

            SerializedProperty ruleGroupsProperty = serializedObject.FindProperty("m_ruleGroups");
            for (int i = 0; i < ruleGroupsProperty.arraySize; i++)
            {
                SerializedProperty ruleGroupProperty = ruleGroupsProperty.GetArrayElementAtIndex(i);

                SerializedProperty textureProperty = ruleGroupProperty.FindPropertyRelative("m_texture");
                Texture2D texture = textureProperty.objectReferenceValue as Texture2D;

                HashSet<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>().ToHashSet();
                List<int> obsoleteIndices = new List<int>();

                SerializedProperty rulesProperty = ruleGroupProperty.FindPropertyRelative("m_rules");
                for (int j = 0; j < rulesProperty.arraySize; j++)
                {
                    SerializedProperty ruleProperty   = rulesProperty.GetArrayElementAtIndex(j);
                    SerializedProperty outputProperty = ruleProperty.FindPropertyRelative("m_output");
                    SerializedProperty spriteProperty = outputProperty.FindPropertyRelative("m_sprite");

                    Sprite sprite = spriteProperty.objectReferenceValue as Sprite;
                    if (!sprites.Remove(sprite))
                    {
                        obsoleteIndices.Add(j);
                    }
                }

                for (int j = obsoleteIndices.Count - 1; j >= 0; j--)
                {
                    int index = obsoleteIndices[j];
                    rulesProperty.DeleteArrayElementAtIndex(index);

                    changed = true;
                }

                foreach (Sprite missingSprite in sprites)
                {
                    rulesProperty.arraySize++;

                    SerializedProperty newRuleProperty   = rulesProperty.GetArrayElementAtIndex(rulesProperty.arraySize - 1);
                    SerializedProperty newOutputProperty = newRuleProperty.FindPropertyRelative("m_output");
                    SerializedProperty newSpriteProperty = newOutputProperty.FindPropertyRelative("m_sprite");

                    newSpriteProperty.objectReferenceValue = missingSprite;

                    changed = true;
                }
            }

            return changed;
        }

        private static bool UpdateSprites(SerializedObject serializedObject, Texture2D texture, Sprite[] textureSprites)
        {
            bool changed = false;

            SerializedProperty ruleGroupsProperty = serializedObject.FindProperty("m_ruleGroups");
            for (int i = 0; i < ruleGroupsProperty.arraySize; i++)
            {
                SerializedProperty ruleGroupProperty = ruleGroupsProperty.GetArrayElementAtIndex(i);

                SerializedProperty textureProperty = ruleGroupProperty.FindPropertyRelative("m_texture");
                if (textureProperty.objectReferenceValue != texture)
                {
                    continue;
                }

                HashSet<Sprite> sprites = textureSprites.ToHashSet();
                List<int> obsoleteIndices = new List<int>();

                SerializedProperty rulesProperty = ruleGroupProperty.FindPropertyRelative("m_rules");
                for (int j = 0; j < rulesProperty.arraySize; j++)
                {
                    SerializedProperty ruleProperty   = rulesProperty.GetArrayElementAtIndex(j);
                    SerializedProperty outputProperty = ruleProperty.FindPropertyRelative("m_output");
                    SerializedProperty spriteProperty = outputProperty.FindPropertyRelative("m_sprite");

                    Sprite sprite = spriteProperty.objectReferenceValue as Sprite;
                    if (!sprites.Remove(sprite))
                    {
                        obsoleteIndices.Add(j);
                    }
                }

                for (int j = obsoleteIndices.Count - 1; j >= 0; j--)
                {
                    int index = obsoleteIndices[j];
                    rulesProperty.DeleteArrayElementAtIndex(index);

                    changed = true;
                }

                foreach (Sprite missingSprite in sprites)
                {
                    rulesProperty.arraySize++;

                    SerializedProperty newRuleProperty   = rulesProperty.GetArrayElementAtIndex(rulesProperty.arraySize - 1);
                    SerializedProperty newOutputProperty = newRuleProperty.FindPropertyRelative("m_output");
                    SerializedProperty newSpriteProperty = newOutputProperty.FindPropertyRelative("m_sprite");

                    newSpriteProperty.objectReferenceValue = missingSprite;

                    changed = true;
                }
            }

            return changed;
        }

        private static IEnumerable<TileSet> tileSets
        {
            get
            {
                string[] guids = AssetDatabase.FindAssets($"t:{typeof(TileSet).Name}");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    TileSet tileSet = AssetDatabase.LoadAssetAtPath<TileSet>(path);
                    if (tileSet != null)
                    {
                        yield return tileSet;
                    }
                }
            }
        }
    }
}
