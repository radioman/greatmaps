using Jama.util;
using System;
namespace Jama
{
	[Serializable]
	public class QRDecomposition
	{
		private double[][] QR;
		private int m;
		private int n;
		private double[] Rdiag;
		public virtual bool FullRank
		{
			get
			{
				bool result;
				for (int i = 0; i < this.n; i++)
				{
					if (this.Rdiag[i] == 0.0)
					{
						result = false;
						return result;
					}
				}
				result = true;
				return result;
			}
		}
		public virtual JamaMatrix H
		{
			get
			{
				JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
				double[][] array = jamaMatrix.Array;
				for (int i = 0; i < this.m; i++)
				{
					for (int j = 0; j < this.n; j++)
					{
						if (i >= j)
						{
							array[i][j] = this.QR[i][j];
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
		public virtual JamaMatrix R
		{
			get
			{
				JamaMatrix jamaMatrix = new JamaMatrix(this.n, this.n);
				double[][] array = jamaMatrix.Array;
				for (int i = 0; i < this.n; i++)
				{
					for (int j = 0; j < this.n; j++)
					{
						if (i < j)
						{
							array[i][j] = this.QR[i][j];
						}
						else
						{
							if (i == j)
							{
								array[i][j] = this.Rdiag[i];
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
		public virtual JamaMatrix Q
		{
			get
			{
				JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
				double[][] array = jamaMatrix.Array;
				for (int i = this.n - 1; i >= 0; i--)
				{
					for (int j = 0; j < this.m; j++)
					{
						array[j][i] = 0.0;
					}
					array[i][i] = 1.0;
					for (int k = i; k < this.n; k++)
					{
						if (this.QR[i][i] != 0.0)
						{
							double num = 0.0;
							for (int j = i; j < this.m; j++)
							{
								num += this.QR[j][i] * array[j][k];
							}
							num = -num / this.QR[i][i];
							for (int j = i; j < this.m; j++)
							{
								array[j][k] += num * this.QR[j][i];
							}
						}
					}
				}
				return jamaMatrix;
			}
		}
		public QRDecomposition(JamaMatrix A)
		{
			this.QR = A.ArrayCopy;
			this.m = A.RowDimension;
			this.n = A.ColumnDimension;
			this.Rdiag = new double[this.n];
			for (int i = 0; i < this.n; i++)
			{
				double num = 0.0;
				for (int j = i; j < this.m; j++)
				{
					num = Maths.hypot(num, this.QR[j][i]);
				}
				if (num != 0.0)
				{
					if (this.QR[i][i] < 0.0)
					{
						num = -num;
					}
					for (int j = i; j < this.m; j++)
					{
						this.QR[j][i] /= num;
					}
					this.QR[i][i] += 1.0;
					for (int k = i + 1; k < this.n; k++)
					{
						double num2 = 0.0;
						for (int j = i; j < this.m; j++)
						{
							num2 += this.QR[j][i] * this.QR[j][k];
						}
						num2 = -num2 / this.QR[i][i];
						for (int j = i; j < this.m; j++)
						{
							this.QR[j][k] += num2 * this.QR[j][i];
						}
					}
				}
				this.Rdiag[i] = -num;
			}
		}
		public virtual JamaMatrix solve(JamaMatrix B)
		{
			if (B.RowDimension != this.m)
			{
				throw new ArgumentException("Matrix row dimensions must agree.");
			}
			if (!this.FullRank)
			{
				throw new SystemException("Matrix is rank deficient.");
			}
			int columnDimension = B.ColumnDimension;
			double[][] arrayCopy = B.ArrayCopy;
			for (int i = 0; i < this.n; i++)
			{
				for (int j = 0; j < columnDimension; j++)
				{
					double num = 0.0;
					for (int k = i; k < this.m; k++)
					{
						num += this.QR[k][i] * arrayCopy[k][j];
					}
					num = -num / this.QR[i][i];
					for (int k = i; k < this.m; k++)
					{
						arrayCopy[k][j] += num * this.QR[k][i];
					}
				}
			}
			for (int i = this.n - 1; i >= 0; i--)
			{
				for (int j = 0; j < columnDimension; j++)
				{
					arrayCopy[i][j] /= this.Rdiag[i];
				}
				for (int k = 0; k < i; k++)
				{
					for (int j = 0; j < columnDimension; j++)
					{
						arrayCopy[k][j] -= arrayCopy[i][j] * this.QR[k][i];
					}
				}
			}
			return new JamaMatrix(arrayCopy, this.n, columnDimension).getMatrix(0, this.n - 1, 0, columnDimension - 1);
		}
	}
}
