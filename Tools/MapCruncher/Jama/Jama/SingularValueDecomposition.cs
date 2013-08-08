using Jama.util;
using System;
namespace Jama
{
	[Serializable]
	public class SingularValueDecomposition
	{
		private double[][] U;
		private double[][] V;
		private double[] s;
		private int m;
		private int n;
		public virtual double[] SingularValues
		{
			get
			{
				return this.s;
			}
		}
		public virtual JamaMatrix S
		{
			get
			{
				JamaMatrix jamaMatrix = new JamaMatrix(this.n, this.n);
				double[][] array = jamaMatrix.Array;
				for (int i = 0; i < this.n; i++)
				{
					for (int j = 0; j < this.n; j++)
					{
						array[i][j] = 0.0;
					}
					array[i][i] = this.s[i];
				}
				return jamaMatrix;
			}
		}
		public SingularValueDecomposition(JamaMatrix Arg)
		{
			double[][] arrayCopy = Arg.ArrayCopy;
			this.m = Arg.RowDimension;
			this.n = Arg.ColumnDimension;
			int num = Math.Min(this.m, this.n);
			this.s = new double[Math.Min(this.m + 1, this.n)];
			this.U = new double[this.m][];
			for (int i = 0; i < this.m; i++)
			{
				this.U[i] = new double[num];
			}
			this.V = new double[this.n][];
			for (int j = 0; j < this.n; j++)
			{
				this.V[j] = new double[this.n];
			}
			double[] array = new double[this.n];
			double[] array2 = new double[this.m];
			bool flag = true;
			bool flag2 = true;
			int num2 = Math.Min(this.m - 1, this.n);
			int num3 = Math.Max(0, Math.Min(this.n - 2, this.m));
			for (int k = 0; k < Math.Max(num2, num3); k++)
			{
				if (k < num2)
				{
					this.s[k] = 0.0;
					for (int i = k; i < this.m; i++)
					{
						this.s[k] = Maths.hypot(this.s[k], arrayCopy[i][k]);
					}
					if (this.s[k] != 0.0)
					{
						if (arrayCopy[k][k] < 0.0)
						{
							this.s[k] = -this.s[k];
						}
						for (int i = k; i < this.m; i++)
						{
							arrayCopy[i][k] /= this.s[k];
						}
						arrayCopy[k][k] += 1.0;
					}
					this.s[k] = -this.s[k];
				}
				for (int l = k + 1; l < this.n; l++)
				{
					if (k < num2 & this.s[k] != 0.0)
					{
						double num4 = 0.0;
						for (int i = k; i < this.m; i++)
						{
							num4 += arrayCopy[i][k] * arrayCopy[i][l];
						}
						num4 = -num4 / arrayCopy[k][k];
						for (int i = k; i < this.m; i++)
						{
							arrayCopy[i][l] += num4 * arrayCopy[i][k];
						}
					}
					array[l] = arrayCopy[k][l];
				}
				if (flag & k < num2)
				{
					for (int i = k; i < this.m; i++)
					{
						this.U[i][k] = arrayCopy[i][k];
					}
				}
				if (k < num3)
				{
					array[k] = 0.0;
					for (int i = k + 1; i < this.n; i++)
					{
						array[k] = Maths.hypot(array[k], array[i]);
					}
					if (array[k] != 0.0)
					{
						if (array[k + 1] < 0.0)
						{
							array[k] = -array[k];
						}
						for (int i = k + 1; i < this.n; i++)
						{
							array[i] /= array[k];
						}
						array[k + 1] += 1.0;
					}
					array[k] = -array[k];
					if (k + 1 < this.m & array[k] != 0.0)
					{
						for (int i = k + 1; i < this.m; i++)
						{
							array2[i] = 0.0;
						}
						for (int l = k + 1; l < this.n; l++)
						{
							for (int i = k + 1; i < this.m; i++)
							{
								array2[i] += array[l] * arrayCopy[i][l];
							}
						}
						for (int l = k + 1; l < this.n; l++)
						{
							double num4 = -array[l] / array[k + 1];
							for (int i = k + 1; i < this.m; i++)
							{
								arrayCopy[i][l] += num4 * array2[i];
							}
						}
					}
					if (flag2)
					{
						for (int i = k + 1; i < this.n; i++)
						{
							this.V[i][k] = array[i];
						}
					}
				}
			}
			int m = Math.Min(this.n, this.m + 1);
			if (num2 < this.n)
			{
				this.s[num2] = arrayCopy[num2][num2];
			}
			if (this.m < m)
			{
				this.s[m - 1] = 0.0;
			}
			if (num3 + 1 < m)
			{
				array[num3] = arrayCopy[num3][m - 1];
			}
			array[m - 1] = 0.0;
			if (flag)
			{
				for (int l = num2; l < num; l++)
				{
					for (int i = 0; i < this.m; i++)
					{
						this.U[i][l] = 0.0;
					}
					this.U[l][l] = 1.0;
				}
				for (int k = num2 - 1; k >= 0; k--)
				{
					if (this.s[k] != 0.0)
					{
						for (int l = k + 1; l < num; l++)
						{
							double num4 = 0.0;
							for (int i = k; i < this.m; i++)
							{
								num4 += this.U[i][k] * this.U[i][l];
							}
							num4 = -num4 / this.U[k][k];
							for (int i = k; i < this.m; i++)
							{
								this.U[i][l] += num4 * this.U[i][k];
							}
						}
						for (int i = k; i < this.m; i++)
						{
							this.U[i][k] = -this.U[i][k];
						}
						this.U[k][k] = 1.0 + this.U[k][k];
						for (int i = 0; i < k - 1; i++)
						{
							this.U[i][k] = 0.0;
						}
					}
					else
					{
						for (int i = 0; i < this.m; i++)
						{
							this.U[i][k] = 0.0;
						}
						this.U[k][k] = 1.0;
					}
				}
			}
			if (flag2)
			{
				for (int k = this.n - 1; k >= 0; k--)
				{
					if (k < num3 & array[k] != 0.0)
					{
						for (int l = k + 1; l < num; l++)
						{
							double num4 = 0.0;
							for (int i = k + 1; i < this.n; i++)
							{
								num4 += this.V[i][k] * this.V[i][l];
							}
							num4 = -num4 / this.V[k + 1][k];
							for (int i = k + 1; i < this.n; i++)
							{
								this.V[i][l] += num4 * this.V[i][k];
							}
						}
					}
					for (int i = 0; i < this.n; i++)
					{
						this.V[i][k] = 0.0;
					}
					this.V[k][k] = 1.0;
				}
			}
			int num5 = m - 1;
			int num6 = 0;
			double num7 = Math.Pow(2.0, -52.0);
			double num8 = Math.Pow(2.0, -966.0);
			while (m > 0)
			{
				int k;
				for (k = m - 2; k >= -1; k--)
				{
					if (k == -1)
					{
						break;
					}
					if (Math.Abs(array[k]) <= num8 + num7 * (Math.Abs(this.s[k]) + Math.Abs(this.s[k + 1])))
					{
						array[k] = 0.0;
						break;
					}
				}
				int num9;
				if (k == m - 2)
				{
					num9 = 4;
				}
				else
				{
					int n;
					for (n = m - 1; n >= k; n--)
					{
						if (n == k)
						{
							break;
						}
						double num4 = ((n != m) ? Math.Abs(array[n]) : 0.0) + ((n != k + 1) ? Math.Abs(array[n - 1]) : 0.0);
						if (Math.Abs(this.s[n]) <= num8 + num7 * num4)
						{
							this.s[n] = 0.0;
							break;
						}
					}
					if (n == k)
					{
						num9 = 3;
					}
					else
					{
						if (n == m - 1)
						{
							num9 = 1;
						}
						else
						{
							num9 = 2;
							k = n;
						}
					}
				}
				k++;
				switch (num9)
				{
				case 1:
				{
					double num10 = array[m - 2];
					array[m - 2] = 0.0;
					for (int l = m - 2; l >= k; l--)
					{
						double num4 = Maths.hypot(this.s[l], num10);
						double num11 = this.s[l] / num4;
						double num12 = num10 / num4;
						this.s[l] = num4;
						if (l != k)
						{
							num10 = -num12 * array[l - 1];
							array[l - 1] = num11 * array[l - 1];
						}
						if (flag2)
						{
							for (int i = 0; i < this.n; i++)
							{
								num4 = num11 * this.V[i][l] + num12 * this.V[i][m - 1];
								this.V[i][m - 1] = -num12 * this.V[i][l] + num11 * this.V[i][m - 1];
								this.V[i][l] = num4;
							}
						}
					}
					break;
				}
				case 2:
				{
					double num10 = array[k - 1];
					array[k - 1] = 0.0;
					for (int l = k; l < m; l++)
					{
						double num4 = Maths.hypot(this.s[l], num10);
						double num11 = this.s[l] / num4;
						double num12 = num10 / num4;
						this.s[l] = num4;
						num10 = -num12 * array[l];
						array[l] = num11 * array[l];
						if (flag)
						{
							for (int i = 0; i < this.m; i++)
							{
								num4 = num11 * this.U[i][l] + num12 * this.U[i][k - 1];
								this.U[i][k - 1] = -num12 * this.U[i][l] + num11 * this.U[i][k - 1];
								this.U[i][l] = num4;
							}
						}
					}
					break;
				}
				case 3:
				{
					double num13 = Math.Max(Math.Max(Math.Max(Math.Max(Math.Abs(this.s[m - 1]), Math.Abs(this.s[m - 2])), Math.Abs(array[m - 2])), Math.Abs(this.s[k])), Math.Abs(array[k]));
					double num14 = this.s[m - 1] / num13;
					double num15 = this.s[m - 2] / num13;
					double num16 = array[m - 2] / num13;
					double num17 = this.s[k] / num13;
					double num18 = array[k] / num13;
					double num19 = ((num15 + num14) * (num15 - num14) + num16 * num16) / 2.0;
					double num20 = num14 * num16 * (num14 * num16);
					double num21 = 0.0;
					if (num19 != 0.0 | num20 != 0.0)
					{
						num21 = Math.Sqrt(num19 * num19 + num20);
						if (num19 < 0.0)
						{
							num21 = -num21;
						}
						num21 = num20 / (num19 + num21);
					}
					double num10 = (num17 + num14) * (num17 - num14) + num21;
					double num22 = num17 * num18;
					for (int l = k; l < m - 1; l++)
					{
						double num4 = Maths.hypot(num10, num22);
						double num11 = num10 / num4;
						double num12 = num22 / num4;
						if (l != k)
						{
							array[l - 1] = num4;
						}
						num10 = num11 * this.s[l] + num12 * array[l];
						array[l] = num11 * array[l] - num12 * this.s[l];
						num22 = num12 * this.s[l + 1];
						this.s[l + 1] = num11 * this.s[l + 1];
						if (flag2)
						{
							for (int i = 0; i < this.n; i++)
							{
								num4 = num11 * this.V[i][l] + num12 * this.V[i][l + 1];
								this.V[i][l + 1] = -num12 * this.V[i][l] + num11 * this.V[i][l + 1];
								this.V[i][l] = num4;
							}
						}
						num4 = Maths.hypot(num10, num22);
						num11 = num10 / num4;
						num12 = num22 / num4;
						this.s[l] = num4;
						num10 = num11 * array[l] + num12 * this.s[l + 1];
						this.s[l + 1] = -num12 * array[l] + num11 * this.s[l + 1];
						num22 = num12 * array[l + 1];
						array[l + 1] = num11 * array[l + 1];
						if (flag && l < this.m - 1)
						{
							for (int i = 0; i < this.m; i++)
							{
								num4 = num11 * this.U[i][l] + num12 * this.U[i][l + 1];
								this.U[i][l + 1] = -num12 * this.U[i][l] + num11 * this.U[i][l + 1];
								this.U[i][l] = num4;
							}
						}
					}
					array[m - 2] = num10;
					num6++;
					break;
				}
				case 4:
					if (this.s[k] <= 0.0)
					{
						this.s[k] = ((this.s[k] < 0.0) ? (-this.s[k]) : 0.0);
						if (flag2)
						{
							for (int i = 0; i <= num5; i++)
							{
								this.V[i][k] = -this.V[i][k];
							}
						}
					}
					while (k < num5)
					{
						if (this.s[k] >= this.s[k + 1])
						{
							break;
						}
						double num4 = this.s[k];
						this.s[k] = this.s[k + 1];
						this.s[k + 1] = num4;
						if (flag2 && k < this.n - 1)
						{
							for (int i = 0; i < this.n; i++)
							{
								num4 = this.V[i][k + 1];
								this.V[i][k + 1] = this.V[i][k];
								this.V[i][k] = num4;
							}
						}
						if (flag && k < this.m - 1)
						{
							for (int i = 0; i < this.m; i++)
							{
								num4 = this.U[i][k + 1];
								this.U[i][k + 1] = this.U[i][k];
								this.U[i][k] = num4;
							}
						}
						k++;
					}
					num6 = 0;
					m--;
					break;
				}
			}
		}
		public virtual JamaMatrix getU()
		{
			return new JamaMatrix(this.U, this.m, Math.Min(this.m + 1, this.n));
		}
		public virtual JamaMatrix getV()
		{
			return new JamaMatrix(this.V, this.n, this.n);
		}
		public virtual double norm2()
		{
			return this.s[0];
		}
		public virtual double cond()
		{
			return this.s[0] / this.s[Math.Min(this.m, this.n) - 1];
		}
		public virtual int rank()
		{
			double num = Math.Pow(2.0, -52.0);
			double num2 = (double)Math.Max(this.m, this.n) * this.s[0] * num;
			int num3 = 0;
			for (int i = 0; i < this.s.Length; i++)
			{
				if (this.s[i] > num2)
				{
					num3++;
				}
			}
			return num3;
		}
	}
}
