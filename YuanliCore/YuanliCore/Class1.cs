using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore
{
    public class Matrix2D
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public double[,] Data { get; set; }

        public double this[int row, int col]
        {
            get { return Data[row, col]; }
            set { Data[row, col] = value; }
        }

        public Matrix2D(int row, int col)
        {
            Row = row;
            Column = col;
            Data = new double[row, col];

            for (var i = 0; i < row; i++)
            {
                for (var j = 0; j < col; j++)
                {
                    Data[i, j] = 0;
                }
            }
        }

        public static Matrix2D operator -(Matrix2D Left, Matrix2D Right)
        {
            var minus = new Matrix2D(Left.Row, Left.Column);

            for (var i = 0; i < minus.Row; i++)
            {
                for (var j = 0; j < minus.Column; j++)
                {
                    minus[i, j] = Left[i, j] - Right[i, j];
                }
            }

            return minus;
        }

        public static Matrix2D operator +(Matrix2D Left, Matrix2D Right)
        {
            var sum = new Matrix2D(Left.Row, Left.Column);

            for (var i = 0; i < sum.Row; i++)
            {
                for (var j = 0; j < sum.Column; j++)
                {
                    sum[i, j] = Left[i, j] + Right[i, j];
                }
            }

            return sum;
        }

        public static Matrix2D operator *(Matrix2D M, double Scalar)
        {
            var RM = new Matrix2D(M.Row, M.Column);

            for (var i = 0; i < RM.Row; i++)
            {
                for (var j = 0; j < RM.Column; j++)
                {
                    RM[i, j] = M[i, j] * Scalar;
                }
            }

            return RM;
        }

        public static Matrix2D operator *(double Scalar, Matrix2D M)
        {
            var RM = new Matrix2D(M.Row, M.Column);

            for (var i = 0; i < RM.Row; i++)
            {
                for (var j = 0; j < RM.Column; j++)
                {
                    RM[i, j] = M[i, j] * Scalar;
                }
            }

            return RM;
        }

        public static Matrix2D operator *(Matrix2D Left, Matrix2D Right)
        {
            if (Left.Column != Right.Row)
            {
                return null;
            }

            var product = new Matrix2D(Left.Row, Right.Column);

            for (var i = 0; i < product.Row; i++)
            {
                for (var j = 0; j < product.Column; j++)
                {
                    product[i, j] = 0;
                    for (int k = 0; k < Left.Column; k++)
                    {
                        product[i, j] += Left[i, k] * Right[k, j];
                    }
                }
            }

            return product;
        }
        public static Matrix2D Transpose(Matrix2D M)
        {
            var RM = new Matrix2D(M.Column, M.Row);

            for (var i = 0; i < RM.Row; i++)
            {
                for (var j = 0; j < RM.Column; j++)
                {
                    RM[i, j] = M[j, i];
                }
            }

            return RM;
        }

        public static Matrix2D Minor(Matrix2D M, int i, int j)
        {
            var RM = new Matrix2D(M.Row - 1, M.Column - 1);

            for (var a = 0; a < RM.Row; a++)
            {
                for (var b = 0; b < RM.Column; b++)
                {
                    var p = (a >= i) ? (a + 1) : a;
                    var q = (b >= j) ? (b + 1) : b;

                    RM[a, b] = M[p, q];
                }
            }

            return RM;
        }

        public static double? Determinant(Matrix2D M)
        {
            if ((M.Row <= 1) || (M.Column <= 1))
            {
                return null;
            }
            if (M.Row != M.Column)
            {
                return null;
            }

            double? result = 0;

            if (M.Row == 2)
            {
                return ((M[0, 0] * M[1, 1]) - (M[0, 1] * M[1, 0]));
            }

            for (var i = 0; i < M.Column; i++)
            {
                result += M[0, i] * (Math.Pow(-1, (i + 2)) * Determinant(Minor(M, 0, i)));
            }

            return result;
        }

        public static Matrix2D Adjugate(Matrix2D M)
        {
            var RM = new Matrix2D(M.Column, M.Row);

            for (var i = 0; i < RM.Row; i++)
            {
                for (var j = 0; j < RM.Column; j++)
                {
                    RM[i, j] = (double)(Math.Pow(-1, (i + j + 2)) * Determinant(Minor(M, j, i)));
                }
            }

            return RM;
        }

        public static Matrix2D Inverse(Matrix2D M)
        {
            var RM = new Matrix2D(M.Column, M.Row);
            var detM = Determinant(M);

            if (detM == null)
            {
                return null;
            }
            if (detM == 0)
            {
                return null;
            }

            RM = Adjugate(M) * (1 / (double)detM);

            return RM;
        }
    }
}
