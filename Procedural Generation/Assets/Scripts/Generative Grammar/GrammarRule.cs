using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrammarRule : MonoBehaviour {

    //Stores the index position of two connected nodes


    [System.Serializable]
    public struct NodeInfo
    {
        public int id;
        public GrammarRuleSet.Symbol Symbol;
        public Vector3 Offset;
        public bool AttachParent;
        public bool AttachChild;
    }

    public GrammarRuleSet.Symbol ConditionSymbol;
    [Tooltip("Nodes to replace the initial node with")]
    public NodeInfo[] ReplacementNodes;
    [Tooltip("Relative connection of the nodes with the rule")]
    public GrammarDungeon.NodeConnection[] Connections;
    [Tooltip("What node to attach the parent nodes to")]
    public int StartNode;
    [Tooltip("What node to attach the child nodes to")]
    public int EndNode;
}
