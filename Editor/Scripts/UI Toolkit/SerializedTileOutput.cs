using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal class SerializedTileOutput
    {
        [SerializeField]
        private Sprite m_sprite;

        [SerializeField]
        private Vector2 m_speed;

        [SerializeField]
        private Vector2 m_startTime;

        [SerializeField]
        private List<Sprite> m_frames = new List<Sprite>();

        public Sprite sprite
        {
            get => m_sprite;
            set => m_sprite = value;
        }

        public Vector2 speed
        {
            get => m_speed;
            set => m_speed = value;
        }

        public Vector2 startTime
        {
            get => m_startTime;
            set => m_startTime = value;
        }

        public float meanSpeed => 0.5f * (m_speed.x + m_speed.y);

        public float meanStartTime => 0.5f * (m_startTime.x + m_startTime.y);

        public List<Sprite> frames => m_frames;

        public bool Update(SerializedProperty tileOutputProperty)
        {
            SerializedProperty spriteProperty = tileOutputProperty.FindPropertyRelative("m_sprite");
            m_sprite = spriteProperty.objectReferenceValue as Sprite;

            SerializedProperty speedProperty = tileOutputProperty.FindPropertyRelative("m_speed");
            m_speed = speedProperty.vector2Value;

            SerializedProperty startTimeProperty = tileOutputProperty.FindPropertyRelative("m_startTime");
            m_startTime = startTimeProperty.vector2Value;

            SerializedProperty framesProperty = tileOutputProperty.FindPropertyRelative("m_frames");
            m_frames.Clear();
            for (int i = 0; i < framesProperty.arraySize; i++)
            {
                m_frames.Add(framesProperty.GetArrayElementAtIndex(i).objectReferenceValue as Sprite);
            }

            return false;
        }

        public void SaveChanges(SerializedProperty tileOutputProperty)
        {
            SerializedProperty spriteProperty = tileOutputProperty.FindPropertyRelative("m_sprite");
            spriteProperty.objectReferenceValue = m_sprite;

            SerializedProperty speedProperty = tileOutputProperty.FindPropertyRelative("m_speed");
            speedProperty.vector2Value = m_speed;

            SerializedProperty startTimeProperty = tileOutputProperty.FindPropertyRelative("m_startTime");
            startTimeProperty.vector2Value = m_startTime;

            SerializedProperty framesProperty = tileOutputProperty.FindPropertyRelative("m_frames");
            framesProperty.arraySize = m_frames.Count;
            for (int i = 0; i < m_frames.Count; i++)
            {
                framesProperty.GetArrayElementAtIndex(i).objectReferenceValue = m_frames[i];
            }
        }
    }
}
