using System;

using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    [ExecuteAlways]
    internal class TilemapUniqueId : MonoBehaviour
    {
        [SerializeField]
        private string m_id;

        public string id
        {
            get
            {
                if (string.IsNullOrEmpty(m_id))
                {
                    m_id = Guid.NewGuid().ToString();
                }
                return m_id;
            }
        }

        private void Update()
        {
            hideFlags = HideFlags.HideInInspector;
        }
    }
}
