using System;
namespace Jama.test
{
	public class TestMatrix
	{
		[STAThread]
		public static void Main(string[] argv)
		{
			int num = 0;
			int value = 0;
			double[] array = new double[]
			{
				1.0,
				2.0,
				3.0,
				4.0,
				5.0,
				6.0,
				7.0,
				8.0,
				9.0,
				10.0,
				11.0,
				12.0
			};
			double[] y = new double[]
			{
				1.0,
				4.0,
				7.0,
				10.0,
				2.0,
				5.0,
				8.0,
				11.0,
				3.0,
				6.0,
				9.0,
				12.0
			};
			double[][] array2 = new double[][]
			{
				new double[]
				{
					1.0,
					4.0,
					7.0,
					10.0
				},
				new double[]
				{
					2.0,
					5.0,
					8.0,
					11.0
				},
				new double[]
				{
					3.0,
					6.0,
					9.0,
					12.0
				}
			};
			double[][] a = array2;
			double[][] a2 = new double[][]
			{
				new double[]
				{
					1.0,
					2.0,
					3.0
				},
				new double[]
				{
					4.0,
					5.0,
					6.0
				},
				new double[]
				{
					7.0,
					8.0,
					9.0
				},
				new double[]
				{
					10.0,
					11.0,
					12.0
				}
			};
			double[][] a3 = new double[][]
			{
				new double[]
				{
					5.0,
					8.0,
					11.0
				},
				new double[]
				{
					6.0,
					9.0,
					12.0
				}
			};
			double[][] a4 = new double[][]
			{
				new double[]
				{
					1.0,
					4.0,
					7.0
				},
				new double[]
				{
					2.0,
					5.0,
					8.0,
					11.0
				},
				new double[]
				{
					3.0,
					6.0,
					9.0,
					12.0
				}
			};
			double[][] a5 = new double[][]
			{
				new double[]
				{
					4.0,
					1.0,
					1.0
				},
				new double[]
				{
					1.0,
					2.0,
					3.0
				},
				new double[]
				{
					1.0,
					3.0,
					6.0
				}
			};
			double[][] array3 = new double[3][];
			double[][] arg_1CC_0 = array3;
			int arg_1CC_1 = 0;
			double[] array4 = new double[4];
			array4[0] = 1.0;
			arg_1CC_0[arg_1CC_1] = array4;
			double[][] arg_1E7_0 = array3;
			int arg_1E7_1 = 1;
			array4 = new double[4];
			array4[1] = 1.0;
			arg_1E7_0[arg_1E7_1] = array4;
			double[][] arg_202_0 = array3;
			int arg_202_1 = 2;
			array4 = new double[4];
			array4[2] = 1.0;
			arg_202_0[arg_202_1] = array4;
			double[][] a6 = array3;
			array3 = new double[4][];
			double[][] arg_229_0 = array3;
			int arg_229_1 = 0;
			array4 = new double[4];
			array4[1] = 1.0;
			arg_229_0[arg_229_1] = array4;
			double[][] arg_251_0 = array3;
			int arg_251_1 = 1;
			array4 = new double[4];
			array4[0] = 1.0;
			array4[2] = 2E-07;
			arg_251_0[arg_251_1] = array4;
			array3[2] = new double[]
			{
				0.0,
				-2E-07,
				0.0,
				1.0
			};
			double[][] arg_294_0 = array3;
			int arg_294_1 = 3;
			array4 = new double[4];
			array4[2] = 1.0;
			arg_294_0[arg_294_1] = array4;
			double[][] a7 = array3;
			double[][] a8 = new double[][]
			{
				new double[]
				{
					166.0,
					188.0,
					210.0
				},
				new double[]
				{
					188.0,
					214.0,
					240.0
				},
				new double[]
				{
					210.0,
					240.0,
					270.0
				}
			};
			double[][] a9 = new double[][]
			{
				new double[]
				{
					13.0
				},
				new double[]
				{
					15.0
				}
			};
			double[][] a10 = new double[][]
			{
				new double[]
				{
					1.0,
					3.0
				},
				new double[]
				{
					7.0,
					9.0
				}
			};
			int num2 = 3;
			int num3 = 4;
			int m = 5;
			int i = 0;
			int j = 4;
			int m2 = 3;
			int m3 = 4;
			int num4 = 1;
			int num5 = 2;
			int num6 = 1;
			int num7 = 3;
			int[] r = new int[]
			{
				1,
				2
			};
			int[] r2 = new int[]
			{
				1,
				3
			};
			int[] c = new int[]
			{
				1,
				2,
				3
			};
			int[] c2 = new int[]
			{
				1,
				2,
				4
			};
			double y2 = 33.0;
			double y3 = 30.0;
			double y4 = 15.0;
			double d = 650.0;
			TestMatrix.print("\nTesting constructors and constructor-like methods...\n");
			JamaMatrix jamaMatrix;
			try
			{
				jamaMatrix = new JamaMatrix(array, m);
				num = TestMatrix.try_failure(num, "Catch invalid length in packed constructor... ", "exception not thrown for invalid input");
			}
			catch (ArgumentException ex)
			{
				TestMatrix.try_success("Catch invalid length in packed constructor... ", ex.Message);
			}
			double num8;
			try
			{
				jamaMatrix = new JamaMatrix(a4);
				num8 = jamaMatrix.get_Renamed(i, j);
			}
			catch (ArgumentException ex)
			{
				TestMatrix.try_success("Catch ragged input to default constructor... ", ex.Message);
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "Catch ragged input to constructor... ", "exception not thrown in construction...ArrayIndexOutOfBoundsException thrown later");
			}
			try
			{
				jamaMatrix = JamaMatrix.constructWithCopy(a4);
				num8 = jamaMatrix.get_Renamed(i, j);
			}
			catch (ArgumentException ex)
			{
				TestMatrix.try_success("Catch ragged input to constructWithCopy... ", ex.Message);
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "Catch ragged input to constructWithCopy... ", "exception not thrown in construction...ArrayIndexOutOfBoundsException thrown later");
			}
			jamaMatrix = new JamaMatrix(array, m2);
			JamaMatrix jamaMatrix2 = new JamaMatrix(array2);
			num8 = jamaMatrix2.get_Renamed(0, 0);
			array2[0][0] = 0.0;
			JamaMatrix jamaMatrix3 = jamaMatrix2.minus(jamaMatrix);
			array2[0][0] = num8;
			jamaMatrix2 = JamaMatrix.constructWithCopy(array2);
			num8 = jamaMatrix2.get_Renamed(0, 0);
			array2[0][0] = 0.0;
			if (num8 - jamaMatrix2.get_Renamed(0, 0) != 0.0)
			{
				num = TestMatrix.try_failure(num, "constructWithCopy... ", "copy not effected... data visible outside");
			}
			else
			{
				TestMatrix.try_success("constructWithCopy... ", "");
			}
			array2[0][0] = array[0];
			JamaMatrix x = new JamaMatrix(a6);
			try
			{
				TestMatrix.check(x, JamaMatrix.identity(3, 4));
				TestMatrix.try_success("identity... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "identity... ", "identity Matrix not successfully created");
			}
			TestMatrix.print("\nTesting access methods...\n");
			jamaMatrix2 = new JamaMatrix(array2);
			if (jamaMatrix2.RowDimension != num2)
			{
				num = TestMatrix.try_failure(num, "getRowDimension... ", "");
			}
			else
			{
				TestMatrix.try_success("getRowDimension... ", "");
			}
			if (jamaMatrix2.ColumnDimension != num3)
			{
				num = TestMatrix.try_failure(num, "getColumnDimension... ", "");
			}
			else
			{
				TestMatrix.try_success("getColumnDimension... ", "");
			}
			jamaMatrix2 = new JamaMatrix(array2);
			double[][] array5 = jamaMatrix2.Array;
			if (array5 != array2)
			{
				num = TestMatrix.try_failure(num, "getArray... ", "");
			}
			else
			{
				TestMatrix.try_success("getArray... ", "");
			}
			array5 = jamaMatrix2.ArrayCopy;
			if (array5 == array2)
			{
				num = TestMatrix.try_failure(num, "getArrayCopy... ", "data not (deep) copied");
			}
			try
			{
				TestMatrix.check(array5, array2);
				TestMatrix.try_success("getArrayCopy... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "getArrayCopy... ", "data not successfully (deep) copied");
			}
			double[] x2 = jamaMatrix2.ColumnPackedCopy;
			try
			{
				TestMatrix.check(x2, array);
				TestMatrix.try_success("getColumnPackedCopy... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "getColumnPackedCopy... ", "data not successfully (deep) copied by columns");
			}
			x2 = jamaMatrix2.RowPackedCopy;
			try
			{
				TestMatrix.check(x2, y);
				TestMatrix.try_success("getRowPackedCopy... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "getRowPackedCopy... ", "data not successfully (deep) copied by rows");
			}
			try
			{
				num8 = jamaMatrix2.get_Renamed(jamaMatrix2.RowDimension, jamaMatrix2.ColumnDimension - 1);
				num = TestMatrix.try_failure(num, "get(int,int)... ", "OutOfBoundsException expected but not thrown");
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					num8 = jamaMatrix2.get_Renamed(jamaMatrix2.RowDimension - 1, jamaMatrix2.ColumnDimension);
					num = TestMatrix.try_failure(num, "get(int,int)... ", "OutOfBoundsException expected but not thrown");
				}
				catch (IndexOutOfRangeException)
				{
					TestMatrix.try_success("get(int,int)... OutofBoundsException... ", "");
				}
			}
			catch (ArgumentException)
			{
				num = TestMatrix.try_failure(num, "get(int,int)... ", "OutOfBoundsException expected but not thrown");
			}
			try
			{
				if (jamaMatrix2.get_Renamed(jamaMatrix2.RowDimension - 1, jamaMatrix2.ColumnDimension - 1) != array2[jamaMatrix2.RowDimension - 1][jamaMatrix2.ColumnDimension - 1])
				{
					num = TestMatrix.try_failure(num, "get(int,int)... ", "Matrix entry (i,j) not successfully retreived");
				}
				else
				{
					TestMatrix.try_success("get(int,int)... ", "");
				}
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "get(int,int)... ", "Unexpected ArrayIndexOutOfBoundsException");
			}
			JamaMatrix jamaMatrix4 = new JamaMatrix(a3);
			JamaMatrix jamaMatrix5;
			try
			{
				jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5 + jamaMatrix2.RowDimension + 1, num6, num7);
				num = TestMatrix.try_failure(num, "getMatrix(int,int,int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5, num6, num7 + jamaMatrix2.ColumnDimension + 1);
					num = TestMatrix.try_failure(num, "getMatrix(int,int,int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (IndexOutOfRangeException)
				{
					TestMatrix.try_success("getMatrix(int,int,int,int)... ArrayIndexOutOfBoundsException... ", "");
				}
			}
			catch (ArgumentException)
			{
				num = TestMatrix.try_failure(num, "getMatrix(int,int,int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			try
			{
				jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5, num6, num7);
				try
				{
					TestMatrix.check(jamaMatrix4, jamaMatrix5);
					TestMatrix.try_success("getMatrix(int,int,int,int)... ", "");
				}
				catch (SystemException)
				{
					num = TestMatrix.try_failure(num, "getMatrix(int,int,int,int)... ", "submatrix not successfully retreived");
				}
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "getMatrix(int,int,int,int)... ", "Unexpected ArrayIndexOutOfBoundsException");
			}
			try
			{
				jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5, c2);
				num = TestMatrix.try_failure(num, "getMatrix(int,int,int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5 + jamaMatrix2.RowDimension + 1, c);
					num = TestMatrix.try_failure(num, "getMatrix(int,int,int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (IndexOutOfRangeException)
				{
					TestMatrix.try_success("getMatrix(int,int,int[])... ArrayIndexOutOfBoundsException... ", "");
				}
			}
			catch (ArgumentException)
			{
				num = TestMatrix.try_failure(num, "getMatrix(int,int,int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			try
			{
				jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5, c);
				try
				{
					TestMatrix.check(jamaMatrix4, jamaMatrix5);
					TestMatrix.try_success("getMatrix(int,int,int[])... ", "");
				}
				catch (SystemException)
				{
					num = TestMatrix.try_failure(num, "getMatrix(int,int,int[])... ", "submatrix not successfully retreived");
				}
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "getMatrix(int,int,int[])... ", "Unexpected ArrayIndexOutOfBoundsException");
			}
			try
			{
				jamaMatrix5 = jamaMatrix2.getMatrix(r2, num6, num7);
				num = TestMatrix.try_failure(num, "getMatrix(int[],int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					jamaMatrix5 = jamaMatrix2.getMatrix(r, num6, num7 + jamaMatrix2.ColumnDimension + 1);
					num = TestMatrix.try_failure(num, "getMatrix(int[],int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (IndexOutOfRangeException)
				{
					TestMatrix.try_success("getMatrix(int[],int,int)... ArrayIndexOutOfBoundsException... ", "");
				}
			}
			catch (ArgumentException)
			{
				num = TestMatrix.try_failure(num, "getMatrix(int[],int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			try
			{
				jamaMatrix5 = jamaMatrix2.getMatrix(r, num6, num7);
				try
				{
					TestMatrix.check(jamaMatrix4, jamaMatrix5);
					TestMatrix.try_success("getMatrix(int[],int,int)... ", "");
				}
				catch (SystemException)
				{
					num = TestMatrix.try_failure(num, "getMatrix(int[],int,int)... ", "submatrix not successfully retreived");
				}
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "getMatrix(int[],int,int)... ", "Unexpected ArrayIndexOutOfBoundsException");
			}
			try
			{
				jamaMatrix5 = jamaMatrix2.getMatrix(r2, c);
				num = TestMatrix.try_failure(num, "getMatrix(int[],int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					jamaMatrix5 = jamaMatrix2.getMatrix(r, c2);
					num = TestMatrix.try_failure(num, "getMatrix(int[],int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (IndexOutOfRangeException)
				{
					TestMatrix.try_success("getMatrix(int[],int[])... ArrayIndexOutOfBoundsException... ", "");
				}
			}
			catch (ArgumentException)
			{
				num = TestMatrix.try_failure(num, "getMatrix(int[],int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			try
			{
				jamaMatrix5 = jamaMatrix2.getMatrix(r, c);
				try
				{
					TestMatrix.check(jamaMatrix4, jamaMatrix5);
					TestMatrix.try_success("getMatrix(int[],int[])... ", "");
				}
				catch (SystemException)
				{
					num = TestMatrix.try_failure(num, "getMatrix(int[],int[])... ", "submatrix not successfully retreived");
				}
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "getMatrix(int[],int[])... ", "Unexpected ArrayIndexOutOfBoundsException");
			}
			try
			{
				jamaMatrix2.set_Renamed(jamaMatrix2.RowDimension, jamaMatrix2.ColumnDimension - 1, 0.0);
				num = TestMatrix.try_failure(num, "set(int,int,double)... ", "OutOfBoundsException expected but not thrown");
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					jamaMatrix2.set_Renamed(jamaMatrix2.RowDimension - 1, jamaMatrix2.ColumnDimension, 0.0);
					num = TestMatrix.try_failure(num, "set(int,int,double)... ", "OutOfBoundsException expected but not thrown");
				}
				catch (IndexOutOfRangeException)
				{
					TestMatrix.try_success("set(int,int,double)... OutofBoundsException... ", "");
				}
			}
			catch (ArgumentException)
			{
				num = TestMatrix.try_failure(num, "set(int,int,double)... ", "OutOfBoundsException expected but not thrown");
			}
			try
			{
				jamaMatrix2.set_Renamed(num4, num6, 0.0);
				num8 = jamaMatrix2.get_Renamed(num4, num6);
				try
				{
					TestMatrix.check(num8, 0.0);
					TestMatrix.try_success("set(int,int,double)... ", "");
				}
				catch (SystemException)
				{
					num = TestMatrix.try_failure(num, "set(int,int,double)... ", "Matrix element not successfully set");
				}
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "set(int,int,double)... ", "Unexpected ArrayIndexOutOfBoundsException");
			}
			jamaMatrix5 = new JamaMatrix(2, 3, 0.0);
			try
			{
				jamaMatrix2.setMatrix(num4, num5 + jamaMatrix2.RowDimension + 1, num6, num7, jamaMatrix5);
				num = TestMatrix.try_failure(num, "setMatrix(int,int,int,int,Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					jamaMatrix2.setMatrix(num4, num5, num6, num7 + jamaMatrix2.ColumnDimension + 1, jamaMatrix5);
					num = TestMatrix.try_failure(num, "setMatrix(int,int,int,int,Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (IndexOutOfRangeException)
				{
					TestMatrix.try_success("setMatrix(int,int,int,int,Matrix)... ArrayIndexOutOfBoundsException... ", "");
				}
			}
			catch (ArgumentException)
			{
				num = TestMatrix.try_failure(num, "setMatrix(int,int,int,int,Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			try
			{
				jamaMatrix2.setMatrix(num4, num5, num6, num7, jamaMatrix5);
				try
				{
					TestMatrix.check(jamaMatrix5.minus(jamaMatrix2.getMatrix(num4, num5, num6, num7)), jamaMatrix5);
					TestMatrix.try_success("setMatrix(int,int,int,int,Matrix)... ", "");
				}
				catch (SystemException)
				{
					num = TestMatrix.try_failure(num, "setMatrix(int,int,int,int,Matrix)... ", "submatrix not successfully set");
				}
				jamaMatrix2.setMatrix(num4, num5, num6, num7, jamaMatrix4);
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "setMatrix(int,int,int,int,Matrix)... ", "Unexpected ArrayIndexOutOfBoundsException");
			}
			try
			{
				jamaMatrix2.setMatrix(num4, num5 + jamaMatrix2.RowDimension + 1, c, jamaMatrix5);
				num = TestMatrix.try_failure(num, "setMatrix(int,int,int[],Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					jamaMatrix2.setMatrix(num4, num5, c2, jamaMatrix5);
					num = TestMatrix.try_failure(num, "setMatrix(int,int,int[],Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (IndexOutOfRangeException)
				{
					TestMatrix.try_success("setMatrix(int,int,int[],Matrix)... ArrayIndexOutOfBoundsException... ", "");
				}
			}
			catch (ArgumentException)
			{
				num = TestMatrix.try_failure(num, "setMatrix(int,int,int[],Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			try
			{
				jamaMatrix2.setMatrix(num4, num5, c, jamaMatrix5);
				try
				{
					TestMatrix.check(jamaMatrix5.minus(jamaMatrix2.getMatrix(num4, num5, c)), jamaMatrix5);
					TestMatrix.try_success("setMatrix(int,int,int[],Matrix)... ", "");
				}
				catch (SystemException)
				{
					num = TestMatrix.try_failure(num, "setMatrix(int,int,int[],Matrix)... ", "submatrix not successfully set");
				}
				jamaMatrix2.setMatrix(num4, num5, num6, num7, jamaMatrix4);
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "setMatrix(int,int,int[],Matrix)... ", "Unexpected ArrayIndexOutOfBoundsException");
			}
			try
			{
				jamaMatrix2.setMatrix(r, num6, num7 + jamaMatrix2.ColumnDimension + 1, jamaMatrix5);
				num = TestMatrix.try_failure(num, "setMatrix(int[],int,int,Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					jamaMatrix2.setMatrix(r2, num6, num7, jamaMatrix5);
					num = TestMatrix.try_failure(num, "setMatrix(int[],int,int,Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (IndexOutOfRangeException)
				{
					TestMatrix.try_success("setMatrix(int[],int,int,Matrix)... ArrayIndexOutOfBoundsException... ", "");
				}
			}
			catch (ArgumentException)
			{
				num = TestMatrix.try_failure(num, "setMatrix(int[],int,int,Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			try
			{
				jamaMatrix2.setMatrix(r, num6, num7, jamaMatrix5);
				try
				{
					TestMatrix.check(jamaMatrix5.minus(jamaMatrix2.getMatrix(r, num6, num7)), jamaMatrix5);
					TestMatrix.try_success("setMatrix(int[],int,int,Matrix)... ", "");
				}
				catch (SystemException)
				{
					num = TestMatrix.try_failure(num, "setMatrix(int[],int,int,Matrix)... ", "submatrix not successfully set");
				}
				jamaMatrix2.setMatrix(num4, num5, num6, num7, jamaMatrix4);
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "setMatrix(int[],int,int,Matrix)... ", "Unexpected ArrayIndexOutOfBoundsException");
			}
			try
			{
				jamaMatrix2.setMatrix(r, c2, jamaMatrix5);
				num = TestMatrix.try_failure(num, "setMatrix(int[],int[],Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					jamaMatrix2.setMatrix(r2, c, jamaMatrix5);
					num = TestMatrix.try_failure(num, "setMatrix(int[],int[],Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (IndexOutOfRangeException)
				{
					TestMatrix.try_success("setMatrix(int[],int[],Matrix)... ArrayIndexOutOfBoundsException... ", "");
				}
			}
			catch (ArgumentException)
			{
				num = TestMatrix.try_failure(num, "setMatrix(int[],int[],Matrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			try
			{
				jamaMatrix2.setMatrix(r, c, jamaMatrix5);
				try
				{
					TestMatrix.check(jamaMatrix5.minus(jamaMatrix2.getMatrix(r, c)), jamaMatrix5);
					TestMatrix.try_success("setMatrix(int[],int[],Matrix)... ", "");
				}
				catch (SystemException)
				{
					num = TestMatrix.try_failure(num, "setMatrix(int[],int[],Matrix)... ", "submatrix not successfully set");
				}
			}
			catch (IndexOutOfRangeException)
			{
				num = TestMatrix.try_failure(num, "setMatrix(int[],int[],Matrix)... ", "Unexpected ArrayIndexOutOfBoundsException");
			}
			TestMatrix.print("\nTesting array-like methods...\n");
			JamaMatrix b = new JamaMatrix(array, m3);
			JamaMatrix jamaMatrix6 = JamaMatrix.random(jamaMatrix.RowDimension, jamaMatrix.ColumnDimension);
			jamaMatrix = jamaMatrix6;
			try
			{
				b = jamaMatrix.minus(b);
				num = TestMatrix.try_failure(num, "minus conformance check... ", "nonconformance not raised");
			}
			catch (ArgumentException)
			{
				TestMatrix.try_success("minus conformance check... ", "");
			}
			if (jamaMatrix.minus(jamaMatrix6).norm1() != 0.0)
			{
				num = TestMatrix.try_failure(num, "minus... ", "(difference of identical Matrices is nonzero,\nSubsequent use of minus should be suspect)");
			}
			else
			{
				TestMatrix.try_success("minus... ", "");
			}
			jamaMatrix = jamaMatrix6.copy();
			jamaMatrix.minusEquals(jamaMatrix6);
			JamaMatrix jamaMatrix7 = new JamaMatrix(jamaMatrix.RowDimension, jamaMatrix.ColumnDimension);
			try
			{
				jamaMatrix.minusEquals(b);
				num = TestMatrix.try_failure(num, "minusEquals conformance check... ", "nonconformance not raised");
			}
			catch (ArgumentException)
			{
				TestMatrix.try_success("minusEquals conformance check... ", "");
			}
			if (jamaMatrix.minus(jamaMatrix7).norm1() != 0.0)
			{
				num = TestMatrix.try_failure(num, "minusEquals... ", "(difference of identical Matrices is nonzero,\nSubsequent use of minus should be suspect)");
			}
			else
			{
				TestMatrix.try_success("minusEquals... ", "");
			}
			jamaMatrix = jamaMatrix6.copy();
			jamaMatrix2 = JamaMatrix.random(jamaMatrix.RowDimension, jamaMatrix.ColumnDimension);
			jamaMatrix3 = jamaMatrix.minus(jamaMatrix2);
			try
			{
				b = jamaMatrix.plus(b);
				num = TestMatrix.try_failure(num, "plus conformance check... ", "nonconformance not raised");
			}
			catch (ArgumentException)
			{
				TestMatrix.try_success("plus conformance check... ", "");
			}
			try
			{
				TestMatrix.check(jamaMatrix3.plus(jamaMatrix2), jamaMatrix);
				TestMatrix.try_success("plus... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "plus... ", "(C = A - B, but C + B != A)");
			}
			jamaMatrix3 = jamaMatrix.minus(jamaMatrix2);
			jamaMatrix3.plusEquals(jamaMatrix2);
			try
			{
				jamaMatrix.plusEquals(b);
				num = TestMatrix.try_failure(num, "plusEquals conformance check... ", "nonconformance not raised");
			}
			catch (ArgumentException)
			{
				TestMatrix.try_success("plusEquals conformance check... ", "");
			}
			try
			{
				TestMatrix.check(jamaMatrix3, jamaMatrix);
				TestMatrix.try_success("plusEquals... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "plusEquals... ", "(C = A - B, but C = C + B != A)");
			}
			jamaMatrix = jamaMatrix6.uminus();
			try
			{
				TestMatrix.check(jamaMatrix.plus(jamaMatrix6), jamaMatrix7);
				TestMatrix.try_success("uminus... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "uminus... ", "(-A + A != zeros)");
			}
			jamaMatrix = jamaMatrix6.copy();
			JamaMatrix y5 = new JamaMatrix(jamaMatrix.RowDimension, jamaMatrix.ColumnDimension, 1.0);
			jamaMatrix3 = jamaMatrix.arrayLeftDivide(jamaMatrix6);
			try
			{
				b = jamaMatrix.arrayLeftDivide(b);
				num = TestMatrix.try_failure(num, "arrayLeftDivide conformance check... ", "nonconformance not raised");
			}
			catch (ArgumentException)
			{
				TestMatrix.try_success("arrayLeftDivide conformance check... ", "");
			}
			try
			{
				TestMatrix.check(jamaMatrix3, y5);
				TestMatrix.try_success("arrayLeftDivide... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "arrayLeftDivide... ", "(M.\\M != ones)");
			}
			try
			{
				jamaMatrix.arrayLeftDivideEquals(b);
				num = TestMatrix.try_failure(num, "arrayLeftDivideEquals conformance check... ", "nonconformance not raised");
			}
			catch (ArgumentException)
			{
				TestMatrix.try_success("arrayLeftDivideEquals conformance check... ", "");
			}
			jamaMatrix.arrayLeftDivideEquals(jamaMatrix6);
			try
			{
				TestMatrix.check(jamaMatrix, y5);
				TestMatrix.try_success("arrayLeftDivideEquals... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "arrayLeftDivideEquals... ", "(M.\\M != ones)");
			}
			jamaMatrix = jamaMatrix6.copy();
			try
			{
				jamaMatrix.arrayRightDivide(b);
				num = TestMatrix.try_failure(num, "arrayRightDivide conformance check... ", "nonconformance not raised");
			}
			catch (ArgumentException)
			{
				TestMatrix.try_success("arrayRightDivide conformance check... ", "");
			}
			jamaMatrix3 = jamaMatrix.arrayRightDivide(jamaMatrix6);
			try
			{
				TestMatrix.check(jamaMatrix3, y5);
				TestMatrix.try_success("arrayRightDivide... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "arrayRightDivide... ", "(M./M != ones)");
			}
			try
			{
				jamaMatrix.arrayRightDivideEquals(b);
				num = TestMatrix.try_failure(num, "arrayRightDivideEquals conformance check... ", "nonconformance not raised");
			}
			catch (ArgumentException)
			{
				TestMatrix.try_success("arrayRightDivideEquals conformance check... ", "");
			}
			jamaMatrix.arrayRightDivideEquals(jamaMatrix6);
			try
			{
				TestMatrix.check(jamaMatrix, y5);
				TestMatrix.try_success("arrayRightDivideEquals... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "arrayRightDivideEquals... ", "(M./M != ones)");
			}
			jamaMatrix = jamaMatrix6.copy();
			jamaMatrix2 = JamaMatrix.random(jamaMatrix.RowDimension, jamaMatrix.ColumnDimension);
			try
			{
				b = jamaMatrix.arrayTimes(b);
				num = TestMatrix.try_failure(num, "arrayTimes conformance check... ", "nonconformance not raised");
			}
			catch (ArgumentException)
			{
				TestMatrix.try_success("arrayTimes conformance check... ", "");
			}
			jamaMatrix3 = jamaMatrix.arrayTimes(jamaMatrix2);
			try
			{
				TestMatrix.check(jamaMatrix3.arrayRightDivideEquals(jamaMatrix2), jamaMatrix);
				TestMatrix.try_success("arrayTimes... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "arrayTimes... ", "(A = R, C = A.*B, but C./B != A)");
			}
			try
			{
				jamaMatrix.arrayTimesEquals(b);
				num = TestMatrix.try_failure(num, "arrayTimesEquals conformance check... ", "nonconformance not raised");
			}
			catch (ArgumentException)
			{
				TestMatrix.try_success("arrayTimesEquals conformance check... ", "");
			}
			jamaMatrix.arrayTimesEquals(jamaMatrix2);
			try
			{
				TestMatrix.check(jamaMatrix.arrayRightDivideEquals(jamaMatrix2), jamaMatrix6);
				TestMatrix.try_success("arrayTimesEquals... ", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "arrayTimesEquals... ", "(A = R, A = A.*B, but A./B != R)");
			}
			TestMatrix.print("\nTesting I/O methods...\n");
			TestMatrix.print("\nTesting linear algebra methods...\n");
			jamaMatrix = new JamaMatrix(array, 3);
			JamaMatrix y6 = new JamaMatrix(a2);
			y6 = jamaMatrix.transpose();
			try
			{
				TestMatrix.check(jamaMatrix.transpose(), y6);
				TestMatrix.try_success("transpose...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "transpose()...", "transpose unsuccessful");
			}
			jamaMatrix.transpose();
			try
			{
				TestMatrix.check(jamaMatrix.norm1(), y2);
				TestMatrix.try_success("norm1...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "norm1()...", "incorrect norm calculation");
			}
			try
			{
				TestMatrix.check(jamaMatrix.normInf(), y3);
				TestMatrix.try_success("normInf()...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "normInf()...", "incorrect norm calculation");
			}
			try
			{
				TestMatrix.check(jamaMatrix.normF(), Math.Sqrt(d));
				TestMatrix.try_success("normF...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "normF()...", "incorrect norm calculation");
			}
			try
			{
				TestMatrix.check(jamaMatrix.trace(), y4);
				TestMatrix.try_success("trace()...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "trace()...", "incorrect trace calculation");
			}
			try
			{
				TestMatrix.check(jamaMatrix.getMatrix(0, jamaMatrix.RowDimension - 1, 0, jamaMatrix.RowDimension - 1).det(), 0.0);
				TestMatrix.try_success("det()...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "det()...", "incorrect determinant calculation");
			}
			JamaMatrix jamaMatrix8 = new JamaMatrix(a8);
			try
			{
				TestMatrix.check(jamaMatrix.times(jamaMatrix.transpose()), jamaMatrix8);
				TestMatrix.try_success("times(Matrix)...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "times(Matrix)...", "incorrect Matrix-Matrix product calculation");
			}
			try
			{
				TestMatrix.check(jamaMatrix.times(0.0), jamaMatrix7);
				TestMatrix.try_success("times(double)...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "times(double)...", "incorrect Matrix-scalar product calculation");
			}
			jamaMatrix = new JamaMatrix(array, 4);
			QRDecomposition qRDecomposition = jamaMatrix.qr();
			jamaMatrix6 = qRDecomposition.R;
			try
			{
				TestMatrix.check(jamaMatrix, qRDecomposition.Q.times(jamaMatrix6));
				TestMatrix.try_success("QRDecomposition...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "QRDecomposition...", "incorrect QR decomposition calculation");
			}
			SingularValueDecomposition singularValueDecomposition = jamaMatrix.svd();
			try
			{
				TestMatrix.check(jamaMatrix, singularValueDecomposition.getU().times(singularValueDecomposition.S.times(singularValueDecomposition.getV().transpose())));
				TestMatrix.try_success("SingularValueDecomposition...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "SingularValueDecomposition...", "incorrect singular value decomposition calculation");
			}
			JamaMatrix jamaMatrix9 = new JamaMatrix(a);
			try
			{
				TestMatrix.check((double)jamaMatrix9.rank(), (double)(Math.Min(jamaMatrix9.RowDimension, jamaMatrix9.ColumnDimension) - 1));
				TestMatrix.try_success("rank()...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "rank()...", "incorrect rank calculation");
			}
			jamaMatrix2 = new JamaMatrix(a10);
			singularValueDecomposition = jamaMatrix2.svd();
			double[] singularValues = singularValueDecomposition.SingularValues;
			try
			{
				TestMatrix.check(jamaMatrix2.cond(), singularValues[0] / singularValues[Math.Min(jamaMatrix2.RowDimension, jamaMatrix2.ColumnDimension) - 1]);
				TestMatrix.try_success("cond()...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "cond()...", "incorrect condition number calculation");
			}
			int columnDimension = jamaMatrix.ColumnDimension;
			jamaMatrix = jamaMatrix.getMatrix(0, columnDimension - 1, 0, columnDimension - 1);
			jamaMatrix.set_Renamed(0, 0, 0.0);
			LUDecomposition lUDecomposition = jamaMatrix.lu();
			try
			{
				TestMatrix.check(jamaMatrix.getMatrix(lUDecomposition.Pivot, 0, columnDimension - 1), lUDecomposition.L.times(lUDecomposition.U));
				TestMatrix.try_success("LUDecomposition...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "LUDecomposition...", "incorrect LU decomposition calculation");
			}
			JamaMatrix b2 = jamaMatrix.inverse();
			try
			{
				TestMatrix.check(jamaMatrix.times(b2), JamaMatrix.identity(3, 3));
				TestMatrix.try_success("inverse()...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "inverse()...", "incorrect inverse calculation");
			}
			y5 = new JamaMatrix(jamaMatrix4.RowDimension, 1, 1.0);
			JamaMatrix b3 = new JamaMatrix(a9);
			jamaMatrix8 = jamaMatrix4.getMatrix(0, jamaMatrix4.RowDimension - 1, 0, jamaMatrix4.RowDimension - 1);
			try
			{
				TestMatrix.check(jamaMatrix8.solve(b3), y5);
				TestMatrix.try_success("solve()...", "");
			}
			catch (ArgumentException ex2)
			{
				num = TestMatrix.try_failure(num, "solve()...", ex2.Message);
			}
			catch (SystemException ex3)
			{
				num = TestMatrix.try_failure(num, "solve()...", ex3.Message);
			}
			jamaMatrix = new JamaMatrix(a5);
			CholeskyDecomposition choleskyDecomposition = jamaMatrix.chol();
			JamaMatrix l = choleskyDecomposition.getL();
			try
			{
				TestMatrix.check(jamaMatrix, l.times(l.transpose()));
				TestMatrix.try_success("CholeskyDecomposition...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "CholeskyDecomposition...", "incorrect Cholesky decomposition calculation");
			}
			b2 = choleskyDecomposition.solve(JamaMatrix.identity(3, 3));
			try
			{
				TestMatrix.check(jamaMatrix.times(b2), JamaMatrix.identity(3, 3));
				TestMatrix.try_success("CholeskyDecomposition solve()...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "CholeskyDecomposition solve()...", "incorrect Choleskydecomposition solve calculation");
			}
			EigenvalueDecomposition eigenvalueDecomposition = jamaMatrix.eig();
			JamaMatrix d2 = eigenvalueDecomposition.D;
			JamaMatrix v = eigenvalueDecomposition.getV();
			try
			{
				TestMatrix.check(jamaMatrix.times(v), v.times(d2));
				TestMatrix.try_success("EigenvalueDecomposition (symmetric)...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "EigenvalueDecomposition (symmetric)...", "incorrect symmetric Eigenvalue decomposition calculation");
			}
			jamaMatrix = new JamaMatrix(a7);
			eigenvalueDecomposition = jamaMatrix.eig();
			d2 = eigenvalueDecomposition.D;
			v = eigenvalueDecomposition.getV();
			try
			{
				TestMatrix.check(jamaMatrix.times(v), v.times(d2));
				TestMatrix.try_success("EigenvalueDecomposition (nonsymmetric)...", "");
			}
			catch (SystemException)
			{
				num = TestMatrix.try_failure(num, "EigenvalueDecomposition (nonsymmetric)...", "incorrect nonsymmetric Eigenvalue decomposition calculation");
			}
			TestMatrix.print("\nTestMatrix completed.\n");
			TestMatrix.print("Total errors reported: " + Convert.ToString(num) + "\n");
			TestMatrix.print("Total warnings reported: " + Convert.ToString(value) + "\n");
		}
		private static void check(double x, double y)
		{
			double num = Math.Pow(2.0, -52.0);
			if (!(x == 0.0 & Math.Abs(y) < 10.0 * num))
			{
				if (!(y == 0.0 & Math.Abs(x) < 10.0 * num))
				{
					if (Math.Abs(x - y) > 10.0 * num * Math.Max(Math.Abs(x), Math.Abs(y)))
					{
						throw new SystemException("The difference x-y is too large: x = " + x.ToString() + "  y = " + y.ToString());
					}
				}
			}
		}
		private static void check(double[] x, double[] y)
		{
			if (x.Length == y.Length)
			{
				for (int i = 0; i < x.Length; i++)
				{
					TestMatrix.check(x[i], y[i]);
				}
				return;
			}
			throw new SystemException("Attempt to compare vectors of different lengths");
		}
		private static void check(double[][] x, double[][] y)
		{
			JamaMatrix x2 = new JamaMatrix(x);
			JamaMatrix y2 = new JamaMatrix(y);
			TestMatrix.check(x2, y2);
		}
		private static void check(JamaMatrix X, JamaMatrix Y)
		{
			double num = Math.Pow(2.0, -52.0);
			if (!(X.norm1() == 0.0 & Y.norm1() < 10.0 * num))
			{
				if (!(Y.norm1() == 0.0 & X.norm1() < 10.0 * num))
				{
					if (X.minus(Y).norm1() > 1000.0 * num * Math.Max(X.norm1(), Y.norm1()))
					{
						throw new SystemException("The norm of (X-Y) is too large: " + X.minus(Y).norm1().ToString());
					}
				}
			}
		}
		private static void print(string s)
		{
			Console.Out.Write(s);
		}
		private static void try_success(string s, string e)
		{
			TestMatrix.print(">    " + s + "success\n");
			if (e != "")
			{
				TestMatrix.print(">      Message: " + e + "\n");
			}
		}
		private static int try_failure(int count, string s, string e)
		{
			TestMatrix.print(string.Concat(new string[]
			{
				">    ",
				s,
				"*** failure ***\n>      Message: ",
				e,
				"\n"
			}));
			return ++count;
		}
		private static int try_warning(int count, string s, string e)
		{
			TestMatrix.print(string.Concat(new string[]
			{
				">    ",
				s,
				"*** warning ***\n>      Message: ",
				e,
				"\n"
			}));
			return ++count;
		}
		private static void print(double[] x, int w, int d)
		{
			Console.Out.Write("\n");
			new JamaMatrix(x, 1).print(w, d);
			TestMatrix.print("\n");
		}
	}
}
