using System.Collections.Generic;
using UnityEngine;

namespace NodeGraph
{
    [CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
    public sealed class RoomNodeGraphSO : ScriptableObject
    {
        [field: SerializeField, HideInInspector] public RoomNodeTypeListSO RoomNodeTypeList { get; set; }
        [field: SerializeField, HideInInspector] public List<RoomNodeSO> RoomNodeList { get; set; } = new List<RoomNodeSO>();
        [field: SerializeField, HideInInspector] public Dictionary<string, RoomNodeSO> RoomNodeDictionary { get; set; } = new Dictionary<string, RoomNodeSO>();

#if UNITY_EDITOR

        [field: SerializeField, HideInInspector] public RoomNodeSO RoomNodeToDrawLineFrom { get; set; }
        [field: SerializeField, HideInInspector] public Vector2 LinePosition { get; set; }

        public void OnValidate() => LoadRoomNodeDictionary();

        public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position)
        {
            RoomNodeToDrawLineFrom = node;
            LinePosition = position;
        }

#endif

        private void Awake() => LoadRoomNodeDictionary();

        private void LoadRoomNodeDictionary()
        {
            RoomNodeDictionary.Clear();

            foreach (RoomNodeSO node in RoomNodeList)
            {
                RoomNodeDictionary[node.Id] = node;
            }
        }

        public RoomNodeSO GetRoomNode(string roomNodeID)
        {
            return RoomNodeDictionary.TryGetValue(roomNodeID, out RoomNodeSO roomNode) ? roomNode : null;
        }

    }
}