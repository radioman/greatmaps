using System;
namespace Jama
{
	[Serializable]
	public class CholeskyDecomposition
	{
		private double[][] L;
		private int n;
		private bool isspd;
		public virtual bool SPD
		{
			get
			{
				return this.isspd;
			}
		}
		public CholeskyDecomposition(JamaMatrix Arg)
		{
			double[][] array = Arg.Array;
			this.n = Arg.RowDimension;
			this.L = new double[this.n][];
			for (int i = 0; i < this.n; i++)
			{
				this.L[i] = new double[this.n];
			}
			this.isspd = (Arg.ColumnDimension == this.n);
			for (int j = 0; j < this.n; j++)
			{
				double[] array2 = this.L[j];
				double num = 0.0;
				for (int k = 0; k < j; k++)
				{
					double[] array3 = this.L[k];
					double num2 = 0.0;
					for (int i = 0; i < k; i++)
					{
						num2 += array3[i] * array2[i];
					}
					num2 = (array2[k] = (array[j][k] - num2) / this.L[k][k]);
					num += num2 * num2;
					this.isspd &= (array[k][j] == array[j][k]);
				}
				num = array[j][j] - num;
				this.isspd &= (num > 0.0);
				this.L[j][j] = Math.Sqrt(Math.Max(num, 0.0));
				for (int k = j + 1; k < this.n; k++)
				{
					this.L[j][k] = 0.0;
				}
			}
		}
		public virtual JamaMatrix getL()
		{
			return new JamaMatrix(this.L, this.n, this.n);
		}
		public virtual JamaMatrix solve(JamaMatrix B)
		{
			if (B.RowDimension != this.n)
			{
				throw new ArgumentException("Matrix row dimensions must agree.");
			}
			if (!this.isspd)
			{
				throw new SystemException("Matrix is not symmetric positive definite.");
			}
			double[][] arrayCopy = B.ArrayCopy;
			int columnDimension = B.ColumnDimension;
			for (int i = 0; i < this.n; i++)
			{
				for (int j = 0; j < columnDimension; j++)
				{
					for (int k = 0; k < i; k++)
					{
						arrayCopy[i][j] -= arrayCopy[k][j] * this.L[i][k];
					}
					arrayCopy[i][j] /= this.L[i][i];
				}
			}
			for (int i = this.n - 1; i >= 0; i--)
			{
				for (int j = 0; j < columnDimension; j++)
				{
					for (int k = i + 1; k < this.n; k++)
					{
						arrayCopy[i][j] -= arrayCopy[k][j] * this.L[k][i];
					}
					arrayCopy[i][j] /= this.L[i][i];
				}
			}
			return new JamaMatrix(arrayCopy, this.n, columnDimension);
		}
	}
}
