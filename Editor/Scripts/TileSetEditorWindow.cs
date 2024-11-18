using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using ColliderType = UnityEngine.Tilemaps.Tile.ColliderType;
using System.Reflection;

namespace Zlitz.Extra2D.BetterTile 
{
    public class TileSetEditorWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_visualTreeAsset = default;

        [SerializeField]
        private Texture2D m_lockIconTexture = default;

        [SerializeField, HideInInspector]
        private TileSet m_tileSet;

        private ObjectField m_tileSetReference;

        private ListView           m_categoriesListView;
        private List<TileCategory> m_categories;
        
        private ListView   m_tilesListView;
        private List<Tile> m_tiles;

        private ListView            m_othersListView;
        private List<MiscPaintable> m_others;

        private TextureSetDisplay m_textureSetDisplay;

        private PaintControl m_paintControl;

        public static void Open(TileSet tileSet)
        {
            TileSetEditorWindow wnd = GetWindow<TileSetEditorWindow>();
            wnd.titleContent = new GUIContent("Tile Set Editor");

            wnd.m_tileSet = tileSet;
            wnd.UpdateUI();
        }

        [OnOpenAsset]
        private static bool OpenTileSet(int instanceID)
        {
            UnityEngine.Object asset = EditorUtility.InstanceIDToObject(instanceID);
            if (asset is TileSet tileSet)
            {
                Open(tileSet);
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            VisualElement uxml = m_visualTreeAsset.Instantiate();
            uxml.style.flexGrow = 1.0f;
            root.Add(uxml);

            // Display a read-only reference to the current tile set

            m_tileSetReference = uxml.Q<ObjectField>(name: "tileset");
            m_tileSetReference.SetEnabled(false);

            // Setup categories list view

            m_categoriesListView = uxml.Q<ListView>(name: "categories");
            Toggle categoriesFoldoutToggle = m_categoriesListView.Q<Toggle>();
            categoriesFoldoutToggle.value = EditorPrefs.GetBool("Zlitz.Extra2D.BetterTile.categoriesFoldout", false);
            categoriesFoldoutToggle.RegisterValueChangedCallback(e => EditorPrefs.SetBool("Zlitz.Extra2D.BetterTile.categoriesFoldout", e.newValue));

            m_categoriesListView.itemIndexChanged += (s, e) =>
            {
                if (s == e)
                {
                    return;
                }

                SerializedObject serializedTileSet = new SerializedObject(m_tileSet);

                SerializedProperty categoriesProperty = serializedTileSet.FindProperty("m_categories");
                categoriesProperty.MoveArrayElement(s, e);

                serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(m_tileSet);
                AssetDatabase.SaveAssetIfDirty(m_tileSet);

                m_paintControl.Rebuild();
            };

            m_categoriesListView.itemsAdded += (indices) =>
            {
                SerializedObject serializedTileSet = new SerializedObject(m_tileSet);

                SerializedProperty categoriesProperty = serializedTileSet.FindProperty("m_categories");

                indices = indices.OrderBy(i => i);
                foreach (int index in indices)
                {
                    TileCategory newCategory = CreateInstance<TileCategory>();
                    newCategory.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector | HideFlags.HideInHierarchy;

                    string newName = "New Category";
                    if (m_tileSet.categories != null)
                    {
                        newName = ObjectNames.GetUniqueName(m_tileSet.categories.Where(c => c != null).Select(c => c.name).ToArray(), newName);
                    }
                    newCategory.name = newName;

                    AssetDatabase.AddObjectToAsset(newCategory, m_tileSet);
                    EditorUtility.SetDirty(m_tileSet);
                    AssetDatabase.SaveAssetIfDirty(m_tileSet);

                    categoriesProperty.InsertArrayElementAtIndex(index);
                    serializedTileSet.ApplyModifiedPropertiesWithoutUndo();

                    SerializedProperty newCategoryProperty = categoriesProperty.GetArrayElementAtIndex(index);
                    newCategoryProperty.objectReferenceValue = newCategory;

                    m_categories[index] = newCategory;

                    serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                }

                serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(m_tileSet);
                AssetDatabase.SaveAssetIfDirty(m_tileSet);

                m_paintControl.Rebuild();
            };

            m_categoriesListView.itemsRemoved += (indices) =>
            {
                SerializedObject serializedTileSet = new SerializedObject(m_tileSet);

                SerializedProperty categoriesProperty = serializedTileSet.FindProperty("m_categories");

                indices = indices.OrderByDescending(i => i);
                foreach (int i in indices)
                {
                    SerializedProperty currentCategoryProperty = categoriesProperty.GetArrayElementAtIndex(i);

                    TileCategory category = currentCategoryProperty.objectReferenceValue as TileCategory;
                    if (category != null)
                    {
                        AssetDatabase.RemoveObjectFromAsset(category);
                        DestroyImmediate(category, true);
                    }

                    categoriesProperty.DeleteArrayElementAtIndex(i);
                }

                serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(m_tileSet);
                AssetDatabase.SaveAssetIfDirty(m_tileSet);

                m_paintControl.Rebuild();
            };

            m_categoriesListView.selectionChanged += (objects) =>
            {
                TileCategory selected = objects.OfType<TileCategory>().FirstOrDefault(c => c != null);
                m_paintControl.SelectFilter(selected);
            };

            m_categoriesListView.RegisterCallback<FocusOutEvent>(e =>
            {
                m_categoriesListView.SetSelectionWithoutNotify(new int[] { });
            });

            m_categoriesListView.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.Delete)
                {
                    int index = m_categoriesListView.selectedIndex;
                    if (index >= 0 && index < m_categories.Count)
                    {
                        SerializedObject serializedTileSet = new SerializedObject(m_tileSet);

                        SerializedProperty categoriesProperty = serializedTileSet.FindProperty("m_categories");

                        SerializedProperty currentCategoryProperty = categoriesProperty.GetArrayElementAtIndex(index);

                        TileCategory category = currentCategoryProperty.objectReferenceValue as TileCategory;
                        if (category != null)
                        {
                            AssetDatabase.RemoveObjectFromAsset(category);
                            DestroyImmediate(category, true);
                        }

                        categoriesProperty.DeleteArrayElementAtIndex(index);

                        serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(m_tileSet);
                        AssetDatabase.SaveAssetIfDirty(m_tileSet);

                        UpdateCategories();

                        m_paintControl.Rebuild();
                    }
                }
            });

            // Setup tiles list view

            m_tilesListView = uxml.Q<ListView>(name: "tiles"); 
            Toggle tilesFoldoutToggle = m_tilesListView.Q<Toggle>();
            tilesFoldoutToggle.value = EditorPrefs.GetBool("Zlitz.Extra2D.BetterTile.tilesFoldout", false);
            tilesFoldoutToggle.RegisterValueChangedCallback(e => EditorPrefs.SetBool("Zlitz.Extra2D.BetterTile.tilesFoldout", e.newValue));

            m_tilesListView.itemIndexChanged += (s, e) =>
            {
                if (s == e)
                {
                    return;
                }

                SerializedObject serializedTileSet = new SerializedObject(m_tileSet);

                SerializedProperty tilesProperty = serializedTileSet.FindProperty("m_tiles");
                tilesProperty.MoveArrayElement(s, e);

                serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(m_tileSet);
                AssetDatabase.SaveAssetIfDirty(m_tileSet);

                m_categoriesListView.RefreshItems();
                m_paintControl.Rebuild();
            };

            m_tilesListView.itemsAdded += (indices) =>
            {
                SerializedObject serializedTileSet = new SerializedObject(m_tileSet);

                SerializedProperty tilesProperty = serializedTileSet.FindProperty("m_tiles");

                indices = indices.OrderBy(i => i);
                foreach (int index in indices)
                {
                    Tile newTile = CreateInstance<Tile>();
                    newTile.hideFlags = HideFlags.NotEditable;

                    string newName = "New Tile";
                    if (m_tileSet.tiles != null)
                    {
                        newName = ObjectNames.GetUniqueName(m_tileSet.tiles.Where(t => t != null).Select(t => t.name).ToArray(), newName);
                    }
                    newTile.name = newName;

                    AssetDatabase.AddObjectToAsset(newTile, m_tileSet);
                    EditorUtility.SetDirty(m_tileSet);
                    AssetDatabase.SaveAssetIfDirty(m_tileSet);

                    tilesProperty.InsertArrayElementAtIndex(index);
                    serializedTileSet.ApplyModifiedPropertiesWithoutUndo();

                    SerializedProperty newTileProperty = tilesProperty.GetArrayElementAtIndex(index);
                    newTileProperty.objectReferenceValue = newTile;

                    m_tiles[index] = newTile;

                    SerializedObject newSerializedTile = new SerializedObject(newTile);
                    newSerializedTile.Update();

                    SerializedProperty tileSetProperty = newSerializedTile.FindProperty("m_tileSet");
                    tileSetProperty.objectReferenceValue = m_tileSet;
                    newSerializedTile.ApplyModifiedPropertiesWithoutUndo();

                    serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                }

                serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(m_tileSet);
                AssetDatabase.SaveAssetIfDirty(m_tileSet);

                m_categoriesListView.RefreshItems();
                m_paintControl.Rebuild();
            };

            m_tilesListView.itemsRemoved += (indices) =>
            {
                SerializedObject serializedTileSet = new SerializedObject(m_tileSet);

                SerializedProperty tilesProperty = serializedTileSet.FindProperty("m_tiles");

                indices = indices.OrderByDescending(i => i);
                foreach (int i in indices)
                {
                    SerializedProperty currentTileProperty = tilesProperty.GetArrayElementAtIndex(i);

                    Tile tile = currentTileProperty.objectReferenceValue as Tile;
                    if (tile != null)
                    {
                        AssetDatabase.RemoveObjectFromAsset(tile);
                        DestroyImmediate(tile, true);
                    }

                    tilesProperty.DeleteArrayElementAtIndex(i);
                }

                serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(m_tileSet);
                AssetDatabase.SaveAssetIfDirty(m_tileSet);

                m_categoriesListView.RefreshItems();
                m_paintControl.Rebuild();
            };

            m_tilesListView.selectionChanged += (objects) =>
            {
                Tile selected = objects.OfType<Tile>().FirstOrDefault(t => t != null);
                m_paintControl.SelectTile(selected);
                m_paintControl.SelectFilter(selected);
            };

            m_tilesListView.RegisterCallback<FocusOutEvent>(e =>
            {
                m_tilesListView.SetSelectionWithoutNotify(new int[] { });
            });

            m_tilesListView.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.Delete)
                {
                    int index = m_tilesListView.selectedIndex;
                    if (index >= 0 && index < m_tiles.Count)
                    {
                        SerializedObject serializedTileSet = new SerializedObject(m_tileSet);

                        SerializedProperty tilesProperty = serializedTileSet.FindProperty("m_tiles");

                        SerializedProperty currentTileProperty = tilesProperty.GetArrayElementAtIndex(index);

                        Tile tile = currentTileProperty.objectReferenceValue as Tile;
                        if (tile != null)
                        {
                            AssetDatabase.RemoveObjectFromAsset(tile);
                            DestroyImmediate(tile, true);
                        }

                        tilesProperty.DeleteArrayElementAtIndex(index);

                        serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(m_tileSet);
                        AssetDatabase.SaveAssetIfDirty(m_tileSet);

                        UpdateTiles();

                        m_categoriesListView.RefreshItems();
                        m_paintControl.Rebuild();
                    }
                }
            });

            // Setup others list view

            m_othersListView = uxml.Q<ListView>(name: "others");
            Toggle othersFoldoutToggle = m_tilesListView.Q<Toggle>();
            othersFoldoutToggle.value = EditorPrefs.GetBool("Zlitz.Extra2D.BetterTile.othersFoldout", false);
            othersFoldoutToggle.RegisterValueChangedCallback(e => EditorPrefs.SetBool("Zlitz.Extra2D.BetterTile.othersFoldout", e.newValue));

            m_othersListView.selectionChanged += (objects) =>
            {
                MiscPaintable paintable = objects.OfType<MiscPaintable>().FirstOrDefault();

                SerializedObject serializedTileSet = new SerializedObject(m_tileSet);
                
                UnityEngine.Object target = null;

                switch (paintable)
                {
                    case MiscPaintable.Self:
                        {
                            target = serializedTileSet.FindProperty("m_selfFilter").objectReferenceValue;
                            break;
                        }
                    case MiscPaintable.Decorator:
                        {
                            target = serializedTileSet.FindProperty("m_decorator").objectReferenceValue;
                            break;
                        }
                }

                m_paintControl.SelectTile(target as ITileIdentifier);
                m_paintControl.SelectFilter(target as ITileFilter);
            };

            // Setup paint control

            m_paintControl = uxml.Q<PaintControl>(name: "paint-control");

            string brushTypeStr = EditorPrefs.GetString("Zlitz.Extra2D.BetterTile.paintControlBrushType", "");
            if (!Enum.TryParse(brushTypeStr, out PaintControl.BrushType brushType))
            {
                brushType = PaintControl.BrushType.Tile;
            }
            m_paintControl.brushType = brushType;

            m_paintControl.onBrushTypeChanged += b =>
            {
                EditorPrefs.SetString("Zlitz.Extra2D.BetterTile.paintControlBrushType", b.ToString());
            };

            // Setup texture set display

            m_textureSetDisplay = uxml.Q<TextureSetDisplay>(name: "texture-set-display");
            m_textureSetDisplay.onNewTextureAdded += UpdateTextureSetDisplay;
            m_textureSetDisplay.paintControl = m_paintControl;
            m_textureSetDisplay.lockIconTexture = m_lockIconTexture;
            m_paintControl.onBrushTypeChanged += m_textureSetDisplay.OnBrushTypeChanged;

            // Update UI for current tile set

            if (m_tileSet != null)
            {
                UpdateUI();
            }
        }

        private void OnEnable()
        {
            TileSetTexturePostprocessor.onTextureReimported += OnTextureReimported;
        }

        private void OnDisable()
        {
            TileSetTexturePostprocessor.onTextureReimported -= OnTextureReimported;
        }

        private void OnTextureReimported()
        {
            EditorApplication.delayCall += UpdateUI;
        }

        private void UpdateUI()
        {
            m_tileSetReference.value = m_tileSet;

            UpdateCategories();
            UpdateTiles();
            UpdateOthers();
            UpdateTextureSetDisplay();
        }

        private void UpdateCategories()
        {
            m_categoriesListView.Clear();

            m_categories = m_tileSet.categories.ToList();
            m_categoriesListView.itemsSource = m_categories;

            m_categoriesListView.makeItem = () => new TileCategoryItem();

            m_categoriesListView.bindItem = (e, i) => (e as TileCategoryItem)?.Bind(m_tileSet, m_categories, i, 
                () =>
                {
                    m_paintControl.Rebuild();
                    m_categoriesListView.RefreshItems();
                },
                () =>
                {
                    m_textureSetDisplay.UpdateFields();
                }
            );
        }

        private void UpdateTiles()
        {
            m_tilesListView.Clear();

            m_tiles = m_tileSet.tiles.ToList();
            m_tilesListView.itemsSource = m_tiles;

            m_tilesListView.makeItem = () => new TileItem(m_tileSet);

            m_tilesListView.bindItem = (e, i) => (e as TileItem)?.Bind(m_tileSet, m_tiles, i, 
                () => 
                {
                    m_paintControl.Rebuild();

                    m_tilesListView.RefreshItems(); 
                    m_categoriesListView.RefreshItems(); 
                },
                () =>
                {
                    m_textureSetDisplay?.UpdateFields();
                },
                () =>
                {
                    UpdateTiles();
                }
            );
        }
    
        private void UpdateOthers()
        {
            m_othersListView.Clear();

            m_others = ValidateOtherPaintables(m_others);

            m_othersListView.itemsSource = m_others;

            m_othersListView.makeItem = () => new MiscItem();

            m_othersListView.bindItem = (e, i) => (e as MiscItem)?.Bind(m_tileSet, m_others[i], 
                () =>
                {
                    m_textureSetDisplay?.UpdateFields();
                }
            );
        }

        private void UpdateTextureSetDisplay()
        {
            m_textureSetDisplay.tileSet = m_tileSet;
            m_textureSetDisplay.OnBrushTypeChanged(m_paintControl.brushType);
        }
    
        private List<MiscPaintable> ValidateOtherPaintables(List<MiscPaintable> others)
        {
            HashSet<MiscPaintable> allOptions = new HashSet<MiscPaintable>();
            allOptions.Add(MiscPaintable.Self);
            allOptions.Add(MiscPaintable.Decorator);

            if (others == null)
            {
                others = new List<MiscPaintable>();
            }

            List<int> unintendedIndices = new List<int>();
            int i = 0;
            foreach (MiscPaintable other in others)
            {
                if (!allOptions.Remove(other))
                {
                    unintendedIndices.Add(i);
                }
                i++;
            }
            unintendedIndices = unintendedIndices.OrderByDescending(i => i).ToList();
            foreach (int index in unintendedIndices)
            {
                others.RemoveAt(index);
            }

            foreach (MiscPaintable remainingOption in allOptions)
            {
                others.Add(remainingOption);
            }

            return others;
        }

        internal enum MiscPaintable
        {
            None,
            Self,
            Decorator
        }
    }

    internal class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }
    }

    internal class EditableLabel : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<EditableLabel, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription m_placeholderText = new UxmlStringAttributeDescription()
            {
                name = "placeholder-text",
                defaultValue = "Enter text..."
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                
                if (ve is not EditableLabel label)
                {
                    return;
                }

                label.placeholderText = m_placeholderText.GetValueFromBag(bag, cc);
            }
        }

        private Label     m_label;
        private TextField m_textField;
        private string    m_placeholderText;
        private string    m_text;

        public event Action<string> onValueChanged;

        private Color m_color = Color.white;

        public Color color
        {
            get => m_color;
            set
            {
                m_color = value;
                m_color.a = 1.0f;
                UpdateLabel();
            }
        }

        public string placeholderText
        {
            get => m_placeholderText;
            set
            {
                m_placeholderText = value;
                UpdateLabel();
            }
        }

        public string text
        {
            get => m_textField.value;
            set
            {
                if (m_textField.value != value)
                {
                    m_textField.value = value;
                    UpdateLabel();
                }
            }
        }

        public EditableLabel()
        {
            m_label = new Label()
            {
                text = m_placeholderText
            };
            m_label.style.flexGrow     = 1.0f;
            m_label.style.marginTop    = 2.0f;
            m_label.style.marginBottom = 2.0f;
            m_label.style.marginLeft   = 8.0f;
            m_label.style.marginRight  = 8.0f;

            m_textField = new TextField();
            m_label.style.flexGrow = 1.0f;

            Add(m_textField);
            Add(m_label);

            m_textField.style.display = DisplayStyle.None;
            m_label.style.display = DisplayStyle.Flex;

            m_label.RegisterCallback<ClickEvent>(OnLabelClicked);

            m_textField.RegisterCallback<FocusOutEvent>(OnTextFocusOut);
        }

        private void OnLabelClicked(ClickEvent e)
        {
            m_label.style.display = DisplayStyle.None;
            m_textField.style.display = DisplayStyle.Flex;
            m_textField.Focus();
        }

        private void OnTextFocusOut(FocusOutEvent e)
        {
            UpdateLabel();
            m_textField.style.display = DisplayStyle.None;
            m_label.style.display = DisplayStyle.Flex;
        }

        private void OnTextKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.KeypadEnter)
            {
                UpdateLabel();
                m_textField.style.display = DisplayStyle.None;
                m_label.style.display = DisplayStyle.Flex;
            }
        }

        private void UpdateLabel()
        {
            string text = m_textField.value;
            if (string.IsNullOrEmpty(text))
            {
                m_label.text = m_placeholderText;
                m_label.style.unityFontStyleAndWeight = FontStyle.Italic;
                m_label.style.color = new Color(0.7f * m_color.r, 0.7f * m_color.g, 0.7f * m_color.b, 1.0f);
            }
            else
            {
                m_label.text = text;
                m_label.style.unityFontStyleAndWeight = FontStyle.Normal;
                m_label.style.color = m_color;
            }

            if (m_text != text)
            {
                m_text = text;
                onValueChanged?.Invoke(m_text);
            }
        }
    }

    internal class ScopedTileSelector : PopupField<Tile>
    {
        private TileSet          m_tileSet;
        private Func<Tile, bool> m_filter;

        public Action<Tile> onValueChanged;

        public Func<Tile, bool> filter
        {
            get => m_filter;
            set
            {
                m_filter = value;
                choices = MakeChoices(m_tileSet, m_filter);
            }
        }

        public ScopedTileSelector(TileSet tileSet, string label) : base(label, MakeChoices(tileSet), 0, null, null)
        {
            m_tileSet = tileSet;

            formatSelectedValueCallback = FormatItem;
            formatListItemCallback      = FormatItem;

            RegisterCallback<ChangeEvent<Tile>>(e =>
            {
                onValueChanged?.Invoke(e.newValue);
            });
        }

        private string FormatItem(Tile tile) => tile != null ? tile.name : "None";

        private static List<Tile> MakeChoices(TileSet tileSet, Func<Tile, bool> filter = null)
        {
            List<Tile> results = tileSet.tiles.Where(t => t != null && (filter == null || filter.Invoke(t))).ToList();
            results.Insert(0, null);
            return results;
        }
    }

    internal class TileCategoryItem : VisualElement
    {
        private EditableLabel m_name;
        private ColorField    m_color;
        private ListView      m_tilesList;
        private Toggle        m_inverted;

        private TileSet            m_tileSet;
        private TileCategory       m_category;
        private List<Tile>         m_tiles;
        private SerializedObject   m_serializedCategory;
        private SerializedProperty m_colorProperty;
        private SerializedProperty m_tilesProperty;
        private SerializedProperty m_invertedProperty;

        private int m_index;
        private IList<TileCategory> m_categories;

        private Action m_onNameChanged;
        private Action m_onColorChanged;

        public void Bind(TileSet tileSet, IList<TileCategory> categories, int i, Action onNameChanged, Action onColorChanged)
        {
            m_index = i;
            m_categories = categories;

            m_tileSet  = tileSet;
            m_category = categories[m_index];
            m_serializedCategory = new SerializedObject(m_category);
            m_colorProperty    = m_serializedCategory.FindProperty("m_color");
            m_tilesProperty    = m_serializedCategory.FindProperty("m_tiles");
            m_invertedProperty = m_serializedCategory.FindProperty("m_inverted");

            m_onNameChanged  = onNameChanged;
            m_onColorChanged = onColorChanged;

            UpdateFields();
        }

        public TileCategoryItem()
        {
            AddToClassList("dark-gray");
            AddToClassList("enclosed");

            VisualElement nameAndColor = new VisualElement();
            nameAndColor.style.flexDirection = FlexDirection.Row;
            Add(nameAndColor);

            m_name = new EditableLabel();
            m_name.color          = Color.white;
            m_name.style.flexGrow = 1.0f;
            m_name.onValueChanged += (newName) =>
            {
                if (m_index >= m_categories.Count)
                {
                    return;
                }

                if (string.IsNullOrEmpty(newName))
                {
                    newName = "Tile Category";
                }
                newName = ObjectNames.GetUniqueName(m_tileSet.categories.Where(c => c != m_category).Select(c => c.name).ToArray(), newName);
                m_category.name = newName;

                EditorUtility.SetDirty(m_category);
                AssetDatabase.SaveAssetIfDirty(m_category);

                m_serializedCategory.Update();
                UpdateFields();

                m_name.text = newName;

                m_onNameChanged?.Invoke();
            };

            nameAndColor.Add(m_name);

            m_color = new ColorField();
            m_color.value          = Color.white;
            m_color.showEyeDropper = false;
            m_color.showAlpha      = false;
            m_color.style.width    = 36.0f;

            m_color.RegisterValueChangedCallback(e =>
            {
                Color newColor = e.newValue;
                newColor.a = 1.0f;

                m_name.color = newColor;
                m_colorProperty.colorValue = newColor;
                m_serializedCategory.ApplyModifiedPropertiesWithoutUndo();

                m_onColorChanged?.Invoke();
            });

            nameAndColor.Add(m_color);

            m_tilesList = new ListView();
            m_tilesList.showAddRemoveFooter = true;

            m_tilesList.itemIndexChanged += (s, e) =>
            {
                if (s == e)
                {
                    return;
                }

                m_tilesProperty.MoveArrayElement(s, e);

                m_serializedCategory.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(m_category);
                AssetDatabase.SaveAssetIfDirty(m_category);
            };

            m_tilesList.itemsAdded += (indices) =>
            {
                indices = indices.OrderBy(i => i);
                foreach (int index in indices)
                {
                    Tile defaultTile = null;

                    m_tilesProperty.InsertArrayElementAtIndex(index);
                    m_serializedCategory.ApplyModifiedPropertiesWithoutUndo();

                    SerializedProperty newTileProperty = m_tilesProperty.GetArrayElementAtIndex(index);
                    newTileProperty.objectReferenceValue = defaultTile;

                    m_tiles[index] = defaultTile;
                }

                m_serializedCategory.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(m_category);
                AssetDatabase.SaveAssetIfDirty(m_category);
            };

            m_tilesList.itemsRemoved += (indices) =>
            {
                indices = indices.OrderByDescending(i => i);
                foreach (int i in indices)
                {
                    m_tilesProperty.DeleteArrayElementAtIndex(i);
                }

                m_serializedCategory.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(m_category);
                AssetDatabase.SaveAssetIfDirty(m_category);
            };

            Add(m_tilesList);

            m_inverted = new Toggle("Inverted");

            m_inverted.RegisterValueChangedCallback(e =>
            {
                m_invertedProperty.boolValue = e.newValue;
                m_serializedCategory.ApplyModifiedPropertiesWithoutUndo();
            });

            Add(m_inverted);
        }

        private void UpdateFields()
        {
            string currentName = m_category.name;
            string validatedName = ObjectNames.GetUniqueName(m_categories.Take(m_index).Where(c => c != m_categories[m_index]).Select(c => c.name).ToArray(), currentName);

            if (currentName != validatedName)
            {
                currentName = validatedName;
                m_category.name = currentName;
                EditorUtility.SetDirty(m_category);
                AssetDatabase.SaveAssetIfDirty(m_category);
            }

            m_name.text = currentName;
            m_name.color = m_colorProperty.colorValue;

            Color currentColor = m_colorProperty.colorValue;
            currentColor.a = 1.0f;
            m_color.value = currentColor;

            m_tiles = m_category.tiles?.ToList() ?? new List<Tile>();

            m_tilesList.Clear();
            m_tilesList.itemsSource = m_tiles;

            m_tilesList.makeItem = () => new ScopedTileSelector(m_tileSet, null);

            m_tilesList.bindItem = (e, i) =>
            {
                if (e is ScopedTileSelector tileSelector)
                {
                    tileSelector.label = $"Element {i}";

                    tileSelector.value = m_tiles[i];
                    tileSelector.onValueChanged = (t) =>
                    {
                        SerializedProperty tileProperty = m_tilesProperty.GetArrayElementAtIndex(i);
                        tileProperty.objectReferenceValue = t;
                        m_tiles[i] = t;
                        m_serializedCategory.ApplyModifiedPropertiesWithoutUndo();
                    };
                }
            };

            m_inverted.value = m_invertedProperty.boolValue;
        }
    }

    internal class TileItem : VisualElement
    {
        private EditableLabel      m_name;
        private ColorField         m_color;
        private EnumField          m_colliderType;
        private ScopedTileSelector m_baseTile;
        private Toggle             m_overwriteRules;

        private TileSet            m_tileSet;
        private Tile               m_tile;
        private SerializedObject   m_serializedTile;
        private SerializedProperty m_colorProperty;
        private SerializedProperty m_colliderTypeProperty;
        private SerializedProperty m_baseTileProperty;
        private SerializedProperty m_overwriteRulesProperty;

        private int         m_index;
        private IList<Tile> m_tiles;

        private Action m_onNameChanged;
        private Action m_onColorChanged;
        private Action m_onBaseTileChanged;

        public void Bind(TileSet tileSet, IList<Tile> tiles, int i, Action onNameChanged, Action onColorChanged, Action onBaseTileChanged)
        {
            m_index = i;
            m_tiles = tiles;

            m_tileSet = tileSet;
            m_tile = tiles[m_index];
            m_serializedTile = new SerializedObject(m_tile);
            m_colorProperty          = m_serializedTile.FindProperty("m_color");
            m_colliderTypeProperty   = m_serializedTile.FindProperty("m_colliderType");
            m_baseTileProperty       = m_serializedTile.FindProperty("m_baseTile");
            m_overwriteRulesProperty = m_serializedTile.FindProperty("m_overwriteRules");

            m_onNameChanged     = onNameChanged;
            m_onColorChanged    = onColorChanged;
            m_onBaseTileChanged = onBaseTileChanged;

            UpdateFields();
        }

        public TileItem(TileSet tileSet)
        {
            AddToClassList("dark-gray");
            AddToClassList("enclosed");

            m_tileSet = tileSet;

            VisualElement nameAndColor = new VisualElement();
            nameAndColor.style.flexDirection = FlexDirection.Row;
            Add(nameAndColor);

            m_name = new EditableLabel();
            m_name.color = Color.white;
            m_name.style.flexGrow = 1.0f;

            m_name.onValueChanged += (newName) =>
            {
                if (m_index >= m_tiles.Count)
                {
                    return;
                }

                if (string.IsNullOrEmpty(newName))
                {
                    newName = "Tile";
                }
                newName = ObjectNames.GetUniqueName(m_tileSet.tiles.Where(t => t != m_tile).Select(c => c.name).ToArray(), newName);
                m_tile.name = newName;

                EditorUtility.SetDirty(m_tile);
                AssetDatabase.SaveAssetIfDirty(m_tile);

                m_serializedTile.Update();
                UpdateFields();

                m_name.text = newName;
                m_onNameChanged?.Invoke();
            };

            nameAndColor.Add(m_name);

            m_color = new ColorField();
            m_color.value = Color.white;
            m_color.showEyeDropper = false;
            m_color.showAlpha = false;
            m_color.style.width = 36.0f;

            m_color.RegisterValueChangedCallback(e =>
            {
                Color newColor = e.newValue;
                newColor.a = 1.0f;

                m_name.color = newColor;
                m_colorProperty.colorValue = newColor;
                m_serializedTile.ApplyModifiedPropertiesWithoutUndo();

                m_onColorChanged?.Invoke();
            });

            nameAndColor.Add(m_color);

            m_colliderType = new EnumField("Collider type", ColliderType.None);
            m_colliderType.style.marginLeft = 16.0f;

            m_colliderType.RegisterValueChangedCallback(e =>
            {
                ColliderType newColliderType = (ColliderType)e.newValue;

                m_colliderTypeProperty.enumValueIndex = (int)newColliderType;
                m_serializedTile.ApplyModifiedPropertiesWithoutUndo();
            });

            Add(m_colliderType);

            m_baseTile = new ScopedTileSelector(m_tileSet, "Base tile");
            m_baseTile.style.marginLeft = 16.0f;
            m_baseTile.onValueChanged += (t) =>
            {
                m_baseTileProperty.objectReferenceValue = t;
                m_serializedTile.ApplyModifiedPropertiesWithoutUndo();

                m_onBaseTileChanged?.Invoke();
            };

            Add(m_baseTile);

            m_overwriteRules = new Toggle("Overwrite rules");
            m_overwriteRules.style.marginLeft = 16.0f;

            m_overwriteRules.RegisterValueChangedCallback(e =>
            {
                m_overwriteRulesProperty.boolValue = e.newValue;
                m_serializedTile.ApplyModifiedPropertiesWithoutUndo();
            });

            Add(m_overwriteRules);
        }

        private void UpdateFields()
        {
            string currentName = m_tiles[m_index].name;
            string validatedName = ObjectNames.GetUniqueName(m_tileSet.tiles.Where(t => t != m_tile).Select(t => t.name).ToArray(), currentName);

            if (currentName != validatedName)
            {
                currentName = validatedName;
                m_tile.name = currentName;
                EditorUtility.SetDirty(m_tile);
                AssetDatabase.SaveAssetIfDirty(m_tile);
            }

            m_name.text = currentName;
            m_name.color = m_colorProperty.colorValue;

            Color currentColor = m_colorProperty.colorValue;
            currentColor.a = 1.0f;
            m_color.value = currentColor;

            ColliderType colliderType = (ColliderType)m_colliderTypeProperty.enumValueIndex;
            m_colliderType.value = colliderType;

            m_baseTile.value = m_baseTileProperty.objectReferenceValue as Tile;
            m_baseTile.filter = t => t == null || !t.IsDescendantOf(m_tile);

            m_overwriteRules.value = m_overwriteRulesProperty.boolValue;
        }
    }

    internal class MiscItem : VisualElement
    {
        private TileSetEditorWindow.MiscPaintable m_paintable;

        private Label      m_name;
        private ColorField m_color;

        private UnityEngine.Object m_target;
        private SerializedObject   m_serializedTarget;
        private SerializedProperty m_colorProperty;

        public TileSetEditorWindow.MiscPaintable paintable => m_paintable;

        public void Bind(TileSet tileSet, TileSetEditorWindow.MiscPaintable paintable, Action onColorChanged)
        {
            m_paintable = paintable;

            SerializedObject serializedTileSet = new SerializedObject(tileSet);
            UnityEngine.Object target = null;

            switch (paintable)
            {
                case TileSetEditorWindow.MiscPaintable.Self:
                    {
                        SerializedProperty selfFilterProperty = serializedTileSet.FindProperty("m_selfFilter");

                        target = selfFilterProperty.objectReferenceValue;
                        if (target == null)
                        {
                            SelfTileFilter selfFilter = ScriptableObject.CreateInstance<SelfTileFilter>();
                            selfFilter.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                            selfFilter.name = "Self";

                            AssetDatabase.AddObjectToAsset(selfFilter, tileSet);

                            selfFilterProperty.objectReferenceValue = selfFilter;

                            serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                            EditorUtility.SetDirty(tileSet);
                            AssetDatabase.SaveAssetIfDirty(tileSet);

                            target = selfFilter;
                        }

                        break;
                    }
                case TileSetEditorWindow.MiscPaintable.Decorator:
                    {
                        SerializedProperty decoratorProperty = serializedTileSet.FindProperty("m_decorator");

                        target = decoratorProperty.objectReferenceValue;
                        if (target == null)
                        {
                            TileDecorator decorator = ScriptableObject.CreateInstance<TileDecorator>();
                            decorator.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                            decorator.name = "Decorator";

                            AssetDatabase.AddObjectToAsset(decorator, tileSet);

                            decoratorProperty.objectReferenceValue = decorator;

                            serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
                            EditorUtility.SetDirty(tileSet);
                            AssetDatabase.SaveAssetIfDirty(tileSet);

                            target = decorator;
                        }

                        break;
                    }
            }

            m_target           = target;
            m_serializedTarget = new SerializedObject(m_target);
            m_colorProperty    = m_serializedTarget.FindProperty("m_color");

            Color color = m_colorProperty.colorValue;
            color.a = 1.0f;

            m_name.text = m_target.name;

            m_name.style.color        = color;
            m_name.style.flexGrow     = 1.0f;
            m_name.style.marginTop    = 2.0f;
            m_name.style.marginBottom = 2.0f;
            m_name.style.marginLeft   = 8.0f;
            m_name.style.marginRight  = 8.0f;

            m_color.value = color;
            m_color.RegisterValueChangedCallback(e =>
            {
                Color newColor = e.newValue;
                newColor.a = 1.0f;

                m_name.style.color = newColor;
                m_colorProperty.colorValue = newColor;
                m_serializedTarget.ApplyModifiedPropertiesWithoutUndo();

                onColorChanged?.Invoke();
            });
        }

        public MiscItem()
        {
            AddToClassList("dark-gray");
            AddToClassList("enclosed");

            VisualElement nameAndColor = new VisualElement();
            nameAndColor.style.flexDirection = FlexDirection.Row;
            Add(nameAndColor);

            m_name = new Label();
            m_name.style.color    = Color.white;
            m_name.style.flexGrow = 1.0f;
            nameAndColor.Add(m_name);

            m_color = new ColorField();
            m_color.value = Color.white;
            m_color.showEyeDropper = false;
            m_color.showAlpha = false;
            m_color.style.width = 36.0f;
            nameAndColor.Add(m_color);
        }
    }

    internal class TextureSetDisplay : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TextureSetDisplay> { };

        public event Action onNewTextureAdded;

        private TileSet            m_tileSet;
        private SerializedObject   m_serializedTileSet;
        private SerializedProperty m_texturesProperty;
        private SerializedProperty m_spriteEntriesProperty;

        private List<TextureDisplay> m_elements = new List<TextureDisplay>();

        private Texture2D m_lockIconTexture;

        private Label m_message;

        private PaintControl m_paintControl;

        private float m_scale = 1.0f;

        private static float s_minScale = 0.01f;
        private static float s_maxScale = 40.0f;

        public TileSet tileSet
        {
            get => m_tileSet;
            set
            {
                m_tileSet               = value;
                m_serializedTileSet     = m_tileSet ? new SerializedObject(m_tileSet) : null;
                m_texturesProperty      = m_tileSet ? m_serializedTileSet.FindProperty("m_textures") : null;
                m_spriteEntriesProperty = m_tileSet ? m_serializedTileSet.FindProperty("m_spriteEntries") : null;

                m_serializedTileSet?.Update();

                ValidateSpriteEntries();
                UpdateElements();
            }
        }

        public PaintControl paintControl
        {
            get => m_paintControl;
            set => m_paintControl = value;
        }

        public Texture2D lockIconTexture
        {
            get => m_lockIconTexture;
            set
            {
                m_lockIconTexture = value;
                foreach (TextureDisplay textureDisplay in m_elements)
                {
                    textureDisplay.lockIconTexture = m_lockIconTexture;
                }
            }
        }

        public void UpdateFields()
        {
            m_serializedTileSet.Update();

            foreach (TextureDisplay textureDisplay in m_elements)
            {
                textureDisplay.UpdateFields();
            }
        }

        public void OnBrushTypeChanged(PaintControl.BrushType brushType)
        {
            foreach (TextureDisplay textureDisplay in m_elements)
            {
                textureDisplay.OnBrushTypeChanged(brushType);
            }
        }

        public TextureSetDisplay()
        {
            m_scale = EditorPrefs.GetFloat("Zlitz.Extra2D.BetterTile.textureSetDisplayScale", 1.0f);

            RegisterCallback<DragUpdatedEvent>(e => OnDragUpdated());
            RegisterCallback<DragPerformEvent>(e => OnDragPerform());
            RegisterCallback<WheelEvent>(e =>
            {
                if (!e.ctrlKey || e.delta.y == 0.0f)
                {
                    return;
                }

                m_scale = Mathf.Clamp(m_scale - e.delta.y * 0.05f, s_minScale, s_maxScale);
                EditorPrefs.SetFloat("Zlitz.Extra2D.BetterTile.textureSetDisplayScale", m_scale);

                foreach (TextureDisplay textureDisplay in m_elements)
                {
                    textureDisplay.Scale(m_scale);
                }

                EditorApplication.delayCall += () =>
                {
                    style.height = contentRect.height - 1.0f;
                    style.flexGrow = 0.0f;
                    MarkDirtyRepaint();

                    EditorApplication.delayCall += () =>
                    {
                        style.height = StyleKeyword.Auto;
                        style.flexGrow = 1.0f;
                        MarkDirtyRepaint();
                    };
                };
            });

            RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0)
                {
                    m_paintControl.painting = true;
                }
            });
            RegisterCallback<MouseUpEvent>(e =>
            {
                m_paintControl.painting = false;
            });
            RegisterCallback<MouseLeaveEvent>(e =>
            {
                m_paintControl.painting = false;
            });
        }

        private void ValidateSpriteEntries()
        {
            if (m_tileSet == null)
            {
                return;
            }

            HashSet<int> invalidIndices = new HashSet<int>();
            for (int i = 0; i < m_spriteEntriesProperty.arraySize; i++)
            {
                invalidIndices.Add(i);
            }

            for (int textureIndex = 0; textureIndex < m_texturesProperty.arraySize; textureIndex++)
            {
                SerializedProperty textureProperty = m_texturesProperty.GetArrayElementAtIndex(textureIndex);
                Texture2D texture = textureProperty.objectReferenceValue as Texture2D;

                if (texture == null)
                {
                    continue;
                }

                Sprite[] textureSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>().ToArray();
                foreach (Sprite sprite in textureSprites)
                {
                    // Search for existing sprite entry of matching sprite

                    int foundIndex = -1;
                    for (int spriteEntryIndex = 0; spriteEntryIndex < m_spriteEntriesProperty.arraySize; spriteEntryIndex++)
                    {
                        SerializedProperty entryProperty = m_spriteEntriesProperty.GetArrayElementAtIndex(spriteEntryIndex);

                        SerializedProperty spriteProperty = entryProperty.FindPropertyRelative("m_sprite");
                        Sprite currentSprite = spriteProperty.objectReferenceValue as Sprite;

                        if (currentSprite == sprite)
                        {
                            foundIndex = spriteEntryIndex;
                            break;
                        }
                    }

                    // If no entry exist, create new one and append to entry list

                    if (foundIndex >= 0)
                    {
                        invalidIndices.Remove(foundIndex);
                    }
                    else
                    {
                        int index = m_spriteEntriesProperty.arraySize;
                        m_spriteEntriesProperty.InsertArrayElementAtIndex(index);

                        SerializedProperty newEntryProperty = m_spriteEntriesProperty.GetArrayElementAtIndex(index);

                        // Initialize new sprite entry

                        SerializedProperty newSpriteProperty = newEntryProperty.FindPropertyRelative("m_sprite");
                        newSpriteProperty.objectReferenceValue = sprite;

                        SerializedProperty newWeightProperty = newEntryProperty.FindPropertyRelative("m_weight");
                        newWeightProperty.floatValue = 1.0f;

                        SerializedProperty newTileProperty = newEntryProperty.FindPropertyRelative("m_tile");
                        newTileProperty.objectReferenceValue = null;

                        SerializedProperty newRuleProperty = newEntryProperty.FindPropertyRelative("m_rule");

                        SerializedProperty newTopLeftProperty = newRuleProperty.FindPropertyRelative("m_topLeft");
                        newTopLeftProperty.objectReferenceValue = null;

                        SerializedProperty newTopProperty = newRuleProperty.FindPropertyRelative("m_top");
                        newTopProperty.objectReferenceValue = null;

                        SerializedProperty newTopRightProperty = newRuleProperty.FindPropertyRelative("m_topRight");
                        newTopRightProperty.objectReferenceValue = null;

                        SerializedProperty newleftProperty = newRuleProperty.FindPropertyRelative("m_left");
                        newleftProperty.objectReferenceValue = null;

                        SerializedProperty newRightProperty = newRuleProperty.FindPropertyRelative("m_right");
                        newRightProperty.objectReferenceValue = null;

                        SerializedProperty newBottomLeftProperty = newRuleProperty.FindPropertyRelative("m_bottomLeft");
                        newBottomLeftProperty.objectReferenceValue = null;

                        SerializedProperty newBottomProperty = newRuleProperty.FindPropertyRelative("m_bottom");
                        newBottomProperty.objectReferenceValue = null;

                        SerializedProperty newBottomRightProperty = newRuleProperty.FindPropertyRelative("m_bottomRight");
                        newBottomRightProperty.objectReferenceValue = null;
                    }
                }
            }

            int[] remainingInvalidIndices = invalidIndices.OrderByDescending(i => i).ToArray();
            foreach (int invalidIndex in remainingInvalidIndices)
            {
                m_spriteEntriesProperty.DeleteArrayElementAtIndex(invalidIndex);
            }

            m_serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
        }

        private void UpdateElements()
        {
            m_scale = EditorPrefs.GetFloat("Zlitz.Extra2D.BetterTile.textureSetDisplayScale", 1.0f);

            Clear();
            m_elements.Clear();

            EditorApplication.delayCall += () =>
            {
                style.height = contentRect.height - 1.0f;
                style.flexGrow = 0.0f;
                MarkDirtyRepaint();

                EditorApplication.delayCall += () =>
                {
                    style.height = StyleKeyword.Auto;
                    style.flexGrow = 1.0f;
                    MarkDirtyRepaint();
                };
            };

            if (m_texturesProperty != null)
            {
                for (int i = 0; i < m_texturesProperty.arraySize; i++)
                {
                    SerializedProperty textureProperty = m_texturesProperty.GetArrayElementAtIndex(i);
                    
                    if (textureProperty.objectReferenceValue is Texture2D texture)
                    {
                        int index = i;
                        TextureDisplay newElemenet = new TextureDisplay(texture, () =>
                        {
                            EditorApplication.delayCall += () =>
                            {
                                m_texturesProperty.DeleteArrayElementAtIndex(index);
                                m_serializedTileSet.ApplyModifiedPropertiesWithoutUndo();

                                ValidateSpriteEntries();
                                UpdateElements();
                            };
                        }, UpdateElements, m_scale, m_spriteEntriesProperty);
                        newElemenet.paintControl = m_paintControl;
                        newElemenet.lockIconTexture = m_lockIconTexture;
                        Add(newElemenet);
                        m_elements.Add(newElemenet);
                    }
                }
            }

            if (m_elements.Count <= 0)
            {
                m_message = new Label();
                m_message.text = "Drag sliced textures here...";

                m_message.style.flexGrow                = 1.0f;
                m_message.style.fontSize                = 18.0f;
                m_message.style.unityTextAlign          = TextAnchor.MiddleCenter;
                m_message.style.unityFontStyleAndWeight = FontStyle.Italic;
                Add(m_message);
            }
        }

        private void OnDragUpdated()
        {
            if (m_texturesProperty == null || !ValidateDragAction(out Texture2D[] textures))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }

        private void OnDragPerform()
        {
            if (m_texturesProperty == null || !ValidateDragAction(out Texture2D[] textures))
            {
                return;
            }

            foreach (Texture2D texture in textures)
            {
                bool repeated = false;
                for (int i = 0; i < m_texturesProperty.arraySize; i++)
                {
                    Texture existingTexture = m_texturesProperty.GetArrayElementAtIndex(i).objectReferenceValue as Texture2D;
                    if (existingTexture == texture)
                    {
                        repeated = true;
                        break;
                    }
                }

                if (repeated)
                {
                    continue;
                }

                int index = m_texturesProperty.arraySize;
                m_texturesProperty.InsertArrayElementAtIndex(index);

                SerializedProperty newTextureProperty = m_texturesProperty.GetArrayElementAtIndex(index);
                newTextureProperty.objectReferenceValue = texture;

                m_serializedTileSet.ApplyModifiedPropertiesWithoutUndo();

                onNewTextureAdded?.Invoke();
            }

            m_serializedTileSet.ApplyModifiedPropertiesWithoutUndo();

            ValidateSpriteEntries();
            UpdateElements();
        }

        private bool ValidateDragAction(out Texture2D[] textures)
        {
            textures = null;

            if (DragAndDrop.objectReferences.Length <= 0)
            {
                return false;
            }

            List<Texture2D> textureList = new List<Texture2D>();
            foreach (UnityEngine.Object objectReference in DragAndDrop.objectReferences)
            {
                if (objectReference is not Texture2D texture)
                {
                    return false;
                }
                textureList.Add(texture);
            }

            textures = textureList.ToArray();
            return true;
        }
    }

    internal class TextureDisplay : VisualElement
    {
        private SerializedProperty m_spriteEntriesProperty;

        private Texture2D    m_texture;
        private Sprite[]     m_sprites;
        private PaintControl m_paintControl;

        private Action    m_onRemove;
        private Action    m_onSize;

        private float m_scale;

        private VisualElement m_texContainer;

        private List<SpriteDisplay> m_elements = new List<SpriteDisplay>();

        private Texture2D m_lockIconTexture;

        public event Action<float> onScale;

        public PaintControl paintControl
        {
            get => m_paintControl;
            set
            {
                m_paintControl = value;
                foreach (SpriteDisplay spriteDisplay in m_elements)
                {
                    spriteDisplay.paintControl = m_paintControl;
                }
            }
        }

        public Texture2D lockIconTexture
        {
            get => m_lockIconTexture;
            set
            {
                m_lockIconTexture = value;
                foreach (SpriteDisplay spriteDisplay in m_elements)
                {
                    spriteDisplay.lockIconTexture = m_lockIconTexture;
                }
            }
        }

        public void UpdateFields()
        {
            foreach (SpriteDisplay spriteDisplay in m_elements)
            {
                spriteDisplay.UpdateFields();
            }
        }

        public void Scale(float scale)
        {
            float ppu = m_sprites[0]?.pixelsPerUnit ?? 32.0f;

            m_scale = scale;
            m_texContainer.style.width  = m_texture.width * m_scale * 32.0f / ppu;
            m_texContainer.style.height = m_texture.height * m_scale * 32.0f / ppu;
            onScale?.Invoke(m_scale);
        }

        public void OnBrushTypeChanged(PaintControl.BrushType brushType)
        {
            foreach (SpriteDisplay spriteDisplay in m_elements)
            {
                spriteDisplay.OnBrushTypeChanged(brushType);
            }
        }

        public TextureDisplay(Texture2D texture, Action onRemove, Action onSize, float initialScale, SerializedProperty spriteEntriesProperty)
        {
            m_spriteEntriesProperty = spriteEntriesProperty;

            m_texture  = texture;
            m_sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>().ToArray();

            m_onRemove = onRemove;
            m_onSize   = onSize;

            m_scale = initialScale;

            AddToClassList("dark-gray");
            AddToClassList("enclosed");

            Rebuild();
        }

        private void Rebuild()
        {
            Clear();
            m_elements.Clear();
            onScale = null;

            float ppu = m_sprites[0]?.pixelsPerUnit ?? 32.0f;

            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            Add(container);

            Foldout foldout = new Foldout()
            {
                text = m_texture.name,
                value = EditorPrefs.GetBool($"Zlitz.Extra2D.BetterTile.textureFoldout_{m_texture.name}", true)
            };
            foldout.style.flexGrow = 1.0f;
            foldout.style.height = StyleKeyword.Auto;
            foldout.style.overflow = Overflow.Hidden;
            foldout.style.alignItems = Align.Stretch;
            foldout.RegisterValueChangedCallback(e =>
            {
                EditorPrefs.SetBool($"Zlitz.Extra2D.BetterTile.textureFoldout_{m_texture.name}", e.newValue);
                m_onSize?.Invoke();
            });
            container.Add(foldout);
            onScale += (s) =>
            {
                foldout.MarkDirtyRepaint();
            };

            Button removeButton = new Button()
            {
                text = "\u0058"
            };
            removeButton.AddToClassList("remove-button");
            removeButton.clicked += m_onRemove;
            removeButton.style.width = 24.0f;
            removeButton.style.height = 24.0f;
            container.Add(removeButton);

            m_texContainer = new VisualElement();
            m_texContainer.style.marginBottom = 8.0f;
            m_texContainer.style.marginTop = 8.0f;
            m_texContainer.style.marginLeft = 8.0f;
            m_texContainer.style.marginRight = 8.0f;
            m_texContainer.style.borderBottomColor = new Color(0.77f, 0.77f, 0.77f, 1.0f);
            m_texContainer.style.borderTopColor = new Color(0.77f, 0.77f, 0.77f, 1.0f);
            m_texContainer.style.borderLeftColor = new Color(0.77f, 0.77f, 0.77f, 1.0f);
            m_texContainer.style.borderRightColor = new Color(0.77f, 0.77f, 0.77f, 1.0f);
            m_texContainer.style.borderBottomWidth = 1.0f;
            m_texContainer.style.borderTopWidth = 1.0f;
            m_texContainer.style.borderLeftWidth = 1.0f;
            m_texContainer.style.borderRightWidth = 1.0f;
            m_texContainer.style.width = m_texture.width * m_scale * 32.0f / ppu;
            m_texContainer.style.height = m_texture.height * m_scale  * 32.0f / ppu;
            m_texContainer.style.flexGrow = 0.0f;
            m_texContainer.style.flexShrink = 0.0f;
            m_texContainer.style.backgroundColor = new Color(0.28f, 0.28f, 0.28f, 1.0f);
            foldout.Add(m_texContainer);

            VisualElement tex = new VisualElement();
            tex.style.flexGrow = 1.0f;
            tex.style.backgroundImage = m_texture;
            m_texContainer.Add(tex);

            foreach (Sprite sprite in m_sprites)
            {
                SerializedProperty matchingSpriteEntryProperty = null;

                for (int i = 0; i < m_spriteEntriesProperty.arraySize; i++)
                {
                    SerializedProperty entryProperty = m_spriteEntriesProperty.GetArrayElementAtIndex(i);

                    SerializedProperty spriteProperty = entryProperty.FindPropertyRelative("m_sprite");
                    Sprite currentSprite = spriteProperty.objectReferenceValue as Sprite;

                    if (currentSprite == sprite)
                    {
                        matchingSpriteEntryProperty = entryProperty;
                        break;
                    }
                }

                Debug.Assert(matchingSpriteEntryProperty != null, "No matching sprite entry available.");

                SpriteDisplay spriteDisplay = new SpriteDisplay(sprite, this, m_scale, matchingSpriteEntryProperty, m_paintControl);
                spriteDisplay.lockIconTexture = m_lockIconTexture;
                tex.Add(spriteDisplay);
                m_elements.Add(spriteDisplay);
            }
        }
    }

    internal class SpriteDisplay : VisualElement
    {
        private Sprite       m_sprite;
        private Texture2D    m_texture;
        private PaintControl m_paintControl;

        private VisualElement m_tileOutline;

        private VisualElement m_tilePainting;
        private VisualElement m_connectionPainting;
        private VisualElement m_weightPainting;

        private Label       m_weightDisplay;
        private RuleDisplay m_ruleDisplay;

        private SerializedProperty m_entryProperty;

        private SerializedProperty m_spriteProperty;
        private SerializedProperty m_tileProperty;
        private SerializedProperty m_ruleProperty;
        private SerializedProperty m_weightProperty;

        private Texture2D m_lockIconTexture;

        public PaintControl paintControl
        {
            get => m_paintControl;
            set
            {
                m_paintControl = value;
                if (m_paintControl != null)
                {
                    OnBrushTypeChanged(paintControl.brushType);
                }
                m_ruleDisplay.paintControl = m_paintControl;
            }
        }

        public Texture2D lockIconTexture
        {
            get => m_lockIconTexture;
            set
            {
                m_lockIconTexture = value;
                m_ruleDisplay.lockIconTexture = m_lockIconTexture;
            }
        }

        public void OnBrushTypeChanged(PaintControl.BrushType brushType)
        {
            m_tilePainting.style.display       = brushType == PaintControl.BrushType.Tile ? DisplayStyle.Flex : DisplayStyle.None;
            m_connectionPainting.style.display = brushType == PaintControl.BrushType.Connection ? DisplayStyle.Flex : DisplayStyle.None;
            m_weightPainting.style.display     = brushType == PaintControl.BrushType.Weight ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void UpdateFields()
        {
            Color tileColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (m_tileProperty.objectReferenceValue is UnityEngine.Object obj)
            {
                tileColor = new SerializedObject(obj).FindProperty("m_color").colorValue;
                tileColor.a = 0.5f;
            }

            m_tilePainting.style.backgroundColor   = tileColor;
            m_weightPainting.style.backgroundColor = tileColor;

            m_ruleDisplay.UpdateFields();

            m_weightDisplay.text = WeightToString(m_weightProperty.floatValue);
        }

        public SpriteDisplay(Sprite sprite, TextureDisplay textureDisplay, float initialScale, SerializedProperty entryProperty, PaintControl paintControl)
        {
            m_sprite  = sprite;
            m_texture = m_sprite.texture;

            m_paintControl = paintControl;
            if (m_paintControl != null)
            {
                OnBrushTypeChanged(paintControl.brushType);
            }

            m_entryProperty = entryProperty;

            m_spriteProperty = m_entryProperty.FindPropertyRelative("m_sprite");
            m_tileProperty   = m_entryProperty.FindPropertyRelative("m_tile");
            m_ruleProperty   = m_entryProperty.FindPropertyRelative("m_rule");
            m_weightProperty = m_entryProperty.FindPropertyRelative("m_weight");

            Rect  spriteRect = m_sprite.rect;
            float ppu        = m_sprite.pixelsPerUnit;

            style.borderBottomWidth = 1.0f;
            style.borderTopWidth    = 1.0f;
            style.borderLeftWidth   = 1.0f;
            style.borderRightWidth  = 1.0f;

            style.borderBottomColor = new Color(0.0f, 1.0f, 0.0f, 0.3f);
            style.borderTopColor    = new Color(0.0f, 1.0f, 0.0f, 0.3f);
            style.borderLeftColor   = new Color(0.0f, 1.0f, 0.0f, 0.3f);
            style.borderRightColor  = new Color(0.0f, 1.0f, 0.0f, 0.3f);

            style.position = Position.Absolute;

            style.top    = initialScale * 32.0f / ppu * (m_texture.height - spriteRect.height - spriteRect.y);
            style.left   = initialScale * 32.0f / ppu * spriteRect.x;
            style.width  = initialScale * 32.0f / ppu * spriteRect.width;
            style.height = initialScale * 32.0f / ppu * spriteRect.height;

            Vector2 pivot = sprite.pivot;

            m_tileOutline = new VisualElement();

            m_tileOutline.style.borderBottomWidth = 1.0f;
            m_tileOutline.style.borderTopWidth    = 1.0f;
            m_tileOutline.style.borderLeftWidth   = 1.0f;
            m_tileOutline.style.borderRightWidth  = 1.0f;

            m_tileOutline.style.borderBottomColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
            m_tileOutline.style.borderTopColor    = new Color(1.0f, 1.0f, 0.0f, 1.0f);
            m_tileOutline.style.borderLeftColor   = new Color(1.0f, 1.0f, 0.0f, 1.0f);
            m_tileOutline.style.borderRightColor  = new Color(1.0f, 1.0f, 0.0f, 1.0f);

            m_tileOutline.style.position = Position.Absolute;

            m_tileOutline.style.top    = initialScale * 32.0f / ppu * (spriteRect.height - pivot.y - 0.5f * ppu);
            m_tileOutline.style.left   = initialScale * 32.0f / ppu * (pivot.x - 0.5f * ppu);
            m_tileOutline.style.width  = initialScale * 32.0f / ppu * ppu;
            m_tileOutline.style.height = initialScale * 32.0f / ppu * ppu;

            Add(m_tileOutline);

            m_tilePainting = new VisualElement();
            m_tilePainting.style.position          = Position.Absolute;
            m_tilePainting.style.width             = Length.Percent(100.0f);
            m_tilePainting.style.height            = Length.Percent(100.0f);
            m_tilePainting.style.borderBottomColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            m_tilePainting.style.borderTopColor    = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            m_tilePainting.style.borderLeftColor   = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            m_tilePainting.style.borderRightColor  = new Color(0.0f, 1.0f, 1.0f, 1.0f);

            m_tilePainting.RegisterCallback<MouseEnterEvent>(e =>
            {
                m_tilePainting.style.borderBottomWidth = 2.0f;
                m_tilePainting.style.borderTopWidth    = 2.0f;
                m_tilePainting.style.borderLeftWidth   = 2.0f;
                m_tilePainting.style.borderRightWidth  = 2.0f;

                if (m_paintControl != null && m_paintControl.painting && m_paintControl.paintingTile != null && m_paintControl.paintingTile is UnityEngine.Object tile)
                {
                    if (m_tileProperty.objectReferenceValue != tile)
                    {
                        m_tileProperty.objectReferenceValue = tile;
                        m_tileProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();

                        if (tile is TileDecorator decorator)
                        {
                            Sprite decoratorSprite = m_spriteProperty.objectReferenceValue as Sprite;

                            DecoratorTile decoratorTile = DecoratorTile.Create(decoratorSprite);
                            decoratorTile.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                            decoratorTile.name = $"Decorator_{decoratorSprite.name}";

                            AssetDatabase.AddObjectToAsset(decoratorTile, decorator);

                            SerializedObject serializedDecorator = new SerializedObject(decorator);

                            SerializedProperty tilesProperty = serializedDecorator.FindProperty("m_tiles");

                            int index = tilesProperty.arraySize;
                            tilesProperty.InsertArrayElementAtIndex(index);

                            SerializedProperty newTileProperty = tilesProperty.GetArrayElementAtIndex(index);
                            newTileProperty.objectReferenceValue = decoratorTile;

                            serializedDecorator.ApplyModifiedPropertiesWithoutUndo();

                            EditorUtility.SetDirty(decorator);
                            AssetDatabase.SaveAssetIfDirty(decorator);
                        }
                    }
                }
            });
            m_tilePainting.RegisterCallback<MouseLeaveEvent>(e =>
            {
                m_tilePainting.style.borderBottomWidth = 0.0f;
                m_tilePainting.style.borderTopWidth    = 0.0f;
                m_tilePainting.style.borderLeftWidth   = 0.0f;
                m_tilePainting.style.borderRightWidth  = 0.0f;
            });
            m_tilePainting.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0 && !e.shiftKey)
                {
                    if (m_paintControl != null && m_paintControl.paintingTile != null && m_paintControl.paintingTile is UnityEngine.Object tile)
                    {
                        if (m_tileProperty.objectReferenceValue != tile)
                        {
                            m_tileProperty.objectReferenceValue = tile;
                            m_tileProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                            UpdateFields();

                            if (tile is TileDecorator decorator)
                            {
                                Sprite decoratorSprite = m_spriteProperty.objectReferenceValue as Sprite;

                                DecoratorTile decoratorTile = DecoratorTile.Create(decoratorSprite);
                                decoratorTile.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                                decoratorTile.name = $"Decorator_{decoratorSprite.name}";

                                AssetDatabase.AddObjectToAsset(decoratorTile, decorator);

                                SerializedObject serializedDecorator = new SerializedObject(decorator);

                                SerializedProperty tilesProperty = serializedDecorator.FindProperty("m_tiles");

                                int index = tilesProperty.arraySize;
                                tilesProperty.InsertArrayElementAtIndex(index);

                                SerializedProperty newTileProperty = tilesProperty.GetArrayElementAtIndex(index);
                                newTileProperty.objectReferenceValue = decoratorTile;

                                serializedDecorator.ApplyModifiedPropertiesWithoutUndo();

                                EditorUtility.SetDirty(decorator);
                                AssetDatabase.SaveAssetIfDirty(decorator);
                            }
                        }
                    }
                }
                else if (e.button == 0 && e.shiftKey)
                {
                    TileDecorator decorator = m_tileProperty.objectReferenceValue as TileDecorator;

                    m_tileProperty.objectReferenceValue = null;
                    m_tileProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();

                    if (decorator != null)
                    {
                        Sprite decoratorSprite = m_spriteProperty.objectReferenceValue as Sprite;

                        SerializedObject serializedDecorator = new SerializedObject(decorator);

                        SerializedProperty tilesProperty = serializedDecorator.FindProperty("m_tiles");

                        int index = -1;
                        DecoratorTile tileToRemove = null;
                        for (int i = 0; i < tilesProperty.arraySize; i++)
                        {
                            SerializedProperty tileProperty = tilesProperty.GetArrayElementAtIndex(i);
                            DecoratorTile tile = tileProperty.objectReferenceValue as DecoratorTile;
                            if (tile != null && tile.sprite == decoratorSprite)
                            {
                                index = i;
                                tileToRemove = tile;
                                break;
                            }
                        }

                        if (index >= 0)
                        {
                            tilesProperty.DeleteArrayElementAtIndex(index);

                            AssetDatabase.RemoveObjectFromAsset(tileToRemove);

                            serializedDecorator.ApplyModifiedPropertiesWithoutUndo();

                            EditorUtility.SetDirty(decorator);
                            AssetDatabase.SaveAssetIfDirty(decorator);
                        }
                    }
                }
            });

            m_tileOutline.Add(m_tilePainting);

            m_connectionPainting = new VisualElement();
            m_connectionPainting.style.position = Position.Absolute;
            m_connectionPainting.style.width    = Length.Percent(100.0f);
            m_connectionPainting.style.height   = Length.Percent(100.0f);

            m_ruleDisplay = new RuleDisplay(m_ruleProperty, m_tileProperty, m_paintControl);
            m_ruleDisplay.style.flexGrow = 1.0f;
            m_ruleDisplay.lockIconTexture = m_lockIconTexture;
            m_connectionPainting.Add(m_ruleDisplay);

            m_tileOutline.Add(m_connectionPainting);

            m_weightPainting = new VisualElement();
            
            m_weightPainting.style.position          = Position.Absolute;
            m_weightPainting.style.width             = Length.Percent(100.0f);
            m_weightPainting.style.height            = Length.Percent(100.0f);
            m_weightPainting.style.borderBottomColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            m_weightPainting.style.borderTopColor    = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            m_weightPainting.style.borderLeftColor   = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            m_weightPainting.style.borderRightColor  = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            m_weightPainting.RegisterCallback<MouseEnterEvent>(e =>
            {
                m_weightPainting.style.borderBottomWidth = 2.0f;
                m_weightPainting.style.borderTopWidth    = 2.0f;
                m_weightPainting.style.borderLeftWidth   = 2.0f;
                m_weightPainting.style.borderRightWidth  = 2.0f;

                if (m_paintControl != null && m_paintControl.painting)
                {
                    m_weightProperty.floatValue = m_paintControl.weight;
                    m_weightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                }
            });
            m_weightPainting.RegisterCallback<MouseLeaveEvent>(e =>
            {
                m_weightPainting.style.borderBottomWidth = 0.0f;
                m_weightPainting.style.borderTopWidth    = 0.0f;
                m_weightPainting.style.borderLeftWidth   = 0.0f;
                m_weightPainting.style.borderRightWidth  = 0.0f;
            });
            m_weightPainting.RegisterCallback<ClickEvent>(e =>
            {
                if (m_paintControl != null)
                {
                    m_weightProperty.floatValue = m_paintControl.weight;
                    m_weightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                }
            });

            m_weightDisplay = new Label();
            m_weightDisplay.style.flexGrow                = 1.0f;
            m_weightDisplay.style.unityTextAlign          = TextAnchor.MiddleCenter;
            m_weightDisplay.style.color                   = new Color(0.77f, 0.77f, 0.77f);
            m_weightDisplay.style.fontSize                = initialScale * 12.0f;
            m_weightDisplay.style.unityFontStyleAndWeight = FontStyle.Bold;
            m_weightDisplay.style.unityTextOutlineColor   = new Color(0.2f, 0.2f, 0.2f);
            m_weightDisplay.style.unityTextOutlineWidth   = 1.0f;

            m_weightPainting.Add(m_weightDisplay);

            m_tileOutline.Add(m_weightPainting);

            textureDisplay.onScale += (scale) =>
            {
                style.top    = scale * 32.0f / ppu * (m_texture.height - spriteRect.height - spriteRect.y);
                style.left   = scale * 32.0f / ppu * spriteRect.x;
                style.width  = scale * 32.0f / ppu * spriteRect.width;
                style.height = scale * 32.0f / ppu * spriteRect.height;

                m_tileOutline.style.top    = scale * 32.0f / ppu * (spriteRect.height - pivot.y - 0.5f * ppu);
                m_tileOutline.style.left   = scale * 32.0f / ppu * (pivot.x - 0.5f * ppu);
                m_tileOutline.style.width  = scale * 32.0f / ppu * ppu;
                m_tileOutline.style.height = scale * 32.0f / ppu * ppu;

                m_weightDisplay.style.fontSize = scale * 12.0f;
            };

            UpdateFields();
        }

        private static string WeightToString(float weight)
        {
            return weight.ToString("0.##");
        }
    }

    internal class RuleDisplay : VisualElement
    {
        private PaintControl m_paintControl;

        private SerializedProperty m_ruleProperty;
        private SerializedProperty m_tileProperty;

        private SerializedProperty m_topLeftProperty;
        private SerializedProperty m_topProperty;
        private SerializedProperty m_topRightProperty;
        private SerializedProperty m_leftProperty;
        private SerializedProperty m_rightProperty;
        private SerializedProperty m_bottomLeftProperty;
        private SerializedProperty m_bottomProperty;
        private SerializedProperty m_bottomRightProperty;
        private SerializedProperty m_alwaysUsedProperty;

        private VisualElement m_tile;
        private VisualElement m_lock;

        private VisualElement m_ruleTopLeft;
        private VisualElement m_ruleTop;
        private VisualElement m_ruleTopRight;
        private VisualElement m_ruleLeft;
        private VisualElement m_ruleRight;
        private VisualElement m_ruleBottomLeft;
        private VisualElement m_ruleBottom;
        private VisualElement m_ruleBottomRight;

        private Texture2D m_lockIconTexture;

        public PaintControl paintControl
        {
            get => m_paintControl;
            set => m_paintControl = value;
        }

        public Texture2D lockIconTexture
        {
            get => m_lockIconTexture;
            set
            {
                m_lockIconTexture = value;
                m_lock.style.backgroundImage = m_lockIconTexture;
            }
        }

        public void UpdateFields()
        {
            UnityEngine.Object currentTile = m_tileProperty.objectReferenceValue;

            Color tileColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (currentTile != null)
            {
                tileColor = new SerializedObject(currentTile).FindProperty("m_color").colorValue;
                tileColor.a = 0.5f;
            }

            m_tile.style.backgroundColor = tileColor;

            m_lock.style.backgroundImage = m_lockIconTexture;
            m_lock.style.unityBackgroundImageTintColor = new Color(
                tileColor.r < 0.5f ? 1.0f : 0.0f,
                tileColor.g < 0.5f ? 1.0f : 0.0f,
                tileColor.b < 0.5f ? 1.0f : 0.0f, 
                m_alwaysUsedProperty.boolValue ? 0.6f : 0.0f
            );

            m_lock.tooltip = m_alwaysUsedProperty.boolValue
                ? "This sprite will always be added to the sprite pool, even if there's a more specific rule."
                : "This sprite might not be used if there's a more specific rule.";

            Color ruleFilterColor;

            ruleFilterColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (m_topLeftProperty.objectReferenceValue is ITileFilter || m_topLeftProperty.objectReferenceValue is ITileIdentifier)
            {
                ruleFilterColor = new SerializedObject(m_topLeftProperty.objectReferenceValue).FindProperty("m_color").colorValue;
                ruleFilterColor.a = 0.5f;
            }
            m_ruleTopLeft.style.backgroundColor = ruleFilterColor;

            ruleFilterColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (m_topProperty.objectReferenceValue is ITileFilter || m_topProperty.objectReferenceValue is ITileIdentifier)
            {
                ruleFilterColor = new SerializedObject(m_topProperty.objectReferenceValue).FindProperty("m_color").colorValue;
                ruleFilterColor.a = 0.5f;
            }
            m_ruleTop.style.backgroundColor = ruleFilterColor;

            ruleFilterColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (m_topRightProperty.objectReferenceValue is ITileFilter || m_topRightProperty.objectReferenceValue is ITileIdentifier)
            {
                ruleFilterColor = new SerializedObject(m_topRightProperty.objectReferenceValue).FindProperty("m_color").colorValue;
                ruleFilterColor.a = 0.5f;
            }
            m_ruleTopRight.style.backgroundColor = ruleFilterColor;

            ruleFilterColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (m_leftProperty.objectReferenceValue is ITileFilter || m_leftProperty.objectReferenceValue is ITileIdentifier)
            {
                ruleFilterColor = new SerializedObject(m_leftProperty.objectReferenceValue).FindProperty("m_color").colorValue;
                ruleFilterColor.a = 0.5f;
            }
            m_ruleLeft.style.backgroundColor = ruleFilterColor;

            ruleFilterColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (m_rightProperty.objectReferenceValue is ITileFilter || m_rightProperty.objectReferenceValue is ITileIdentifier)
            {
                ruleFilterColor = new SerializedObject(m_rightProperty.objectReferenceValue).FindProperty("m_color").colorValue;
                ruleFilterColor.a = 0.5f;
            }
            m_ruleRight.style.backgroundColor = ruleFilterColor;

            ruleFilterColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (m_bottomLeftProperty.objectReferenceValue is ITileFilter || m_bottomLeftProperty.objectReferenceValue is ITileIdentifier)
            {
                ruleFilterColor = new SerializedObject(m_bottomLeftProperty.objectReferenceValue).FindProperty("m_color").colorValue;
                ruleFilterColor.a = 0.5f;
            }
            m_ruleBottomLeft.style.backgroundColor = ruleFilterColor;

            ruleFilterColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (m_bottomProperty.objectReferenceValue is ITileFilter || m_bottomProperty.objectReferenceValue is ITileIdentifier)
            {
                ruleFilterColor = new SerializedObject(m_bottomProperty.objectReferenceValue).FindProperty("m_color").colorValue;
                ruleFilterColor.a = 0.5f;
            }
            m_ruleBottom.style.backgroundColor = ruleFilterColor;

            ruleFilterColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            if (m_bottomRightProperty.objectReferenceValue is ITileFilter || m_bottomRightProperty.objectReferenceValue is ITileIdentifier)
            {
                ruleFilterColor = new SerializedObject(m_bottomRightProperty.objectReferenceValue).FindProperty("m_color").colorValue;
                ruleFilterColor.a = 0.5f;
            }
            m_ruleBottomRight.style.backgroundColor = ruleFilterColor;
        }

        public RuleDisplay(SerializedProperty ruleProperty, SerializedProperty tileProperty, PaintControl paintControl)
        {
            m_paintControl = paintControl;

            m_ruleProperty = ruleProperty;
            m_tileProperty = tileProperty;

            m_topLeftProperty     = m_ruleProperty.FindPropertyRelative("m_topLeft");
            m_topProperty         = m_ruleProperty.FindPropertyRelative("m_top");
            m_topRightProperty    = m_ruleProperty.FindPropertyRelative("m_topRight");
            m_leftProperty        = m_ruleProperty.FindPropertyRelative("m_left");
            m_rightProperty       = m_ruleProperty.FindPropertyRelative("m_right");
            m_bottomLeftProperty  = m_ruleProperty.FindPropertyRelative("m_bottomLeft");
            m_bottomProperty      = m_ruleProperty.FindPropertyRelative("m_bottom");
            m_bottomRightProperty = m_ruleProperty.FindPropertyRelative("m_bottomRight");
            m_alwaysUsedProperty  = m_ruleProperty.FindPropertyRelative("m_alwaysUsed");

            m_tile = new VisualElement();
            m_tile.style.position        = Position.Absolute;
            m_tile.style.left            = Length.Percent(25.0f);
            m_tile.style.top             = Length.Percent(25.0f);
            m_tile.style.width           = Length.Percent(50.0f);
            m_tile.style.height          = Length.Percent(50.0f);
            
            Add(m_tile);

            m_lock = new VisualElement();

            m_lock.style.position = Position.Absolute;
            m_lock.style.bottom   = 0.0f;
            m_lock.style.right    = 0.0f;
            m_lock.style.width    = Length.Percent(35.0f);
            m_lock.style.height   = Length.Percent(35.0f);

            m_lock.style.borderBottomWidth = 1.0f;
            m_lock.style.borderTopWidth    = 1.0f;
            m_lock.style.borderLeftWidth   = 1.0f;
            m_lock.style.borderRightWidth  = 1.0f;

            m_lock.style.borderBottomColor = new Color(0.0f, 1.0f, 1.0f, 0.2f);
            m_lock.style.borderTopColor    = new Color(0.0f, 1.0f, 1.0f, 0.2f);
            m_lock.style.borderLeftColor   = new Color(0.0f, 1.0f, 1.0f, 0.2f);
            m_lock.style.borderRightColor  = new Color(0.0f, 1.0f, 1.0f, 0.2f);

            m_lock.RegisterCallback<MouseEnterEvent>(e =>
            {
                m_lock.style.borderBottomWidth = 2.0f;
                m_lock.style.borderTopWidth    = 2.0f;
                m_lock.style.borderLeftWidth   = 2.0f;
                m_lock.style.borderRightWidth  = 2.0f;

                m_lock.style.borderBottomColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
                m_lock.style.borderTopColor    = new Color(0.0f, 1.0f, 1.0f, 1.0f);
                m_lock.style.borderLeftColor   = new Color(0.0f, 1.0f, 1.0f, 1.0f);
                m_lock.style.borderRightColor  = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            });
            m_lock.RegisterCallback<MouseLeaveEvent>(e =>
            {
                m_lock.style.borderBottomWidth = 1.0f;
                m_lock.style.borderTopWidth    = 1.0f;
                m_lock.style.borderLeftWidth   = 1.0f;
                m_lock.style.borderRightWidth  = 1.0f;

                m_lock.style.borderBottomColor = new Color(0.0f, 1.0f, 1.0f, 0.2f);
                m_lock.style.borderTopColor    = new Color(0.0f, 1.0f, 1.0f, 0.2f);
                m_lock.style.borderLeftColor   = new Color(0.0f, 1.0f, 1.0f, 0.2f);
                m_lock.style.borderRightColor  = new Color(0.0f, 1.0f, 1.0f, 0.2f);
            });
            m_lock.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0)
                {
                    m_alwaysUsedProperty.boolValue = !m_alwaysUsedProperty.boolValue;
                    m_alwaysUsedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                }
            });

            m_tile.Add(m_lock);

            m_ruleTopLeft = CreatePaintable(
                () =>
                {
                    if (m_paintControl != null && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_topLeftProperty.objectReferenceValue = filter;
                        m_topLeftProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                },
                () =>
                {
                    m_topLeftProperty.objectReferenceValue = null;
                    m_topLeftProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                },
                () =>
                {
                    if (m_paintControl != null && m_paintControl.painting && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_topLeftProperty.objectReferenceValue = filter;
                        m_topLeftProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                }
            );
            m_ruleTopLeft.style.top    = Length.Percent(0.0f);
            m_ruleTopLeft.style.left   = Length.Percent(0.0f);
            m_ruleTopLeft.style.width  = Length.Percent(25.0f);
            m_ruleTopLeft.style.height = Length.Percent(25.0f);
            Add(m_ruleTopLeft);

            m_ruleTop = CreatePaintable(
                () =>
                {
                    if (m_paintControl != null && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_topProperty.objectReferenceValue = filter;
                        m_topProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                },
                () =>
                {
                    m_topProperty.objectReferenceValue = null;
                    m_topProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                },
                () =>
                {
                    if (m_paintControl != null && m_paintControl.painting && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_topProperty.objectReferenceValue = filter;
                        m_topProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                }
            );
            m_ruleTop.style.top    = Length.Percent(0.0f);
            m_ruleTop.style.left   = Length.Percent(25.0f);
            m_ruleTop.style.width  = Length.Percent(50.0f);
            m_ruleTop.style.height = Length.Percent(25.0f);
            Add(m_ruleTop);

            m_ruleTopRight = CreatePaintable(
                () =>
                {
                    if (m_paintControl != null && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_topRightProperty.objectReferenceValue = filter;
                        m_topRightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                },
                () =>
                {
                    m_topRightProperty.objectReferenceValue = null;
                    m_topRightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                },
                () =>
                {
                    if (m_paintControl != null && m_paintControl.painting && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_topRightProperty.objectReferenceValue = filter;
                        m_topRightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                }
            );
            m_ruleTopRight.style.top    = Length.Percent(0.0f);
            m_ruleTopRight.style.left   = Length.Percent(75.0f);
            m_ruleTopRight.style.width  = Length.Percent(25.0f);
            m_ruleTopRight.style.height = Length.Percent(25.0f);
            Add(m_ruleTopRight);

            m_ruleLeft = CreatePaintable(
                () =>
                {
                    if (m_paintControl != null && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_leftProperty.objectReferenceValue = filter;
                        m_leftProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                },
                () =>
                {
                    m_leftProperty.objectReferenceValue = null;
                    m_leftProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                },
                () =>
                {
                    if (m_paintControl != null && m_paintControl.painting && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_leftProperty.objectReferenceValue = filter;
                        m_leftProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                }
            );
            m_ruleLeft.style.top    = Length.Percent(25.0f);
            m_ruleLeft.style.left   = Length.Percent(0.0f);
            m_ruleLeft.style.width  = Length.Percent(25.0f);
            m_ruleLeft.style.height = Length.Percent(50.0f);
            Add(m_ruleLeft);

            m_ruleRight = CreatePaintable(
                () =>
                {
                    if (m_paintControl != null && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_rightProperty.objectReferenceValue = filter;
                        m_rightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                },
                () =>
                {
                    m_rightProperty.objectReferenceValue = null;
                    m_rightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                },
                () =>
                {
                    if (m_paintControl != null && m_paintControl.painting && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_rightProperty.objectReferenceValue = filter;
                        m_rightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                }
            );
            m_ruleRight.style.top    = Length.Percent(25.0f);
            m_ruleRight.style.left   = Length.Percent(75.0f);
            m_ruleRight.style.width  = Length.Percent(25.0f);
            m_ruleRight.style.height = Length.Percent(50.0f);
            Add(m_ruleRight);

            m_ruleBottomLeft = CreatePaintable(
                () =>
                {
                    if (m_paintControl != null && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_bottomLeftProperty.objectReferenceValue = filter;
                        m_bottomLeftProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                },
                () =>
                {
                    m_bottomLeftProperty.objectReferenceValue = null;
                    m_bottomLeftProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                },
                () =>
                {
                    if (m_paintControl != null && m_paintControl.painting && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_bottomLeftProperty.objectReferenceValue = filter;
                        m_bottomLeftProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                }
            );
            m_ruleBottomLeft.style.top    = Length.Percent(75.0f);
            m_ruleBottomLeft.style.left   = Length.Percent(0.0f);
            m_ruleBottomLeft.style.width  = Length.Percent(25.0f);
            m_ruleBottomLeft.style.height = Length.Percent(25.0f);
            Add(m_ruleBottomLeft);

            m_ruleBottom = CreatePaintable(
                () =>
                {
                    if (m_paintControl != null && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_bottomProperty.objectReferenceValue = filter;
                        m_bottomProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                },
                () =>
                {
                    m_bottomProperty.objectReferenceValue = null;
                    m_bottomProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                },
                () =>
                {
                    if (m_paintControl != null && m_paintControl.painting && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_bottomProperty.objectReferenceValue = filter;
                        m_bottomProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                }
            );
            m_ruleBottom.style.top    = Length.Percent(75.0f);
            m_ruleBottom.style.left   = Length.Percent(25.0f);
            m_ruleBottom.style.width  = Length.Percent(50.0f);
            m_ruleBottom.style.height = Length.Percent(25.0f);
            Add(m_ruleBottom);

            m_ruleBottomRight = CreatePaintable(
                () =>
                {
                    if (m_paintControl != null && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_bottomRightProperty.objectReferenceValue = filter;
                        m_bottomRightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                },
                () =>
                {
                    m_bottomRightProperty.objectReferenceValue = null;
                    m_bottomRightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    UpdateFields();
                },
                () =>
                {
                    if (m_paintControl != null && m_paintControl.painting && m_paintControl.paintingFilter != null && m_paintControl.paintingFilter is UnityEngine.Object filter)
                    {
                        m_bottomRightProperty.objectReferenceValue = filter;
                        m_bottomRightProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                        UpdateFields();
                    }
                }
            );
            m_ruleBottomRight.style.top    = Length.Percent(75.0f);
            m_ruleBottomRight.style.left   = Length.Percent(75.0f);
            m_ruleBottomRight.style.width  = Length.Percent(25.0f);
            m_ruleBottomRight.style.height = Length.Percent(25.0f);
            Add(m_ruleBottomRight);

            UpdateFields();
        }

        private static VisualElement CreatePaintable(Action onPaint, Action onReset, Action onHover)
        {
            VisualElement element = new VisualElement();

            element.style.position = Position.Absolute;

            element.style.borderBottomWidth = 1.0f;
            element.style.borderTopWidth    = 1.0f;
            element.style.borderLeftWidth   = 1.0f;
            element.style.borderRightWidth  = 1.0f;

            element.style.borderBottomColor = new Color(0.0f, 1.0f, 1.0f, 0.2f);
            element.style.borderTopColor    = new Color(0.0f, 1.0f, 1.0f, 0.2f);
            element.style.borderLeftColor   = new Color(0.0f, 1.0f, 1.0f, 0.2f);
            element.style.borderRightColor  = new Color(0.0f, 1.0f, 1.0f, 0.2f);

            element.RegisterCallback<MouseEnterEvent>(e =>
            {
                element.style.borderBottomWidth = 2.0f;
                element.style.borderTopWidth    = 2.0f;
                element.style.borderLeftWidth   = 2.0f;
                element.style.borderRightWidth  = 2.0f;

                element.style.borderBottomColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
                element.style.borderTopColor    = new Color(0.0f, 1.0f, 1.0f, 1.0f);
                element.style.borderLeftColor   = new Color(0.0f, 1.0f, 1.0f, 1.0f);
                element.style.borderRightColor  = new Color(0.0f, 1.0f, 1.0f, 1.0f);

                onHover?.Invoke();
            });
            element.RegisterCallback<MouseLeaveEvent>(e =>
            {
                element.style.borderBottomWidth = 1.0f;
                element.style.borderTopWidth    = 1.0f;
                element.style.borderLeftWidth   = 1.0f;
                element.style.borderRightWidth  = 1.0f;

                element.style.borderBottomColor = new Color(0.0f, 1.0f, 1.0f, 0.2f);
                element.style.borderTopColor    = new Color(0.0f, 1.0f, 1.0f, 0.2f);
                element.style.borderLeftColor   = new Color(0.0f, 1.0f, 1.0f, 0.2f);
                element.style.borderRightColor  = new Color(0.0f, 1.0f, 1.0f, 0.2f);
            });
            element.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0 && !e.shiftKey)
                {
                    onPaint?.Invoke();
                }
                else if (e.button == 0 && e.shiftKey)
                {
                    onReset?.Invoke();
                }
            });

            return element;
        }
    }

    internal class PaintControl : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<PaintControl> { }

        private EnumField m_brushTypeField;
        public event Action<BrushType> onBrushTypeChanged;

        private VisualElement m_tilePaintingConfig;
        private Label m_currentTile;

        private ITileIdentifier m_paintingTile;

        public ITileIdentifier paintingTile => m_paintingTile;

        private VisualElement m_connectionPaintingConfig;
        private Label m_currentFilter;

        private ITileFilter m_paintingFilter;

        public ITileFilter paintingFilter => m_paintingFilter;

        private VisualElement m_weightPaintingConfig;
        private FloatField m_weight;

        private bool m_painting;

        public bool painting
        {
            get => m_painting;
            set => m_painting = value;
        }

        public float weight => m_weight.value;

        public BrushType brushType
        {
            get => (BrushType)m_brushTypeField.value;
            set => m_brushTypeField.value = value;
        }

        public void SelectTile(ITileIdentifier tile)
        {
            m_paintingTile = tile;
            m_currentTile.text = tile == null ? "" : tile is UnityEngine.Object obj ? obj.name : tile.ToString();

            Color color = Color.white;
            if (tile != null && tile is UnityEngine.Object unityObject)
            {
                color = new SerializedObject(unityObject).FindProperty("m_color").colorValue;
                color.a = 1.0f;
            }
            m_currentTile.style.color = color;
        }

        public void SelectFilter(ITileFilter filter)
        {
            m_paintingFilter = filter;
            m_currentFilter.text = filter == null ? "" : filter is UnityEngine.Object obj ? obj.name : filter.ToString();

            Color color = Color.white;
            if (filter != null && filter is UnityEngine.Object unityObject)
            {
                color = new SerializedObject(unityObject).FindProperty("m_color")?.colorValue ?? Color.white;
                color.a = 1.0f;
            }
            m_currentFilter.style.color = color;
        }

        public void Rebuild()
        {
            BrushType currentBrushType = m_brushTypeField == null ? BrushType.Tile : (BrushType)m_brushTypeField.value;

            Clear();

            m_brushTypeField = new EnumField("Brush type", currentBrushType);
            m_brushTypeField.RegisterValueChangedCallback(e =>
            {
                m_tilePaintingConfig.style.display       = (BrushType)m_brushTypeField.value == BrushType.Tile ? DisplayStyle.Flex : DisplayStyle.None;
                m_connectionPaintingConfig.style.display = (BrushType)m_brushTypeField.value == BrushType.Connection ? DisplayStyle.Flex : DisplayStyle.None;
                m_weightPaintingConfig.style.display     = (BrushType)m_brushTypeField.value == BrushType.Weight ? DisplayStyle.Flex : DisplayStyle.None;

                onBrushTypeChanged?.Invoke((BrushType)e.newValue);
            });
            Add(m_brushTypeField);

            m_tilePaintingConfig = new VisualElement();
            Add(m_tilePaintingConfig);

            m_connectionPaintingConfig = new VisualElement();
            Add(m_connectionPaintingConfig);

            m_weightPaintingConfig = new VisualElement();
            Add(m_weightPaintingConfig);

            m_tilePaintingConfig.style.display       = (BrushType)m_brushTypeField.value == BrushType.Tile ? DisplayStyle.Flex : DisplayStyle.None;
            m_connectionPaintingConfig.style.display = (BrushType)m_brushTypeField.value == BrushType.Connection ? DisplayStyle.Flex : DisplayStyle.None;
            m_weightPaintingConfig.style.display     = (BrushType)m_brushTypeField.value == BrushType.Weight ? DisplayStyle.Flex : DisplayStyle.None;

            m_currentTile = new Label();
            m_currentTile.style.marginLeft = 4.0f;
            m_currentTile.style.marginTop  = 4.0f;
            m_currentTile.style.unityFontStyleAndWeight = FontStyle.Bold;
            m_tilePaintingConfig.Add(m_currentTile);

            m_currentFilter = new Label();
            m_currentFilter.style.marginLeft = 4.0f;
            m_currentFilter.style.marginTop  = 4.0f;
            m_currentFilter.style.unityFontStyleAndWeight = FontStyle.Bold;
            m_connectionPaintingConfig.Add(m_currentFilter);

            m_weight = new FloatField("Weight");
            m_weight.value = EditorPrefs.GetFloat("Zlitz.Extra2D.BetterTile.paintControlWeight", 1.0f);
            m_weight.RegisterValueChangedCallback(e =>
            {
                float value = Mathf.Max(0.0f, e.newValue);
                m_weight.SetValueWithoutNotify(value);
                EditorPrefs.SetFloat("Zlitz.Extra2D.BetterTile.paintControlWeight", value);
            });
            m_weightPaintingConfig.Add(m_weight);
        }

        public PaintControl()
        {
            AddToClassList("dark-gray");
            Rebuild();
        }

        public enum BrushType
        {
            Tile,
            Connection,
            Weight
        }
    }
}
