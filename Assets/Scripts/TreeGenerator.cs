using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{

    [SerializeField]
    private TreeManager treeManager;

    private void Start()
    {
    }

    #region ============= Full binary =============
    [Header("Full Binary Tree")]
    [SerializeField]
    private int fullBinaryDepth = 6;

    [ButtonInstead(nameof(FullBinary), "Full Binary")]
    public bool _FullBinary;

    [ContextMenu("Full binary")]
    private void FullBinary()
    {
        Dictionary<int, NodeData> indexToData = new Dictionary<int, NodeData>();
        

        for (int i = 0; i < (1 << fullBinaryDepth) - 1; i++)
        {
            NodeData data = new NodeData()
            {
                index = i,
                parent = Mathf.FloorToInt((i - 1) / 2.0f),
                leftChild = 2 * i + 1,
                rightChild = 2 * i + 2,
            };
            indexToData.Add(i, data);
        }

        for (int i = (1 << fullBinaryDepth) - 1; i < 2 * (1 << fullBinaryDepth) - 1; i++)
        {
            NodeData data = new NodeData()
            {
                index = i,
                parent = Mathf.FloorToInt((i - 1) / 2),
                leftChild = -1,
                rightChild = -1,
            };
            indexToData.Add(i, data);
        }

        treeManager.GenerateTree(indexToData, 0);
    }

    #endregion


    #region ============= tree decoding =============
    [Header("Tree Decoding")]
    [SerializeField]
    private string treeEncoding = "";

    [ButtonInstead(nameof(DecodeTree), "Decode Tree")]
    public bool _DecodeTree;

    private void DecodeTree()
    {
        Dictionary<int, NodeData> indexToData = new Dictionary<int, NodeData>();
        DecodeTreeFrom(0, -1, treeEncoding.ToCharArray(), 0, indexToData);
        treeManager.GenerateTree(indexToData, 0);
    }

    //      nextIndex, nextPosition
    private (int, int) DecodeTreeFrom(int index, int parentIndex, char[] s, int position, Dictionary<int, NodeData> indexToData)
    {
        int currentIndex = index;
        index += 1;
        int leftIndex = -1;
        int rightIndex = -1;

        if (s[position] == 'u') // A leaf
        {
            position += 1;
        }
        else
        {
            if (s[position] == 'l')
            {
                leftIndex = index;
                (index, position) = DecodeTreeFrom(index, currentIndex, s, position + 1, indexToData);
            }

            if (s[position] == 'r')
            {
                rightIndex = index;
                (index, position) = DecodeTreeFrom(index, currentIndex, s, position + 1, indexToData);
            }

            // must finish with an 'u'
            position += 1;
        }

        NodeData data = new NodeData()
        {
            index = currentIndex,
            parent = parentIndex,
            leftChild = leftIndex,
            rightChild = rightIndex,
        };
        indexToData.Add(currentIndex, data);

        return (index, position);
    }

    #endregion


    #region ============= Add Node =============
    [Header("Ordered Nodes")]

    [SerializeField]
    private string nodeIndices = "0,1,2,3";

    [ButtonInstead(nameof(AddNode), "Add Ordered Nodes")]
    public bool _AddNode;

    private void AddNode()
    {        
        treeManager.AddOrdered(nodeIndices.Split(',').Select(int.Parse).ToArray());
    }

    [SerializeField]
    private int sizeOrderedTree = 31;

    [ButtonInstead(nameof(CreateOrderedTree), "Create Ordered Tree")]
    public bool _CreateOrderedTree;


    private void CreateOrderedTree()
    {
        int[] indices = new int[sizeOrderedTree];
        int arrayIndex = 0;


        void CreateOrderedTree(int fromIndex, int toIndex)
        {
            if (fromIndex >= toIndex)
                return;

            if (fromIndex + 1 == toIndex)
            {
                indices[arrayIndex++] = fromIndex;
                return;
            }

            if (fromIndex + 2 == toIndex)
            {
                indices[arrayIndex++] = fromIndex;
                indices[arrayIndex++] = fromIndex+1;
                return;
            }

            int midIndex = Mathf.FloorToInt((fromIndex + toIndex) / 2);
            indices[arrayIndex++] = midIndex;
            CreateOrderedTree(fromIndex, midIndex);
            CreateOrderedTree(midIndex + 1, toIndex);
        }

        CreateOrderedTree(1, sizeOrderedTree + 1);

        treeManager.ClearTree();
        treeManager.AddOrdered(indices);

    }

    #endregion

    #region ============= Clear Tree =============
    [Header("Clear Tree")]

    [ButtonInstead(nameof(ClearTree), "Clear Tree")]
    public bool _ClearTree;

    private void ClearTree()
    {
        treeManager.ClearTree();
    }

    #endregion
}
