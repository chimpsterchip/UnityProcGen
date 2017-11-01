using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrammarNode : MonoBehaviour {

    GrammarDungeon MasterDungeon;
    public GrammarRuleSet.Symbol Symbol; 
    // Is this node Terminal (No rules to replace it)
    bool Terminal = false;

    GrammarRuleSet RuleSet;
    GrammarRule Rule;

    GrammarNode[] Parents;
    GrammarNode[] Children;

	// Use this for initialization
	void Start () {
        MasterDungeon = GameObject.FindGameObjectWithTag("MasterDungeon").GetComponent<GrammarDungeon>();
        RuleSet = GameObject.FindGameObjectWithTag("RuleSet").GetComponent<GrammarRuleSet>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void ProcessRule()
    {
        // If there is no rule, then why process one?
        if(Rule == null)
        {
            return;
        }
        GrammarNode[] LocalNodes = new GrammarNode[Rule.ReplacementNodes.Length];
        GrammarNode AttachParentTo = null;
        GrammarNode AttachChildrenTo = null;
        // Create the nodes to replace this one
        for(int i = 0; i < Rule.ReplacementNodes.Length; ++i)
        {
            GrammarRule.NodeInfo NodeInfo = Rule.ReplacementNodes[i];
            GameObject tempObject = new GameObject();
            tempObject.AddComponent<GrammarNode>();
            tempObject.GetComponent<GrammarNode>().Symbol = NodeInfo.Symbol;
            tempObject.transform.position = tempObject.transform.position + NodeInfo.Offset;
            if(NodeInfo.AttachParent && AttachParentTo == null)
            {
                AttachParentTo = tempObject.GetComponent<GrammarNode>();
            }
            else if(NodeInfo.AttachChild && AttachChildrenTo == null)
            {
                AttachChildrenTo = tempObject.GetComponent<GrammarNode>();
            }
            LocalNodes[i] = tempObject.GetComponent<GrammarNode>();
        }
        // Connect those nodes together
        for(int i = 0; i < Rule.Connections.Length; ++i)
        {
            GrammarDungeon.NodeConnection NewConnect;
            NewConnect.Node1 = LocalNodes[Rule.Connections[i].p1];
            NewConnect.Node2 = LocalNodes[Rule.Connections[i].p2];
            NewConnect.p1 = MasterDungeon.GetNodeID() + Rule.Connections[i].p1 + 1;
            NewConnect.p2 = MasterDungeon.GetNodeID() + Rule.Connections[i].p2 + 1;
            MasterDungeon.AddConnection(NewConnect);
        }
        // Add Nodes to Dungeon List
        for(int i = 0; i < LocalNodes.Length; ++i)
        {
            MasterDungeon.AddNode(LocalNodes[i]);
        }
        // Hook up the parent nodes
        for(int i = 0; i < Parents.Length; ++i)
        {
            
        }
        // Hook up the children nodes
        for (int i = 0; i < Parents.Length; ++i)
        {

        }
    }

    // Check if the node can be replaced and gets rule reference
    bool CheckRules()
    {
        Rule = RuleSet.GetRule(Symbol);
        if(Rule == null)
        {
            Terminal = false;
            return true;
        }
        else
        {
            Terminal = true;
            return false;
        }
        
    }
}
