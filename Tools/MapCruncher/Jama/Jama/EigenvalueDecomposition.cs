using Jama.util;
using System;
namespace Jama
{
	[Serializable]
	public class EigenvalueDecomposition
	{
		private int n;
		private bool issymmetric;
		private double[] d;
		private double[] e;
		private double[][] V;
		private double[][] H;
		private double[] ort;
		[NonSerialized]
		private double cdivr;
		[NonSerialized]
		private double cdivi;
		public virtual double[] RealEigenvalues
		{
			get
			{
				return this.d;
			}
		}
		public virtual double[] ImagEigenvalues
		{
			get
			{
				return this.e;
			}
		}
		public virtual JamaMatrix D
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
					array[i][i] = this.d[i];
					if (this.e[i] > 0.0)
					{
						array[i][i + 1] = this.e[i];
					}
					else
					{
						if (this.e[i] < 0.0)
						{
							array[i][i - 1] = this.e[i];
						}
					}
				}
				return jamaMatrix;
			}
		}
		private void tred2()
		{
			for (int i = 0; i < this.n; i++)
			{
				this.d[i] = this.V[this.n - 1][i];
			}
			for (int j = this.n - 1; j > 0; j--)
			{
				double num = 0.0;
				double num2 = 0.0;
				for (int k = 0; k < j; k++)
				{
					num += Math.Abs(this.d[k]);
				}
				if (num == 0.0)
				{
					this.e[j] = this.d[j - 1];
					for (int i = 0; i < j; i++)
					{
						this.d[i] = this.V[j - 1][i];
						this.V[j][i] = 0.0;
						this.V[i][j] = 0.0;
					}
				}
				else
				{
					for (int k = 0; k < j; k++)
					{
						this.d[k] /= num;
						num2 += this.d[k] * this.d[k];
					}
					double num3 = this.d[j - 1];
					double num4 = Math.Sqrt(num2);
					if (num3 > 0.0)
					{
						num4 = -num4;
					}
					this.e[j] = num * num4;
					num2 -= num3 * num4;
					this.d[j - 1] = num3 - num4;
					for (int i = 0; i < j; i++)
					{
						this.e[i] = 0.0;
					}
					for (int i = 0; i < j; i++)
					{
						num3 = this.d[i];
						this.V[i][j] = num3;
						num4 = this.e[i] + this.V[i][i] * num3;
						for (int k = i + 1; k <= j - 1; k++)
						{
							num4 += this.V[k][i] * this.d[k];
							this.e[k] += this.V[k][i] * num3;
						}
						this.e[i] = num4;
					}
					num3 = 0.0;
					for (int i = 0; i < j; i++)
					{
						this.e[i] /= num2;
						num3 += this.e[i] * this.d[i];
					}
					double num5 = num3 / (num2 + num2);
					for (int i = 0; i < j; i++)
					{
						this.e[i] -= num5 * this.d[i];
					}
					for (int i = 0; i < j; i++)
					{
						num3 = this.d[i];
						num4 = this.e[i];
						for (int k = i; k <= j - 1; k++)
						{
							this.V[k][i] -= num3 * this.e[k] + num4 * this.d[k];
						}
						this.d[i] = this.V[j - 1][i];
						this.V[j][i] = 0.0;
					}
				}
				this.d[j] = num2;
			}
			for (int j = 0; j < this.n - 1; j++)
			{
				this.V[this.n - 1][j] = this.V[j][j];
				this.V[j][j] = 1.0;
				double num2 = this.d[j + 1];
				if (num2 != 0.0)
				{
					for (int k = 0; k <= j; k++)
					{
						this.d[k] = this.V[k][j + 1] / num2;
					}
					for (int i = 0; i <= j; i++)
					{
						double num4 = 0.0;
						for (int k = 0; k <= j; k++)
						{
							num4 += this.V[k][j + 1] * this.V[k][i];
						}
						for (int k = 0; k <= j; k++)
						{
							this.V[k][i] -= num4 * this.d[k];
						}
					}
				}
				for (int k = 0; k <= j; k++)
				{
					this.V[k][j + 1] = 0.0;
				}
			}
			for (int i = 0; i < this.n; i++)
			{
				this.d[i] = this.V[this.n - 1][i];
				this.V[this.n - 1][i] = 0.0;
			}
			this.V[this.n - 1][this.n - 1] = 1.0;
			this.e[0] = 0.0;
		}
		private void tql2()
		{
			for (int i = 1; i < this.n; i++)
			{
				this.e[i - 1] = this.e[i];
			}
			this.e[this.n - 1] = 0.0;
			double num = 0.0;
			double num2 = 0.0;
			double num3 = Math.Pow(2.0, -52.0);
			for (int j = 0; j < this.n; j++)
			{
				num2 = Math.Max(num2, Math.Abs(this.d[j]) + Math.Abs(this.e[j]));
				int k;
				for (k = j; k < this.n; k++)
				{
					if (Math.Abs(this.e[k]) <= num3 * num2)
					{
						break;
					}
				}
				if (k > j)
				{
					int num4 = 0;
					do
					{
						num4++;
						double num5 = this.d[j];
						double num6 = (this.d[j + 1] - num5) / (2.0 * this.e[j]);
						double num7 = Maths.hypot(num6, 1.0);
						if (num6 < 0.0)
						{
							num7 = -num7;
						}
						this.d[j] = this.e[j] / (num6 + num7);
						this.d[j + 1] = this.e[j] * (num6 + num7);
						double num8 = this.d[j + 1];
						double num9 = num5 - this.d[j];
						for (int i = j + 2; i < this.n; i++)
						{
							this.d[i] -= num9;
						}
						num += num9;
						num6 = this.d[k];
						double num10 = 1.0;
						double num11 = num10;
						double num12 = num10;
						double num13 = this.e[j + 1];
						double num14 = 0.0;
						double num15 = 0.0;
						for (int i = k - 1; i >= j; i--)
						{
							num12 = num11;
							num11 = num10;
							num15 = num14;
							num5 = num10 * this.e[i];
							num9 = num10 * num6;
							num7 = Maths.hypot(num6, this.e[i]);
							this.e[i + 1] = num14 * num7;
							num14 = this.e[i] / num7;
							num10 = num6 / num7;
							num6 = num10 * this.d[i] - num14 * num5;
							this.d[i + 1] = num9 + num14 * (num10 * num5 + num14 * this.d[i]);
							for (int l = 0; l < this.n; l++)
							{
								num9 = this.V[l][i + 1];
								this.V[l][i + 1] = num14 * this.V[l][i] + num10 * num9;
								this.V[l][i] = num10 * this.V[l][i] - num14 * num9;
							}
						}
						num6 = -num14 * num15 * num12 * num13 * this.e[j] / num8;
						this.e[j] = num14 * num6;
						this.d[j] = num10 * num6;
					}
					while (Math.Abs(this.e[j]) > num3 * num2);
				}
				this.d[j] = this.d[j] + num;
				this.e[j] = 0.0;
			}
			for (int i = 0; i < this.n - 1; i++)
			{
				int l = i;
				double num6 = this.d[i];
				for (int m = i + 1; m < this.n; m++)
				{
					if (this.d[m] < num6)
					{
						l = m;
						num6 = this.d[m];
					}
				}
				if (l != i)
				{
					this.d[l] = this.d[i];
					this.d[i] = num6;
					for (int m = 0; m < this.n; m++)
					{
						num6 = this.V[m][i];
						this.V[m][i] = this.V[m][l];
						this.V[m][l] = num6;
					}
				}
			}
		}
		private void orthes()
		{
			int num = 0;
			int num2 = this.n - 1;
			for (int i = num + 1; i <= num2 - 1; i++)
			{
				double num3 = 0.0;
				for (int j = i; j <= num2; j++)
				{
					num3 += Math.Abs(this.H[j][i - 1]);
				}
				if (num3 != 0.0)
				{
					double num4 = 0.0;
					for (int j = num2; j >= i; j--)
					{
						this.ort[j] = this.H[j][i - 1] / num3;
						num4 += this.ort[j] * this.ort[j];
					}
					double num5 = Math.Sqrt(num4);
					if (this.ort[i] > 0.0)
					{
						num5 = -num5;
					}
					num4 -= this.ort[i] * num5;
					this.ort[i] = this.ort[i] - num5;
					for (int k = i; k < this.n; k++)
					{
						double num6 = 0.0;
						for (int j = num2; j >= i; j--)
						{
							num6 += this.ort[j] * this.H[j][k];
						}
						num6 /= num4;
						for (int j = i; j <= num2; j++)
						{
							this.H[j][k] -= num6 * this.ort[j];
						}
					}
					for (int j = 0; j <= num2; j++)
					{
						double num6 = 0.0;
						for (int k = num2; k >= i; k--)
						{
							num6 += this.ort[k] * this.H[j][k];
						}
						num6 /= num4;
						for (int k = i; k <= num2; k++)
						{
							this.H[j][k] -= num6 * this.ort[k];
						}
					}
					this.ort[i] = num3 * this.ort[i];
					this.H[i][i - 1] = num3 * num5;
				}
			}
			for (int j = 0; j < this.n; j++)
			{
				for (int k = 0; k < this.n; k++)
				{
					this.V[j][k] = ((j == k) ? 1.0 : 0.0);
				}
			}
			for (int i = num2 - 1; i >= num + 1; i--)
			{
				if (this.H[i][i - 1] != 0.0)
				{
					for (int j = i + 1; j <= num2; j++)
					{
						this.ort[j] = this.H[j][i - 1];
					}
					for (int k = i; k <= num2; k++)
					{
						double num5 = 0.0;
						for (int j = i; j <= num2; j++)
						{
							num5 += this.ort[j] * this.V[j][k];
						}
						num5 = num5 / this.ort[i] / this.H[i][i - 1];
						for (int j = i; j <= num2; j++)
						{
							this.V[j][k] += num5 * this.ort[j];
						}
					}
				}
			}
		}
		private void cdiv(double xr, double xi, double yr, double yi)
		{
			if (Math.Abs(yr) > Math.Abs(yi))
			{
				double num = yi / yr;
				double num2 = yr + num * yi;
				this.cdivr = (xr + num * xi) / num2;
				this.cdivi = (xi - num * xr) / num2;
			}
			else
			{
				double num = yr / yi;
				double num2 = yi + num * yr;
				this.cdivr = (num * xr + xi) / num2;
				this.cdivi = (num * xi - xr) / num2;
			}
		}
		private void hqr2()
		{
			int num = this.n;
			int i = num - 1;
			int num2 = 0;
			int num3 = num - 1;
			double num4 = Math.Pow(2.0, -52.0);
			double num5 = 0.0;
			double num6 = 0.0;
			double num7 = 0.0;
			double num8 = 0.0;
			double num9 = 0.0;
			double num10 = 0.0;
			double num11 = 0.0;
			for (int j = 0; j < num; j++)
			{
				if (j < num2 | j > num3)
				{
					this.d[j] = this.H[j][j];
					this.e[j] = 0.0;
				}
				for (int k = Math.Max(j - 1, 0); k < num; k++)
				{
					num11 += Math.Abs(this.H[j][k]);
				}
			}
			int num12 = 0;
			while (i >= num2)
			{
				int l;
				for (l = i; l > num2; l--)
				{
					num9 = Math.Abs(this.H[l - 1][l - 1]) + Math.Abs(this.H[l][l]);
					if (num9 == 0.0)
					{
						num9 = num11;
					}
					if (Math.Abs(this.H[l][l - 1]) < num4 * num9)
					{
						break;
					}
				}
				if (l == i)
				{
					this.H[i][i] = this.H[i][i] + num5;
					this.d[i] = this.H[i][i];
					this.e[i] = 0.0;
					i--;
					num12 = 0;
				}
				else
				{
					if (l == i - 1)
					{
						double num13 = this.H[i][i - 1] * this.H[i - 1][i];
						num6 = (this.H[i - 1][i - 1] - this.H[i][i]) / 2.0;
						num7 = num6 * num6 + num13;
						num10 = Math.Sqrt(Math.Abs(num7));
						this.H[i][i] = this.H[i][i] + num5;
						this.H[i - 1][i - 1] = this.H[i - 1][i - 1] + num5;
						double num14 = this.H[i][i];
						if (num7 >= 0.0)
						{
							if (num6 >= 0.0)
							{
								num10 = num6 + num10;
							}
							else
							{
								num10 = num6 - num10;
							}
							this.d[i - 1] = num14 + num10;
							this.d[i] = this.d[i - 1];
							if (num10 != 0.0)
							{
								this.d[i] = num14 - num13 / num10;
							}
							this.e[i - 1] = 0.0;
							this.e[i] = 0.0;
							num14 = this.H[i][i - 1];
							num9 = Math.Abs(num14) + Math.Abs(num10);
							num6 = num14 / num9;
							num7 = num10 / num9;
							num8 = Math.Sqrt(num6 * num6 + num7 * num7);
							num6 /= num8;
							num7 /= num8;
							for (int k = i - 1; k < num; k++)
							{
								num10 = this.H[i - 1][k];
								this.H[i - 1][k] = num7 * num10 + num6 * this.H[i][k];
								this.H[i][k] = num7 * this.H[i][k] - num6 * num10;
							}
							for (int j = 0; j <= i; j++)
							{
								num10 = this.H[j][i - 1];
								this.H[j][i - 1] = num7 * num10 + num6 * this.H[j][i];
								this.H[j][i] = num7 * this.H[j][i] - num6 * num10;
							}
							for (int j = num2; j <= num3; j++)
							{
								num10 = this.V[j][i - 1];
								this.V[j][i - 1] = num7 * num10 + num6 * this.V[j][i];
								this.V[j][i] = num7 * this.V[j][i] - num6 * num10;
							}
						}
						else
						{
							this.d[i - 1] = num14 + num6;
							this.d[i] = num14 + num6;
							this.e[i - 1] = num10;
							this.e[i] = -num10;
						}
						i -= 2;
						num12 = 0;
					}
					else
					{
						double num14 = this.H[i][i];
						double num15 = 0.0;
						double num13 = 0.0;
						if (l < i)
						{
							num15 = this.H[i - 1][i - 1];
							num13 = this.H[i][i - 1] * this.H[i - 1][i];
						}
						if (num12 == 10)
						{
							num5 += num14;
							for (int j = num2; j <= i; j++)
							{
								this.H[j][j] -= num14;
							}
							num9 = Math.Abs(this.H[i][i - 1]) + Math.Abs(this.H[i - 1][i - 2]);
							num15 = (num14 = 0.75 * num9);
							num13 = -0.4375 * num9 * num9;
						}
						if (num12 == 30)
						{
							num9 = (num15 - num14) / 2.0;
							num9 = num9 * num9 + num13;
							if (num9 > 0.0)
							{
								num9 = Math.Sqrt(num9);
								if (num15 < num14)
								{
									num9 = -num9;
								}
								num9 = num14 - num13 / ((num15 - num14) / 2.0 + num9);
								for (int j = num2; j <= i; j++)
								{
									this.H[j][j] -= num9;
								}
								num5 += num9;
								num15 = (num14 = (num13 = 0.964));
							}
						}
						num12++;
						int m;
						for (m = i - 2; m >= l; m--)
						{
							num10 = this.H[m][m];
							num8 = num14 - num10;
							num9 = num15 - num10;
							num6 = (num8 * num9 - num13) / this.H[m + 1][m] + this.H[m][m + 1];
							num7 = this.H[m + 1][m + 1] - num10 - num8 - num9;
							num8 = this.H[m + 2][m + 1];
							num9 = Math.Abs(num6) + Math.Abs(num7) + Math.Abs(num8);
							num6 /= num9;
							num7 /= num9;
							num8 /= num9;
							if (m == l)
							{
								break;
							}
							if (Math.Abs(this.H[m][m - 1]) * (Math.Abs(num7) + Math.Abs(num8)) < num4 * (Math.Abs(num6) * (Math.Abs(this.H[m - 1][m - 1]) + Math.Abs(num10) + Math.Abs(this.H[m + 1][m + 1]))))
							{
								break;
							}
						}
						for (int j = m + 2; j <= i; j++)
						{
							this.H[j][j - 2] = 0.0;
							if (j > m + 2)
							{
								this.H[j][j - 3] = 0.0;
							}
						}
						for (int n = m; n <= i - 1; n++)
						{
							bool flag = n != i - 1;
							if (n != m)
							{
								num6 = this.H[n][n - 1];
								num7 = this.H[n + 1][n - 1];
								num8 = (flag ? this.H[n + 2][n - 1] : 0.0);
								num14 = Math.Abs(num6) + Math.Abs(num7) + Math.Abs(num8);
								if (num14 != 0.0)
								{
									num6 /= num14;
									num7 /= num14;
									num8 /= num14;
								}
							}
							if (num14 == 0.0)
							{
								break;
							}
							num9 = Math.Sqrt(num6 * num6 + num7 * num7 + num8 * num8);
							if (num6 < 0.0)
							{
								num9 = -num9;
							}
							if (num9 != 0.0)
							{
								if (n != m)
								{
									this.H[n][n - 1] = -num9 * num14;
								}
								else
								{
									if (l != m)
									{
										this.H[n][n - 1] = -this.H[n][n - 1];
									}
								}
								num6 += num9;
								num14 = num6 / num9;
								num15 = num7 / num9;
								num10 = num8 / num9;
								num7 /= num6;
								num8 /= num6;
								for (int k = n; k < num; k++)
								{
									num6 = this.H[n][k] + num7 * this.H[n + 1][k];
									if (flag)
									{
										num6 += num8 * this.H[n + 2][k];
										this.H[n + 2][k] = this.H[n + 2][k] - num6 * num10;
									}
									this.H[n][k] = this.H[n][k] - num6 * num14;
									this.H[n + 1][k] = this.H[n + 1][k] - num6 * num15;
								}
								for (int j = 0; j <= Math.Min(i, n + 3); j++)
								{
									num6 = num14 * this.H[j][n] + num15 * this.H[j][n + 1];
									if (flag)
									{
										num6 += num10 * this.H[j][n + 2];
										this.H[j][n + 2] = this.H[j][n + 2] - num6 * num8;
									}
									this.H[j][n] = this.H[j][n] - num6;
									this.H[j][n + 1] = this.H[j][n + 1] - num6 * num7;
								}
								for (int j = num2; j <= num3; j++)
								{
									num6 = num14 * this.V[j][n] + num15 * this.V[j][n + 1];
									if (flag)
									{
										num6 += num10 * this.V[j][n + 2];
										this.V[j][n + 2] = this.V[j][n + 2] - num6 * num8;
									}
									this.V[j][n] = this.V[j][n] - num6;
									this.V[j][n + 1] = this.V[j][n + 1] - num6 * num7;
								}
							}
						}
					}
				}
			}
			if (num11 != 0.0)
			{
				for (i = num - 1; i >= 0; i--)
				{
					num6 = this.d[i];
					num7 = this.e[i];
					if (num7 == 0.0)
					{
						int l = i;
						this.H[i][i] = 1.0;
						for (int j = i - 1; j >= 0; j--)
						{
							double num13 = this.H[j][j] - num6;
							num8 = 0.0;
							for (int k = l; k <= i; k++)
							{
								num8 += this.H[j][k] * this.H[k][i];
							}
							if (this.e[j] < 0.0)
							{
								num10 = num13;
								num9 = num8;
							}
							else
							{
								l = j;
								double num16;
								if (this.e[j] == 0.0)
								{
									if (num13 != 0.0)
									{
										this.H[j][i] = -num8 / num13;
									}
									else
									{
										this.H[j][i] = -num8 / (num4 * num11);
									}
								}
								else
								{
									double num14 = this.H[j][j + 1];
									double num15 = this.H[j + 1][j];
									num7 = (this.d[j] - num6) * (this.d[j] - num6) + this.e[j] * this.e[j];
									num16 = (num14 * num9 - num10 * num8) / num7;
									this.H[j][i] = num16;
									if (Math.Abs(num14) > Math.Abs(num10))
									{
										this.H[j + 1][i] = (-num8 - num13 * num16) / num14;
									}
									else
									{
										this.H[j + 1][i] = (-num9 - num15 * num16) / num10;
									}
								}
								num16 = Math.Abs(this.H[j][i]);
								if (num4 * num16 * num16 > 1.0)
								{
									for (int k = j; k <= i; k++)
									{
										this.H[k][i] = this.H[k][i] / num16;
									}
								}
							}
						}
					}
					else
					{
						if (num7 < 0.0)
						{
							int l = i - 1;
							if (Math.Abs(this.H[i][i - 1]) > Math.Abs(this.H[i - 1][i]))
							{
								this.H[i - 1][i - 1] = num7 / this.H[i][i - 1];
								this.H[i - 1][i] = -(this.H[i][i] - num6) / this.H[i][i - 1];
							}
							else
							{
								this.cdiv(0.0, -this.H[i - 1][i], this.H[i - 1][i - 1] - num6, num7);
								this.H[i - 1][i - 1] = this.cdivr;
								this.H[i - 1][i] = this.cdivi;
							}
							this.H[i][i - 1] = 0.0;
							this.H[i][i] = 1.0;
							for (int j = i - 2; j >= 0; j--)
							{
								double num17 = 0.0;
								double num18 = 0.0;
								for (int k = l; k <= i; k++)
								{
									num17 += this.H[j][k] * this.H[k][i - 1];
									num18 += this.H[j][k] * this.H[k][i];
								}
								double num13 = this.H[j][j] - num6;
								if (this.e[j] < 0.0)
								{
									num10 = num13;
									num8 = num17;
									num9 = num18;
								}
								else
								{
									l = j;
									if (this.e[j] == 0.0)
									{
										this.cdiv(-num17, -num18, num13, num7);
										this.H[j][i - 1] = this.cdivr;
										this.H[j][i] = this.cdivi;
									}
									else
									{
										double num14 = this.H[j][j + 1];
										double num15 = this.H[j + 1][j];
										double num19 = (this.d[j] - num6) * (this.d[j] - num6) + this.e[j] * this.e[j] - num7 * num7;
										double num20 = (this.d[j] - num6) * 2.0 * num7;
										if (num19 == 0.0 & num20 == 0.0)
										{
											num19 = num4 * num11 * (Math.Abs(num13) + Math.Abs(num7) + Math.Abs(num14) + Math.Abs(num15) + Math.Abs(num10));
										}
										this.cdiv(num14 * num8 - num10 * num17 + num7 * num18, num14 * num9 - num10 * num18 - num7 * num17, num19, num20);
										this.H[j][i - 1] = this.cdivr;
										this.H[j][i] = this.cdivi;
										if (Math.Abs(num14) > Math.Abs(num10) + Math.Abs(num7))
										{
											this.H[j + 1][i - 1] = (-num17 - num13 * this.H[j][i - 1] + num7 * this.H[j][i]) / num14;
											this.H[j + 1][i] = (-num18 - num13 * this.H[j][i] - num7 * this.H[j][i - 1]) / num14;
										}
										else
										{
											this.cdiv(-num8 - num15 * this.H[j][i - 1], -num9 - num15 * this.H[j][i], num10, num7);
											this.H[j + 1][i - 1] = this.cdivr;
											this.H[j + 1][i] = this.cdivi;
										}
									}
									double num16 = Math.Max(Math.Abs(this.H[j][i - 1]), Math.Abs(this.H[j][i]));
									if (num4 * num16 * num16 > 1.0)
									{
										for (int k = j; k <= i; k++)
										{
											this.H[k][i - 1] = this.H[k][i - 1] / num16;
											this.H[k][i] = this.H[k][i] / num16;
										}
									}
								}
							}
						}
					}
				}
				for (int j = 0; j < num; j++)
				{
					if (j < num2 | j > num3)
					{
						for (int k = j; k < num; k++)
						{
							this.V[j][k] = this.H[j][k];
						}
					}
				}
				for (int k = num - 1; k >= num2; k--)
				{
					for (int j = num2; j <= num3; j++)
					{
						num10 = 0.0;
						for (int n = num2; n <= Math.Min(k, num3); n++)
						{
							num10 += this.V[j][n] * this.H[n][k];
						}
						this.V[j][k] = num10;
					}
				}
			}
		}
		public EigenvalueDecomposition(JamaMatrix Arg)
		{
			double[][] array = Arg.Array;
			this.n = Arg.ColumnDimension;
			this.V = new double[this.n][];
			for (int i = 0; i < this.n; i++)
			{
				this.V[i] = new double[this.n];
			}
			this.d = new double[this.n];
			this.e = new double[this.n];
			this.issymmetric = true;
			int j = 0;
			while (j < this.n & this.issymmetric)
			{
				int i = 0;
				while (i < this.n & this.issymmetric)
				{
					this.issymmetric = (array[i][j] == array[j][i]);
					i++;
				}
				j++;
			}
			if (this.issymmetric)
			{
				for (int i = 0; i < this.n; i++)
				{
					for (j = 0; j < this.n; j++)
					{
						this.V[i][j] = array[i][j];
					}
				}
				this.tred2();
				this.tql2();
			}
			else
			{
				double[][] array2 = new double[this.n][];
				for (int k = 0; k < this.n; k++)
				{
					array2[k] = new double[this.n];
				}
				this.H = array2;
				this.ort = new double[this.n];
				for (j = 0; j < this.n; j++)
				{
					for (int i = 0; i < this.n; i++)
					{
						this.H[i][j] = array[i][j];
					}
				}
				this.orthes();
				this.hqr2();
			}
		}
		public virtual JamaMatrix getV()
		{
			return new JamaMatrix(this.V, this.n, this.n);
		}
	}
}
