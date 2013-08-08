using System;
namespace MSR.CVE.BackMaker
{
	public class RBTreeNode<T>
	{
		private enum COLOR
		{
			BLACK,
			RED
		}
		private RedBlackTree<T> tree;
		private RBTreeNode<T> Left;
		private RBTreeNode<T> Right;
		private T value;
		private RBTreeNode<T> Parent;
		private RBTreeNode<T>.COLOR Color = RBTreeNode<T>.COLOR.RED;
		public T Data
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
			}
		}
		public RBTreeNode(RedBlackTree<T> tree) : this(tree, default(T))
		{
		}
		private RBTreeNode(RedBlackTree<T> tree, T value)
		{
			this.tree = tree;
			this.value = value;
			this.Left = null;
			this.Right = null;
			this.Color = RBTreeNode<T>.COLOR.RED;
			this.Parent = null;
		}
		internal static RBTreeNode<T> MakeRootNode(RedBlackTree<T> tree)
		{
			return new RBTreeNode<T>(tree)
			{
				Left = tree.NIL,
				Right = tree.NIL
			};
		}
		internal static RBTreeNode<T> MakeNilNode(RedBlackTree<T> tree)
		{
			RBTreeNode<T> rBTreeNode = new RBTreeNode<T>(tree);
			rBTreeNode.Color = RBTreeNode<T>.COLOR.BLACK;
			rBTreeNode.Left = (rBTreeNode.Right = (rBTreeNode.Parent = rBTreeNode));
			return rBTreeNode;
		}
		public RBTreeNode<T> GetLeft()
		{
			return this.Left;
		}
		public RBTreeNode<T> GetRight()
		{
			return this.Right;
		}
		public RBTreeNode<T> GetRoot()
		{
			return this.tree.root;
		}
		private bool IsRed(RBTreeNode<T> T)
		{
			return T.Color == RBTreeNode<T>.COLOR.RED;
		}
		private bool IsBlack(RBTreeNode<T> T)
		{
			return T.Color == RBTreeNode<T>.COLOR.BLACK;
		}
		public bool IsRed()
		{
			return this.Color == RBTreeNode<T>.COLOR.RED;
		}
		public bool IsBlack()
		{
			return this.Color == RBTreeNode<T>.COLOR.BLACK;
		}
		private RBTreeNode<T> GetGrandFather()
		{
			if (this.Parent == null)
			{
				return null;
			}
			if (this.Parent.Parent != null)
			{
				return this.Parent.Parent;
			}
			return null;
		}
		private RBTreeNode<T> GetRightUncle()
		{
			RBTreeNode<T> grandFather = this.GetGrandFather();
			if (grandFather != null)
			{
				return grandFather.Right;
			}
			return null;
		}
		private RBTreeNode<T> GetLeftUncle()
		{
			RBTreeNode<T> grandFather = this.GetGrandFather();
			if (grandFather != null)
			{
				return grandFather.Left;
			}
			return null;
		}
		private bool IsFatherLeftofGrandFather()
		{
			return !this.IsRoot() && this.Parent.Parent != null && this.Parent.Parent.Left == this;
		}
		private bool IsFatherRightofGrandFather()
		{
			return !this.IsRoot() && this.Parent.Parent != null && this.Parent.Parent.Right == this;
		}
		private void RotateLeft(RBTreeNode<T> x)
		{
			RBTreeNode<T> right = x.Right;
			if (right != this.tree.NIL)
			{
				x.Right = right.Left;
				if (right.Left != this.tree.NIL)
				{
					right.Left.Parent = x;
				}
				right.Parent = x.Parent;
				if (x.Parent == null || x.Parent == this.tree.NIL)
				{
					this.tree.root = right;
				}
				else
				{
					if (x.Parent.Left == x)
					{
						x.Parent.Left = right;
					}
					else
					{
						x.Parent.Right = right;
					}
				}
				right.Left = x;
				x.Parent = right;
			}
		}
		private void RotateRight(RBTreeNode<T> x)
		{
			RBTreeNode<T> left = x.Left;
			if (left != this.tree.NIL)
			{
				x.Left = left.Right;
				if (left.Right != this.tree.NIL)
				{
					left.Right.Parent = x;
				}
				left.Parent = x.Parent;
				if (x.Parent == null || x.Parent == this.tree.NIL)
				{
					this.tree.root = left;
				}
				else
				{
					if (x.Parent.Right == x)
					{
						x.Parent.Right = left;
					}
					else
					{
						x.Parent.Left = left;
					}
				}
				left.Right = x;
				x.Parent = left;
			}
		}
		private bool IsRoot(RBTreeNode<T> T)
		{
			return T.Parent == null;
		}
		private bool IsRoot()
		{
			return this.Parent == null;
		}
		private void ColorGrandFather(RBTreeNode<T>.COLOR c)
		{
			if (this.Parent.Parent != null)
			{
				this.Parent.Parent.Color = c;
			}
		}
		private void ColorFather(RBTreeNode<T>.COLOR c)
		{
			if (this.Parent != null)
			{
				this.Parent.Color = c;
			}
		}
		private void CheckBalance()
		{
			if (!this.IsRoot() && this.Parent.IsRed())
			{
				if (this.IsFatherLeftofGrandFather())
				{
					RBTreeNode<T> rightUncle = this.GetRightUncle();
					if (rightUncle != this.tree.NIL && rightUncle != null && rightUncle.IsRed())
					{
						this.ColorGrandFather(RBTreeNode<T>.COLOR.RED);
						rightUncle.Color = RBTreeNode<T>.COLOR.BLACK;
						this.Parent.Color = RBTreeNode<T>.COLOR.BLACK;
						RBTreeNode<T> grandFather = this.GetGrandFather();
						if (grandFather != null)
						{
							grandFather.CheckBalance();
						}
					}
					else
					{
						if (this == this.Parent.Right)
						{
							this.RotateLeft(this.Parent);
						}
						this.ColorFather(RBTreeNode<T>.COLOR.BLACK);
						this.ColorGrandFather(RBTreeNode<T>.COLOR.RED);
						if (this.Parent.Parent != null)
						{
							this.RotateRight(this.Parent.Parent);
						}
					}
				}
				else
				{
					RBTreeNode<T> leftUncle = this.GetLeftUncle();
					if (leftUncle != this.tree.NIL && leftUncle != null && leftUncle.IsRed())
					{
						this.ColorGrandFather(RBTreeNode<T>.COLOR.RED);
						leftUncle.Color = RBTreeNode<T>.COLOR.BLACK;
						this.Parent.Color = RBTreeNode<T>.COLOR.BLACK;
						RBTreeNode<T> grandFather2 = this.GetGrandFather();
						if (grandFather2 != null)
						{
							grandFather2.CheckBalance();
						}
					}
					else
					{
						if (this == this.Parent.Left)
						{
							this.RotateRight(this.Parent);
						}
						this.ColorFather(RBTreeNode<T>.COLOR.BLACK);
						this.ColorGrandFather(RBTreeNode<T>.COLOR.RED);
						if (this.Parent.Parent != null)
						{
							this.RotateLeft(this.Parent.Parent);
						}
					}
				}
			}
			this.tree.root.Color = RBTreeNode<T>.COLOR.BLACK;
		}
		private RBTreeNode<T> TreeMinimum(RBTreeNode<T> T)
		{
			if (T == null)
			{
				return null;
			}
			while (T.Left != this.tree.NIL)
			{
				T = T.Left;
			}
			return T;
		}
		private RBTreeNode<T> TreeMaximum(RBTreeNode<T> T)
		{
			if (T == null)
			{
				return null;
			}
			while (T.Right != this.tree.NIL)
			{
				T = T.Right;
			}
			return T;
		}
		private RBTreeNode<T> TreeSuccessor(RBTreeNode<T> T)
		{
			if (T == this.tree.NIL)
			{
				return null;
			}
			if (T.Right != this.tree.NIL)
			{
				return this.TreeMinimum(T.Right);
			}
			RBTreeNode<T> parent = T.Parent;
			while (parent != null && T == parent.Right)
			{
				T = parent;
				parent = parent.Parent;
			}
			return parent;
		}
		private RBTreeNode<T> Search(RBTreeNode<T> tn, T Value)
		{
			while (tn != this.tree.NIL && this.tree.comparer.Compare(tn.Data, Value) != 0)
			{
				if (this.tree.comparer.Compare(Value, tn.Data) < 0)
				{
					tn = tn.Left;
				}
				else
				{
					tn = tn.Right;
				}
			}
			return tn;
		}
		internal bool InsertValue(T newValue)
		{
			RBTreeNode<T> rBTreeNode = this;
			if (this.tree.NIL != rBTreeNode.Search(rBTreeNode, newValue))
			{
				return false;
			}
			while (rBTreeNode != this.tree.NIL)
			{
				if (this.tree.comparer.Compare(newValue, rBTreeNode.value) <= 0)
				{
					if (rBTreeNode.Left == this.tree.NIL)
					{
						break;
					}
					rBTreeNode = rBTreeNode.Left;
				}
				else
				{
					if (rBTreeNode.Right == this.tree.NIL)
					{
						break;
					}
					rBTreeNode = rBTreeNode.Right;
				}
			}
			if (this.tree.comparer.Compare(newValue, rBTreeNode.value) <= 0)
			{
				RBTreeNode<T> rBTreeNode2 = new RBTreeNode<T>(this.tree, newValue);
				rBTreeNode2.Color = RBTreeNode<T>.COLOR.RED;
				rBTreeNode.Left = rBTreeNode2;
				rBTreeNode2.Parent = rBTreeNode;
				rBTreeNode2.Left = (rBTreeNode2.Right = this.tree.NIL);
				rBTreeNode2.CheckBalance();
			}
			else
			{
				RBTreeNode<T> rBTreeNode3 = new RBTreeNode<T>(this.tree, newValue);
				rBTreeNode3.Color = RBTreeNode<T>.COLOR.RED;
				rBTreeNode.Right = rBTreeNode3;
				rBTreeNode3.Parent = rBTreeNode;
				rBTreeNode3.Left = (rBTreeNode3.Right = this.tree.NIL);
				rBTreeNode3.CheckBalance();
			}
			return true;
		}
		public void Delete(T i)
		{
			RBTreeNode<T> z = this.Search(this.tree.root, i);
			this.RBDelete(z);
		}
		private RBTreeNode<T> RBDelete(RBTreeNode<T> z)
		{
			RBTreeNode<T> rBTreeNode;
			if (z.Left == this.tree.NIL || z.Right == this.tree.NIL)
			{
				rBTreeNode = z;
			}
			else
			{
				rBTreeNode = this.TreeSuccessor(z);
			}
			RBTreeNode<T> rBTreeNode2;
			if (rBTreeNode.Left != this.tree.NIL)
			{
				rBTreeNode2 = rBTreeNode.Left;
			}
			else
			{
				rBTreeNode2 = rBTreeNode.Right;
			}
			if (this.tree.root == (rBTreeNode2.Parent = rBTreeNode.Parent))
			{
				this.tree.root.Left = rBTreeNode2;
			}
			else
			{
				if (rBTreeNode == rBTreeNode.Parent.Left)
				{
					rBTreeNode.Parent.Left = rBTreeNode2;
				}
				else
				{
					rBTreeNode.Parent.Right = rBTreeNode2;
				}
			}
			if (rBTreeNode != z)
			{
				z.Data = rBTreeNode.Data;
			}
			if (rBTreeNode.IsBlack())
			{
				this.RBDeleteFixUp(rBTreeNode2);
			}
			return rBTreeNode;
		}
		private void RBDeleteFixUp(RBTreeNode<T> x)
		{
			while (x != this.tree.root && x.IsBlack())
			{
				if (x == x.Parent.Left)
				{
					RBTreeNode<T> right = x.Parent.Right;
					if (right.IsRed())
					{
						right.Color = RBTreeNode<T>.COLOR.BLACK;
						x.Parent.Color = RBTreeNode<T>.COLOR.RED;
						this.RotateLeft(x.Parent);
						right = x.Parent.Right;
					}
					if (right.Left.IsBlack() && right.Right.IsBlack())
					{
						right.Color = RBTreeNode<T>.COLOR.RED;
						x = x.Parent;
					}
					else
					{
						if (right.Right.IsBlack())
						{
							right.Left.Color = RBTreeNode<T>.COLOR.BLACK;
							right.Color = RBTreeNode<T>.COLOR.RED;
							this.RotateRight(right);
							right = x.Parent.Right;
						}
						right.Color = x.Parent.Color;
						x.Parent.Color = RBTreeNode<T>.COLOR.BLACK;
						right.Right.Color = RBTreeNode<T>.COLOR.BLACK;
						this.RotateLeft(x.Parent);
						x = this.tree.root;
					}
				}
				else
				{
					RBTreeNode<T> left = x.Parent.Left;
					if (left.IsRed())
					{
						left.Color = RBTreeNode<T>.COLOR.BLACK;
						x.Parent.Color = RBTreeNode<T>.COLOR.RED;
						this.RotateRight(x.Parent);
						left = x.Parent.Left;
					}
					if (left.Right.IsBlack() && left.Left.IsBlack())
					{
						left.Color = RBTreeNode<T>.COLOR.RED;
						x = x.Parent;
					}
					else
					{
						if (left.Left.IsBlack())
						{
							left.Right.Color = RBTreeNode<T>.COLOR.BLACK;
							left.Color = RBTreeNode<T>.COLOR.RED;
							this.RotateRight(left);
							left = x.Parent.Left;
						}
						left.Color = x.Parent.Color;
						x.Parent.Color = RBTreeNode<T>.COLOR.BLACK;
						left.Left.Color = RBTreeNode<T>.COLOR.BLACK;
						this.RotateRight(x.Parent);
						x = this.tree.root;
					}
				}
			}
			x.Color = RBTreeNode<T>.COLOR.BLACK;
		}
		public void Print(int depth, bool Tab)
		{
			if (this != this.tree.NIL)
			{
				this.Right.Print(depth + 1, Tab);
				int num = 0;
				while (num++ < depth && Tab)
				{
					Console.Write("   ");
				}
				Console.WriteLine(this.value + "-" + Convert.ToString(this.Color));
				this.Left.Print(depth + 1, Tab);
			}
		}
		private static RBTreeNode<T> GetMaxRight(ref RBTreeNode<T> Cursor)
		{
			if (Cursor == null)
			{
				return null;
			}
			if (Cursor.Right != null)
			{
				Cursor = Cursor.Right;
				RBTreeNode<T>.GetMaxRight(ref Cursor);
			}
			return Cursor;
		}
		public static void MakeNull(ref RBTreeNode<T> root)
		{
			if (root != null)
			{
				if (root.tree.comparer.Compare(root.Data, default(T)) == 0)
				{
					root = null;
					return;
				}
				RBTreeNode<T>.MakeNull(ref root.Left);
				RBTreeNode<T>.MakeNull(ref root.Right);
			}
		}
	}
}
