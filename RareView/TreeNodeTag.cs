/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/7/2018
 * Time: 1:38 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RareView
{
	/// <summary>
	/// Description of TreeNodeTag.
	/// </summary>
	public class TreeNodeTag
	{
		public static int OBJECT_TYPE_NONE = 0;
		public static int OBJECT_TYPE_TEXTURE = 1;
		
		public static int OBJECT_TYPE_RENDERGRAPH = 2;
		public static int OBJECT_TYPE_RENDERGRAPH_VERTEX_BATCH = 3;
		public static int OBJECT_TYPE_RENDERGRAPH_TRIANGLE_STRIP = 4;
		public static int OBJECT_TYPE_RENDERGRAPH_MATERIAL = 5;
		
		
		public enum NodeType {File, Main, SubNode};
		public CaffFile File = null;
		public int ObjectID = 0;
		public int SubObjectID = 0;
		public int ObjectType = 0;
		public NodeType Type = NodeType.SubNode;
		
		public TreeNodeTag()
		{
		}
		
		public TreeNodeTag(CaffFile _File, int _objectID, int _objectType)
		{
			File = _File;
			ObjectID = _objectID;
			ObjectType = _objectType;
			Type = NodeType.Main;
			SubObjectID = 0;
		}
		
		public TreeNodeTag(CaffFile _File, int _objectID, int _objectType, int _subObjectID, NodeType _nodeType)
		{
			File = _File;
			ObjectID = _objectID;
			ObjectType = _objectType;
			Type = _nodeType;
			SubObjectID = _subObjectID;
		}
	}
}
