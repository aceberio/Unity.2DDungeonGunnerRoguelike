using GameManager;
using Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NodeGraph
{
    public sealed class RoomNodeSO : ScriptableObject
    {
        [field: SerializeField]
        [field: HideInInspector]
        public string Id { get; set; }

        [field: SerializeField]
        [field: HideInInspector]
        public List<string> ParentRoomNodeIdList { get; set; } = new List<string>();

        [field: SerializeField]
        [field: HideInInspector]
        public List<string> ChildRoomNodeIdList { get; set; } = new List<string>();

        [field: SerializeField]
        [field: HideInInspector]
        public RoomNodeGraphSO RoomNodeGraph { get; set; }

        [field: SerializeField]
        [field: HideInInspector]
        public RoomNodeTypeSO RoomNodeType { get; set; }

        [field: SerializeField]
        [field: HideInInspector]
        public RoomNodeTypeListSO RoomNodeTypeList { get; set; }

        [field: SerializeField]
        [field: HideInInspector]
        public bool IsLeftClickDragging { get; set; }

        [field: SerializeField]
        [field: HideInInspector]
        public bool IsSelected { get; set; }

#if UNITY_EDITOR

        [field: SerializeField]
        [field: HideInInspector]
        public Rect Rect { get; set; }

        public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
        {
            Rect = rect;
            Id = Guid.NewGuid().ToString();
            name = "RoomNode";
            RoomNodeGraph = nodeGraph;
            RoomNodeType = roomNodeType;

            RoomNodeTypeList = GameResources.Instance.RoomNodeTypeList;
        }

        public void Draw(GUIStyle nodeStyle)
        {
            GUILayout.BeginArea(Rect, nodeStyle);

            EditorGUI.BeginChangeCheck();

            if (ParentRoomNodeIdList.Count > 0 || RoomNodeType.IsEntrance)
            {
                EditorGUILayout.LabelField(RoomNodeType.RoomNodeTypeName);
            }
            else
            {
                int selected = RoomNodeTypeList.List.FindIndex(x => x == RoomNodeType);

                int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

                RoomNodeType = RoomNodeTypeList.List[selection];

                if ((RoomNodeTypeList.List[selected].IsCorridor && !RoomNodeTypeList.List[selection].IsCorridor) ||
                    (!RoomNodeTypeList.List[selected].IsCorridor && RoomNodeTypeList.List[selection].IsCorridor) ||
                    (!RoomNodeTypeList.List[selected].IsBossRoom && RoomNodeTypeList.List[selection].IsBossRoom))
                    if (ChildRoomNodeIdList.Count > 0)
                        for (int i = ChildRoomNodeIdList.Count - 1; i >= 0; i--)
                        {
                            RoomNodeSO childRoomNode = RoomNodeGraph.GetRoomNode(ChildRoomNodeIdList[i]);

                            if (childRoomNode != null)
                            {
                                RemoveChildRoomNodeIdFromRoomNode(childRoomNode.Id);
                                childRoomNode.RemoveParentRoomNodeIdFromRoomNode(Id);
                            }
                        }
            }

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(this);

            GUILayout.EndArea();
        }

        public bool RemoveChildRoomNodeIdFromRoomNode(string childID)
        {
            if (ChildRoomNodeIdList.Contains(childID))
            {
                ChildRoomNodeIdList.Remove(childID);
                return true;
            }

            return false;
        }

        public bool RemoveParentRoomNodeIdFromRoomNode(string parentID)
        {
            if (ParentRoomNodeIdList.Contains(parentID))
            {
                ParentRoomNodeIdList.Remove(parentID);
                return true;
            }

            return false;
        }

        public string[] GetRoomNodeTypesToDisplay()
        {
            var roomArray = new string[RoomNodeTypeList.List.Count];

            for (var i = 0; i < RoomNodeTypeList.List.Count; i++)
                if (RoomNodeTypeList.List[i].DisplayInNodeGraphEditor)
                    roomArray[i] = RoomNodeTypeList.List[i].RoomNodeTypeName;

            return roomArray;
        }

        public void ProcessEvents(Event currentEvent)
        {
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    ProcessMouseDownEvent(currentEvent);
                    break;

                case EventType.MouseUp:
                    ProcessMouseUpEvent(currentEvent);
                    break;

                case EventType.MouseDrag:
                    ProcessMouseDragEvent(currentEvent);
                    break;
            }
        }

        public void DragNode(Vector2 delta)
        {
            Rect = new Rect(Rect.position + delta, Rect.size);
            EditorUtility.SetDirty(this);
        }

        public bool AddChildRoomNodeIdToRoomNode(string childID)
        {
            if (!IsChildRoomValid(childID)) return false;

            ChildRoomNodeIdList.Add(childID);

            return true;
        }

        public bool AddParentRoomNodeIdToRoomNode(string parentID)
        {
            ParentRoomNodeIdList.Add(parentID);
            return true;
        }

        public bool IsChildRoomValid(string childId)
        {
            if (Id == childId)
                return false;

            bool isConnectedBossNodeAlready =
                RoomNodeGraph.RoomNodeList.Any(roomNode =>
                    roomNode.RoomNodeType.IsBossRoom && roomNode.ParentRoomNodeIdList.Count > 0);

            if (RoomNodeGraph.GetRoomNode(childId).RoomNodeType.IsBossRoom && isConnectedBossNodeAlready)
                return false;

            if (RoomNodeGraph.GetRoomNode(childId).RoomNodeType.IsNone)
                return false;

            if (ChildRoomNodeIdList.Contains(childId))
                return false;

            if (ParentRoomNodeIdList.Contains(childId))
                return false;

            if (RoomNodeGraph.GetRoomNode(childId).ParentRoomNodeIdList.Count > 0)
                return false;

            if (RoomNodeGraph.GetRoomNode(childId).RoomNodeType.IsCorridor && RoomNodeType.IsCorridor)
                return false;

            if (!RoomNodeGraph.GetRoomNode(childId).RoomNodeType.IsCorridor && !RoomNodeType.IsCorridor)
                return false;

            if (RoomNodeGraph.GetRoomNode(childId).RoomNodeType.IsCorridor &&
                ChildRoomNodeIdList.Count >= Settings.MaxChildCorridors)
                return false;

            if (RoomNodeGraph.GetRoomNode(childId).RoomNodeType.IsEntrance)
                return false;

            return RoomNodeGraph.GetRoomNode(childId).RoomNodeType.IsCorridor || ChildRoomNodeIdList.Count <= 0;
        }

        private void ProcessMouseDownEvent(Event currentEvent)
        {
            switch (currentEvent.button)
            {
                case 0:
                    ProcessLeftClickDownEvent();
                    break;
                case 1:
                    ProcessRightClickDownEvent(currentEvent);
                    break;
            }
        }

        private void ProcessLeftClickDownEvent()
        {
            Selection.activeObject = this;

            IsSelected = !IsSelected;
        }

        private void ProcessRightClickDownEvent(Event currentEvent)
        {
            RoomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
        }

        private void ProcessMouseUpEvent(Event currentEvent)
        {
            if (currentEvent.button == 0) ProcessLeftClickUpEvent();
        }

        private void ProcessLeftClickUpEvent()
        {
            if (IsLeftClickDragging) IsLeftClickDragging = false;
        }

        private void ProcessMouseDragEvent(Event currentEvent)
        {
            if (currentEvent.button == 0) ProcessLeftMouseDragEvent(currentEvent);
        }

        private void ProcessLeftMouseDragEvent(Event currentEvent)
        {
            IsLeftClickDragging = true;

            DragNode(currentEvent.delta);
            GUI.changed = true;
        }
#endif
    }
}