using GameManager;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NodeGraph.Editor
{
    public sealed class RoomNodeGraphEditor : EditorWindow
    {
        private const float NodeWidth = 160f;
        private const float NodeHeight = 75f;
        private const int NodePadding = 25;
        private const int NodeBorder = 12;
        private const float ConnectingLineWidth = 3f;
        private const float ConnectingLineArrowSize = 6f;
        private const float GridLarge = 100f;
        private const float GridSmall = 25f;
        private static RoomNodeGraphSO _currentRoomNodeGraph;
        private GUIStyle _roomNodeStyle;
        private GUIStyle _roomNodeSelectedStyle;
        private RoomNodeTypeListSO _roomNodeTypeList;
        private RoomNodeSO _currentRoomNode;
        private Vector2 _graphOffset;
        private Vector2 _graphDrag;

        [OnOpenAsset(0)]
        public static bool OnDoubleClickAsset(int instanceID, int line)
        {
            var roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

            if (roomNodeGraph == null) return false;
            OpenWindow();

            _currentRoomNodeGraph = roomNodeGraph;

            return true;
        }

        public void DragConnectingLine(Vector2 delta)
        {
            _currentRoomNodeGraph.LinePosition += delta;
        }

        [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
        private static void OpenWindow() => GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");

        private void OnEnable()
        {
            Selection.selectionChanged += InspectorSelectionChanged;

            _roomNodeStyle = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D,
                    textColor = Color.white
                },
                padding = new RectOffset(NodePadding, NodePadding, NodePadding, NodePadding),
                border = new RectOffset(NodeBorder, NodeBorder, NodeBorder, NodeBorder)
            };

            _roomNodeSelectedStyle = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D,
                    textColor = Color.white
                },
                padding = new RectOffset(NodePadding, NodePadding, NodePadding, NodePadding),
                border = new RectOffset(NodeBorder, NodeBorder, NodeBorder, NodeBorder)
            };

            _roomNodeTypeList = GameResources.Instance.RoomNodeTypeList;
        }

        private void OnDisable() => Selection.selectionChanged -= InspectorSelectionChanged;

        private void OnGUI()
        {
            if (_currentRoomNodeGraph != null)
            {
                DrawBackgroundGrid(GridSmall, 0.2f, Color.gray);
                DrawBackgroundGrid(GridLarge, 0.3f, Color.gray);
                DrawDraggedLine();
                ProcessEvents(Event.current);
                DrawRoomConnections();
                DrawRoomNodes();
            }

            if (GUI.changed)
                Repaint();
        }

        private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
        {
            int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
            int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

            Color previousColor = Handles.color;
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            _graphOffset += _graphDrag * 0.5f;

            var gridOffset = new Vector3(_graphOffset.x % gridSize, _graphOffset.y % gridSize, 0);

            for (var i = 0; i < verticalLineCount; i++)
                Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset,
                    new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);

            for (var j = 0; j < horizontalLineCount; j++)
                Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset,
                    new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);

            Handles.color = previousColor;
        }

        private void DrawDraggedLine()
        {
            if (_currentRoomNodeGraph.LinePosition != Vector2.zero)
                Handles.DrawBezier(_currentRoomNodeGraph.RoomNodeToDrawLineFrom.Rect.center,
                    _currentRoomNodeGraph.LinePosition,
                    _currentRoomNodeGraph.RoomNodeToDrawLineFrom.Rect.center,
                    _currentRoomNodeGraph.LinePosition,
                    Color.white,
                    null,
                    ConnectingLineWidth);
        }

        private void ProcessEvents(Event currentEvent)
        {
            _graphDrag = Vector2.zero;

            if (_currentRoomNode == null || _currentRoomNode.IsLeftClickDragging == false)
                _currentRoomNode = IsMouseOverRoomNode(currentEvent);

            if (_currentRoomNode == null || _currentRoomNodeGraph.RoomNodeToDrawLineFrom != null)
                ProcessRoomNodeGraphEvents(currentEvent);
            else
                _currentRoomNode.ProcessEvents(currentEvent);
        }

        private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
        {
            for (int i = _currentRoomNodeGraph.RoomNodeList.Count - 1; i >= 0; i--)
                if (_currentRoomNodeGraph.RoomNodeList[i].Rect.Contains(currentEvent.mousePosition))
                    return _currentRoomNodeGraph.RoomNodeList[i];

            return null;
        }

        private void ProcessRoomNodeGraphEvents(Event currentEvent)
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

        private void ProcessMouseDownEvent(Event currentEvent)
        {
            switch (currentEvent.button)
            {
                case 0:
                    ClearLineDrag();
                    ClearAllSelectedRoomNodes();
                    break;
                case 1:
                    ShowContextMenu(currentEvent.mousePosition);
                    break;
            }
        }

        private void ShowContextMenu(Vector2 mousePosition)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Create 8x5 Rooms Template"), false, CreateRoomsTemplate, mousePosition);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
            menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);

            menu.ShowAsContext();
        }

        private void SelectAllRoomNodes()
        {
            foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.RoomNodeList) roomNode.IsSelected = true;
            GUI.changed = true;
        }

        private void CreateRoomNode(object mousePositionObject)
        {
            if (_currentRoomNodeGraph.RoomNodeList.Count == 0)
                CreateRoomNode(new Vector2(200f, 200f), _roomNodeTypeList.List.Find(x => x.IsEntrance));

            CreateRoomNode(mousePositionObject, _roomNodeTypeList.List.Find(x => x.IsNone));
        }

        private void CreateRoomsTemplate(object mousePositionObject)
        {
            if (_currentRoomNodeGraph.RoomNodeList.Count > 0) return;

            RoomNodeTypeSO roomNodeTypeNone = _roomNodeTypeList.List.Find(x => x.IsNone);
            RoomNodeTypeSO roomNodeTypeEntrance = _roomNodeTypeList.List.Find(x => x.IsEntrance);
            for (var i = 0; i < 8; i++)
            for (var j = 0; j < 5; j++)
                CreateRoomNode(new Vector2(230f * i, 185 * j),
                    i == 0 && j == 0 ? roomNodeTypeEntrance : roomNodeTypeNone);
        }

        private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
        {
            var mousePosition = (Vector2)mousePositionObject;

            var roomNode = CreateInstance<RoomNodeSO>();

            _currentRoomNodeGraph.RoomNodeList.Add(roomNode);

            roomNode.Initialize(new Rect(mousePosition, new Vector2(NodeWidth, NodeHeight)),
                _currentRoomNodeGraph,
                roomNodeType);

            AssetDatabase.AddObjectToAsset(roomNode, _currentRoomNodeGraph);

            AssetDatabase.SaveAssets();

            _currentRoomNodeGraph.OnValidate();
        }

        private void DeleteSelectedRoomNodes()
        {
            var roomNodeDeletionQueue = new Queue<RoomNodeSO>();

            foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.RoomNodeList)
                if (roomNode.IsSelected && !roomNode.RoomNodeType.IsEntrance)
                {
                    roomNodeDeletionQueue.Enqueue(roomNode);

                    foreach (string childRoomNodeID in roomNode.ChildRoomNodeIdList)
                    {
                        RoomNodeSO childRoomNode = _currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                        if (childRoomNode != null) childRoomNode.RemoveParentRoomNodeIdFromRoomNode(roomNode.Id);
                    }

                    foreach (string parentRoomNodeID in roomNode.ParentRoomNodeIdList)
                    {
                        RoomNodeSO parentRoomNode = _currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                        if (parentRoomNode != null) parentRoomNode.RemoveChildRoomNodeIdFromRoomNode(roomNode.Id);
                    }
                }

            while (roomNodeDeletionQueue.Count > 0)
            {
                RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

                _currentRoomNodeGraph.RoomNodeDictionary.Remove(roomNodeToDelete.Id);
                _currentRoomNodeGraph.RoomNodeList.Remove(roomNodeToDelete);
                DestroyImmediate(roomNodeToDelete, true);

                AssetDatabase.SaveAssets();
            }
        }

        private void DeleteSelectedRoomNodeLinks()
        {
            foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.RoomNodeList)
                if (roomNode.IsSelected && roomNode.ChildRoomNodeIdList.Count > 0)
                    for (int i = roomNode.ChildRoomNodeIdList.Count - 1; i >= 0; i--)
                    {
                        RoomNodeSO childRoomNode = _currentRoomNodeGraph.GetRoomNode(roomNode.ChildRoomNodeIdList[i]);

                        if (childRoomNode != null && childRoomNode.IsSelected)
                        {
                            roomNode.RemoveChildRoomNodeIdFromRoomNode(childRoomNode.Id);
                            childRoomNode.RemoveParentRoomNodeIdFromRoomNode(roomNode.Id);
                        }
                    }

            ClearAllSelectedRoomNodes();
        }

        private void ClearAllSelectedRoomNodes()
        {
            foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.RoomNodeList)
                if (roomNode.IsSelected)
                {
                    roomNode.IsSelected = false;
                    GUI.changed = true;
                }
        }

        private void ProcessMouseUpEvent(Event currentEvent)
        {
            if (currentEvent.button == 1 && _currentRoomNodeGraph.RoomNodeToDrawLineFrom != null)
            {
                RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

                if (roomNode != null)
                    if (_currentRoomNodeGraph.RoomNodeToDrawLineFrom.AddChildRoomNodeIdToRoomNode(roomNode.Id))
                        roomNode.AddParentRoomNodeIdToRoomNode(_currentRoomNodeGraph.RoomNodeToDrawLineFrom.Id);

                ClearLineDrag();
            }
        }

        private void ProcessMouseDragEvent(Event currentEvent)
        {
            switch (currentEvent.button)
            {
                case 0:
                    ProcessLeftMouseDragEvent(currentEvent.delta);
                    break;
                case 1:
                    ProcessRightMouseDragEvent(currentEvent);
                    break;
             }
        }

        private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
        {
            _graphDrag = dragDelta;

            foreach (RoomNodeSO node in _currentRoomNodeGraph.RoomNodeList)
                node.DragNode(dragDelta);

            GUI.changed = true;
        }

        private void ProcessRightMouseDragEvent(Event currentEvent)
        {
            if (_currentRoomNodeGraph.RoomNodeToDrawLineFrom != null)
            {
                DragConnectingLine(currentEvent.delta);
                GUI.changed = true;
            }
        }

        private void ClearLineDrag()
        {
            _currentRoomNodeGraph.RoomNodeToDrawLineFrom = null;
            _currentRoomNodeGraph.LinePosition = Vector2.zero;
            GUI.changed = true;
        }

        private void DrawRoomConnections()
        {
            foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.RoomNodeList)
            foreach (string childRoomNodeID in roomNode.ChildRoomNodeIdList)
                if (_currentRoomNodeGraph.RoomNodeDictionary.ContainsKey(childRoomNodeID))
                {
                    DrawConnectionLine(roomNode, _currentRoomNodeGraph.RoomNodeDictionary[childRoomNodeID]);

                    GUI.changed = true;
                }
        }

        private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
        {
            Vector2 startPosition = parentRoomNode.Rect.center;
            Vector2 endPosition = childRoomNode.Rect.center;

            Vector2 midPosition = (endPosition + startPosition) / 2f;

            Vector2 direction = endPosition - startPosition;

            Vector2 arrowTailPoint1 =
                midPosition - new Vector2(-direction.y, direction.x).normalized * ConnectingLineArrowSize;
            Vector2 arrowTailPoint2 =
                midPosition + new Vector2(-direction.y, direction.x).normalized * ConnectingLineArrowSize;

            Vector2 arrowHeadPoint = midPosition + direction.normalized * ConnectingLineArrowSize;

            Handles.DrawBezier(arrowHeadPoint,
                arrowTailPoint1,
                arrowHeadPoint,
                arrowTailPoint1,
                Color.white,
                null,
                ConnectingLineWidth);

            Handles.DrawBezier(arrowHeadPoint,
                arrowTailPoint2,
                arrowHeadPoint,
                arrowTailPoint2,
                Color.white,
                null,
                ConnectingLineWidth);

            Handles.DrawBezier(startPosition,
                endPosition,
                startPosition,
                endPosition,
                Color.white,
                null,
                ConnectingLineWidth);

            GUI.changed = true;
        }

        private void DrawRoomNodes()
        {
            foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.RoomNodeList)
                if (roomNode.IsSelected)
                    roomNode.Draw(_roomNodeSelectedStyle);
                else
                    roomNode.Draw(_roomNodeStyle);

            GUI.changed = true;
        }

        private void InspectorSelectionChanged()
        {
            var roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

            if (roomNodeGraph != null)
            {
                _currentRoomNodeGraph = roomNodeGraph;
                GUI.changed = true;
            }
        }
    }
}