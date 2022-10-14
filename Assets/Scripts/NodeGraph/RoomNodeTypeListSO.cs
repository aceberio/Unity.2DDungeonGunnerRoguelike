using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace NodeGraph
{
    [CreateAssetMenu(fileName = "RoomNodeTypeListSO", menuName = "Scriptable Objects/Dungeon/Room Node Type List")]
    public class RoomNodeTypeListSO : ScriptableObject
    {
        [field: SerializeField, Space(10), Header("Room Node Type List")]
        [Tooltip("This list should be populated with all the RoomNodeTypeSO for the game - it is used instead of an enum")]
        public List<RoomNodeTypeSO> List { get; set; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(List), List);
        }
#endif
    }
}