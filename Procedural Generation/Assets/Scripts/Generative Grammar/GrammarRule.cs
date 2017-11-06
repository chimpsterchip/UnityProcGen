using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrammarRule : MonoBehaviour {

    //Stores the index position of two connected nodes


    [System.Serializable]
    public struct NodeInfo
    {
        [Tooltip("Local ID use to identify nodes in a connection")]
        public int id;
        public GrammarRuleSet.Symbol Symbol;
        [Tooltip("Offset from its parent")]
        public Vector3 Offset;
        [Tooltip("Attach the original nodes parents to this node")]
        public bool AttachParent;
        [Tooltip("Attach the original nodes children to this node")]
        public bool AttachChild;
    }

    public GrammarRuleSet.Symbol ConditionSymbol;
    [Tooltip("Nodes to replace the initial node with")]
    public NodeInfo[] ReplacementNodes;
    [Tooltip("Relative connection of the nodes within the rule (Only ids are required)")]
    public GrammarDungeon.NodeConnection[] Connections;
    [Tooltip("What node to attach the parent nodes to")]
    public int StartNode;
    [Tooltip("What node to attach the child nodes to")]
    public int EndNode;
}
