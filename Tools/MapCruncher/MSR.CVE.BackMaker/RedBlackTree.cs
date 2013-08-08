using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker
{
	public class RedBlackTree<T>
	{
		internal RBTreeNode<T> NIL;
		internal RBTreeNode<T> root;
		internal IComparer<T> comparer;
		public RedBlackTree()
		{
			this.NIL = RBTreeNode<T>.MakeNilNode(this);
			this.root = null;
		}
		public void Insert(T value)
		{
			this.root.InsertValue(value);
		}
	}
}
