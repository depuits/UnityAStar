using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AStarPath
{
	public bool DEBUG_bShowPath = false;
	List<GameObject> showPathList = new List<GameObject>();
	List<GameObject> showCheckedPathList = new List<GameObject>();

	public List<Vector3> pathList = new List<Vector3>();
	public bool CanWalkDiagonal = true;

	public delegate bool CanMoveToTileDelegate( Vector3 from, Vector3 to);
	public CanMoveToTileDelegate CanMoveToTile;
	public bool HasPath { get { return (pathList.Count > 0); } }

	public int earlyFailCount = 200;
	public float gridSize = 1f;
	
	public bool GetCurrent(out Vector3 n)
	{
		n = new Vector3();
		if( pathList.Count <= 0 )
			return false;
		
		n = pathList[pathList.Count - 1];
		return true;
	}
	
	public bool GetNext(out Vector3 n)
	{
		n = new Vector3();
		if( pathList.Count <= 0 )
			return false;
		
		int index = pathList.Count - 1;
		n = pathList[index];
		pathList.RemoveAt(index);
		// -------------------------------- debug ----------------------------------
		if (DEBUG_bShowPath)
		{
			GameObject.Destroy(showPathList[index]);
			showPathList.RemoveAt(index);
		}
		// -------------------------------- debug end ----------------------------------
		return true;
	}
	
	public bool Progress()
	{
		if( pathList.Count <= 0 )
			return false;
		
		int index = pathList.Count - 1;
		pathList.RemoveAt(index);
		// -------------------------------- debug ----------------------------------
		if (DEBUG_bShowPath)
		{
			GameObject.Destroy(showPathList[index]);
			showPathList.RemoveAt(index);
		}
		// -------------------------------- debug end ----------------------------------

		if( index == 0)
		{
			if (DEBUG_bShowPath)
				foreach (var go in showCheckedPathList)
					GameObject.Destroy(go);

			return false; // last item was removed
		}

		return true;
	}

	//TODO create x y and z support aka 3D
    public bool FindPath(Vector3 fp, Vector3 tp)
	{
		foreach (var go in showCheckedPathList)
			GameObject.Destroy(go);
		
		showCheckedPathList.Clear();

		//align positions to grid
		fp = SnapToGrid(fp);
		tp = SnapToGrid(tp);

	    List<AStar.Node> lOpen = new List<AStar.Node>();
	    List<AStar.Node> lClosed = new List<AStar.Node>();
	
	    AStar.Node nEnd = new AStar.Node(tp);

		AStar.Node n = new AStar.Node(fp);
	    n.G = 0;
		n.H = AStar.CalcH(fp, tp);
	    n.F = n.G + n.H;
	
	    //start by adding the original position to the open list
	    lOpen.Add(n);
	    do
		{
		    // Get the square with the lowest F score 
		    AStar.Node c = AStar.LowestFScore(ref lOpen);

		    // add the current square to the closed list
			lClosed.Add(c);
			// ----------------------------------- debug extend closed list --------------------------------------
			if (DEBUG_bShowPath)
			{ 
				var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = "z_PathFindNodeCheck";
				go.renderer.material.color = new Color(0.0f, 0.0f, 0.75f);
				go.renderer.castShadows = false;
				go.transform.position = new Vector3(c.pos.x, c.pos.y, c.pos.z);
				go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f) * gridSize;
				GameObject.Destroy(go.collider);
				showCheckedPathList.Add(go);
			} // ----------------------------------- debug extend closed list end --------------------------------------

		    // remove it to the open list
		    if(!lOpen.Remove(c))
			    throw new System.Exception("Failed to remove item");
		
		    // if we added the destination to the closed list, we've found a path
		    if( lClosed.Contains(nEnd) )
		    {
				pathList.Clear();
				AStar.FillPath(ref lClosed, nEnd, ref pathList);

                // -------------------------------- debug ----------------------------------
                if (DEBUG_bShowPath)
                {
					foreach (var go in showPathList)
                        GameObject.Destroy(go);

					showPathList.Clear();
					foreach (var p in pathList)
                    {
                        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
						go.name = "z_PathFindNode";
						go.renderer.material.color = new Color(0.75f, 0.0f, 0f);
						go.renderer.castShadows = false;
                        go.transform.position = new Vector3(p.x, p.y, p.z) - Vector3.forward;
                        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f) * gridSize;
						GameObject.Destroy(go.collider);
						showPathList.Add(go);
					}
                }
                // -------------------------------- debug end ----------------------------------
				
//				Debug.Log("Way found\nOpen # : " + lOpen.Count + "\nclosed # : " + lClosed.Count);

			    return true; // break the loop
		    }

			if( lClosed.Count >= earlyFailCount )
			{
//				Debug.LogWarning("No way found because of Early Fail\nOpen # : " + lOpen.Count + "\nclosed # : " + lClosed.Count);
				return false;
			}

		    // Retrieve all its walkable adjacent squares
		    for( int x = -1; x <= 1; ++x )
		    {
			    for( int y = -1; y <= 1; ++y ) //TODO add z loop
			    {
					if( !CanWalkDiagonal && ( (x == 0 && y == 0) || (x != 0 && y != 0)) )
					    continue; // don't handle the current square

					Vector3 nextPos = c.pos + (new Vector3(x, y, 0) * gridSize);

					// check here if you can walk on or move to the tile
					if( CanMoveToTile != null )
						if( !CanMoveToTile(/*From*/ c.pos, /*To*/ nextPos) )
							continue;

					AStar.Node nn = new AStar.Node(nextPos);
				    // if this adjacent square is already in the closed list ignore it
				    if( lClosed.Contains(nn) )
                        continue; // Go to the next adjacent square

				    // if its not in the open list
				    if( !lOpen.Contains(nn) )
				    {
					    // compute its score, set the parent
					    nn.G = c.G + (c.pos - nn.pos).magnitude;
					    nn.H = AStar.CalcH(nn.pos, tp);
					    nn.F = nn.G + nn.H;

					    nn.parent = c.pos;
					    // and add it to the open list
					    lOpen.Add(nn);
				    }
				    // if its already in the open list
				    else
				    {
						float ng = c.G + (c.pos - nn.pos).magnitude;
						// test if using the current G score make the aSquare F score lower, if yes update the parent because it means its a better path		
					    if( nn.F > nn.H + ng )
					    {
						    lOpen.Remove(nn);
						    // compute its score, set the parent
						    nn.G = ng;
						    //nn.H = AStar.CalcH(nn.pos, tp);
						    nn.F = nn.G + nn.H;

						    nn.parent = c.pos;
						    // and add it to the open list
						    lOpen.Add(nn);
					    }
				    }
			    }
		    }
	    }
	    while( lOpen.Count > 0);

//		Debug.LogWarning("No way found\nOpen # : " + lOpen.Count + "\nclosed # : " + lClosed.Count);

	    return false;
    }

	Vector3 SnapToGrid(Vector3 v)
	{
		v /= gridSize;
		v = new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
		v *= gridSize;

		return v;
	}
}
