using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphWindow : EditorWindow
{
    private DialogueGraphView graphView;
    private DialogueSO currentDialogue;

    [MenuItem("Window/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraphWindow>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void ConstructGraphView()
    {
        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var objField = new ObjectField("Dialogue SO");
        objField.objectType = typeof(DialogueSO);
        objField.RegisterValueChangedCallback(evt => 
        {
            currentDialogue = evt.newValue as DialogueSO;
            LoadGraph();
        });
        toolbar.Add(objField);

        var btnSave = new Button(() => SaveGraph()) { text = "Save Graph" };
        toolbar.Add(btnSave);

        var lblHelp = new Label("  (Right-click in the graph to add nodes)");
        toolbar.Add(lblHelp);

        rootVisualElement.Add(toolbar);
    }

    private void LoadGraph()
    {
        graphView.ClearGraph();
        if (currentDialogue == null) return;

        // 1. Create all nodes visually
        foreach (var node in currentDialogue.nodes)
        {
            graphView.CreateNodeView(node);
        }

        // 2. Connect the nodes
        foreach (var node in currentDialogue.nodes)
        {
            var sourceView = graphView.GetNodeByGuid(node.id);
            if (sourceView == null) continue;

            // Connect outputs
            if (node.nodeType == NodeType.Condition)
            {
                if (!string.IsNullOrEmpty(node.conditionTrueNodeId))
                {
                    var targetView = graphView.GetNodeByGuid(node.conditionTrueNodeId);
                    if (targetView != null && sourceView.ports.Count > 0) graphView.AddElement(sourceView.ports[0].ConnectTo(targetView.inputPort));
                }
                if (!string.IsNullOrEmpty(node.conditionFalseNodeId))
                {
                    var targetView = graphView.GetNodeByGuid(node.conditionFalseNodeId);
                    if (targetView != null && sourceView.ports.Count > 1) graphView.AddElement(sourceView.ports[1].ConnectTo(targetView.inputPort));
                }
            }
            else if (node.nodeType == NodeType.Start || node.nodeType == NodeType.Dialogue || node.nodeType == NodeType.Action)
            {
                if (!string.IsNullOrEmpty(node.nextDialogueNodeId))
                {
                    var targetView = graphView.GetNodeByGuid(node.nextDialogueNodeId);
                    if (targetView != null && sourceView.ports.Count > 0)
                    {
                        var edge = sourceView.ports[0].ConnectTo(targetView.inputPort);
                        graphView.AddElement(edge);
                    }
                }
            }
            else if (node.nodeType == NodeType.Choice)
            {
                for (int i = 0; i < node.choices.Count; i++)
                {
                    var choice = node.choices[i];
                    if (string.IsNullOrEmpty(choice.targetNodeId)) continue;

                    var targetView = graphView.GetNodeByGuid(choice.targetNodeId);
                    if (targetView == null) continue;

                    if (i < sourceView.ports.Count)
                    {
                        var edge = sourceView.ports[i].ConnectTo(targetView.inputPort);
                        graphView.AddElement(edge);
                    }
                }
            }
        }
    }

    private void SaveGraph()
    {
        if (currentDialogue == null) return;

        currentDialogue.nodes.Clear();

        // Save nodes
        foreach (var view in graphView.nodes.ToList().Cast<DialogueNodeView>())
        {
            var saveNode = new DialogueNode
            {
                id = view.nodeGuid,
                name = view.title,
                nodeType = view.nodeType,
                endNodeType = view.endNodeType,
                dialogueText = view.dialogueText,
                actionType = view.actionType,
                actionId = view.actionId,
                actionValue = view.actionValue,
                conditionType = view.conditionType,
                conditionOperator = view.conditionOperator,
                conditionId = view.conditionId,
                conditionValue = view.conditionValue,
                position = view.GetPosition().position
            };

            if (view.nodeType == NodeType.Condition)
            {
                if (view.ports.Count > 0 && view.ports[0].connections.Any())
                    saveNode.conditionTrueNodeId = (view.ports[0].connections.First().input.node as DialogueNodeView).nodeGuid;
                if (view.ports.Count > 1 && view.ports[1].connections.Any())
                    saveNode.conditionFalseNodeId = (view.ports[1].connections.First().input.node as DialogueNodeView).nodeGuid;
            }
            else if (view.nodeType == NodeType.Start || view.nodeType == NodeType.Dialogue || view.nodeType == NodeType.Action)
            {
                if (view.ports.Count > 0 && view.ports[0].connections.Count() > 0)
                {
                    saveNode.nextDialogueNodeId = (view.ports[0].connections.First().input.node as DialogueNodeView).nodeGuid;
                }
            }
            else if (view.nodeType == NodeType.Choice)
            {
                // Save choices (outputs)
                foreach (var port in view.ports)
                {
                    var choice = new DialogueChoice { text = port.portName };
                    if (port.connections.Count() > 0)
                    {
                        choice.targetNodeId = (port.connections.First().input.node as DialogueNodeView).nodeGuid;
                    }
                    saveNode.choices.Add(choice);
                }
            }

            currentDialogue.nodes.Add(saveNode);
        }

        // Auto-assign start node
        var startNode = currentDialogue.nodes.FirstOrDefault(n => n.nodeType == NodeType.Start);
        currentDialogue.startNodeId = startNode != null ? startNode.id : string.Empty;

        EditorUtility.SetDirty(currentDialogue);
        AssetDatabase.SaveAssets();
        Debug.Log("Dialogue graph saved!");
    }
}

public class DialogueGraphView : GraphView
{
    private DialogueGraphWindow window;
    
    public DialogueGraphView(DialogueGraphWindow window)
    {
        this.window = window;

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }

    public void ClearGraph()
    {
        DeleteElements(nodes.ToList());
        DeleteElements(edges.ToList());
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        Vector2 nodePos = contentViewContainer.WorldToLocal(evt.mousePosition);

        evt.menu.AppendAction("Create Start Node", _ => CreateNewNode("Start", NodeType.Start, nodePos));
        evt.menu.AppendAction("Create Dialogue Node", _ => CreateNewNode("Dialogue", NodeType.Dialogue, nodePos));
        evt.menu.AppendAction("Create Choice Node", _ => CreateNewNode("Choice", NodeType.Choice, nodePos));
        evt.menu.AppendAction("Create Action Node", _ => CreateNewNode("Action", NodeType.Action, nodePos));
        evt.menu.AppendAction("Create If Node", _ => CreateNewNode("If", NodeType.Condition, nodePos));
        evt.menu.AppendAction("Create End Node", _ => CreateNewNode("End", NodeType.End, nodePos));
    }

    public void CreateNewNode(string nodeName, NodeType type, Vector2 position)
    {
        var node = new DialogueNode
        {
            id = Guid.NewGuid().ToString(),
            name = nodeName,
            nodeType = type,
            dialogueText = "...",
            position = position
        };
        CreateNodeView(node);
    }

    public void CreateNodeView(DialogueNode node)
    {
        var nodeView = new DialogueNodeView(node, this);
        AddElement(nodeView);
    }

    public DialogueNodeView GetNodeByGuid(string guid)
    {
        return nodes.ToList().Cast<DialogueNodeView>().FirstOrDefault(x => x.nodeGuid == guid);
    }
}

public class DialogueNodeView : Node
{
    public string nodeGuid;
    public string dialogueText;
    public NodeType nodeType;
    public EndNodeType endNodeType;
    
    // Action fields
    public ActionType actionType;
    public string actionId;
    public int actionValue;

    // Condition fields
    public ConditionType conditionType;
    public ConditionOperator conditionOperator;
    public string conditionId;
    public int conditionValue;

    public Port inputPort;
    public List<Port> ports = new List<Port>(); // Ouput ports (Choices / Next)
    private DialogueGraphView graphView;

    public DialogueNodeView(DialogueNode node, DialogueGraphView graphView)
    {
        this.graphView = graphView;
        this.nodeGuid = node.id;
        this.title = node.name;
        this.dialogueText = node.dialogueText;
        this.nodeType = node.nodeType;
        this.endNodeType = node.endNodeType;
        
        this.actionType = node.actionType;
        this.actionId = node.actionId;
        this.actionValue = node.actionValue;

        this.conditionType = node.conditionType;
        this.conditionOperator = node.conditionOperator;
        this.conditionId = node.conditionId;
        this.conditionValue = node.conditionValue;

        SetPosition(new Rect(node.position, Vector2.zero));

        // Create Input Port
        if (nodeType != NodeType.Start)
        {
            inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);
        }

        // Style the Title bar color based on node type
        switch (nodeType)
        {
            case NodeType.Start: titleContainer.style.backgroundColor = new Color(0.1f, 0.4f, 0.2f); break;
            case NodeType.Dialogue: titleContainer.style.backgroundColor = new Color(0.1f, 0.4f, 0.4f); break;
            case NodeType.End: titleContainer.style.backgroundColor = new Color(0.4f, 0.1f, 0.1f); break;
            case NodeType.Choice: titleContainer.style.backgroundColor = new Color(0.4f, 0.3f, 0.1f); break;
            case NodeType.Action: titleContainer.style.backgroundColor = new Color(0.1f, 0.2f, 0.4f); break;
            case NodeType.Condition: titleContainer.style.backgroundColor = new Color(0.4f, 0.1f, 0.5f); break;
        }

        // Edit Title Field
        var titleField = new TextField { value = title };
        titleField.RegisterValueChangedCallback(evt => title = evt.newValue);
        if (nodeType != NodeType.Start && nodeType != NodeType.End) 
            mainContainer.Add(titleField);

        // Edit Text Field (Only for Dialogue and Choice)
        if (nodeType == NodeType.Dialogue || nodeType == NodeType.Choice)
        {
            var textArea = new TextField { value = dialogueText, multiline = true };
            textArea.RegisterValueChangedCallback(evt => dialogueText = evt.newValue);
            textArea.style.minHeight = 60;
            textArea.style.maxWidth = 250;
            textArea.style.whiteSpace = WhiteSpace.Normal;
            mainContainer.Add(textArea);
        }
        else if (nodeType == NodeType.Action)
        {
            var typeField = new UnityEngine.UIElements.EnumField("Action Type", actionType);
            typeField.RegisterValueChangedCallback(evt => actionType = (ActionType)evt.newValue);
            mainContainer.Add(typeField);

            var idField = new TextField("Target ID") { value = actionId };
            idField.RegisterValueChangedCallback(evt => actionId = evt.newValue);
            mainContainer.Add(idField);

            var valField = new UnityEngine.UIElements.IntegerField("Value (Amount)") { value = actionValue };
            valField.RegisterValueChangedCallback(evt => actionValue = evt.newValue);
            mainContainer.Add(valField);
        }
        else if (nodeType == NodeType.Condition)
        {
            var typeField = new UnityEngine.UIElements.EnumField("Check", conditionType);
            typeField.RegisterValueChangedCallback(evt => conditionType = (ConditionType)evt.newValue);
            mainContainer.Add(typeField);

            var opField = new UnityEngine.UIElements.EnumField("Operator", conditionOperator);
            opField.RegisterValueChangedCallback(evt => conditionOperator = (ConditionOperator)evt.newValue);
            mainContainer.Add(opField);

            var idField = new TextField("Target ID") { value = conditionId };
            idField.RegisterValueChangedCallback(evt => conditionId = evt.newValue);
            mainContainer.Add(idField);

            var valField = new UnityEngine.UIElements.IntegerField("Value (Amount)") { value = conditionValue };
            valField.RegisterValueChangedCallback(evt => conditionValue = evt.newValue);
            mainContainer.Add(valField);
        }
        else if (nodeType == NodeType.End)
        {
            var enumField = new UnityEngine.UIElements.EnumField("End Behavior", endNodeType);
            enumField.RegisterValueChangedCallback(evt => endNodeType = (EndNodeType)evt.newValue);
            mainContainer.Add(enumField);
        }

        // Setup Outputs based on node type
        if (nodeType == NodeType.Start || nodeType == NodeType.Dialogue || nodeType == NodeType.Action)
        {
            var outPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outPort.portName = "Next";
            outputContainer.Add(outPort);
            ports.Add(outPort);
        }
        else if (nodeType == NodeType.Condition)
        {
            var truePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            truePort.portName = "True";
            outputContainer.Add(truePort);
            ports.Add(truePort);

            var falsePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            falsePort.portName = "False";
            outputContainer.Add(falsePort);
            ports.Add(falsePort);
        }
        else if (nodeType == NodeType.Choice)
        {
            var btnAddChoice = new Button(() => AddOutputPort("New Choice")) { text = "Add Choice" };
            titleButtonContainer.Add(btnAddChoice);

            foreach (var choice in node.choices)
            {
                AddOutputPort(choice.text);
            }
        }

        RefreshExpandedState();
        RefreshPorts();
    }

    public void AddOutputPort(string portName)
    {
        var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        outputPort.portName = portName;

        // Allow editing the choice text inside the port
        var textField = new TextField() { value = portName };
        textField.RegisterValueChangedCallback(evt => outputPort.portName = evt.newValue);
        outputPort.contentContainer.Add(textField);

        // Delete choice button
        var deleteBtn = new Button(() => 
        {
            graphView.DeleteElements(outputPort.connections);
            outputContainer.Remove(outputPort);
            ports.Remove(outputPort);
            RefreshPorts();
        }) { text = "X" };
        outputPort.contentContainer.Add(deleteBtn);

        outputContainer.Add(outputPort);
        ports.Add(outputPort);
        
        RefreshExpandedState();
        RefreshPorts();
    }
}