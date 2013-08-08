using System;
namespace Jama.examples
{
	public class MagicSquareExample
	{
		public static JamaMatrix magic(int n)
		{
			double[][] array = new double[n][];
			for (int i = 0; i < n; i++)
			{
				array[i] = new double[n];
			}
			if (n % 2 == 1)
			{
				int num = (n + 1) / 2;
				int num2 = n + 1;
				for (int j = 0; j < n; j++)
				{
					for (int i = 0; i < n; i++)
					{
						array[i][j] = (double)(n * ((i + j + num) % n) + (i + 2 * j + num2) % n + 1);
					}
				}
			}
			else
			{
				if (n % 4 == 0)
				{
					for (int j = 0; j < n; j++)
					{
						for (int i = 0; i < n; i++)
						{
							if ((i + 1) / 2 % 2 == (j + 1) / 2 % 2)
							{
								array[i][j] = (double)(n * n - n * i - j);
							}
							else
							{
								array[i][j] = (double)(n * i + j + 1);
							}
						}
					}
				}
				else
				{
					int num3 = n / 2;
					int num4 = (n - 2) / 4;
					JamaMatrix jamaMatrix = MagicSquareExample.magic(num3);
					for (int j = 0; j < num3; j++)
					{
						for (int i = 0; i < num3; i++)
						{
							double num5 = jamaMatrix.get_Renamed(i, j);
							array[i][j] = num5;
							array[i][j + num3] = num5 + (double)(2 * num3 * num3);
							array[i + num3][j] = num5 + (double)(3 * num3 * num3);
							array[i + num3][j + num3] = num5 + (double)(num3 * num3);
						}
					}
					for (int i = 0; i < num3; i++)
					{
						for (int j = 0; j < num4; j++)
						{
							double num6 = array[i][j];
							array[i][j] = array[i + num3][j];
							array[i + num3][j] = num6;
						}
						for (int j = n - num4 + 1; j < n; j++)
						{
							double num6 = array[i][j];
							array[i][j] = array[i + num3][j];
							array[i + num3][j] = num6;
						}
					}
					double num7 = array[num4][0];
					array[num4][0] = array[num4 + num3][0];
					array[num4 + num3][0] = num7;
					num7 = array[num4][num4];
					array[num4][num4] = array[num4 + num3][num4];
					array[num4 + num3][num4] = num7;
				}
			}
			return new JamaMatrix(array);
		}
		private static void print(string s)
		{
			Console.Out.Write(s);
		}
		public static string fixedWidthDoubletoString(double x, int w, int d)
		{
			throw new Exception("unimplemented");
		}
		public static string fixedWidthIntegertoString(int n, int w)
		{
			string text = Convert.ToString(n);
			while (text.Length < w)
			{
				text = " " + text;
			}
			return text;
		}
		[STAThread]
		public static void Main(string[] argv)
		{
			MagicSquareExample.print("\n    Test of Matrix Class, using magic squares.\n");
			MagicSquareExample.print("    See MagicSquareExample.main() for an explanation.\n");
			MagicSquareExample.print("\n      n     trace       max_eig   rank        cond      lu_res      qr_res\n\n");
			DateTime now = DateTime.Now;
			double num = Math.Pow(2.0, -52.0);
			for (int i = 3; i <= 32; i++)
			{
				MagicSquareExample.print(MagicSquareExample.fixedWidthIntegertoString(i, 7));
				JamaMatrix jamaMatrix = MagicSquareExample.magic(i);
				int n = (int)jamaMatrix.trace();
				MagicSquareExample.print(MagicSquareExample.fixedWidthIntegertoString(n, 10));
				EigenvalueDecomposition eigenvalueDecomposition = new EigenvalueDecomposition(jamaMatrix.plus(jamaMatrix.transpose()).times(0.5));
				double[] realEigenvalues = eigenvalueDecomposition.RealEigenvalues;
				MagicSquareExample.print(MagicSquareExample.fixedWidthDoubletoString(realEigenvalues[i - 1], 14, 3));
				int n2 = jamaMatrix.rank();
				MagicSquareExample.print(MagicSquareExample.fixedWidthIntegertoString(n2, 7));
				double num2 = jamaMatrix.cond();
				MagicSquareExample.print((num2 < 1.0 / num) ? MagicSquareExample.fixedWidthDoubletoString(num2, 12, 3) : "         Inf");
				LUDecomposition lUDecomposition = new LUDecomposition(jamaMatrix);
				JamaMatrix l = lUDecomposition.L;
				JamaMatrix u = lUDecomposition.U;
				int[] pivot = lUDecomposition.Pivot;
				JamaMatrix jamaMatrix2 = l.times(u).minus(jamaMatrix.getMatrix(pivot, 0, i - 1));
				double x = jamaMatrix2.norm1() / ((double)i * num);
				MagicSquareExample.print(MagicSquareExample.fixedWidthDoubletoString(x, 12, 3));
				QRDecomposition qRDecomposition = new QRDecomposition(jamaMatrix);
				JamaMatrix q = qRDecomposition.Q;
				jamaMatrix2 = qRDecomposition.R;
				jamaMatrix2 = q.times(jamaMatrix2).minus(jamaMatrix);
				x = jamaMatrix2.norm1() / ((double)i * num);
				MagicSquareExample.print(MagicSquareExample.fixedWidthDoubletoString(x, 12, 3));
				MagicSquareExample.print("\n");
			}
			double x2 = (double)(DateTime.Now.Ticks - now.Ticks) / 1000.0;
			MagicSquareExample.print("\nElapsed Time = " + MagicSquareExample.fixedWidthDoubletoString(x2, 12, 3) + " seconds\n");
			MagicSquareExample.print("Adios\n");
		}
	}
}
