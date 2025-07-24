using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using UnityTile   = UnityEngine.Tilemaps.Tile;
using UnityObject = UnityEngine.Object;

namespace Zlitz.Extra2D.BetterTile
{
    internal class TileSetEditorWindow : EditorWindow
    {
        #region Opening

        [MenuItem("Window/Zlitz/Tile Set Editor")]
        private static void Open()
        {
            Open(null);
        }

        internal static void Open(TileSet tileSet)
        {
            TileSetEditorWindow window = GetWindow<TileSetEditorWindow>();
            window.titleContent = new GUIContent("Tile Set Editor");
            window.saveChangesMessage = "There are unsaved changes. Would you like to save?";

            if (window.m_tileSet != tileSet)
            {
                if (window.hasUnsavedChanges)
                {
                    int option = EditorUtility.DisplayDialogComplex(
                    "Tile Set Editor - Unsaved Changes Detected",
                    window.saveChangesMessage,
                    "Save",
                    "Discard",
                    "Cancel"
                );

                    switch (option)
                    {
                        case 0:
                            {
                                window.SaveChanges();

                                window.m_tileSet = tileSet;
                                window.OnTileSetChanged();

                                window.hasUnsavedChanges = false;
                                break;
                            }
                        case 1:
                            {
                                window.DiscardChanges();

                                window.m_tileSet = tileSet;
                                window.OnTileSetChanged();

                                window.hasUnsavedChanges = false;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                else
                {
                    window.m_tileSet = tileSet;
                    window.OnTileSetChanged();
                }
            }
        }

        [OnOpenAsset]
        private static bool OpenTileSetAsset(int instanceID, int line)
        {
            UnityObject obj = EditorUtility.InstanceIDToObject(instanceID);
            
            if (obj is TileSet tileSet)
            {
                Open(tileSet);
                return true;
            }
            if (obj is Tile tile)
            {
                TileSet parentTileSet = new SerializedObject(tile).FindProperty("m_tileSet").objectReferenceValue as TileSet;
                if (parentTileSet != null)
                {
                    Open(parentTileSet);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Handle Changes

        private Action m_onSaved;

        private static MethodInfo s_initializeMethod;

        public override void SaveChanges()
        {
            m_serializedTileSet.SaveChanges();
            m_onSaved?.Invoke();

            hasUnsavedChanges = false;

            if (m_tileSet != null)
            {
                if (s_initializeMethod == null)
                {
                    s_initializeMethod = typeof(TileSet).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                s_initializeMethod?.Invoke(m_tileSet, new object[0]);
            }
        }

        #endregion

        #region GUI

        private Action m_onUpdate;

        private void OnEnable()
        {
            TileSetValidator.onTileSetValidated += OnTileSetValidated;
        }

        private void OnDisable()
        {
            TileSetValidator.onTileSetValidated -= OnTileSetValidated;
        }

        private void OnGUI()
        {
            m_onUpdate?.Invoke();

            if (Event.current == null)
            {
                return;
            }

            if (Event.current.type == EventType.KeyDown && hasUnsavedChanges)
            {
                bool isSaveShortcut = Event.current.keyCode == KeyCode.S && (Event.current.control || Event.current.command);
                if (isSaveShortcut)
                {
                    SaveChanges();
                }
            }
        }

        private void CreateGUI()
        {
            m_onTileSetChanged    = null;
            m_onTilesChanged      = null;
            m_onTilesDeleted      = null;
            m_onCategoriesChanged = null;
            m_onCategoriesDeleted = null;

            m_onTileNameChanged      = null;
            m_onTileColorChanged     = null;
            m_onCategoryNameChanged  = null;
            m_onCategoryColorChanged = null;
            m_onFilterColorChanged   = null;

            m_paintContext ??= new PaintContext(this);
            m_paintContext.ClearBrushEvent();

            VisualElement root = rootVisualElement;
            root.RegisterCallback<KeyDownEvent>(e =>
            {
                if (hasUnsavedChanges && e.keyCode == KeyCode.S && (e.commandKey || e.ctrlKey))
                {
                    SaveChanges();
                }
            });

            TwoPaneSplitView splitView = CreateSplitView(root, out VisualElement leftPane, out VisualElement rightPane);

            ScrollView leftContent = CreateScrollView(leftPane);

            VisualElement general = CreateGeneralFoldout(leftContent);
            VisualElement content = CreateContentFoldout(leftContent);
            VisualElement info    = CreateInfoFoldout(leftContent);

            VisualElement paintControl = CreatePaintControl(rightPane);
            VisualElement mainEditor   = CreateMainEditor(rightPane);

            OnTileSetChanged();
        }

        private TwoPaneSplitView CreateSplitView(VisualElement parent, out VisualElement leftPane, out VisualElement rightPane)
        {
            float splitViewWidth = EditorPrefs.GetFloat("Zlitz.Extra2D.BetterTile.TileSetEditorWindow.splitViewWidth", 360.0f);

            TwoPaneSplitView splitView = new TwoPaneSplitView(0, splitViewWidth, TwoPaneSplitViewOrientation.Horizontal);
            splitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
            splitView.style.flexGrow = 1.0f;
            parent.Add(splitView);

            leftPane = new VisualElement();
            leftPane.style.minWidth = 360.0f;
            splitView.Add(leftPane);

            rightPane = new VisualElement();
            splitView.Add(rightPane);

            leftPane.RegisterCallback<GeometryChangedEvent>(e =>
            {
                float newWidth = e.newRect.width;
                EditorPrefs.SetFloat("namespace Zlitz.Extra2D.BetterTile.TileSetEditorWindow.splitViewWidth", Mathf.Max(360.0f, newWidth));
            });

            return splitView;
        }

        private VisualElement CreateGeneralFoldout(VisualElement parent)
        {
            VisualElement container = CreateFoldout(parent, "General", "Zlitz.Extra2D.BetterTile.TileSetEditorWindow.generalFoldout");
            container.style.flexDirection = FlexDirection.RowReverse;

            ObjectField tileSetField = new ObjectField();
            tileSetField.objectType = typeof(TileSet);
            tileSetField.label = "Tile Set";
            tileSetField.style.flexGrow = 1.0f;
            tileSetField.value = m_tileSet;

            tileSetField.RegisterValueChangedCallback(e =>
            {
                TileSet newTileSet = e.newValue as TileSet;

                if (m_tileSet != newTileSet)
                {
                    if (hasUnsavedChanges)
                    {
                        int option = EditorUtility.DisplayDialogComplex(
                        "Tile Set Editor - Unsaved Changes Detected",
                        saveChangesMessage,
                        "Save",
                        "Discard",
                        "Cancel"
                    );

                        switch (option)
                        {
                            case 0:
                                {
                                    SaveChanges();

                                    m_tileSet = newTileSet;
                                    OnTileSetChanged();
                                    
                                    hasUnsavedChanges = false;
                                    break;
                                }
                            case 1:
                                {
                                    DiscardChanges();
                                    
                                    m_tileSet = newTileSet;
                                    OnTileSetChanged();
                                    
                                    hasUnsavedChanges = false;
                                    break;
                                }
                            default:
                                {
                                    tileSetField.SetValueWithoutNotify(m_tileSet);
                                    break;
                                }
                        }
                    }
                    else
                    {
                        m_tileSet = newTileSet;
                        OnTileSetChanged();
                    }
                }
            });

            m_onTileSetChanged += () =>
            {
                tileSetField.value = m_tileSet;
            };

            Button createButton = new Button(() =>
            {
                bool cancelled = false;
                if (hasUnsavedChanges)
                {
                    int option = EditorUtility.DisplayDialogComplex(
                    "Tile Set Editor - Unsaved Changes Detected",
                    saveChangesMessage,
                    "Save",
                    "Discard",
                    "Cancel"
                );

                    switch (option)
                    {
                        case 0:
                            {
                                SaveChanges();
                                hasUnsavedChanges = false;
                                break;
                            }
                        case 1:
                            {
                                DiscardChanges();
                                hasUnsavedChanges = false;
                                break;
                            }
                        default:
                            {
                                cancelled = true;
                                break;
                            }
                    }
                }

                if (cancelled)
                {
                    return;
                }

                string path = EditorUtility.SaveFilePanelInProject(
                    "Create Tile Set",
                    "New Tile Set.asset",
                    "asset",
                    "Enter a save name to save the tile set to"
                );

                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                TileSet newTileSet = CreateInstance<TileSet>();

                AssetDatabase.CreateAsset(newTileSet, path);
                AssetDatabase.SaveAssets();

                m_tileSet = newTileSet;
                OnTileSetChanged();

                tileSetField.value = newTileSet;
            });
            createButton.text = "Create";
            createButton.style.width = 60.0f;
            container.Add(createButton);

            container.Add(tileSetField);

            return container;
        }

        private VisualElement CreateContentFoldout(VisualElement parent)
        {
            VisualElement container = CreateFoldout(parent, "Content", "Zlitz.Extra2D.BetterTile.TileSetEditorWindow.contentFoldout");

            VisualElement invalid = new VisualElement();
            container.Add(invalid);

            HelpBox invalidHelpBox = new HelpBox("Select an existing TileSet or create a new one to edit", HelpBoxMessageType.Info);
            invalid.Add(invalidHelpBox);

            SelectionGroup valid = new SelectionGroup();
            valid.onCurrentElementChanged += (selected) =>
            {
                if (selected == null)
                {
                    m_brush.SetSpecialFilter(m_paintContext, TileFilterType.Any);
                }
                else if (selected is TileItem tileItem)
                {
                    m_brush.SetTile(m_paintContext, tileItem.serializedTile.tile);
                }
                else if (selected is TileCategoryItem categoryItem)
                {
                    m_brush.SetCategory(m_paintContext, categoryItem.serializedCategory.category);
                }
                else if (selected is SpecialFilterItem filterItem)
                {
                    m_brush.SetSpecialFilter(m_paintContext, filterItem.serializedSpecialFilter.filterType);
                }
            };
            container.Add(valid);

            // Tiles

            ListView tilesListView = CreateListView(valid, "Tiles", "Zlitz.Extra2D.BetterTile.TileSetEditorWindow.tilesList", false);

            tilesListView.makeItem = () =>
            {
                TileItem tileItem = new TileItem(valid, () =>
                {
                    hasUnsavedChanges = true;
                }, m_onTileNameChanged, m_onTileColorChanged);

                tileItem.style.marginLeft  = -28.0f;
                tileItem.style.marginRight = -4.0f;

                return tileItem;
            };
            tilesListView.bindItem = (e, i) =>
            {
                (e as TileItem)?.Bind(m_serializedTileSet.tiles[i], m_serializedTileSet.tiles);
            };

            tilesListView.itemsAdded += (indices) =>
            {
                List<string> existingNames = m_serializedTileSet.tiles.Where(t => t?.tile != null).Select(t => t.tile.name).ToList();

                bool changed = false;
                foreach (int i in indices)
                {
                    Tile newTile = CreateInstance<Tile>();
                    string newName = ObjectNames.GetUniqueName(existingNames.ToArray(), "New Tile");

                    existingNames.Add(newName);
                    newTile.name = newName;

                    SerializedObject so = new SerializedObject(newTile);

                    SerializedProperty tileSetProperty = so.FindProperty("m_tileSet");
                    tileSetProperty.objectReferenceValue = m_tileSet;

                    so.ApplyModifiedPropertiesWithoutUndo();

                    m_serializedTileSet.tiles[i] = new SerializedTile();
                    m_serializedTileSet.tiles[i].Update(newTile);

                    changed = true;
                }

                if (changed)
                {
                    hasUnsavedChanges = true;
                    EditorApplication.delayCall += () =>
                    {
                        m_onTilesChanged?.Invoke();
                    };
                }
            };
            tilesListView.itemsRemoved += (indices) =>
            {
                if (indices?.Any() ?? false)
                {
                    m_onTilesDeleted?.Invoke(indices.Select(i => m_serializedTileSet.tiles[i]));
                    
                    hasUnsavedChanges = true;
                    EditorApplication.delayCall += () =>
                    {
                        m_onTilesChanged?.Invoke();
                    };
                }
            };
            tilesListView.itemIndexChanged += (oldIndex, newIndex) =>
            {
                if (oldIndex != newIndex)
                {
                    hasUnsavedChanges = true;
                    EditorApplication.delayCall += () =>
                    {
                        m_onTilesChanged?.Invoke();
                    };
                }
            };

            // Categories

            ListView categoriesListView = CreateListView(valid, "Categories", "Zlitz.Extra2D.BetterTile.TileSetEditorWindow.categoriesList", false);

            categoriesListView.makeItem = () =>
            {
                TileCategoryItem categoryItem = new TileCategoryItem(valid, () =>
                {
                    hasUnsavedChanges = true;
                }, m_onCategoryNameChanged, m_onCategoryColorChanged);

                categoryItem.SetAvailableTiles(m_serializedTileSet.tiles);
                m_onTilesChanged += () =>
                {
                    categoryItem.SetAvailableTiles(m_serializedTileSet.tiles);
                };

                categoryItem.style.marginLeft  = -28.0f;
                categoryItem.style.marginRight = -4.0f;

                return categoryItem;
            };
            categoriesListView.bindItem = (e, i) =>
            {
                (e as TileCategoryItem)?.Bind(m_serializedTileSet.categories[i], m_serializedTileSet.categories, m_serializedTileSet.tiles);
            };
            
            categoriesListView.itemsAdded += (indices) =>
            {
                List<string> existingNames = m_serializedTileSet.categories.Where(c => c?.category != null).Select(c => c.category.name).ToList();

                bool changed = false;
                foreach (int i in indices)
                {
                    TileCategory newCategory = CreateInstance<TileCategory>();
                    newCategory.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    string newName = ObjectNames.GetUniqueName(existingNames.ToArray(), "New Tile Category");

                    existingNames.Add(newName);
                    newCategory.name = newName;

                    SerializedObject so = new SerializedObject(newCategory);

                    SerializedProperty tileSetProperty = so.FindProperty("m_tileSet");
                    tileSetProperty.objectReferenceValue = m_tileSet;

                    so.ApplyModifiedPropertiesWithoutUndo();

                    m_serializedTileSet.categories[i] = new SerializedCategory();
                    m_serializedTileSet.categories[i].Update(newCategory);

                    changed = true;
                }

                if (changed)
                {
                    hasUnsavedChanges = true;
                    EditorApplication.delayCall += () =>
                    {
                        m_onCategoriesChanged?.Invoke();
                    };
                }
            };
            categoriesListView.itemsRemoved += (indices) =>
            {
                if (indices?.Any() ?? false)
                {
                    m_onCategoriesDeleted?.Invoke(indices.Select(i => m_serializedTileSet.categories[i]));

                    hasUnsavedChanges = true;
                    EditorApplication.delayCall += () =>
                    {
                        m_onCategoriesChanged?.Invoke();
                    };
                }
            };
            categoriesListView.itemIndexChanged += (oldIndex, newIndex) =>
            {
                if (oldIndex != newIndex)
                {
                    hasUnsavedChanges = true;
                }
            };

            // Others

            ListView othersListView = CreateListView(valid, "Others", "Zlitz.Extra2D.BetterTile.TileSetEditorWindow.othersList", true);

            othersListView.makeItem = () =>
            {
                SpecialFilterItem filterItem = new SpecialFilterItem(valid, () =>
                {
                    hasUnsavedChanges = true;
                }, m_onFilterColorChanged);

                filterItem.style.marginLeft  = -28.0f;
                filterItem.style.marginRight = -4.0f;

                return filterItem;
            };
            othersListView.bindItem = (e, i) =>
            {
                (e as SpecialFilterItem)?.Bind(m_serializedTileSet.specialFilters[i]);
            };

            othersListView.itemIndexChanged += (oldIndex, newIndex) =>
            {
                if (oldIndex != newIndex)
                {
                    hasUnsavedChanges = true;
                }
            };

            m_onTileSetChanged += () =>
            {
                if (m_tileSet != null)
                {
                    tilesListView.itemsSource      = m_serializedTileSet.tiles;
                    categoriesListView.itemsSource = m_serializedTileSet.categories;
                    othersListView.itemsSource     = m_serializedTileSet.specialFilters;
                }

                invalid.style.display = m_tileSet == null ? DisplayStyle.Flex : DisplayStyle.None;
                valid.style.display   = m_tileSet != null ? DisplayStyle.Flex : DisplayStyle.None;
            };

            m_onSaved += () =>
            {
                tilesListView.itemsSource = m_serializedTileSet.tiles;
                tilesListView.RefreshItems();

                categoriesListView.itemsSource = m_serializedTileSet.categories;
                categoriesListView.RefreshItems();

                othersListView.itemsSource = m_serializedTileSet.specialFilters;
                othersListView.RefreshItems();
            };

            return container;
        }

        private VisualElement CreateInfoFoldout(VisualElement parent)
        {
            VisualElement container = CreateFoldout(parent, "Info", "Zlitz.Extra2D.BetterTile.TileSetEditorWindow.infoFoldout");

            string[] infoContents = {
                "Select an item from the content section to start painting rules (Note that you cannot paint the center with a category).",
                "Left click to paint, hold L-Shift while clicking to paint in invert (blacklist) mode.",
                "Right click a sprite to edit its properties and animation.",
                "By default, if a tile satisfies multiple rules, the more specific rule is selected. Static sprites will always be included in the sprite pool no matter how specific the rules are."
            };

            foreach (string content in infoContents )
            {
                HelpBox infoBox = new HelpBox();
                infoBox.messageType = HelpBoxMessageType.Info;
                infoBox.text = content;
                container.Add(infoBox);
            }
            

            return container;
        }

        private VisualElement CreatePaintControl(VisualElement parent)
        {
            VisualElement root = new VisualElement();
            root.style.backgroundColor = UIColors.darken;
            root.style.flexDirection = FlexDirection.Row;
            root.style.paddingLeft   = 2.0f;
            root.style.paddingRight  = 2.0f;
            root.style.paddingTop    = 2.0f;
            root.style.paddingBottom = 2.0f;
            root.style.borderBottomWidth = 2.0f;
            root.style.borderBottomColor = UIColors.border;
            parent.Add(root);

            VisualElement left = new VisualElement();
            left.style.width = 120.0f;
            left.style.paddingLeft   = 4.0f;
            left.style.paddingRight  = 4.0f;
            left.style.paddingTop    = 4.0f;
            left.style.paddingBottom = 4.0f;
            root.Add(left);

            VisualElement right = new VisualElement();
            right.style.flexGrow = 1.0f;
            right.style.borderLeftWidth = 1.0f;
            right.style.borderLeftColor = UIColors.brighten;
            right.style.paddingLeft   = 4.0f;
            right.style.paddingRight  = 4.0f;
            right.style.paddingTop    = 4.0f;
            right.style.paddingBottom = 4.0f;
            root.Add(right);

            Label label = new Label();
            label.text = "Paint Settings";
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginLeft = 4.0f;
            label.style.marginTop  = 4.0f;
            left.Add(label);

            EnumField paintMode = new EnumField(m_brush.brushType);
            paintMode.label = "Paint Mode";
            right.Add(paintMode);

            VisualElement selectedContainer = new VisualElement();
            selectedContainer.style.flexDirection = FlexDirection.Row;
            selectedContainer.style.marginLeft = 4.0f;
            selectedContainer.style.marginTop  = 2.0f;
            selectedContainer.style.display = m_brush.brushType == BrushType.Rule ? DisplayStyle.Flex : DisplayStyle.None;
            right.Add(selectedContainer);

            Label selected = new Label();
            selected.text = "Selected";
            selected.style.width = 124.0f;
            selectedContainer.Add(selected);

            Label currentFilterName = new Label();
            currentFilterName.text = "Any";
            currentFilterName.style.unityFontStyleAndWeight = FontStyle.Bold;
            currentFilterName.style.color = Color.white;
            selectedContainer.Add(currentFilterName);

            FloatField weight = new FloatField();
            weight.value = m_brush.weight;
            weight.label = "Weight";
            weight.style.marginTop    = 0.0f;
            weight.style.marginBottom = 0.0f;
            weight.style.display = m_brush.brushType == BrushType.Weight ? DisplayStyle.Flex : DisplayStyle.None;
            right.Add(weight);
            weight.RegisterValueChangedCallback(e =>
            {
                float newValue = e.newValue;
                if (newValue < 0.0f)
                {
                    newValue = 0.0f;
                    weight.SetValueWithoutNotify(newValue);
                }
                m_brush.SetWeight(m_paintContext, newValue);
            });

            IntegerField alternatingIndex = new IntegerField();
            alternatingIndex.value = m_brush.alternatingIndex;
            alternatingIndex.label = "Alt. Index";
            alternatingIndex.style.marginTop = 0.0f;
            alternatingIndex.style.marginBottom = 0.0f;
            alternatingIndex.style.display = m_brush.brushType == BrushType.AlternatingIndex ? DisplayStyle.Flex : DisplayStyle.None;
            right.Add(alternatingIndex);
            alternatingIndex.RegisterValueChangedCallback(e =>
            {
                m_brush.SetAlternatingIndex(m_paintContext, e.newValue);
            });

            paintMode.RegisterValueChangedCallback(e =>
            {
                BrushType newPaintMode = (BrushType)e.newValue;
                m_brush.SetBrushType(m_paintContext, newPaintMode);

                selectedContainer.style.display = newPaintMode == BrushType.Rule ? DisplayStyle.Flex : DisplayStyle.None;
                weight.style.display            = newPaintMode == BrushType.Weight ? DisplayStyle.Flex : DisplayStyle.None;
                alternatingIndex.style.display  = newPaintMode == BrushType.AlternatingIndex ? DisplayStyle.Flex : DisplayStyle.None;
            });

            m_paintContext.onBrushChanged += (brush) =>
            {
                currentFilterName.text = "Any";
                currentFilterName.style.color = Color.white;

                if (brush is TileFilterBrush tileFilterBrush)
                {
                    switch (tileFilterBrush.filterType)
                    {
                        case TileFilterType.Any:
                            {
                                break;
                            }
                        case TileFilterType.Tile:
                            {
                                Tile tile = tileFilterBrush.tile;

                                SerializedTile serializedTile = m_serializedTileSet.tiles.First(t => t.tile == tile);

                                currentFilterName.text = serializedTile.tileName;
                                currentFilterName.style.color = serializedTile.tileColor;

                                break;
                            }
                        case TileFilterType.TileCategory:
                            {
                                TileCategory category = tileFilterBrush.category;

                                SerializedCategory serializedCategory = m_serializedTileSet.categories.First(c => c.category == category);

                                currentFilterName.text = serializedCategory.categoryName;
                                currentFilterName.style.color = serializedCategory.categoryColor;

                                break;
                            }
                        default:
                            {
                                TileFilterType filterType = tileFilterBrush.filterType;

                                SerializedSpecialFilter serializedSpecialFilter = m_serializedTileSet.specialFilters.First(f => f.filterType == filterType);

                                currentFilterName.text = ObjectNames.NicifyVariableName(filterType.ToString());
                                currentFilterName.style.color = serializedSpecialFilter.filterColor;

                                break;
                            }
                    }
                }
            };

            Action updateFilterName = () =>
            {
                currentFilterName.text = "None";
                currentFilterName.style.color = Color.white;

                if (m_paintContext.currentBrush is TileFilterBrush tileFilterBrush)
                {
                    switch (tileFilterBrush.filterType)
                    {
                        case TileFilterType.Tile:
                            {
                                Tile tile = tileFilterBrush.tile;

                                SerializedTile serializedTile = m_serializedTileSet.tiles.First(t => t.tile == tile);

                                currentFilterName.text = serializedTile.tileName;
                                currentFilterName.style.color = serializedTile.tileColor;

                                break;
                            }
                        case TileFilterType.TileCategory:
                            {
                                TileCategory category = tileFilterBrush.category;

                                SerializedCategory serializedCategory = m_serializedTileSet.categories.First(c => c.category == category);

                                currentFilterName.text = serializedCategory.categoryName;
                                currentFilterName.style.color = serializedCategory.categoryColor;

                                break;
                            }
                        default:
                            {
                                TileFilterType filterType = tileFilterBrush.filterType;

                                SerializedSpecialFilter serializedSpecialFilter = m_serializedTileSet.specialFilters.First(f => f.filterType == filterType);

                                currentFilterName.text = ObjectNames.NicifyVariableName(filterType.ToString());
                                currentFilterName.style.color = serializedSpecialFilter.filterColor;

                                break;
                            }
                    }
                }
            };

            m_onTileNameChanged      += (t) => m_paintContext.OnTileInfoChanged(t, updateFilterName);
            m_onTileColorChanged     += (t) => m_paintContext.OnTileInfoChanged(t, updateFilterName);
            m_onCategoryNameChanged  += (c) => m_paintContext.OnCategoryInfoChanged(c, updateFilterName);
            m_onCategoryColorChanged += (c) => m_paintContext.OnCategoryInfoChanged(c, updateFilterName);
            m_onFilterColorChanged   += (f) => m_paintContext.OnFilterInfoChanged(f, updateFilterName);

            m_onTileSetChanged    += () => m_brush.OnTileSetChanged(m_paintContext, m_serializedTileSet);
            m_onTilesChanged      += () => m_brush.OnTileSetChanged(m_paintContext, m_serializedTileSet);
            m_onCategoriesChanged += () => m_brush.OnTileSetChanged(m_paintContext, m_serializedTileSet);

            return root;
        }

        private VisualElement CreateMainEditor(VisualElement parent)
        {
            ScrollView root = CreateScrollView(parent);
            root.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            root.style.flexGrow = 1.0f;
            root.style.marginTop = 4.0f;

            // Empty sprites

            EmptyRuleGroupItem emptySpriteRules = new EmptyRuleGroupItem(m_paintContext, () =>
            {
                hasUnsavedChanges = true;
            });
            emptySpriteRules.style.marginBottom = 4.0f;
            root.Add(emptySpriteRules);

            // Sprites

            Button addTexture = new Button(() =>
            {
                EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, "", 0);
                m_onUpdate += WaitForObjectPicker;
            });
            addTexture.text = "Add Texture";
            addTexture.style.width  = 124.0f;
            addTexture.style.height = 32.0f;
            addTexture.style.alignSelf = Align.FlexEnd;
            addTexture.style.marginRight  = 1.0f;
            addTexture.style.marginBottom = 4.0f;
            addTexture.style.backgroundColor = UIColors.green;
            root.Add(addTexture);

            ListView ruleGroupsListView = CreateListView(root, null, "Zlitz.Extra2D.BetterTile.TileSetEditorWindow.rulesList", true);

            ruleGroupsListView.makeItem = () =>
            {
                RuleGroupItem ruleGroupItem = new RuleGroupItem(m_paintContext, () =>
                {
                    hasUnsavedChanges = true;
                });

                ruleGroupItem.style.marginLeft  = -28.0f;
                ruleGroupItem.style.marginRight = -4.0f;

                return ruleGroupItem;
            };
            ruleGroupsListView.bindItem = (e, i) =>
            {
                (e as RuleGroupItem)?.Bind(m_tileSet, m_serializedTileSet.ruleGroups[i],
                    () =>
                    {
                        if (i >= 0 && i < m_serializedTileSet.ruleGroups.Count)
                        {
                            m_serializedTileSet.ruleGroups.RemoveAt(i);
                            hasUnsavedChanges = true;

                            m_onRuleGroupsChanged?.Invoke();
                        }
                    });
            };

            ruleGroupsListView.itemIndexChanged += (oldIndex, newIndex) =>
            {
                if (oldIndex != newIndex)
                {
                    hasUnsavedChanges = true;
                }
            };

            // Overlays

            Button addOverlayGroup = new Button();
            addOverlayGroup.text = "Add Overlay Group";
            addOverlayGroup.style.width = 124.0f;
            addOverlayGroup.style.height = 32.0f;
            addOverlayGroup.style.alignSelf = Align.FlexEnd;
            addOverlayGroup.style.marginRight = 1.0f;
            addOverlayGroup.style.marginBottom = 4.0f;
            addOverlayGroup.style.backgroundColor = UIColors.green;
            root.Add(addOverlayGroup);

            ListView overlayGroupsListView = CreateListView(root, null, "Zlitz.Extra2D.BetterTile.TileSetEditorWindow.overlaysList", true);

            overlayGroupsListView.makeItem = () =>
            {
                OverlayGroupItem overlayGroupItem = new OverlayGroupItem(m_paintContext, () =>
                {
                    hasUnsavedChanges = true;
                });

                m_onUpdate += overlayGroupItem.Update;

                overlayGroupItem.style.marginLeft = -28.0f;
                overlayGroupItem.style.marginRight = -4.0f;

                return overlayGroupItem;
            };
            overlayGroupsListView.destroyItem = (e) =>
            {
                if (e is OverlayGroupItem overlayGroupItem)
                {
                    m_onUpdate -= overlayGroupItem.Update;
                }
            };
            overlayGroupsListView.bindItem = (e, i) =>
            {
                (e as OverlayGroupItem)?.Bind(m_tileSet, m_serializedTileSet.overlayGroups[i],
                    () =>
                    {
                        if (i >= 0 && i < m_serializedTileSet.overlayGroups.Count)
                        {
                            m_serializedTileSet.overlayGroups.RemoveAt(i);
                            hasUnsavedChanges = true;

                            m_onOverlayGroupsChanged?.Invoke();
                        }
                    });
            };

            overlayGroupsListView.itemIndexChanged += (oldIndex, newIndex) =>
            {
                if (oldIndex != newIndex)
                {
                    hasUnsavedChanges = true;
                }
            };

            addOverlayGroup.clicked += () =>
            {
                m_serializedTileSet.overlayGroups.Add(new SerializedOverlayGroup(Guid.NewGuid().ToString()));
                overlayGroupsListView.itemsSource = m_serializedTileSet.overlayGroups;

                m_onOverlayGroupsChanged?.Invoke();

                hasUnsavedChanges = true;
            };

            m_onTileSetChanged += () =>
            {
                addTexture.SetEnabled(m_tileSet != null);

                if (m_tileSet != null)
                {
                    emptySpriteRules.Bind(m_tileSet, m_serializedTileSet.emptySpriteRules);

                    ruleGroupsListView.itemsSource = m_serializedTileSet.ruleGroups;
                    ruleGroupsListView.Rebuild();

                    overlayGroupsListView.itemsSource = m_serializedTileSet.overlayGroups;
                    overlayGroupsListView.Rebuild();
                }
            };

            m_onTilesDeleted += (tiles) =>
            {
                if (m_serializedTileSet.OnTilesDeleted(tiles)) 
                {
                    ruleGroupsListView.RefreshItems();
                    overlayGroupsListView.RefreshItems();
                }
            };

            m_onCategoriesDeleted += (categories) =>
            {
                if (m_serializedTileSet.OnCategoriesDeleted(categories))
                {
                    ruleGroupsListView.RefreshItems();
                    overlayGroupsListView.RefreshItems();
                }
            };

            m_onRuleGroupsChanged += () =>
            {
                ruleGroupsListView.Rebuild();
            };

            m_onOverlayGroupsChanged += () =>
            {
                overlayGroupsListView.Rebuild();
            };

            m_onTileSetValidated += () =>
            {
                ruleGroupsListView.itemsSource = m_serializedTileSet.ruleGroups;
                ruleGroupsListView.Rebuild();

                overlayGroupsListView.itemsSource = m_serializedTileSet.overlayGroups;
                overlayGroupsListView.Rebuild();
            };

            m_onSaved += () =>
            {
                ruleGroupsListView.itemsSource = m_serializedTileSet.ruleGroups;
                ruleGroupsListView.RefreshItems();

                overlayGroupsListView.itemsSource = m_serializedTileSet.overlayGroups;
                overlayGroupsListView.Rebuild();
            };

            return root;
        }

        private VisualElement CreateFoldout(VisualElement parent, string title, string foldoutStateCacheKey)
        {
            bool foldoutState = EditorPrefs.GetBool(foldoutStateCacheKey, true);

            Foldout foldout = new Foldout();
            foldout.text  = title;
            foldout.value = foldoutState;
            foldout.style.marginBottom      = 6.0f;
            foldout.style.borderBottomWidth = 2.0f;
            foldout.style.backgroundColor   = UIColors.pink;
            foldout.style.borderBottomColor = UIColors.border;
            parent.Add(foldout);

            VisualElement container = new VisualElement();
            container.style.paddingLeft   = 2.0f;
            container.style.paddingRight  = 2.0f;
            container.style.paddingTop    = 2.0f;
            container.style.paddingBottom = 2.0f;
            container.style.marginLeft    = 0.0f;
            container.style.marginRight   = 0.0f;
            container.style.marginTop     = 0.0f;
            container.style.marginBottom  = 0.0f;
            foldout.Add(container);

            Label foldoutTitle = foldout.Q<Label>();
            foldoutTitle.style.unityFontStyleAndWeight = FontStyle.Bold;

            Toggle foldoutToggle = foldout.Q<Toggle>();
            foldoutToggle.style.marginLeft    = 0.0f;
            foldoutToggle.style.marginRight   = 0.0f;
            foldoutToggle.style.marginTop     = 0.0f;
            foldoutToggle.style.marginBottom  = 0.0f;
            foldoutToggle.style.paddingLeft   = 2.0f;
            foldoutToggle.style.paddingRight  = 2.0f;
            foldoutToggle.style.paddingTop    = 2.0f;
            foldoutToggle.style.paddingBottom = 2.0f;

            foldoutToggle.style.backgroundColor = UIColors.darken;

            foldout.RegisterValueChangedCallback(e =>
            {
                EditorPrefs.SetBool(foldoutStateCacheKey, e.newValue);
            });

            return container;
        }

        private ListView CreateListView(VisualElement parent, string title, string toggleStateCacheKey, bool fixedSize)
        {
            ListView listView = new ListView();

            if (string.IsNullOrEmpty(title))
            {
                listView.showFoldoutHeader = false;
            }
            else
            {
                listView.showFoldoutHeader = true;
                listView.headerTitle = title;
            }

            listView.showAddRemoveFooter = true;
            listView.selectionType = SelectionType.None;
            listView.showBoundCollectionSize = true;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.style.paddingBottom = 4.0f;

            Toggle listViewToggle = listView.Q<Toggle>();
            if (listViewToggle != null)
            {
                listViewToggle.style.backgroundColor = UIColors.darken;
                listViewToggle.style.paddingTop = 2.0f;
                listViewToggle.style.paddingBottom = 2.0f;
                listViewToggle.RegisterValueChangedCallback(e =>
                {
                    EditorPrefs.SetBool(toggleStateCacheKey, e.newValue);
                });
            }

            VisualElement listViewFooter = listView.Q<VisualElement>(name: "unity-list-view__footer");
            if (listViewFooter != null)
            {
                if (fixedSize)
                {
                    listViewFooter.style.display = DisplayStyle.None;
                }

                listViewFooter.style.borderBottomLeftRadius  = 0.0f;
                listViewFooter.style.borderBottomRightRadius = 0.0f;
                listViewFooter.style.borderLeftWidth         = 0.0f;
                listViewFooter.style.borderRightWidth        = 0.0f;
                listViewFooter.style.borderBottomWidth       = 0.0f;
                listViewFooter.style.paddingLeft             = 0.0f;
                listViewFooter.style.paddingRight            = 0.0f;
                listViewFooter.style.backgroundColor         = UIColors.darken;
            
                Button[] buttons = new Button[]
                {
                    listViewFooter.Q<Button>(name: "unity-list-view__add-button"),
                    listViewFooter.Q<Button>(name: "unity-list-view__remove-button")
                };

                foreach (Button button in buttons)
                {        
                    button.style.width = 40.0f;
                    button.style.borderBottomLeftRadius  = 0.0f;
                    button.style.borderBottomRightRadius = 0.0f;
                    button.style.borderTopLeftRadius     = 0.0f;
                    button.style.borderTopRightRadius    = 0.0f;
                    button.style.backgroundColor = UIColors.transparent;

                    button.RegisterCallback<MouseEnterEvent>(evt => {
                        button.style.backgroundColor = UIColors.brighten;
                    });

                    button.RegisterCallback<MouseLeaveEvent>(evt => {
                        button.style.backgroundColor = UIColors.transparent;
                    });
                }
            }

            TextField listViewSizeField = listView.Q<TextField>(name: "unity-list-view__size-field");
            if (fixedSize)
            {
                listViewSizeField?.SetEnabled(false);
            }

            listView.itemsSourceChanged += () =>
            {
                Toggle toggle = listView.Q<Toggle>();
                if (toggle != null)
                {
                    toggle.value = EditorPrefs.GetBool(toggleStateCacheKey, true);
                }
            };

            parent.Add(listView);

            return listView;
        }

        private ScrollView CreateScrollView(VisualElement parent)
        {
            ScrollView scrollView = new ScrollView();
            scrollView.style.flexGrow = 1.0f;
            parent.Add(scrollView);
            return scrollView;
        }

        private Texture2D m_selectedTexture;

        private void WaitForObjectPicker()
        {
            if (Event.current == null || Event.current.type != EventType.ExecuteCommand)
            {
                return;
            }

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                m_selectedTexture = EditorGUIUtility.GetObjectPickerObject() as Texture2D;
            }

            if (Event.current.commandName == "ObjectSelectorClosed")
            {
                AddTexture(m_selectedTexture);

                m_selectedTexture = null;
                m_onUpdate -= WaitForObjectPicker;
            }
        }

        private void AddTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            SerializedRuleGroup newRuleGroup = new SerializedRuleGroup(Guid.NewGuid().ToString());
            newRuleGroup.texture = texture;

            foreach (Sprite sprite in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>())
            {
                SerializedTileRule newRule = new SerializedTileRule();
                newRule.output.sprite = sprite;
                newRuleGroup.rules.Add(newRule);
            }

            m_serializedTileSet.ruleGroups.Add(newRuleGroup);
            m_onRuleGroupsChanged?.Invoke();

            hasUnsavedChanges = true;
        }

        private void OnTileSetValidated(TileSet tileSet)
        {
            if (m_serializedTileSet.OnValidated(tileSet))
            {
                m_onTileSetValidated?.Invoke();
            }
        }

        #endregion

        #region TileSet

        [SerializeField]
        private TileSet m_tileSet;

        [SerializeField]
        private SerializedTileSet m_serializedTileSet = new SerializedTileSet();

        private Action m_onTileSetChanged;
        private Action m_onTilesChanged;
        private Action m_onCategoriesChanged;
        private Action m_onRuleGroupsChanged;
        private Action m_onOverlayGroupsChanged;
        private Action m_onTileSetValidated;

        private Action<SerializedTile>          m_onTileNameChanged;
        private Action<SerializedTile>          m_onTileColorChanged;
        private Action<SerializedCategory>      m_onCategoryNameChanged;
        private Action<SerializedCategory>      m_onCategoryColorChanged;
        private Action<SerializedSpecialFilter> m_onFilterColorChanged;

        private Action<IEnumerable<SerializedTile>>     m_onTilesDeleted;
        private Action<IEnumerable<SerializedCategory>> m_onCategoriesDeleted;

        private void OnTileSetChanged()
        {
            m_serializedTileSet.Update(m_tileSet);

            m_onTileSetChanged?.Invoke();
        }

        [Serializable]
        internal class SerializedTileSet
        {
            [SerializeField]
            private TileSet m_tileSet;

            [SerializeField]
            private string m_buildId;

            [SerializeField]
            private List<SerializedTile> m_tiles;

            [SerializeField]
            private List<SerializedCategory> m_categories;

            [SerializeField]
            private List<SerializedSpecialFilter> m_specialFilters;

            [SerializeField]
            private List<SerializedRuleGroup> m_ruleGroups;

            [SerializeField]
            private List<SerializedOverlayGroup> m_overlayGroups;

            [SerializeField]
            private List<SerializedTileRule> m_emptySpriteRules;

            public TileSet tileSet => m_tileSet;

            public List<SerializedTile> tiles => m_tiles;

            public List<SerializedCategory> categories => m_categories;

            public List<SerializedSpecialFilter> specialFilters => m_specialFilters;

            public List<SerializedRuleGroup> ruleGroups => m_ruleGroups;

            public List<SerializedOverlayGroup> overlayGroups => m_overlayGroups;

            public List<SerializedTileRule> emptySpriteRules => m_emptySpriteRules;

            public void SaveChanges()
            {
                if (m_tileSet == null)
                {
                    return;
                }

                SerializedObject serializedObject = new SerializedObject(m_tileSet);

                SerializedProperty buildIdProperty = serializedObject.FindProperty("m_newBuildId");
                m_buildId = Guid.NewGuid().ToString();
                buildIdProperty.stringValue = m_buildId;

                SerializedProperty tilesProperty = serializedObject.FindProperty("m_tiles");
                SyncTiles(tilesProperty);

                SerializedProperty categoriesProperty = serializedObject.FindProperty("m_categories");
                SyncCategories(categoriesProperty);

                SerializedProperty specialFiltersProperty = serializedObject.FindProperty("m_specialFilters");
                SyncSpecialFilters(specialFiltersProperty);

                List<KeyValuePair<string, SerializedTileOutput>> decorators = new List<KeyValuePair<string, SerializedTileOutput>>();

                SerializedProperty ruleGroupsProperty = serializedObject.FindProperty("m_ruleGroups");
                SyncRuleGroups(ruleGroupsProperty, decorators);

                SerializedProperty overlayGroupsProperty = serializedObject.FindProperty("m_overlayGroups");
                SyncOverlayGroups(m_tileSet, overlayGroupsProperty);

                SerializedProperty emptySpritesRulesProperty = serializedObject.FindProperty("m_emptySpriteRules");
                SyncEmptySpriteRules(emptySpritesRulesProperty, decorators);

                SerializedProperty decoratorsProperty = serializedObject.FindProperty("m_decorators");
                SyncDecorators(decoratorsProperty, decorators);

                serializedObject.ApplyModifiedPropertiesWithoutUndo();

                AssetDatabase.SaveAssets();
            }

            public void Update(TileSet tileSet)
            {
                m_tiles            ??= new List<SerializedTile>();
                m_categories       ??= new List<SerializedCategory>();
                m_specialFilters   ??= new List<SerializedSpecialFilter>();
                m_ruleGroups       ??= new List<SerializedRuleGroup>();
                m_overlayGroups    ??= new List<SerializedOverlayGroup>();
                m_emptySpriteRules ??= new List<SerializedTileRule>();

                if (m_tileSet != tileSet)
                {
                    m_tileSet = tileSet;

                    m_tiles.Clear();
                    m_categories.Clear();
                    m_specialFilters.Clear();
                    m_ruleGroups.Clear();
                    m_overlayGroups.Clear();
                    m_emptySpriteRules.Clear();

                    if (m_tileSet != null)
                    {
                        SerializedObject serializedObject = new SerializedObject(m_tileSet);
                        bool shouldSave = false;

                        SerializedProperty idProperty = serializedObject.FindProperty("m_id");
                        if (string.IsNullOrEmpty(idProperty.stringValue))
                        {
                            idProperty.stringValue = Guid.NewGuid().ToString();
                            shouldSave = true;
                        }

                        SerializedProperty buildIdProperty = serializedObject.FindProperty("m_newBuildId");
                        m_buildId = buildIdProperty.stringValue;

                        SerializedProperty tilesProperty = serializedObject.FindProperty("m_tiles");
                        if (UpdateTiles(tilesProperty))
                        {
                            shouldSave = true;
                        }

                        SerializedProperty categoriesProperty = serializedObject.FindProperty("m_categories");
                        if (UpdateCategories(categoriesProperty))
                        {
                            shouldSave = true;
                        }

                        SerializedProperty specialFiltersProperty = serializedObject.FindProperty("m_specialFilters");
                        if (UpdateSpecialFilters(specialFiltersProperty))
                        {
                            shouldSave = true;
                        }

                        SerializedProperty ruleGroupsProperty = serializedObject.FindProperty("m_ruleGroups");
                        if (UpdateRuleGroups(ruleGroupsProperty))
                        {
                            shouldSave = true;
                        }

                        SerializedProperty overlayGroupsProperty = serializedObject.FindProperty("m_overlayGroups");
                        if (UpdateOverlayGroups(overlayGroupsProperty))
                        {
                            shouldSave = true;
                        }

                        SerializedProperty emptySpriteRulesProperty = serializedObject.FindProperty("m_emptySpriteRules");
                        if (UpdateEmptySpriteRules(emptySpriteRulesProperty))
                        {
                            shouldSave = true;
                        }

                        if (shouldSave)
                        {
                            serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        }
                    }
                }
            }

            public bool OnValidated(TileSet tileSet)
            {
                if (m_tileSet != tileSet || m_tileSet == null)
                {
                    return false;
                }

                for (int i = m_ruleGroups.Count - 1; i >= 0; i--)
                {
                    SerializedRuleGroup ruleGroup = m_ruleGroups[i];

                    if (ruleGroup.texture == null)
                    {
                        m_ruleGroups.RemoveAt(i);
                        continue;
                    }
                }

                return true;
            }

            public bool OnTilesDeleted(IEnumerable<SerializedTile> tiles)
            {
                bool changed = false;

                foreach (SerializedRuleGroup ruleGroup in m_ruleGroups)
                {
                    if (ruleGroup.OnTilesDeleted(tiles))
                    {
                        changed = true;
                    }
                }
                foreach (SerializedOverlayGroup overlayGroup in m_overlayGroups)
                {
                    if (overlayGroup.OnTilesDeleted(tiles))
                    {
                        changed = true;
                    }
                }

                return changed;
            }

            public bool OnCategoriesDeleted(IEnumerable<SerializedCategory> categories)
            {
                bool changed = false;

                foreach (SerializedRuleGroup ruleGroup in m_ruleGroups)
                {
                    if (ruleGroup.OnCategoriesDeleted(categories))
                    {
                        changed = true;
                    }
                }
                foreach (SerializedOverlayGroup overlayGroup in m_overlayGroups)
                {
                    if (overlayGroup.OnCategoriesDeleted(categories))
                    {
                        changed = true;
                    }
                }

                return changed;
            }

            private bool UpdateTiles(SerializedProperty tilesProperty)
            {
                bool shouldSave = false;

                for (int i = 0; i < tilesProperty.arraySize; i++)
                {
                    Tile tile = tilesProperty.GetArrayElementAtIndex(i).objectReferenceValue as Tile;
                    if (tile != null)
                    {
                        SerializedTile serializedTile = new SerializedTile();
                        if (serializedTile.Update(tile))
                        {
                            shouldSave = true;
                        }

                        m_tiles.Add(serializedTile);
                    }
                }

                return shouldSave;
            }

            private bool UpdateCategories(SerializedProperty categoriesProperty)
            {
                bool shouldSave = false;

                for (int i = 0; i < categoriesProperty.arraySize; i++)
                {
                    TileCategory category = categoriesProperty.GetArrayElementAtIndex(i).objectReferenceValue as TileCategory;
                    if (category != null)
                    {
                        SerializedCategory serializedCategory = new SerializedCategory();
                        if (serializedCategory.Update(category))
                        {
                            shouldSave = true;
                        }

                        m_categories.Add(serializedCategory);
                    }
                }

                return shouldSave;
            }

            private bool UpdateSpecialFilters(SerializedProperty specialFiltersProperty)
            {
                bool shouldSave = false;

                HashSet<TileFilterType> requiredFilterTypes = TileFilterTypes.specialFilterTypes.ToHashSet();
                List<int> obsoleteFilterIndices = new List<int>();

                for (int i = 0; i < specialFiltersProperty.arraySize; i++)
                {
                    SerializedProperty specialFilterProperty = specialFiltersProperty.GetArrayElementAtIndex(i);

                    SerializedProperty filterTypeProperty = specialFilterProperty.FindPropertyRelative("m_type");
                    TileFilterType filterType = (TileFilterType)filterTypeProperty.enumValueIndex;
                    if (!requiredFilterTypes.Remove(filterType))
                    {
                        shouldSave = true;
                        obsoleteFilterIndices.Add(i);
                        continue;
                    }

                    SerializedSpecialFilter serializedSpecialFilter = new SerializedSpecialFilter();
                    if (serializedSpecialFilter.Update(specialFilterProperty))
                    {
                        shouldSave = true;
                    }

                    m_specialFilters.Add(serializedSpecialFilter);
                }

                for (int i = obsoleteFilterIndices.Count - 1; i >= 0; i--)
                {
                    int index = obsoleteFilterIndices[i];
                    specialFiltersProperty.DeleteArrayElementAtIndex(index);
                }

                foreach (TileFilterType missingFilterType in requiredFilterTypes)
                {
                    shouldSave = true;

                    SerializedSpecialFilter filter = new SerializedSpecialFilter();
                    filter.filterType = missingFilterType;
                    filter.filterColor = Color.black;

                    m_specialFilters.Add(filter);

                    specialFiltersProperty.arraySize++;
                    SerializedProperty newFilterProperty = specialFiltersProperty.GetArrayElementAtIndex(specialFiltersProperty.arraySize - 1);

                    SerializedProperty newFilterTypeProperty = newFilterProperty.FindPropertyRelative("m_type");
                    newFilterTypeProperty.enumValueIndex = (int)missingFilterType;

                    SerializedProperty newFilterColorProperty = newFilterProperty.FindPropertyRelative("m_color");
                    newFilterColorProperty.colorValue = Color.black;
                }

                return shouldSave;
            }

            private bool UpdateRuleGroups(SerializedProperty ruleGroupsProperty)
            {
                bool shouldSave = false;

                for (int i = 0; i < ruleGroupsProperty.arraySize; i++)
                {
                    SerializedProperty ruleGroupProperty = ruleGroupsProperty.GetArrayElementAtIndex(i);

                    SerializedRuleGroup serializedRuleGroup = new SerializedRuleGroup();
                    if (serializedRuleGroup.Update(ruleGroupProperty))
                    {
                        shouldSave = true;
                    }

                    m_ruleGroups.Add(serializedRuleGroup);
                }

                return shouldSave;
            }

            private bool UpdateOverlayGroups(SerializedProperty overlayGroupsProperty)
            {
                bool shouldSave = false;

                for (int i = 0; i < overlayGroupsProperty.arraySize; i++)
                {
                    SerializedProperty overlayGroupProperty = overlayGroupsProperty.GetArrayElementAtIndex(i);

                    SerializedOverlayGroup serializedOverlayGroup = new SerializedOverlayGroup();
                    if (serializedOverlayGroup.Update(overlayGroupProperty))
                    {
                        shouldSave = true;
                    }

                    m_overlayGroups.Add(serializedOverlayGroup);
                }

                return shouldSave;
            }

            private bool UpdateEmptySpriteRules(SerializedProperty emptySpriteRulesProperty)
            {
                bool shouldSave = false;

                for (int i = 0; i < emptySpriteRulesProperty.arraySize; i++)
                {
                    SerializedProperty ruleProperty = emptySpriteRulesProperty.GetArrayElementAtIndex(i);

                    SerializedTileRule serializedRule = new SerializedTileRule();
                    if (serializedRule.Update(ruleProperty))
                    {
                        shouldSave = true;
                    }

                    m_emptySpriteRules.Add(serializedRule);
                }

                return shouldSave;
            }

            private void SyncTiles(SerializedProperty tilesProperty)
            {
                // Obtain existing tiles
                HashSet<Tile> oldTiles = new HashSet<Tile>();
                for (int i = 0; i < tilesProperty.arraySize; i++)
                {
                    Tile tile = tilesProperty.GetArrayElementAtIndex(i).objectReferenceValue as Tile;
                    if (tile != null)
                    {
                        oldTiles.Add(tile);
                    }
                }

                // Remove nulls
                m_tiles = tiles.Where(t => t?.tile != null).ToList();

                // Save each tile as a sub-asset if not already
                foreach (SerializedTile t in m_tiles)
                {
                    t.SaveChanges();

                    oldTiles.Remove(t.tile);
                    if (AssetDatabase.Contains(t.tile))
                    {
                        continue;
                    }

                    AssetDatabase.AddObjectToAsset(t.tile, m_tileSet);
                }

                // Destroy unused tiles
                foreach (Tile oldTile in oldTiles)
                {
                    DestroyImmediate(oldTile, true);
                }

                // Save changes to serialized property
                tilesProperty.arraySize = m_tiles.Count;
                for (int i = 0; i < m_tiles.Count; i++)
                {
                    SerializedProperty tileProperty = tilesProperty.GetArrayElementAtIndex(i);
                    tileProperty.objectReferenceValue = m_tiles[i].tile;
                }
            }
        
            private void SyncCategories(SerializedProperty categoriesProperty)
            {
                // Obtain existing categories
                HashSet<TileCategory> oldCategories = new HashSet<TileCategory>();
                for (int i = 0; i < categoriesProperty.arraySize; i++)
                {
                    TileCategory category = categoriesProperty.GetArrayElementAtIndex(i).objectReferenceValue as TileCategory;
                    if (category != null)
                    {
                        oldCategories.Add(category);
                    }
                }

                // Remove nulls
                m_categories = m_categories.Where(c => c?.category != null).ToList();

                // Save each category as a sub-asset if not already
                foreach (SerializedCategory c in m_categories)
                {
                    c.SaveChanges();

                    oldCategories.Remove(c.category);
                    if (AssetDatabase.Contains(c.category))
                    {
                        continue;
                    }

                    AssetDatabase.AddObjectToAsset(c.category, m_tileSet);
                }

                // Destroy unused tiles
                foreach (TileCategory oldCategory in oldCategories)
                {
                    DestroyImmediate(oldCategory, true);
                }

                // Save changes to serialized property
                categoriesProperty.arraySize = m_categories.Count;
                for (int i = 0; i < m_categories.Count; i++)
                {
                    SerializedProperty categoryProperty = categoriesProperty.GetArrayElementAtIndex(i);
                    categoryProperty.objectReferenceValue = m_categories[i].category;
                }
            }
        
            private void SyncSpecialFilters(SerializedProperty specialFiltersProperty)
            {
                specialFiltersProperty.arraySize = m_specialFilters.Count;
                for (int i = 0; i < m_specialFilters.Count; i++)
                {
                    SerializedProperty specialFilterProperty = specialFiltersProperty.GetArrayElementAtIndex(i);
                    m_specialFilters[i].SaveChanges(specialFilterProperty);
                }
            }
        
            private void SyncRuleGroups(SerializedProperty ruleGroupsProperty, List<KeyValuePair<string, SerializedTileOutput>> decorators)
            {
                ruleGroupsProperty.arraySize = m_ruleGroups.Count;
                for (int i = 0; i < m_ruleGroups.Count; i++)
                {
                    SerializedProperty ruleGroupProperty = ruleGroupsProperty.GetArrayElementAtIndex(i);
                    m_ruleGroups[i].SaveChanges(ruleGroupProperty);

                    string id = m_ruleGroups[i].id;

                    foreach (SerializedTileRule rule in m_ruleGroups[i].rules)
                    {
                        if (!rule.identity.IsDecorator())
                        {
                            continue;
                        }

                        Sprite sprite = rule.output.sprite;
                        decorators.Add(new KeyValuePair<string, SerializedTileOutput>(id, rule.output));
                    }
                }
            }

            private void SyncOverlayGroups(TileSet tileSet, SerializedProperty overlayGroupsProperty)
            {
                overlayGroupsProperty.arraySize = m_overlayGroups.Count;
                for (int i = 0; i < m_overlayGroups.Count; i++)
                {
                    SerializedProperty overlayGroupProperty = overlayGroupsProperty.GetArrayElementAtIndex(i);
                    m_overlayGroups[i].SaveChanges(tileSet, overlayGroupProperty);
                }
            }

            private void SyncEmptySpriteRules(SerializedProperty emptySpriteRulesProperty, List<KeyValuePair<string, SerializedTileOutput>> decorators)
            {
                emptySpriteRulesProperty.arraySize = m_emptySpriteRules.Count;
                for (int i = 0; i < m_emptySpriteRules.Count; i++)
                {
                    m_emptySpriteRules[i].output.sprite = null;
                    
                    SerializedProperty ruleProperty = emptySpriteRulesProperty.GetArrayElementAtIndex(i);
                    m_emptySpriteRules[i].SaveChanges(ruleProperty);

                    if (m_emptySpriteRules[i].identity.IsDecorator())
                    {
                        decorators.Add(new KeyValuePair<string, SerializedTileOutput>($"empty.{i}", m_emptySpriteRules[i].output));
                    }
                }
            }

            private void SyncDecorators(SerializedProperty decoratorsProperty, IEnumerable<KeyValuePair<string, SerializedTileOutput>> decorators)
            {
                HashSet<SimpleTile> oldDecorators = new HashSet<SimpleTile>();
                for (int i = 0; i < decoratorsProperty.arraySize; i++)
                {
                    SimpleTile decorator = decoratorsProperty.GetArrayElementAtIndex(i).objectReferenceValue as SimpleTile;
                    if (decorator != null)
                    {
                        oldDecorators.Add(decorator);
                    }
                }

                List<SimpleTile> newDecorators = new List<SimpleTile>();
                foreach (KeyValuePair<string, SerializedTileOutput> newDecorator in decorators)
                {
                    string id = newDecorator.Key;

                    SerializedTileOutput output = newDecorator.Value;
                
                    SimpleTile decorator = oldDecorators.FirstOrDefault(d => CompareDecorator(d, id, output.sprite));
                    if (decorator != null)
                    {
                        oldDecorators.Remove(decorator);
                    }
                    else
                    {
                        decorator = CreateInstance<SimpleTile>();
                        decorator.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                        AssetDatabase.AddObjectToAsset(decorator, m_tileSet);
                    }
                    newDecorators.Add(decorator);

                    SerializedObject serializedObject = new SerializedObject(decorator);

                    SerializedProperty idProperty = serializedObject.FindProperty("m_id");
                    idProperty.stringValue = id;

                    SerializedProperty colliderTypeProperty = serializedObject.FindProperty("m_colliderType");
                    colliderTypeProperty.enumValueIndex = (int)UnityTile.ColliderType.None;

                    SerializedProperty outputProperty = serializedObject.FindProperty("m_output");
                    output.SaveChanges(outputProperty);

                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                foreach (SimpleTile oldDecorator in oldDecorators)
                {
                    DestroyImmediate(oldDecorator, true);
                }

                decoratorsProperty.arraySize = newDecorators.Count;
                for (int i = 0; i < newDecorators.Count; i++)
                {
                    decoratorsProperty.GetArrayElementAtIndex(i).objectReferenceValue = newDecorators[i];
                }
            }

            private bool CompareDecorator(SimpleTile decorator, string id, Sprite sprite)
            {
                if (decorator == null)
                {
                    return false;
                }

                SerializedObject serializedObject = new SerializedObject(decorator);

                SerializedProperty idProperty = serializedObject.FindProperty("m_id");
                if (idProperty.stringValue != id)
                {
                    return false;
                }

                SerializedProperty spriteProperty = serializedObject.FindProperty("m_output").FindPropertyRelative("m_sprite");
                if (spriteProperty.objectReferenceValue as Sprite != sprite)
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        #region Painting

        private PaintContext m_paintContext;

        [SerializeField]
        private BrushInfo m_brush;
        
        [Serializable]
        internal struct BrushInfo
        {
            [SerializeField]
            private BrushType m_brushType;

            [SerializeField]
            private TileFilterType m_filterType;

            [SerializeField]
            private Tile m_tile;

            [SerializeField]
            private TileCategory m_category;

            [SerializeField]
            private float m_weight;

            [SerializeField]
            private int m_alternatingIndex;

            public BrushType brushType => m_brushType;

            public float weight => m_weight;

            public int alternatingIndex => m_alternatingIndex;

            public TileFilterType filterType => m_filterType;

            public Tile tile => m_tile;

            public TileCategory category => m_category;

            public void SetBrushType(PaintContext context, BrushType brushType)
            {
                m_brushType = brushType;
                context.UpdateBrush(this);
            }

            public void SetTile(PaintContext context, Tile tile)
            {
                m_filterType = TileFilterType.Tile;
                m_tile = tile;
                context.UpdateBrush(this);
            }

            public void SetCategory(PaintContext context, TileCategory category)
            {
                m_filterType = TileFilterType.TileCategory;
                m_category = category;
                context.UpdateBrush(this);
            }

            public void SetSpecialFilter(PaintContext context, TileFilterType filterType)
            {
                m_filterType = filterType;
                context.UpdateBrush(this);
            }
        
            public void SetWeight(PaintContext context, float weight)
            {
                m_weight = weight;
                context.UpdateBrush(this);
            }

            public void SetAlternatingIndex(PaintContext context, int alternatingIndex)
            {
                m_alternatingIndex = alternatingIndex;
                context.UpdateBrush(this);
            }

            public void OnTileSetChanged(PaintContext context, SerializedTileSet serializedTileSet)
            {
                bool containsTile = false;
                foreach (SerializedTile tile in serializedTileSet.tiles)
                {
                    if (tile.tile == m_tile)
                    {
                        containsTile = true;
                        break;
                    }
                }
                if (!containsTile)
                {
                    m_tile = null;
                }

                bool containsCategory = false;
                foreach (SerializedCategory category in serializedTileSet.categories)
                {
                    if (category.category == m_category)
                    {
                        containsCategory = true;
                        break;
                    }
                }
                if (!containsCategory)
                {
                    m_category = null;
                }

                if (m_filterType != TileFilterType.Tile && m_filterType != TileFilterType.TileCategory)
                {
                    TileFilterType oldFilterType = m_filterType;
                    SetTile(context, null);
                    SetSpecialFilter(context, oldFilterType);
                }
                else
                {
                    context.UpdateBrush(this);
                }
            }
        }

        internal enum BrushType
        {
            Rule,
            Weight,
            AlternatingIndex
        }

        internal class PaintContext
        {
            public event Action<Brush> onBrushChanged;

            public event Action<Brush, bool> onPaintStarted;

            public event Action<SerializedTile>          onTileColorChanged;
            public event Action<SerializedCategory>      onCategoryColorChanged;
            public event Action<SerializedSpecialFilter> onFilterColorChanged;

            private SerializedTileSet m_serializedTileSet;

            private Brush m_currentBrush;

            private bool m_active;
            private bool m_inverted;

            private TileFilterBrush       m_tileFilterBrush;
            private WeightBrush           m_weightBrush;
            private AlternatingIndexBrush m_alternatingIndexBrush;

            public Brush currentBrush => m_currentBrush;

            public bool active
            {
                get => m_active;
                set
                {
                    if (!m_active && value)
                    {
                        onPaintStarted?.Invoke(m_currentBrush, m_inverted);
                    }
                    m_active = value;
                }
            }

            public bool inverted
            {
                get => m_inverted;
                set => m_inverted = value;
            }

            public void UpdateBrush(BrushInfo brushInfo)
            {
                if (brushInfo.brushType == BrushType.Rule)
                {
                    switch (brushInfo.filterType)
                    {
                        case TileFilterType.Tile:
                            {
                                m_tileFilterBrush.SetTile(brushInfo.tile);
                                break;
                            }
                        case TileFilterType.TileCategory:
                            {
                                m_tileFilterBrush.SetCategory(brushInfo.category);
                                break;
                            }
                        default:
                            {
                                m_tileFilterBrush.SetSpecialFilter(brushInfo.filterType);
                                break;
                            }
                    }

                    m_currentBrush = m_tileFilterBrush;
                    onBrushChanged?.Invoke(m_currentBrush);

                    return;
                }
                if (brushInfo.brushType == BrushType.Weight)
                {
                    m_weightBrush.SetWeight(brushInfo.weight);

                    m_currentBrush = m_weightBrush;
                    onBrushChanged?.Invoke(m_currentBrush);

                    return;
                }
                if (brushInfo.brushType == BrushType.AlternatingIndex)
                {
                    m_alternatingIndexBrush.SetAlternatingIndex(brushInfo.alternatingIndex);

                    m_currentBrush = m_alternatingIndexBrush;
                    onBrushChanged?.Invoke(m_currentBrush);

                    return;
                }
            }

            public void OnTileInfoChanged(SerializedTile serializedTile, Action onChanged)
            {
                if (m_currentBrush is TileFilterBrush tileFilterBrush && tileFilterBrush.filterType == TileFilterType.Tile && tileFilterBrush.tile == serializedTile.tile)
                {
                    onChanged?.Invoke();
                }
            }

            public void OnCategoryInfoChanged(SerializedCategory serializedCategory, Action onChanged)
            {
                if (m_currentBrush is TileFilterBrush tileFilterBrush && tileFilterBrush.filterType == TileFilterType.TileCategory && tileFilterBrush.category == serializedCategory.category)
                {
                    onChanged?.Invoke();
                }
            }

            public void OnFilterInfoChanged(SerializedSpecialFilter serializedSpecialFilter, Action onChanged)
            {
                if (m_currentBrush is TileFilterBrush tileFilterBrush && tileFilterBrush.filterType == serializedSpecialFilter.filterType)
                {
                    onChanged?.Invoke();
                }
            }

            public void ClearBrushEvent()
            {
                onBrushChanged = null;
            }

            public SerializedTile GetSerializedTile(Tile tile)
            {
                return m_serializedTileSet.tiles.FirstOrDefault(t => t.tile == tile);
            }

            public SerializedCategory GetSerializedCategory(TileCategory category)
            {
                return m_serializedTileSet.categories.FirstOrDefault(c => c.category == category);
            }

            public SerializedSpecialFilter GetSerializedSpecialFilter(TileFilterType filterType)
            {
                return m_serializedTileSet.specialFilters.FirstOrDefault(f => f.filterType == filterType);
            }

            public PaintContext(TileSetEditorWindow tileSetEditor)
            {
                tileSetEditor.m_onTileColorChanged     += (t) => onTileColorChanged?.Invoke(t);
                tileSetEditor.m_onCategoryColorChanged += (c) => onCategoryColorChanged?.Invoke(c);
                tileSetEditor.m_onFilterColorChanged   += (f) => onFilterColorChanged?.Invoke(f);

                m_serializedTileSet = tileSetEditor.m_serializedTileSet;

                m_tileFilterBrush = new TileFilterBrush();
                m_tileFilterBrush.SetSpecialFilter(TileFilterType.Any);

                m_weightBrush = new WeightBrush();

                m_alternatingIndexBrush = new AlternatingIndexBrush();

                m_currentBrush = m_tileFilterBrush;
            }
        }

        internal abstract class Brush
        {

        }

        internal class TileFilterBrush : Brush
        {
            public TileFilterType filterType { get; private set; }

            public Tile tile { get; private set; }

            public TileCategory category { get; private set; }

            public bool SetTile(Tile tile)
            {
                if (tile == null)
                {
                    return SetSpecialFilter(TileFilterType.Any);
                }

                if (filterType != TileFilterType.Tile || this.tile != tile)
                {
                    filterType = TileFilterType.Tile;
                    this.tile = tile;
                    return true;
                }

                return false;
            }

            public bool SetCategory(TileCategory category)
            {
                if (category == null)
                {
                    return SetSpecialFilter(TileFilterType.Any);
                }

                if (filterType != TileFilterType.TileCategory || this.category != category)
                {
                    filterType = TileFilterType.TileCategory;
                    this.category = category;
                    return true;
                }

                return false;
            }

            public bool SetSpecialFilter(TileFilterType filterType)
            {
                if (this.filterType != filterType)
                {
                    this.filterType = filterType;
                    tile = null;
                    category = null;
                    return true;
                }

                return false;
            }
        }

        internal class WeightBrush : Brush
        {
            public float weight { get; private set; }

            public bool SetWeight(float weight)
            {
                if (this.weight != weight)
                {
                    this.weight = weight;
                    return true;
                }

                return false;
            }
        }

        internal class AlternatingIndexBrush : Brush
        {
            public int alternatingIndex { get; private set; }

            public bool SetAlternatingIndex(int alternatingIndex)
            {
                if (this.alternatingIndex != alternatingIndex)
                {
                    this.alternatingIndex = alternatingIndex;
                    return true;
                }

                return false;
            }
        }

        #endregion
    }
}
