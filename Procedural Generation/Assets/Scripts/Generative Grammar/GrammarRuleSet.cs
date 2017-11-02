using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrammarRuleSet : MonoBehaviour {

    public enum Symbol
    {
        entrance,
        obstacle,
        monster,
        tSplitObstacle,
        reward,
        chest,
        exit
    };

    public GrammarRule[] Rules;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public GrammarRule GetRule(Symbol _Symbol)
    {
        List<GrammarRule> MatchingRules = new List<GrammarRule>();
        for (int i = 0; i < Rules.Length; ++i)
        {
            if (Rules[i].ConditionSymbol == _Symbol)
            {
                MatchingRules.Add(Rules[i]);
            }
        }
        if (MatchingRules.Count == 0)
        {
            return null;
        }
        else
        {
            int RandInt = Random.Range(0, MatchingRules.Count - 1);
            return MatchingRules[RandInt];
        }
    }
}
