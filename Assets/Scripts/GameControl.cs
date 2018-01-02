using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour {

    public static GameControl controller;
	// Use this for initialization
	void Awake () {
		
        if (controller == null)
        {
            DontDestroyOnLoad(gameObject);
            controller = this;
        } else if (controller != this)
        {
            Destroy(gameObject);
        }
	}
}
