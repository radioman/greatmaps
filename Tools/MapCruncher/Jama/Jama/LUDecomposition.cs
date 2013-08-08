using System;
namespace Jama
{
	[Serializable]
	public class LUDecomposition
	{
		private double[][] LU;
		private int m;
		private int n;
		private int pivsign;
		private int[] piv;
		public virtual bool Nonsingular
		{
			get
			{
				bool result;
				for (int i = 0; i < this.n; i++)
				{
					if (this.LU[i][i] == 0.0)
					{
						result = false;
						return result;
					}
				}
				result = true;
				return result;
			}
		}
		public virtual JamaMatrix L
		{
			get
			{
				JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
				double[][] array = jamaMatrix.Array;
				for (int i = 0; i < this.m; i++)
				{
					for (int j = 0; j < this.n; j++)
					{
						if (i > j)
						{
							array[i][j] = this.LU[i][j];
						}
						else
						{
							if (i == j)
							{
								array[i][j] = 1.0;
							}
							else
							{
								array[i][j] = 0.0;
							}
						}
					}
				}
				return jamaMatrix;
			}
		}
		public virtual JamaMatrix U
		{
			get
			{
				JamaMatrix jamaMatrix = new JamaMatrix(this.n, this.n);
				double[][] array = jamaMatrix.Array;
				for (int i = 0; i < this.n; i++)
				{
					for (int j = 0; j < this.n; j++)
					{
						if (i <= j)
						{
							array[i][j] = this.LU[i][j];
						}
						else
						{
							array[i][j] = 0.0;
						}
					}
				}
				return jamaMatrix;
			}
		}
		public virtual int[] Pivot
		{
			get
			{
				int[] array = new int[this.m];
				for (int i = 0; i < this.m; i++)
				{
					array[i] = this.piv[i];
				}
				return array;
			}
		}
		public virtual double[] DoublePivot
		{
			get
			{
				double[] array = new double[this.m];
				for (int i = 0; i < this.m; i++)
				{
					array[i] = (double)this.piv[i];
				}
				return array;
			}
		}
		public LUDecomposition(JamaMatrix A)
		{
			this.LU = A.ArrayCopy;
			this.m = A.RowDimension;
			this.n = A.ColumnDimension;
			this.piv = new int[this.m];
			for (int i = 0; i < this.m; i++)
			{
				this.piv[i] = i;
			}
			this.pivsign = 1;
			double[] array = new double[this.m];
			for (int j = 0; j < this.n; j++)
			{
				for (int i = 0; i < this.m; i++)
				{
					array[i] = this.LU[i][j];
				}
				for (int i = 0; i < this.m; i++)
				{
					double[] array2 = this.LU[i];
					int num = Math.Min(i, j);
					double num2 = 0.0;
					for (int k = 0; k < num; k++)
					{
						num2 += array2[k] * array[k];
					}
					array2[j] = (array[i] -= num2);
				}
				int num3 = j;
				for (int i = j + 1; i < this.m; i++)
				{
					if (Math.Abs(array[i]) > Math.Abs(array[num3]))
					{
						num3 = i;
					}
				}
				if (num3 != j)
				{
					for (int k = 0; k < this.n; k++)
					{
						double num4 = this.LU[num3][k];
						this.LU[num3][k] = this.LU[j][k];
						this.LU[j][k] = num4;
					}
					int num5 = this.piv[num3];
					this.piv[num3] = this.piv[j];
					this.piv[j] = num5;
					this.pivsign = -this.pivsign;
				}
				if (j < this.m & this.LU[j][j] != 0.0)
				{
					for (int i = j + 1; i < this.m; i++)
					{
						this.LU[i][j] /= this.LU[j][j];
					}
				}
			}
		}
		public virtual double det()
		{
			if (this.m != this.n)
			{
				throw new ArgumentException("Matrix must be square.");
			}
			double num = (double)this.pivsign;
			for (int i = 0; i < this.n; i++)
			{
				num *= this.LU[i][i];
			}
			return num;
		}
		public virtual JamaMatrix solve(JamaMatrix B)
		{
			if (B.RowDimension != this.m)
			{
				throw new ArgumentException("Matrix row dimensions must agree.");
			}
			if (!this.Nonsingular)
			{
				throw new CorrespondencesAreSingularException();
			}
			int columnDimension = B.ColumnDimension;
			JamaMatrix matrix = B.getMatrix(this.piv, 0, columnDimension - 1);
			double[][] array = matrix.Array;
			for (int i = 0; i < this.n; i++)
			{
				for (int j = i + 1; j < this.n; j++)
				{
					for (int k = 0; k < columnDimension; k++)
					{
						array[j][k] -= array[i][k] * this.LU[j][i];
					}
				}
			}
			for (int i = this.n - 1; i >= 0; i--)
			{
				for (int k = 0; k < columnDimension; k++)
				{
					array[i][k] /= this.LU[i][i];
				}
				for (int j = 0; j < i; j++)
				{
					for (int k = 0; k < columnDimension; k++)
					{
						array[j][k] -= array[i][k] * this.LU[j][i];
					}
				}
			}
			return matrix;
		}
	}
}
