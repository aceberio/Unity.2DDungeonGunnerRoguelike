using NodeGraph;
using UnityEngine;

namespace GameManager
{
    public class GameResources : MonoBehaviour
    {
        private static GameResources _instance;

        public static GameResources Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameResources>("GameResources");
                }
                return _instance;
            }
        }

        [field: Space(10), Header("Dungeon"), SerializeField]
        [Tooltip("Populate with the dungeon RoomNodeTypeListSO")]
        public RoomNodeTypeListSO RoomNodeTypeList { get; set; }
    }
}