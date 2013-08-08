using Jama.util;
using System;
using System.Collections;
using System.IO;
using System.Text;
namespace Jama
{
	[Serializable]
	public class JamaMatrix : ICloneable
	{
		private double[][] A;
		private int m;
		private int n;
		public virtual double[][] Array
		{
			get
			{
				return this.A;
			}
		}
		public virtual double[][] ArrayCopy
		{
			get
			{
				double[][] array = new double[this.m][];
				for (int i = 0; i < this.m; i++)
				{
					array[i] = new double[this.n];
				}
				for (int i = 0; i < this.m; i++)
				{
					for (int j = 0; j < this.n; j++)
					{
						array[i][j] = this.A[i][j];
					}
				}
				return array;
			}
		}
		public virtual double[] ColumnPackedCopy
		{
			get
			{
				double[] array = new double[this.m * this.n];
				for (int i = 0; i < this.m; i++)
				{
					for (int j = 0; j < this.n; j++)
					{
						array[i + j * this.m] = this.A[i][j];
					}
				}
				return array;
			}
		}
		public virtual double[] RowPackedCopy
		{
			get
			{
				double[] array = new double[this.m * this.n];
				for (int i = 0; i < this.m; i++)
				{
					for (int j = 0; j < this.n; j++)
					{
						array[i * this.n + j] = this.A[i][j];
					}
				}
				return array;
			}
		}
		public virtual int RowDimension
		{
			get
			{
				return this.m;
			}
		}
		public virtual int ColumnDimension
		{
			get
			{
				return this.n;
			}
		}
		public JamaMatrix(int m, int n)
		{
			this.m = m;
			this.n = n;
			this.A = new double[m][];
			for (int i = 0; i < m; i++)
			{
				this.A[i] = new double[n];
			}
		}
		public JamaMatrix(int m, int n, double s)
		{
			this.m = m;
			this.n = n;
			this.A = new double[m][];
			for (int i = 0; i < m; i++)
			{
				this.A[i] = new double[n];
			}
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					this.A[i][j] = s;
				}
			}
		}
		public JamaMatrix(double[][] A)
		{
			this.m = A.Length;
			this.n = A[0].Length;
			for (int i = 0; i < this.m; i++)
			{
				if (A[i].Length != this.n)
				{
					throw new ArgumentException("All rows must have the same length.");
				}
			}
			this.A = A;
		}
		public JamaMatrix(double[][] A, int m, int n)
		{
			this.A = A;
			this.m = m;
			this.n = n;
		}
		public JamaMatrix(double[] vals, int m)
		{
			this.m = m;
			this.n = ((m != 0) ? (vals.Length / m) : 0);
			if (m * this.n != vals.Length)
			{
				throw new ArgumentException("Array length must be a multiple of m.");
			}
			this.A = new double[m][];
			for (int i = 0; i < m; i++)
			{
				this.A[i] = new double[this.n];
			}
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					this.A[i][j] = vals[i + j * m];
				}
			}
		}
		public static JamaMatrix constructWithCopy(double[][] A)
		{
			int num = A.Length;
			int num2 = A[0].Length;
			JamaMatrix jamaMatrix = new JamaMatrix(num, num2);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < num; i++)
			{
				if (A[i].Length != num2)
				{
					throw new ArgumentException("All rows must have the same length.");
				}
				for (int j = 0; j < num2; j++)
				{
					array[i][j] = A[i][j];
				}
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix copy()
		{
			JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					array[i][j] = this.A[i][j];
				}
			}
			return jamaMatrix;
		}
		public virtual object Clone()
		{
			return this.copy();
		}
		public double GetElement(int i, int j)
		{
			return this.A[i][j];
		}
		public void SetElement(int i, int j, double v)
		{
			this.A[i][j] = v;
		}
		public virtual double get_Renamed(int i, int j)
		{
			return this.A[i][j];
		}
		public virtual JamaMatrix getMatrix(int i0, int i1, int j0, int j1)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(i1 - i0 + 1, j1 - j0 + 1);
			double[][] array = jamaMatrix.Array;
			try
			{
				for (int k = i0; k <= i1; k++)
				{
					for (int l = j0; l <= j1; l++)
					{
						array[k - i0][l - j0] = this.A[k][l];
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new IndexOutOfRangeException("Submatrix indices");
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix getMatrix(int[] r, int[] c)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(r.Length, c.Length);
			double[][] array = jamaMatrix.Array;
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int j = 0; j < c.Length; j++)
					{
						array[i][j] = this.A[r[i]][c[j]];
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new IndexOutOfRangeException("Submatrix indices");
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix getMatrix(int i0, int i1, int[] c)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(i1 - i0 + 1, c.Length);
			double[][] array = jamaMatrix.Array;
			try
			{
				for (int j = i0; j <= i1; j++)
				{
					for (int k = 0; k < c.Length; k++)
					{
						array[j - i0][k] = this.A[j][c[k]];
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new IndexOutOfRangeException("Submatrix indices");
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix getMatrix(int[] r, int j0, int j1)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(r.Length, j1 - j0 + 1);
			double[][] array = jamaMatrix.Array;
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int k = j0; k <= j1; k++)
					{
						array[i][k - j0] = this.A[r[i]][k];
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new IndexOutOfRangeException("Submatrix indices");
			}
			return jamaMatrix;
		}
		public virtual void set_Renamed(int i, int j, double s)
		{
			this.A[i][j] = s;
		}
		public virtual void setMatrix(int i0, int i1, int j0, int j1, JamaMatrix X)
		{
			try
			{
				for (int k = i0; k <= i1; k++)
				{
					for (int l = j0; l <= j1; l++)
					{
						this.A[k][l] = X.get_Renamed(k - i0, l - j0);
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new IndexOutOfRangeException("Submatrix indices");
			}
		}
		public virtual void setMatrix(int[] r, int[] c, JamaMatrix X)
		{
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int j = 0; j < c.Length; j++)
					{
						this.A[r[i]][c[j]] = X.get_Renamed(i, j);
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new IndexOutOfRangeException("Submatrix indices");
			}
		}
		public virtual void setMatrix(int[] r, int j0, int j1, JamaMatrix X)
		{
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int k = j0; k <= j1; k++)
					{
						this.A[r[i]][k] = X.get_Renamed(i, k - j0);
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new IndexOutOfRangeException("Submatrix indices");
			}
		}
		public virtual void setMatrix(int i0, int i1, int[] c, JamaMatrix X)
		{
			try
			{
				for (int j = i0; j <= i1; j++)
				{
					for (int k = 0; k < c.Length; k++)
					{
						this.A[j][c[k]] = X.get_Renamed(j - i0, k);
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new IndexOutOfRangeException("Submatrix indices");
			}
		}
		public virtual JamaMatrix transpose()
		{
			JamaMatrix jamaMatrix = new JamaMatrix(this.n, this.m);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					array[j][i] = this.A[i][j];
				}
			}
			return jamaMatrix;
		}
		public virtual double norm1()
		{
			double num = 0.0;
			for (int i = 0; i < this.n; i++)
			{
				double num2 = 0.0;
				for (int j = 0; j < this.m; j++)
				{
					num2 += Math.Abs(this.A[j][i]);
				}
				num = Math.Max(num, num2);
			}
			return num;
		}
		public virtual double norm2()
		{
			return new SingularValueDecomposition(this).norm2();
		}
		public virtual double normInf()
		{
			double num = 0.0;
			for (int i = 0; i < this.m; i++)
			{
				double num2 = 0.0;
				for (int j = 0; j < this.n; j++)
				{
					num2 += Math.Abs(this.A[i][j]);
				}
				num = Math.Max(num, num2);
			}
			return num;
		}
		public virtual double normF()
		{
			double num = 0.0;
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					num = Maths.hypot(num, this.A[i][j]);
				}
			}
			return num;
		}
		public virtual JamaMatrix uminus()
		{
			JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					array[i][j] = -this.A[i][j];
				}
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix plus(JamaMatrix B)
		{
			this.checkMatrixDimensions(B);
			JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					array[i][j] = this.A[i][j] + B.A[i][j];
				}
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix plusEquals(JamaMatrix B)
		{
			this.checkMatrixDimensions(B);
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					this.A[i][j] = this.A[i][j] + B.A[i][j];
				}
			}
			return this;
		}
		public virtual JamaMatrix minus(JamaMatrix B)
		{
			this.checkMatrixDimensions(B);
			JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					array[i][j] = this.A[i][j] - B.A[i][j];
				}
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix minusEquals(JamaMatrix B)
		{
			this.checkMatrixDimensions(B);
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					this.A[i][j] = this.A[i][j] - B.A[i][j];
				}
			}
			return this;
		}
		public virtual JamaMatrix arrayTimes(JamaMatrix B)
		{
			this.checkMatrixDimensions(B);
			JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					array[i][j] = this.A[i][j] * B.A[i][j];
				}
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix arrayTimesEquals(JamaMatrix B)
		{
			this.checkMatrixDimensions(B);
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					this.A[i][j] = this.A[i][j] * B.A[i][j];
				}
			}
			return this;
		}
		public virtual JamaMatrix arrayRightDivide(JamaMatrix B)
		{
			this.checkMatrixDimensions(B);
			JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					array[i][j] = this.A[i][j] / B.A[i][j];
				}
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix arrayRightDivideEquals(JamaMatrix B)
		{
			this.checkMatrixDimensions(B);
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					this.A[i][j] = this.A[i][j] / B.A[i][j];
				}
			}
			return this;
		}
		public virtual JamaMatrix arrayLeftDivide(JamaMatrix B)
		{
			this.checkMatrixDimensions(B);
			JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					array[i][j] = B.A[i][j] / this.A[i][j];
				}
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix arrayLeftDivideEquals(JamaMatrix B)
		{
			this.checkMatrixDimensions(B);
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					this.A[i][j] = B.A[i][j] / this.A[i][j];
				}
			}
			return this;
		}
		public virtual JamaMatrix times(double s)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(this.m, this.n);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					array[i][j] = s * this.A[i][j];
				}
			}
			return jamaMatrix;
		}
		public virtual JamaMatrix timesEquals(double s)
		{
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					this.A[i][j] = s * this.A[i][j];
				}
			}
			return this;
		}
		public virtual JamaMatrix times(JamaMatrix B)
		{
			if (B.m != this.n)
			{
				throw new ArgumentException("Matrix inner dimensions must agree.");
			}
			JamaMatrix jamaMatrix = new JamaMatrix(this.m, B.n);
			double[][] array = jamaMatrix.Array;
			double[] array2 = new double[this.n];
			for (int i = 0; i < B.n; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					array2[j] = B.A[j][i];
				}
				for (int k = 0; k < this.m; k++)
				{
					double[] array3 = this.A[k];
					double num = 0.0;
					for (int j = 0; j < this.n; j++)
					{
						num += array3[j] * array2[j];
					}
					array[k][i] = num;
				}
			}
			return jamaMatrix;
		}
		public virtual LUDecomposition lu()
		{
			return new LUDecomposition(this);
		}
		public virtual QRDecomposition qr()
		{
			return new QRDecomposition(this);
		}
		public virtual CholeskyDecomposition chol()
		{
			return new CholeskyDecomposition(this);
		}
		public virtual SingularValueDecomposition svd()
		{
			return new SingularValueDecomposition(this);
		}
		public virtual EigenvalueDecomposition eig()
		{
			return new EigenvalueDecomposition(this);
		}
		public virtual JamaMatrix solve(JamaMatrix B)
		{
			return (this.m == this.n) ? new LUDecomposition(this).solve(B) : new QRDecomposition(this).solve(B);
		}
		public virtual JamaMatrix solveTranspose(JamaMatrix B)
		{
			return this.transpose().solve(B.transpose());
		}
		public virtual JamaMatrix inverse()
		{
			return this.solve(JamaMatrix.identity(this.m, this.m));
		}
		public virtual double det()
		{
			return new LUDecomposition(this).det();
		}
		public virtual int rank()
		{
			return new SingularValueDecomposition(this).rank();
		}
		public virtual double cond()
		{
			return new SingularValueDecomposition(this).cond();
		}
		public virtual double trace()
		{
			double num = 0.0;
			for (int i = 0; i < Math.Min(this.m, this.n); i++)
			{
				num += this.A[i][i];
			}
			return num;
		}
		public static JamaMatrix random(int m, int n)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(m, n);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					array[i][j] = SupportClass.Random.NextDouble();
				}
			}
			return jamaMatrix;
		}
		public static JamaMatrix identity(int m, int n)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(m, n);
			double[][] array = jamaMatrix.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					array[i][j] = ((i == j) ? 1.0 : 0.0);
				}
			}
			return jamaMatrix;
		}
		public virtual void print(int w, int d)
		{
			this.print(new StreamWriter(Console.OpenStandardOutput(), Encoding.Default)
			{
				AutoFlush = true
			}, w, d);
		}
		public virtual void print(StreamWriter output, int w, int d)
		{
			throw new NotImplementedException();
		}
		public virtual void print(SupportClass.TextNumberFormat format, int width)
		{
			this.print(new StreamWriter(Console.OpenStandardOutput(), Encoding.Default)
			{
				AutoFlush = true
			}, format, width);
		}
		public virtual void print(StreamWriter output, SupportClass.TextNumberFormat format, int width)
		{
			output.WriteLine();
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					string text = format.FormatDouble(this.A[i][j]);
					int num = Math.Max(1, width - text.Length);
					for (int k = 0; k < num; k++)
					{
						output.Write(' ');
					}
					output.Write(text);
				}
				output.WriteLine();
			}
			output.WriteLine();
		}
		public static JamaMatrix read(StreamReader input)
		{
			SupportClass.StreamTokenizerSupport streamTokenizerSupport = new SupportClass.StreamTokenizerSupport(input);
			streamTokenizerSupport.ResetSyntax();
			streamTokenizerSupport.WordChars(0, 255);
			streamTokenizerSupport.WhitespaceChars(0, 32);
			streamTokenizerSupport.EOLIsSignificant(true);
			ArrayList arrayList = ArrayList.Synchronized(new ArrayList(10));
			while (streamTokenizerSupport.NextToken() == 10)
			{
			}
			if (streamTokenizerSupport.ttype == -1)
			{
				throw new IOException("Unexpected EOF on matrix read.");
			}
			do
			{
				arrayList.Add(double.Parse(streamTokenizerSupport.sval));
			}
			while (streamTokenizerSupport.NextToken() == -3);
			int count = arrayList.Count;
			double[] array = new double[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = (double)arrayList[i];
			}
			arrayList.Clear();
			arrayList.Add(array);
			IL_16F:
			while (streamTokenizerSupport.NextToken() == -3)
			{
				arrayList.Add(array = new double[count]);
				int i = 0;
				while (i < count)
				{
					array[i++] = double.Parse(streamTokenizerSupport.sval);
					if (streamTokenizerSupport.NextToken() != -3)
					{
						if (i < count)
						{
							throw new IOException("Row " + arrayList.Count + " is too short.");
						}
						goto IL_16F;
					}
				}
				throw new IOException("Row " + arrayList.Count + " is too long.");
			}
			int count2 = arrayList.Count;
			double[][] array2 = new double[count2][];
			arrayList.CopyTo(array2);
			return new JamaMatrix(array2);
		}
		private void checkMatrixDimensions(JamaMatrix B)
		{
			if (B.m != this.m || B.n != this.n)
			{
				throw new ArgumentException("Matrix dimensions must agree.");
			}
		}
		public override string ToString()
		{
			int[] array = new int[this.ColumnDimension];
			string text = "";
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					string text2 = this.A[i][j].ToString("g05");
					array[j] = Math.Max(array[j], text2.Length);
				}
			}
			for (int i = 0; i < this.m; i++)
			{
				for (int j = 0; j < this.n; j++)
				{
					string text2 = this.A[i][j].ToString("g05");
					text += text2.PadLeft(array[j] + 1);
				}
				text += "\n";
			}
			return text;
		}
	}
}
