using UnityEngine;
using Utilities;

namespace NodeGraph
{
    [CreateAssetMenu(fileName = "RoomNodeType", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
    public sealed class RoomNodeTypeSO : ScriptableObject
    {
        [field: SerializeField]
        public string RoomNodeTypeName { get; set; }

        [field: SerializeField]
        [field: Header("Only flag the RoomNodeTypes that should be visible in the editor")]
        public bool DisplayInNodeGraphEditor { get; set; } = true;

        [field: SerializeField]
        [field: Header("One Type Should Be A Corridor")]
        public bool IsCorridor { get; set; }

        [field: SerializeField]
        [field: Header("One Type Should Be A CorridorNS")]
        public bool IsCorridorNS { get; set; }

        [field: SerializeField]
        [field: Header("One Type Should Be A CorridorEW")]
        public bool IsCorridorEW { get; set; }

        [field: SerializeField]
        [field: Header("One Type Should Be An Entrance")]
        public bool IsEntrance { get; set; }

        [field: SerializeField]
        [field: Header("One Type Should Be A Boss Room")]
        public bool IsBossRoom { get; set; }

        [field: SerializeField]
        [field: Header("One Type Should Be None (Unassigned)")]
        public bool IsNone { get; set; }

#if UNITY_EDITOR
        private void OnValidate() => HelperUtilities.ValidateCheckEmptyString(this, nameof(RoomNodeTypeName), RoomNodeTypeName);
#endif
    }
}