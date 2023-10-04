using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGoblin {
    public Dictionary<string, float> GetStats(); 
	public GameObject[] GetMoves();
	public void UsePrimary(GameObject goblin);
	public void UseSecondary(GameObject goblin);
}