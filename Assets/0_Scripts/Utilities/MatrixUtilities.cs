using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixUtilities
{
   // Reverse each row of matrix
    public static void Reverse<T>(int size, T[,] matrix)
    {
        // Traverse each row of [,]mat
        for (int i = 0; i < size; i++) {
 
            // Initialise start and end index
            int start = 0;
            int end = size - 1;
 
            // Till start < end, swap the element
            // at start and end index
            while (start < end) {
 
                // Swap the element
                T temp = matrix[i, start];
                matrix[i, start] = matrix[i, end];
                matrix[i, end] = temp;
 
                // Increment start and decrement
                // end for next pair of swapping
                start++;
                end--;
            }
        }
    }

    // An Inplace function to
    // rotate a N x N matrix
    // by 90 degrees in anti-
    // clockwise direction
    public static void RotateMatrix<T>(int size, T[,] matrix)
    {
        Reverse(size, matrix);
 
        // Performing Transpose
        for (int i = 0; i < size; i++) {
            for (int j = i; j < size; j++) {
                T temp = matrix[i, j];
                matrix[i, j] = matrix[j, i];
                matrix[j, i] = temp;
            }
        }
    }

    public static void ReflectMatrixHorizontally<T>(int size, T[,] matrix)
    {
        for(int j = 0; j < size; j++)
        {
            for(int i = 0; i < size; i++)
            {
                
            }
        }
    }
 
    // Function to print the matrix
    public static void PrintMatrix<T>(int size, T[, ] matrix)
    {
        string output = "";
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++)
                output += (matrix[i, j] + " ");
            output += '\n';
        }
        Debug.Log(output);
    }


    // Creates a shallow copy of a matrix
    public static T[,] CopyMatrix<T>(int size, T[,] original)
    {
        T[,] copy = new T[size, size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++)
                copy[i, j] = original[i, j];
        }

        return copy;
    }
}
