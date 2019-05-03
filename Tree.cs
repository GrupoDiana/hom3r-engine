﻿// GENERIC IMPLEMENTATION OF NON-BINARY TREES
//**************************************************

using System.Collections.Generic;

public delegate bool TreeNodeCompare<T>(T nodeData1, T nodeData2);

public delegate bool TreeNodeCondition<T>(T nodeData);

public delegate void TreeNodeAction<T>(T nodeData);

public delegate void TreeNodeDo<T> (T source, T target);

public delegate void TreeAction<T>(Tree<T> subtree);

public class Tree<T>
{
	private T data;
	//private LinkedList<Tree<T>> children; // Use linkedlist if you want faster insert. we rather prefer faster access by index
	private List<Tree<T>> children;
	private Tree<T> parent;

//*****************************   
	public Tree(T ndata)
	{
		data = ndata;
		children = new List<Tree<T>>();
		parent = null;
	}

	//*****************************

	public void AddChild(T data)
	{
		//children.AddFirst(new Tree<T>(data)); // With linked list
		Tree<T> child = new Tree<T>(data);
		child.parent = this;
		children.Add(child); 
	}

    public bool isEmpty()
    {
        return (children == null);
    }

	//*****************************

	public Tree<T> GetChild(int i)
	{
		// Linked list:
		//foreach (Tree<T> n in children)
		//if (--i == 0)
		//	return n;
		//return null;
		return children [i];
	}

	//*****************************

	public int GetChildCount()
	{
		return children.Count;
	}

	//*****************************

	public T GetData()
	{
		return data;
	}

	//*****************************

	public void SetData(T newData)
	{
		data = newData;
	}
	
	//*****************************

	public Tree<T> GetParent()
	{
		return parent;
	}

	//*****************************

	public void DoIf(Tree<T> node, TreeNodeCompare<T> comparer, T otherData, TreeNodeAction<T> action)
	{
		bool comparison = comparer (node.data, otherData);
		if (comparison) 
		{
			action(node.data);
			foreach (Tree<T> kid in node.children)
				//DoIf (node, comparer, kid.GetData(), action);
				DoIf (kid, comparer, otherData, action);
		} 
		else 
		{
			foreach (Tree<T> kid in node.children)
				DoIf (kid, comparer, otherData, action);
		}
	}

	//*****************************

	public void DoIf(Tree<T> node, TreeNodeCompare<T> comparer, T otherData, TreeNodeDo<T> action)
	{
		bool comparison = comparer (node.data, otherData);
		if (comparison) 
		{
			action(node.data, otherData);
			foreach (Tree<T> kid in node.children)
				//DoIf (node, comparer, kid.GetData(), action);
				DoIf (kid, comparer, otherData, action);
		} 
		else 
		{
			foreach (Tree<T> kid in node.children)
				DoIf (kid, comparer, otherData, action);
		}
	}
	
	//*****************************

	public void DoIf(Tree<T> node, TreeNodeCondition<T> condition, TreeNodeAction<T> action)
	{
		bool meetsCondition = condition (node.data);
		if (meetsCondition) 
		{
			action(node.data);
			foreach (Tree<T> kid in node.children)
				DoIf (kid, condition, action);
		} 
		else 
		{
			foreach (Tree<T> kid in node.children)
				DoIf (kid, condition, action);
		}
	}
	
	//*****************************

	public void DoIf(Tree<T> node, TreeNodeCompare<T> comparer, T otherData, TreeAction<T> action)
	{
		bool comparison = comparer (node.data, otherData);
		if (comparison) 
		{
			action(node);
			foreach (Tree<T> kid in node.children)
				//DoIf (node, comparer, kid.GetData(), action);
				DoIf (kid, comparer, otherData, action);
		} 
		else 
		{
			foreach (Tree<T> kid in node.children)
				DoIf (kid, comparer, otherData, action);
		}
	}
	
	//*****************************

	// Do something only to direct children (not recursive), if a condition is met between the parent and each child
	public void DoIfParentToChildren(Tree<T> parent, TreeNodeCompare<T> comparer, TreeAction<T> action)
	{
		foreach (Tree<T> kid in parent.children) 
		{
			if (comparer (parent.data, kid.data))
				action(kid);
		}
	}
	
	//*****************************

	// Do something only to direct children (not recursive), if a condition is met for each child
	public void DoIfToChildren(Tree<T> parent, TreeNodeCondition<T> condition, TreeAction<T> action)
	{
		foreach (Tree<T> kid in parent.children) 
		{
			if (condition (kid.data))
				action(kid);
		}
	}
	
	//*****************************

	// Do something only to direct children (not recursive)
	public void DoToChildren(Tree<T> parent, TreeAction<T> action)
	{
		foreach (Tree<T> kid in parent.children) 
		{
			action(kid);
		}
	}
	
	//*****************************

	public void DoIf(Tree<T> node, TreeNodeCondition<T> condition, TreeAction<T> action)
	{
		bool meetsCondition = condition (node.data);
		if (meetsCondition) 
			action(node);

		foreach (Tree<T> kid in node.children)
				DoIf (kid, condition, action);
	}
	
	//*****************************

	public void Do(Tree<T> node, TreeNodeAction<T> action)
	{
		action (node.data);

		foreach (Tree<T> kid in node.children)
			Do (kid, action);
	}
	
	//*****************************

//	public T GetIf(Tree<T> node, TreeNodeCompare<T> comparer, T otherData)
//	{
//		bool comparison = comparer (node.data, otherData);
//		if (comparison) 
//		{
//			return node.data;
//		} 
//		else 
//		{
//			foreach (Tree<T> kid in node.children)
//			{
//				T kidResult = GetIf (kid, comparer, otherData);
//				if (kidResult != null)
//					return kidResult;
//			}
//		}
//
//		return default(T);
//	}
//	
	//*****************************

	public Tree<T> GetIf(Tree<T> node, TreeNodeCompare<T> comparer, T otherData)
	{
		bool comparison = comparer (node.data, otherData);
		if (comparison) 
		{
			return node;
		} 
		else 
		{
			foreach (Tree<T> kid in node.children)
			{
				Tree<T> kidResult = GetIf (kid, comparer, otherData);
				if (kidResult != null)
					return kidResult;
			}
		}
		
		return null;
	}
	
	//*****************************
    
	public bool MeetsCondition(Tree<T> node, TreeNodeCondition<T> visitor)
	{
		bool condition = visitor (node.data);
		if (condition) 
		{
			return true;
		} 
		else 
		{
			foreach (Tree<T> kid in node.children)
			{
				bool kidResult = MeetsCondition (kid, visitor);
				if (kidResult)
					return true;
			}
		}
		
		return false;
	}
	
	//*****************************

	public int GetChildPosition(TreeNodeCompare<T> visitor, T otherData)
	{
		int position = -1;

		for (int i=0; i < children.Count; i++) 
		{
			bool compare = visitor (children[i].data, otherData);
			if (compare)
			{
				position = i;
				break;
			}
		}

		return position;
	}

    public void Clear()
    {
        children.Clear();
        parent = null;
    }
}

