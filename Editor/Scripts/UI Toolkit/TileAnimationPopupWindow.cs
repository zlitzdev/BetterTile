using System;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class TileAnimationPopupWindow : EditorWindow
    {
        private SerializedTileOutput m_serializedOutput;

        private Action m_onChanges;

        private Vector2Field m_speed;
        private Vector2Field m_startTime;

        private ListView m_frames;

        public static void Open(Vector2 center, SerializedTileOutput output, Action onChanges)
        {
            TileAnimationPopupWindow window = CreateInstance<TileAnimationPopupWindow>();

            Vector2 size = new Vector2(480.0f, 320.0f);
            Rect position = new Rect(center - size * 0.5f, size);
            
            window.ShowAsDropDown(position, size);

            window.position = position;

            window.m_serializedOutput = output;
            window.m_onChanges = onChanges;

            window.Bind();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.borderLeftWidth   = 1.0f;
            root.style.borderRightWidth  = 1.0f;
            root.style.borderTopWidth    = 1.0f;
            root.style.borderBottomWidth = 1.0f;
            root.style.borderLeftColor   = UIColors.border;
            root.style.borderRightColor  = UIColors.border;
            root.style.borderTopColor    = UIColors.border;
            root.style.borderBottomColor = UIColors.border;

            VisualElement header = new VisualElement();
            header.style.backgroundColor = UIColors.brighten;
            header.style.height = 22.0f;
            header.style.minHeight = 22.0f;
            header.style.marginBottom = 2.0f;
            root.Add(header);

            Label title = new Label();
            title.text = "Tile Animation";
            title.style.flexGrow = 1.0f;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.Add(title);

            VisualElement mainInfo = new VisualElement();
            mainInfo.style.flexDirection = FlexDirection.Row;
            root.Add(mainInfo);

            VisualElement previewContainer = new VisualElement();
            previewContainer.pickingMode = PickingMode.Ignore;
            previewContainer.style.width  = 64.0f;
            previewContainer.style.height = 64.0f;
            previewContainer.style.borderLeftWidth   = 1.0f;
            previewContainer.style.borderRightWidth  = 1.0f;
            previewContainer.style.borderTopWidth    = 1.0f;
            previewContainer.style.borderBottomWidth = 1.0f;
            previewContainer.style.marginLeft        = 6.0f;
            previewContainer.style.marginRight       = 2.0f;
            previewContainer.style.marginTop         = 6.0f;
            previewContainer.style.marginBottom      = 6.0f;
            previewContainer.style.borderLeftColor   = UIColors.border;
            previewContainer.style.borderRightColor  = UIColors.border;
            previewContainer.style.borderTopColor    = UIColors.border;
            previewContainer.style.borderBottomColor = UIColors.border;
            previewContainer.style.backgroundColor   = UIColors.darken;
            mainInfo.Add(previewContainer);

            VisualElement preview = new VisualElement();
            preview.pickingMode = PickingMode.Ignore;
            preview.style.flexGrow = 1.0f;
            preview.style.marginLeft   = 8.0f;
            preview.style.marginRight  = 8.0f;
            preview.style.marginTop    = 8.0f;
            preview.style.marginBottom = 8.0f;
            previewContainer.Add(preview);
            preview.schedule.Execute(() =>
            {
                Sprite sprite = null;
                if (m_serializedOutput != null && m_serializedOutput.frames.Count > 0)
                {
                    int index = (int)((EditorApplication.timeSinceStartup - m_serializedOutput.meanStartTime) * m_serializedOutput.meanSpeed);
                    while (index < 0)
                    {
                        index += m_serializedOutput.frames.Count;
                    }
                    sprite = m_serializedOutput.frames[index % m_serializedOutput.frames.Count];
                }

                preview.style.backgroundImage = new StyleBackground(sprite);
            }).Every(15);

            VisualElement animationPropertiesContainer = new VisualElement();
            animationPropertiesContainer.style.flexGrow = 1.0f;
            mainInfo.Add(animationPropertiesContainer);

            Vector2Field speed = new Vector2Field();
            speed.label = "Speed";
            animationPropertiesContainer.Add(speed);
            speed.RegisterValueChangedCallback(e =>
            {
                Vector2 oldValue = e.previousValue;
                Vector2 newValue = e.newValue;

                if (newValue.x != oldValue.x)
                {
                    newValue.y = Mathf.Max(newValue.x, newValue.y);
                    speed.SetValueWithoutNotify(newValue);
                }
                else if (newValue.y != oldValue.y)
                {
                    newValue.x = Mathf.Min(newValue.x, newValue.y);
                    speed.SetValueWithoutNotify(newValue);
                }

                if (m_serializedOutput != null)
                {
                    if (m_serializedOutput.speed != newValue)
                    {
                        m_serializedOutput.speed = newValue;
                        m_onChanges?.Invoke();
                    }
                }
            });
            m_speed = speed;

            Vector2Field startTime = new Vector2Field();
            startTime.label = "Start Time";
            animationPropertiesContainer.Add(startTime);
            startTime.RegisterValueChangedCallback(e =>
            {
                Vector2 oldValue = e.previousValue;
                Vector2 newValue = e.newValue;

                if (newValue.x != oldValue.x)
                {
                    newValue.y = Mathf.Max(newValue.x, newValue.y);
                    startTime.SetValueWithoutNotify(newValue);
                }
                else if (newValue.y != oldValue.y)
                {
                    newValue.x = Mathf.Min(newValue.x, newValue.y);
                    startTime.SetValueWithoutNotify(newValue);
                }

                if (m_serializedOutput != null)
                {
                    if (m_serializedOutput.startTime != newValue)
                    {
                        m_serializedOutput.startTime = newValue;
                        m_onChanges?.Invoke();
                    }
                }
            });
            m_startTime = startTime;

            ListView frames = new ListView();
            frames.showAddRemoveFooter = true;
            frames.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            frames.showBoundCollectionSize = true;
            frames.showFoldoutHeader = true;
            frames.headerTitle = "Frames";
            frames.reorderable = true;
            frames.reorderMode = ListViewReorderMode.Simple;
            frames.style.maxHeight = 216.0f;
            root.Add(frames);

            frames.makeItem = () => new SpriteItem((i, s) =>
            {
                if (m_serializedOutput.frames[i] != s)
                {
                    m_serializedOutput.frames[i] = s;
                    m_onChanges?.Invoke();
                }
            });
            frames.bindItem = (e, i) =>
            {
                if (e is SpriteItem spriteField)
                {
                    spriteField.label = $"Frame {i}";
                    spriteField.Bind(i, m_serializedOutput.frames[i]);
                }
            };

            frames.itemsAdded += (indices) =>
            {
                if (indices.Any())
                {
                    m_onChanges?.Invoke();
                    frames.RefreshItems();
                }
            };
            frames.itemsRemoved += (indices) =>
            {
                if (indices.Any())
                {
                    m_onChanges?.Invoke();
                    frames.RefreshItems();
                }
            };
            frames.itemIndexChanged += (oldIndex, newIndex) =>
            {
                if (oldIndex != newIndex)
                {
                    m_onChanges?.Invoke();
                    frames.RefreshItems();
                }
            };

            m_frames = frames;
        }

        private void Bind()
        {
            m_speed.value = m_serializedOutput.speed;
            m_startTime.value = m_serializedOutput.startTime;

            m_frames.itemsSource = m_serializedOutput.frames;
        }

        private class SpriteItem : ObjectField
        {
            public int index { get; private set; }

            private Action<int, Sprite> m_onValueChanged;

            public void Bind(int index, Sprite value)
            {
                this.index = index;
                SetValueWithoutNotify(value);
            }

            public SpriteItem(Action<int, Sprite> onValueChanged)
            {
                m_onValueChanged = onValueChanged;

                objectType = typeof(Sprite);

                this.RegisterValueChangedCallback(e =>
                {
                    m_onValueChanged?.Invoke(index, e.newValue as Sprite);
                });
            }
        }
    }
}
