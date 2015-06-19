using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class AStar
{
    public class Node
    {
	    public Vector3 parent, pos;
	    public float G, H, F;

        public Node()
        {
            parent = Vector3.down;
            pos = Vector3.zero;

			G = float.MaxValue;
			H = float.MaxValue;
			F = float.MaxValue;
        }
        public Node(Vector3 pPos)
        {
            parent = Vector3.down;
            pos = pPos;

			G = float.MaxValue;
			H = float.MaxValue;
			F = float.MaxValue;
        }

        public bool Equals(Node obj)
        {
            return (obj.pos == pos);
        }

        public override bool Equals(object obj)
        {
            Node nObj = obj as Node;
            if (nObj == null)
                return false;
            else
                return Equals(nObj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    };

	//TODO add z
    public static float CalcH(Vector3 a, Vector3 b)
    {
	    return Mathf.Sqrt(Mathf.Pow(((float)a.x - (float)b.x), 2.0f) + Mathf.Pow(((float)a.y - (float)b.y), 2.0f));
	    //manhattan
		//return (int)(Mathf.Abs((float)a.x-(float)b.x) + Mathf.Abs((float)a.y-(float)b.y) + Mathf.Abs((float)a.z-(float)b.z));
    }

    public static Node LowestFScore(ref List<Node> lOpen)
    {
	    Node s = lOpen[0];
	
        foreach( Node n in lOpen )
	        if (n.F < s.F)
			    s = n;

	    return s;
    }


    public static Node Loop1ForStart(ref List<Node> l, Node end)
    {
	    //std::vector<Node>::iterator pos;
	    Node    o = new Node(), 
                n = end;
	
	    do
	    {
            n = l.Find( i => i.Equals(n) );
		    if( n.parent == Vector3.down ) // this is the node where we started
			    break;
		    o = n;
		    n.pos = n.parent;
	    }
        while (n.parent != Vector3.down);
        
	    return o;
    }


    public static void FillPath(ref List<Node> l, Node end, ref List<Vector3> lp)
    {
	    Node n = end;

	    do
	    {
            n = l.Find( i => i.Equals(n) );
		    if( n.parent == Vector3.down ) // this is the node where we started
			    break;
            lp.Add(n.pos);
		    n.pos = n.parent;
	    }
        while (n != null);
    }
}
