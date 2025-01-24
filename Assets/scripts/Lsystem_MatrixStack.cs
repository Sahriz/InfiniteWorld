using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;



public static class Lsystem_MatrixStack
{
    public static Stack<Matrix4x4> matrix_stack;
    public static Matrix4x4 current_matrix;

    
    static Lsystem_MatrixStack() 
    {
        matrix_stack = new Stack<Matrix4x4>();
        matrix_stack.Push(Matrix4x4.identity);
        current_matrix = matrix_stack.Peek();
	}
    

    public static void Push()
    {
        matrix_stack.Push(current_matrix);
    }

    public static void Pop()
    {
        if(matrix_stack.Count > 0) { current_matrix = matrix_stack.Peek(); matrix_stack.Pop(); }
        else { Debug.Log("Stack is empty!"); }
        
    }

    public static Matrix4x4 Peek()
    {
        return matrix_stack.Peek();
    }

    public static void Reset()
    {
        matrix_stack.Clear();
        matrix_stack.Push(Matrix4x4.identity);
        current_matrix = matrix_stack.Peek();
    }

    public static void Rotation(Vector3 rotation_angle)
    {
        Matrix4x4 rotationX = new Matrix4x4();
		Matrix4x4 rotationY = new Matrix4x4();
		Matrix4x4 rotationZ = new Matrix4x4();
		
        rotationX.m00 = 1; rotationX.m01 = 0; rotationX.m02 = 0; rotationX.m03 = 0;
        rotationX.m10 = 0; rotationX.m11 = Mathf.Cos(rotation_angle.x); rotationX.m12 = Mathf.Sin(rotation_angle.x); rotationX.m13 = 0;
        rotationX.m20 = 0; rotationX.m21 = -Mathf.Sin(rotation_angle.x); rotationX.m22 = Mathf.Cos(rotation_angle.x); rotationX.m23 = 0;
        rotationX.m30 = 0; rotationX.m31 = 0; rotationX.m32 = 0; rotationX.m33 = 1;

		rotationY.m00 = Mathf.Cos(rotation_angle.y); rotationY.m01 = 0; rotationY.m02 = -Mathf.Sin(rotation_angle.y); rotationY.m03 = 0;
		rotationY.m10 = 0; rotationY.m11 = 1; rotationY.m12 = 0; rotationY.m13 = 0;
		rotationY.m20 = Mathf.Sin(rotation_angle.y); rotationY.m21 = 0; rotationY.m22 = Mathf.Cos(rotation_angle.y); rotationY.m23 = 0;
		rotationY.m30 = 0; rotationY.m31 = 0; rotationY.m32 = 0; rotationY.m33 = 1;

		rotationZ.m00 = Mathf.Cos(rotation_angle.z); rotationZ.m01 = -Mathf.Sin(rotation_angle.z); rotationZ.m02 = 0; rotationZ.m03 = 0;
		rotationZ.m10 = Mathf.Sin(rotation_angle.z); rotationZ.m11 = Mathf.Cos(rotation_angle.z); rotationZ.m12 = 0; rotationZ.m13 = 0;
		rotationZ.m20 = 0; rotationZ.m21 = 0; rotationZ.m22 = 1; rotationZ.m23 = 0;
		rotationZ.m30 = 0; rotationZ.m31 = 0; rotationZ.m32 = 0; rotationZ.m33 = 1;

        

        Matrix4x4 rotationMatrix = rotationX*rotationY*rotationZ;
        current_matrix = current_matrix * rotationMatrix;

	}

    public static void Translation(Vector3 translation_coords)
    {

        Matrix4x4 translation = Matrix4x4.Translate(translation_coords);


        current_matrix = current_matrix * translation;

	}

    

   

	public static Quaternion GetRotationFromMatrix()
	{
		// Get the top-left 3x3 part of the matrix
		Matrix4x4 rotationMatrix = new Matrix4x4();
        
		rotationMatrix.m00 = current_matrix.m00;
		rotationMatrix.m01 = current_matrix.m01;
		rotationMatrix.m02 = current_matrix.m02;

		rotationMatrix.m10 = current_matrix.m10;
		rotationMatrix.m11 = current_matrix.m11;
		rotationMatrix.m12 = current_matrix.m12;

		rotationMatrix.m20 = current_matrix.m20;
		rotationMatrix.m21 = current_matrix.m21;
		rotationMatrix.m22 = current_matrix.m22;
        
		// Extract the rotation as a quaternion
		Quaternion rotation = Quaternion.LookRotation(rotationMatrix.GetColumn(2), rotationMatrix.GetColumn(1));

		return rotation;
	}

}
