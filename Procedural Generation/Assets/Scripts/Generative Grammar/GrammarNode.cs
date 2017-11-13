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

    public int ID;

    public List<GrammarNode> Parents;
    public List<GrammarNode> Children;
    public Vector3 RelativePosition; // Its Position in relation to the parent or its world position if no parent

    public bool Marked = false;

	// Use this for initialization
	void Start () {
        Init();
	}

    public void Init()
    {
        MasterDungeon = GameObject.FindGameObjectWithTag("MasterDungeon").GetComponent<GrammarDungeon>();
        RuleSet = GameObject.FindGameObjectWithTag("RuleSet").GetComponent<GrammarRuleSet>();
        if(Parents == null) Parents = new List<GrammarNode>();
        if(Children == null) Children = new List<GrammarNode>();
        //RelativePosition = Vector3.zero;
    }
	
	// Update is called once per frame
	void Update () {

	}

    public void ProcessRule()
    {
        // If there is no rule, then why process one?
        if(Terminal || Rule == null)
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
            tempObject.GetComponent<GrammarNode>().Init();
            tempObject.GetComponent<GrammarNode>().RelativePosition = NodeInfo.Offset;
            tempObject.GetComponent<GrammarNode>().Symbol = NodeInfo.Symbol;
            tempObject.name = tempObject.GetComponent<GrammarNode>().Symbol.ToString();
            //Add Cellular Automata
            tempObject.AddComponent<CellularAutomata>();
            tempObject.GetComponent<CellularAutomata>().Floor = MasterDungeon.FloorTile;

            if (Parents.Count == 0)
            {
                tempObject.transform.position = tempObject.GetComponent<GrammarNode>().RelativePosition;
            }
            else
            {
                tempObject.transform.position = Parents[0].transform.position + tempObject.GetComponent<GrammarNode>().RelativePosition;
            }
            
            if(NodeInfo.AttachParent && AttachParentTo == null)
            {
                AttachParentTo = tempObject.GetComponent<GrammarNode>();
            }
            if(NodeInfo.AttachChild && AttachChildrenTo == null)
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

            LocalNodes[Rule.Connections[i].p1].AddChild(LocalNodes[Rule.Connections[i].p2]);
            LocalNodes[Rule.Connections[i].p2].AddParent(LocalNodes[Rule.Connections[i].p1]);
        }
        // Add Nodes to Dungeon List
        for(int i = 0; i < LocalNodes.Length; ++i)
        {
            MasterDungeon.AddNode(LocalNodes[i]);
        }
        // Hook up the parent nodes
        foreach(GrammarNode _Parent in Parents)
        {
            AttachParentTo.AddParent(_Parent);
            MasterDungeon.AddConnection(_Parent, AttachParentTo);
            _Parent.RemoveChild(this);
            _Parent.AddChild(AttachParentTo);
        }
        // Hook up the children nodes
        foreach (GrammarNode _Child in Children)
        {
            AttachChildrenTo.AddChild(_Child);
            MasterDungeon.AddConnection(AttachChildrenTo, _Child);
            _Child.RemoveParent(this);
            _Child.AddParent(AttachChildrenTo);
        }
        //Get rid of the original node
        AttachParentTo.GetComponent<GrammarNode>().RelativePosition = this.RelativePosition;
        MasterDungeon.RemoveConnection(this);
        MasterDungeon.RemoveNode(this);
        Destroy(this.gameObject);
    }

    // Check if the node can be replaced and gets rule reference
    public bool CheckRules()
    {
        Rule = RuleSet.GetRule(Symbol);
        if(Rule == null)
        {
            Terminal = true;
            return true;
        }
        else
        {
            Terminal = false;
            return false;
        }
        
    }

    public void Reposition()
    {
        if(Parents.Count == 0)
        {
            transform.position = RelativePosition;
        }
        else
        {
            transform.position = Parents[0].transform.position + RelativePosition;
        }
        foreach(GrammarNode _Node in Children)
        {
            _Node.Reposition();
        }
    }

    public void AddParent(GrammarNode _NewParent)
    {
        Parents.Add(_NewParent);
    }

    public void RemoveParent(GrammarNode _ParentToRemove)
    {
        Parents.Remove(_ParentToRemove);
    }

    public void AddChild(GrammarNode _NewChild)
    {
        Children.Add(_NewChild);
    }

    public void RemoveChild(GrammarNode _ChildToRemove)
    {
        Children.Remove(_ChildToRemove);
    }

    public List<GrammarNode> GetChildren()
    {
        return Children;
    }

    public List<GrammarNode> GetParents()
    {
        return Parents;
    }
}
