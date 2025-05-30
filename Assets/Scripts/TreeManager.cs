using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

[Serializable]
public class NodeData
{
    public static NodeData Empty = new NodeData()
    {
        index      = -1,
        parent     = -1,
        leftChild  = -1,
        rightChild = -1
    };

    public int index;
    public int parent;
    public int leftChild;
    public int rightChild;

    override public string ToString()
    {
        return $"Node {index}->({leftChild},{rightChild})";
    }
}

public class TreeManager : MonoBehaviour, TreeViewer
{

    private const int LEFT = 0;
    private const int RIGHT = 1;
    static private readonly Color[] edgeColors = new Color[] { Color.red, Color.blue };
    static private readonly Color2[] edgeColors2 = new Color2[] { new Color2(Color.black, Color.red), new Color2(Color.black, Color.blue) };
    static private readonly Color2[] flippedEdgeColors2 = new Color2[] { new Color2(Color.red, Color.black), new Color2(Color.blue, Color.black) };

    [SerializeField]
    private Node nodePrefab;
    [SerializeField]
    List<NodeData> nodesData = new List<NodeData>();  // Since you can't serialize the dictionary
    Dictionary<int, NodeData> indexToData = new Dictionary<int, NodeData>();
    [SerializeField]
    private int root = -1;



    [SerializeField]
    private float r = 25;

    [SerializeField] 
    private MouseTypeEvent mouseEvent;

    [Header("Rotation animation parameters")]
    [SerializeField]
    private float duration = 3;
    [SerializeField]
    private float delay = 0.5f;


    private List<Node> nodes = new List<Node>();
    Dictionary<int, Node> indexToNode = new Dictionary<int, Node>();


    // Start is called before the first frame update
    void Awake()
    {
        mouseEvent = (mouseEvent == null) ? DefaultMouseTypeEvent.standard : mouseEvent;

        Dictionary<int, NodeData> treeData = new Dictionary<int, NodeData>();
        foreach (NodeData nodeData in nodesData)
        {
            treeData[nodeData.index] = nodeData;
        }
        GenerateTree(treeData, root);
    }

    public int GetRoot()
    {
        return root;
    }

    public NodeData GetNodeData(int index)
    {
        return indexToData[index];
    }

    public Node GetNode(int index)
    {
        return indexToNode[index];
    }

    public int Size()
    {
        return indexToNode.Count;
    }

    #region =================== Rotation ===================

    private void OnEnable()
    {
        mouseEvent.OnObjectSelect += OnObjectPressed;
    }

    private void OnDisable()
    {
        mouseEvent.OnObjectSelect -= OnObjectPressed;
    }

    private void OnObjectPressed(object sender, Transform pressedObject)
    {
        Transform pressedObjectParent = pressedObject.parent;
        if (pressedObjectParent == null)
            return;

        Node node = pressedObjectParent.GetComponent<Node>();
        if (node == null)
            return;
        
        if (!inRotation && node.GetIndex() != root)
        {
            rotationData = CreateRotationData(node.GetIndex());
        }
    }


    private bool inRotation = false;
    private RotationData rotationData;

    public event EventHandler<TreeViewer> OnTreeRotated;
    public UnityEvent<TreeViewer> UOnTreeRotated;
    public event EventHandler<TreeViewer> OnTreeChanged;
    public UnityEvent<TreeViewer> UOnTreeChanged;

    private RotationData CreateRotationData(int index)
    {
        if (index == root)
            throw new Exception($"Cannot rotate up the root {index}");

        if (!indexToData.TryGetValue(index, out NodeData childData) ||
            !indexToNode.TryGetValue(index, out Node childNode))
            throw new Exception($"Could not find childNode {index}");

        if (childData.parent == -1)
            throw new Exception($"Node {index} is root and cannot be rotated");

        if (!indexToData.TryGetValue(childData.parent, out NodeData parentData) ||
            !indexToNode.TryGetValue(childData.parent, out Node parentNode))
            throw new Exception($"Could not find parent {childData.parent} of childNode {index}");

        bool childOnLeft = (parentData.leftChild == index);
        int side = childOnLeft ? LEFT : RIGHT;

        Transform GenerateFollower(string name, Node node, Edge.Side edgeSide)
        {
            GameObject followObject = new GameObject(name);
            followObject.transform.parent = transform;
            Edge edge = node.GetEdgeToParent();
            followObject.transform.position = edge.GetPosition(edgeSide);
            node.GetEdgeToParent().SetFollowPosition(edgeSide, followObject.transform);
            return followObject.transform;
        }

        RotationData rotationData = new RotationData()
        {
            child = childNode.transform,
            childIndex = childData.index,
            childOnLeft = childOnLeft,

            parent = parentNode.transform,
            parentIndex = parentData.index,

            grandparentIndex = -1,
            migratingIndex = -1
        };


        Sequence sequence = DOTween.Sequence();

        PolarPosition parentPolarPosition = PolarPosition.FromVector(parentNode.transform.localPosition / r);
        PolarPosition otherChildPolarPosition = parentPolarPosition.Child(!childOnLeft);

        sequence.Join(childNode.transform.DOMove(parentNode.transform.localPosition, duration));
        sequence.Join(parentNode.transform.DOMove(otherChildPolarPosition.ToVector() * r, duration));
        LineRenderer childLineRenderer = childNode.GetEdgeToParent().GetComponent<LineRenderer>();
        sequence.Join(childLineRenderer.DOColor(edgeColors2[side], flippedEdgeColors2[1 - side], duration));

        if (parentData.index != root)
        {
            rotationData.followParent = GenerateFollower("Follow Parent", parentNode, Edge.Side.FROM_POSITION);
        }

        if (parentData.parent != -1)
        {
            if (!indexToData.TryGetValue(parentData.parent, out NodeData grandparentData))
            {
                throw new Exception($"Could not find grandparent {parentData.parent} of childNode {index}");
            }
            rotationData.grandparentIndex = grandparentData.index;
            rotationData.parentOnLeft = (grandparentData.leftChild == parentData.index);
        }

        int migratingIndex = childOnLeft ? childData.rightChild : childData.leftChild;
        if (migratingIndex != -1)
        {

            if (!indexToData.TryGetValue(migratingIndex, out NodeData migratingData) ||
                !indexToNode.TryGetValue(migratingIndex, out Node migratingNode))
                throw new Exception($"Could not find migrating childNode {migratingIndex} of childNode {index}");

            GameObject followMigrating = new GameObject("followMigrating");
            followMigrating.transform.parent = transform;
            followMigrating.transform.position = migratingNode.GetEdgeToParent().GetPosition(Edge.Side.TO_POSITION);
            migratingNode.GetEdgeToParent().SetFollowPosition(Edge.Side.TO_POSITION, followMigrating.transform);

            rotationData.migratingIndex = migratingData.index;
            rotationData.migrating = migratingNode.transform;
            rotationData.followMigrating = followMigrating.transform;

            sequence.Join(migratingNode.transform.DOMove(otherChildPolarPosition.Child(childOnLeft).ToVector() * r, duration));
            sequence.Join(followMigrating.transform.DOMove(otherChildPolarPosition.ToVector() * r, duration));
            LineRenderer lineRenderer = migratingNode.GetEdgeToParent().GetComponent<LineRenderer>();

            sequence.Join(lineRenderer.DOColor(edgeColors2[1 - side], edgeColors2[side], duration));

        }

        inRotation = true;

        sequence.SetDelay(delay)
                .OnStart(() => {
                    rotationData.Disconnect(indexToData);
                    if (rotationData.parentIndex == root)
                        root = -1;
                })
                .OnUpdate(() => { UpdatePositions(); })
                .OnComplete(() => {
                    rotationData.Reconnect(indexToData, indexToNode);
                    if (root == -1)
                        root = rotationData.childIndex;

                    inRotation = false;
                    OnTreeRotated?.Invoke(this, this);
                    UOnTreeRotated?.Invoke(this);
                });

        return rotationData;

    }


    private struct RotationData
    {
        public int childIndex;
        public bool childOnLeft;
        public int parentIndex;
        public bool parentOnLeft;
        public int grandparentIndex;
        public int migratingIndex;

        public Transform parent;
        public Transform followParent;
        public Transform child;
        public Transform followChild;
        public Transform migrating;
        public Transform followMigrating;


        private void Disconnect(int parentIndex, int childIndex, Dictionary<int, NodeData> indexToData)
        {
            if (parentIndex == -1 || childIndex == -1)
                return;

            NodeData parentData = indexToData[parentIndex];
            NodeData childData = indexToData[childIndex];

            childData.parent = -1;
            indexToData[childData.index] = childData;

            if (parentData.leftChild == childData.index)
                parentData.leftChild = -1;
            else
                parentData.rightChild = -1;
            indexToData[parentData.index] = parentData;
        }

        void Connect(int parentIndex, int childIndex, bool leftSide, Dictionary<int, NodeData> indexToData, Dictionary<int, Node> indexToNode)
        {
            if (childIndex == -1)
                return;

            NodeData childData = indexToData[childIndex];
            Node childNode = indexToNode[childIndex];
            Edge childEdge = childNode.GetEdgeToParent();
            SetEdgeWidth(childEdge, leftSide);


            childEdge.SetFollowPosition(Edge.Side.FROM_POSITION, childNode.transform);

            if (parentIndex == -1)
            {
                childEdge.SetFollowPosition(Edge.Side.TO_POSITION, childNode.transform);
                return;
            }

            NodeData parentData = indexToData[parentIndex];
            Node parentNode = indexToNode[parentIndex];
            parentNode.gameObject.name = parentData.ToString();

            childEdge.SetFollowPosition(Edge.Side.TO_POSITION, parentNode.transform);
            childEdge.SetColor(Edge.Side.TO_POSITION, TreeManager.edgeColors[leftSide ? LEFT : RIGHT]);
            childEdge.SetColor(Edge.Side.FROM_POSITION, Color.black);
            childEdge.UpdateEdge();


            childData.parent = parentData.index;
            indexToData[childData.index] = childData;

            if (leftSide)
                parentData.leftChild = childData.index;
            else
                parentData.rightChild = childData.index;
            indexToData[parentData.index] = parentData;
        }

        public void Disconnect(Dictionary<int, NodeData> indexToData)
        {
            Disconnect(grandparentIndex, parentIndex, indexToData);
            Disconnect(parentIndex, childIndex, indexToData);
            Disconnect(childIndex, migratingIndex, indexToData);
        }

        public void Reconnect(Dictionary<int, NodeData> indexToData, Dictionary<int, Node> indexToNode)
        {
            Connect(grandparentIndex, childIndex, parentOnLeft, indexToData, indexToNode);
            Connect(parentIndex, migratingIndex, childOnLeft, indexToData, indexToNode);
            Connect(childIndex, parentIndex, !childOnLeft, indexToData, indexToNode);

            if (followMigrating != null)
                Destroy(followMigrating.gameObject);
            if (followChild != null)
                Destroy(followChild.gameObject);
            if (followParent != null)
                Destroy(followParent.gameObject);
        }

    }

    #endregion


    #region =================== Tree GameObjects generation ===================

    private void GenerateSubTree(int index, int parentIndex, bool left)
    {
        if (index == -1)
            return;

        if (!indexToData.TryGetValue(index, out NodeData data))
        {
            throw new Exception($"NodeData for {index} was not generated properly.");
        }

        GenerateChild(parentIndex, left, index);

        GenerateSubTree(data.leftChild, index, true);
        GenerateSubTree(data.rightChild, index, false);
    }

    public void GenerateTree(Dictionary<int, NodeData> treeData, int root)
    {
        if (root == -1)
        {
            throw new Exception("Root index is not valid");
        }

        indexToData = new Dictionary<int, NodeData>(treeData);
        foreach ((int key, NodeData nodeData) in treeData)
        {
            Assert.AreEqual(key, nodeData.index);
            Assert.IsTrue(key >= 0);
        }

        nodesData.Clear();
        nodesData.AddRange(indexToData.Values);

        this.root = root;


        nodes.Clear();
        indexToNode.Clear();
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        indexToNode.Add(-1, null);
        GenerateSubTree(root, -1, true);

        UpdatePositions();
        OnTreeChanged?.Invoke(this, this);
        UOnTreeChanged?.Invoke(this);
    }

    private void GenerateChild(int parentIndex, bool left, int childIndex)
    {

        NodeData nodeData = new NodeData()
        {
            index = childIndex,
            parent = parentIndex,
            leftChild = -1,
            rightChild = -1,
        };


        Node node = Instantiate(nodePrefab, transform);
        node.gameObject.name = nodeData.ToString();
        node.SetLabel("" + childIndex);
        node.SetIndex(childIndex);

        nodes.Add(node);
        indexToNode[childIndex] = node;
        nodesData.Add(nodeData);
        indexToData[childIndex] = nodeData;

        Edge edge = node.GetEdgeToParent();

        if (parentIndex != -1)
        {
            NodeData parentData = indexToData[parentIndex];
            if (left)
                parentData.leftChild = childIndex;
            else
                parentData.rightChild = childIndex;
            indexToData[parentIndex] = parentData;
            Node parentNode = indexToNode[parentIndex];

            parentNode.gameObject.name = parentData.ToString();
            edge.SetFollowPosition(Edge.Side.TO_POSITION, parentNode.transform);
            SetEdgeWidth(edge, left);
        } else
        {
            edge.SetFollowPosition(Edge.Side.TO_POSITION, node.transform);
            root = childIndex;
        }
        edge.SetColor(Edge.Side.TO_POSITION, edgeColors[left ? LEFT : RIGHT]);
        edge.UpdateEdge();
    }

    static private void SetEdgeWidth(Edge edge, bool left)
    {
        LineRenderer lineRenderer = edge.GetComponent<LineRenderer>();
        if (left)
        {
            lineRenderer.startWidth = 0.2f;
            lineRenderer.endWidth = 0.9f;
        }
        else
        {
            lineRenderer.startWidth = 0.9f;
            lineRenderer.endWidth = 0.2f;
        }
    }

    public void AddOrdered(params int[] indices)
    {

        if (indices.Length == 0)
            return;

        foreach (int index in indices)
        {
            if (indexToData.ContainsKey(index))
            {
                throw new Exception($"Tree already has index {index}");
            }

            int parentIndex = -1;
            int childIndex = root;
            while (childIndex > -1)
            {
                parentIndex = childIndex;
                childIndex = (index < childIndex) ? indexToData[childIndex].leftChild : indexToData[childIndex].rightChild;
            }

            GenerateChild(parentIndex, index < parentIndex, index);
            UpdatePositions();
        }
        OnTreeChanged?.Invoke(this, this);
        UOnTreeChanged?.Invoke(this);
    }

    public void ClearTree()
    {
        indexToNode.Clear();
        nodesData.Clear();
        indexToData.Clear();
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        root = -1;
    }

    #endregion


    private struct PolarPosition
    {
        public float radius;
        public float angle;

        public Vector2 ToVector()
        {
            return new Vector2(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle));
        }

        public static PolarPosition FromVector(Vector3 v)
        {
            return new PolarPosition()
            {
                angle = -Vector2.SignedAngle(Vector2.up, v) * Mathf.PI / 180,
                radius = v.magnitude
            };
        }

        public PolarPosition Child(bool left)
        {
            return new PolarPosition()
            {
                radius = radius + 1,
                angle = angle + (left ? -1 : 1) * 0.5f * Mathf.PI / Mathf.Pow(2, radius)
            };
        }

        public PolarPosition LeftChild()
        {
            return new PolarPosition()
            {
                radius = radius + 1,
                angle = angle - 0.5f * Mathf.PI / Mathf.Pow(2, radius)
            };
        }

        public PolarPosition RightChild()
        {
            return new PolarPosition()
            {
                radius = radius + 1,
                angle = angle + 0.5f * Mathf.PI / Mathf.Pow(2, radius)
            };
        }
    }
    
    private void UpdatePositions()
    {
        if (root != -1)
            UpdatePosition(root, new PolarPosition() { angle = 0, radius = 0});

        if (inRotation)
        {
            UpdatePosition(rotationData.childIndex, PolarPosition.FromVector(rotationData.child.localPosition / r));
            UpdatePosition(rotationData.parentIndex, PolarPosition.FromVector(rotationData.parent.localPosition / r));
            if (rotationData.migratingIndex != -1)
            {
                UpdatePosition(rotationData.migratingIndex, PolarPosition.FromVector(rotationData.migrating.localPosition / r));
            }
        }


        foreach (Node node in indexToNode.Values)
        {
            if (node == null)    // TODO: I don't like this
                continue;
            node.GetEdgeToParent().UpdateEdge();
        }
    }

    private void UpdatePosition(int nodeIndex, PolarPosition pPos)
    {
        if (nodeIndex == -1)
            return;


        if (indexToData.TryGetValue(nodeIndex, out NodeData nodeData) &&
            indexToNode.TryGetValue(nodeIndex, out Node node))
        {
            //Debug.Log($"Updating {nodeIndex}");
            node.transform.localPosition = pPos.ToVector() * r;
            if (pPos.radius < r / 2)
            {
                pPos.angle *= (pPos.radius * 2 / r);
            }

            UpdatePosition(nodeData.leftChild,  pPos.LeftChild());
            UpdatePosition(nodeData.rightChild, pPos.RightChild());
        }
    }

    private string DFS_string(int index)
    {
        if (index == -1)
            return "";
        string leftStr = DFS_string(indexToData[index].leftChild);
        if (leftStr.Length > 0)
            leftStr = $"[{leftStr}]";
        string rightStr = DFS_string(indexToData[index].rightChild);
        if (rightStr.Length > 0)
            rightStr = $"[{rightStr}]";
        return $"{index}{leftStr} {rightStr}";
    }

}

public interface TreeViewer
{
    public int GetRoot();

    public NodeData GetNodeData(int index);

    public Node GetNode(int nodeIndex);

    public int Size();
}
