using System;

using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal struct TileRule
    {
        [SerializeField]
        private TileOutput m_output;

        [SerializeField]
        private float m_weight;

        [SerializeField]
        private bool m_static;

        [SerializeField]
        private TileIdentity m_identity;

        [SerializeField]
        private TileFilter m_nx;

        [SerializeField]
        private TileFilter m_px;

        [SerializeField]
        private TileFilter m_ny;

        [SerializeField]
        private TileFilter m_py;

        [SerializeField]
        private TileFilter m_nxny;

        [SerializeField]
        private TileFilter m_pxny;

        [SerializeField]
        private TileFilter m_nxpy;

        [SerializeField]
        private TileFilter m_pxpy;

        public TileOutput output => m_output;

        public float weight => m_weight;

        public bool isStatic => m_static;

        public TileIdentity identity => m_identity;

        public TileFilter nx => m_nx;
        
        public TileFilter px => m_px;
        
        public TileFilter ny => m_ny;
        
        public TileFilter py => m_py;
        
        public TileFilter nxny => m_nxny;
        
        public TileFilter pxny => m_pxny;
        
        public TileFilter nxpy => m_nxpy;

        public TileFilter pxpy => m_pxpy;
    }
}
