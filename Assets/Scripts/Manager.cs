using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Manager : NetworkBehaviour
{
    //instance global reference
    public static Manager Instance { get; private set; }

	[SerializeField]
	private GameObject PiecePrefab;

    //material colors
    public Color[] mat;
	
	[Tooltip("Where the object spawning circumference is centered.")]
	[SerializeField]
	private Vector3 SpawnCenter;
	[Tooltip("Maximum distance from the center.")]
	[SerializeField]
	private float max_dist;
	[Tooltip("Minimum distance from the center.")]
	[SerializeField]
	private float min_dist;

    private void Awake()
    {
        //sets global reference
        if (Instance == null) Instance = this;
        //if there's  already an instance, remove this
        else Destroy(gameObject);
    }
	
	public void SpawnPiece()
	{
		//pos X
		float sign = (Random.Range(0, 1) == 0 ? -1 : 1);
		float x = SpawnCenter.x + Random.Range(min_dist, max_dist) * sign;
		//pos Z
		sign = (Random.Range(0, 1) == 0 ? -1 : 1);
		float z = SpawnCenter.z + Random.Range(min_dist, max_dist) * sign;
		
		//creates the piece
		GameObject obj = Instantiate(PiecePrefab,
									 new Vector3(x, SpawnCenter.y, z),
									 Quaternion.identity);
		//makes the piece spawn on all the clients
		NetworkServer.Spawn(obj);
	}
}
