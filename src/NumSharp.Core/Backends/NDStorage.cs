﻿using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NumSharp.Backends
{
    /// <summary>
    /// Storage
    ///
    /// Responsible for :
    ///
    ///  - store data type, elements, Shape
    ///  - offers methods for accessing elements depending on shape
    ///  - offers methods for casting elements
    ///  - offers methods for change tensor order
    ///  - GetData always return reference object to the true storage
    ///  - GetData<T> and SetData<T> change dtype and cast storage
    ///  - CloneData always create a clone of storage and return this as reference object
    ///  - CloneData<T> clone storage and cast this clone 
    ///     
    /// </summary>
    public class NDStorage : IStorage
    {
        protected Array _values;
        protected Type _DType;
        protected Shape _Shape;
        protected int _TensorLayout;
        
        protected Array _ChangeTypeOfArray(Array arrayVar, Type dtype)
        {
            Array newValues = null;

            switch (Type.GetTypeCode(dtype)) 
            {
                case TypeCode.Double : 
                {
                    newValues = new double[arrayVar.Length];
                    for(int idx = 0;idx < arrayVar.Length;idx++)
                        newValues.SetValue(Convert.ToDouble(arrayVar.GetValue(idx)),idx);
                    break;
                }
                case TypeCode.Single : 
                {
                    newValues = new float[arrayVar.Length];
                    for(int idx = 0;idx < arrayVar.Length;idx++)
                        newValues.SetValue(Convert.ToSingle(arrayVar.GetValue(idx)),idx);
                    break;
                }
                case TypeCode.Decimal : 
                {
                    newValues = new Decimal[arrayVar.Length];
                    for(int idx = 0;idx < arrayVar.Length;idx++)
                        newValues.SetValue(Convert.ToDecimal(arrayVar.GetValue(idx)),idx);
                    break;
                }
                case TypeCode.Int32 : 
                {
                    newValues = new int[arrayVar.Length];
                    for(int idx = 0;idx < arrayVar.Length;idx++)
                        newValues.SetValue(Convert.ToInt32(arrayVar.GetValue(idx)),idx);
                    break;
                }
                case TypeCode.Int64 :
                {
                    newValues = new Int64[arrayVar.Length];
                    for(int idx = 0;idx < arrayVar.Length;idx++)
                        newValues.SetValue(Convert.ToInt64(arrayVar.GetValue(idx)),idx);
                    break;
                }
                case TypeCode.Object : 
                {
                    if( dtype == typeof(System.Numerics.Complex) )
                    {
                        newValues = new System.Numerics.Complex[arrayVar.Length];
                        for(int idx = 0;idx < arrayVar.Length;idx++)
                            newValues.SetValue(new System.Numerics.Complex((double)arrayVar.GetValue(idx),0),idx);
                        break;
                    }
                    /*else if ( dtype == typeof(System.Numerics.Quaternion) )
                    {
                        newValues = new System.Numerics.Quaternion[arrayVar.Length];
                        for(int idx = 0;idx < arrayVar.Length;idx++)
                            newValues.SetValue(new System.Numerics.Quaternion(new System.Numerics.Vector3(0,0,0) , (float)arrayVar.GetValue(idx)),idx);
                        break;
                    }*/
                    else 
                    {
                        newValues = new object[arrayVar.Length];
                        for(int idx = 0;idx < arrayVar.Length;idx++)
                            newValues.SetValue(arrayVar.GetValue(idx),idx);
                        break;
                    }
                    
                }
                default : 
                {
                        break;
                }
            }

            return newValues;
        }

        /// <summary>
        /// Data Type of stored elements
        /// </summary>
        /// <value>numpys equal dtype</value>
        public Type DType {get {return _DType;}}

        public int DTypeSize
        {
            get
            {
                if(_DType == typeof(string))
                {
                    return 0;
                }
                else
                {
                    return Marshal.SizeOf(_DType);
                }
            }
        }
        /// <summary>
        /// storage shape for outside representation
        /// </summary>
        /// <value>numpys equal shape</value>
        public Shape Shape {get {return _Shape;}}

        public NDStorage()
        {
            _DType = np.float64;
            _values = new double[0];
            _Shape = new Shape(0);
        }

        public NDStorage(Type dtype)
        {
            _DType = dtype;
            _values = Array.CreateInstance(dtype, 1);
            _Shape = new Shape(1);
        }

        public NDStorage(double[] values)
        {
            _DType = typeof(double);
            _Shape = new Shape(values.Length);
            _values = values;
        }

        public NDStorage(object[] values)
        {
            _DType = values.GetType().GetElementType();
            _Shape = new Shape(values.Length);
            _values = values;
        }

        /// <summary>
        /// Allocate memory by dtype, shape, tensororder (default column wise)
        /// </summary>
        /// <param name="dtype">storage data type</param>
        /// <param name="shape">storage data shape</param>
        public void Allocate(Type dtype, Shape shape)
        {
            _DType = dtype;
            _Shape = shape;
            _Shape.ChangeTensorLayout();
            int elementNumber = 1;
            for(int idx = 0; idx < shape.Dimensions.Length;idx++)
                elementNumber *= shape.Dimensions[idx];

            _values = Array.CreateInstance(dtype,elementNumber);
        }

        /// <summary>
        /// Allocate memory by Array and tensororder and deduce shape and dtype (default column wise)
        /// </summary>
        /// <param name="values">elements to store</param>
        public void Allocate(Array values)
        {
            int[] dim = new int[values.Rank];
            for (int idx = 0; idx < dim.Length;idx++)
                dim[idx] = values.GetLength(idx);
            
            _Shape = new Shape(dim);
            Type elementType = values.GetType();
            while (elementType.IsArray)
                elementType = elementType.GetElementType();
            
            _DType = elementType;
        }

        /// <summary>
        /// Get Back Storage with Columnwise tensor Layout
        /// By this method the layout is changed if layout is not columnwise
        /// </summary>
        /// <returns>reference to storage (transformed or not)</returns>
        public IStorage GetColumWiseStorage()
        {
            //if ( _TensorLayout != 2 )
                //this._ChangeRowToColumnLayout();
            
            return this;
        }

        /// <summary>
        /// Get reference to internal data storage
        /// </summary>
        /// <returns>reference to internal storage as System.Array</returns>
        public Array GetData()
        {
            return _values;
        }

        /// <summary>
        /// Clone internal storage and get reference to it
        /// </summary>
        /// <returns>reference to cloned storage as System.Array</returns>
        public Array CloneData()
        {
            return (Array) _values.Clone();
        }

        /// <summary>
        /// Get reference to internal data storage and cast elements to new dtype
        /// </summary>
        /// <param name="dtype">new storage data type</param>
        /// <returns>reference to internal (casted) storage as System.Array </returns>
        public Array GetData(Type dtype)
        {
            var methods = this.GetType().GetMethods().Where(x => x.Name.Equals("GetData") && x.IsGenericMethod && x.ReturnType.Name.Equals("T[]"));
            var genMethods = methods.First().MakeGenericMethod(dtype);

            return (Array) genMethods.Invoke(this,null);
        }

        /// <summary>
        /// Clone internal storage and cast elements to new dtype
        /// </summary>
        /// <param name="dtype">cloned storage data type</param>
        /// <returns>reference to cloned storage as System.Array</returns>
        public Array CloneData(Type dtype)
        {
            return (Array) this.GetData().Clone();
        }

        /// <summary>
        /// Get reference to internal data storage and cast elements to new dtype
        /// </summary>
        /// <typeparam name="T">new storage data type</typeparam>
        /// <returns>reference to internal (casted) storage as T[]</returns>
        public T[] GetData<T>()
        {
            return _values as T[];
        }

        /// <summary>
        /// Get all elements from cloned storage as T[] and cast dtype
        /// </summary>
        /// <typeparam name="T">cloned storgae dtype</typeparam>
        /// <returns>reference to cloned storage as T[]</returns>
        public T[] CloneData<T>()
        {
            var puffer = (Array) this.GetData().Clone();

            if (puffer.GetType().GetElementType() != typeof(T))
                puffer = _ChangeTypeOfArray(puffer,typeof(T));

            return puffer as T[];
        }

        /// <summary>
        /// Get single value from internal storage and do not cast dtype
        /// </summary>
        /// <param name="indexes">indexes</param>
        /// <returns>element from internal storage</returns>
        public object GetData(params int[] indexes)
        {
            object element = null;
            if (indexes.Length == Shape.NDim)
                element = _values.GetValue(Shape.GetIndexInShape(indexes));
            else if (Shape.Dimensions.Last() == 1)
                element = _values.GetValue(Shape.GetIndexInShape(indexes));
            else if (indexes.Length == Shape.NDim - 1)
            {
                var offset = new int[Shape.NDim];
                for (int i = 0; i < Shape.NDim - 1; i++)
                    offset[i] = indexes[i];

                NDArray nd = new NDArray(DType, Shape.Dimensions[Shape.NDim - 1]);
                for (int i = 0; i < Shape.Dimensions[Shape.NDim - 1]; i++)
                {
                    offset[offset.Length - 1] = i;
                    nd[i] = _values.GetValue(Shape.GetIndexInShape(offset));
                }

                return nd;
            }
            // 3 Dim
            else if (indexes.Length == Shape.NDim - 2)
            {
                var offset = new int[Shape.NDim];
                NDArray nd = new NDArray(DType, new int[]{ Shape.Dimensions[Shape.NDim - 2] , Shape.Dimensions[Shape.NDim - 1] });
                
                for (int i = 0; i < Shape.Dimensions[Shape.NDim - 2]; i++)
                {
                    for (int j = 0; j < Shape.Dimensions[Shape.NDim - 1]; j++)
                    {
                        offset[0] = 0;
                        offset[1] = i;
                        offset[2] = j;
                        nd[i,j] = _values.GetValue(Shape.GetIndexInShape(offset));
                    }
                }
                return nd;
            }
            else
                throw new Exception("indexes must be equal to number of dimension.");
            return element;
        }

        /// <summary>
        /// Get single value from internal storage as type T and cast dtype to T
        /// </summary>
        /// <param name="indexes">indexes</param>
        /// <typeparam name="T">new storage data type</typeparam>
        /// <returns>element from internal storage</returns>
        public T GetData<T>(params int[] indexes)
        {
            T[] values = this.GetData() as T[];

            return values[Shape.GetIndexInShape(indexes)];
        }

        /// <summary>
        /// Set an array to internal storage and keep dtype
        /// </summary>
        /// <param name="values"></param>
        public void SetData(Array values)
        {
            _values = values;
            this.ChangeDataType(this._DType);
        }

        /// <summary>
        /// Set 1 single value to internal storage and keep dtype
        /// </summary>
        /// <param name="value"></param>
        /// <param name="indexes"></param>
        public void SetData(object value, params int[] indexes)
        {
            _values.SetValue(value,_Shape.GetIndexInShape(indexes));
        }

        /// <summary>
        /// Set a 1D Array of type T to internal storage and cast dtype
        /// </summary>
        /// <param name="values"></param>
        /// <typeparam name="T"></typeparam>
        public void SetData<T>(Array values)
        {
            _values = values;
            this.ChangeDataType(typeof(T));
        }

        /// <summary>
        /// Set an Array to internal storage, cast it to new dtype and change dtype  
        /// </summary>
        /// <param name="values"></param>
        /// <param name="dtype"></param>
        public void SetData(Array values, Type dtype)
        {
            _values = values;
            this.ChangeDataType(dtype);
        } 

        /// <summary>
        /// Change dtype of elements
        /// </summary>
        /// <param name="dtype">new storage data type</param>
        /// <returns>sucess or not</returns>
        public void ChangeDataType(Type dtype)
        {
            _DType = dtype;
        }

        public void SetNewShape(params int[] dimensions)
        {
            _Shape = new Shape(dimensions);
        }

        public void Reshape(params int[] dimensions)
        {
            _Shape = new Shape(dimensions);
        }

        public object Clone()
        {
            var puffer = new NDStorage();
            puffer.Allocate(_DType, new Shape(_Shape.Dimensions));
            puffer.SetData((Array)_values.Clone());

            return puffer;
        }

        public void SetData<T>(T value, int offset)
        {
            throw new NotImplementedException();
        }

        public NDArray Dot(NDArray x, NDArray y)
        {
            var dtype = x.dtype;

            if (x.ndim == 0 && y.ndim == 0)
            {
                switch (dtype.Name)
                {
                    case "Int32":
                        return y.Data<int>(0) * x.Data<int>(0);
                }
            }
            else if (x.ndim == 1 && x.ndim == 1)
            {
                int sum = 0;
                switch (dtype.Name)
                {
                    case "Int32":
                        for (int i = 0; i < x.size; i++)
                            sum += x.Data<int>(i) * y.Data<int>(i);
                        break;
                }
                return sum;
            }
            else if (x.ndim == 2 && y.ndim == 1)
            {
                var nd = new NDArray(dtype, new Shape(x.shape[0]));
                switch (dtype.Name)
                {
                    case "Int32":
                        for (int i = 0; i < x.shape[0]; i++)
                            for (int j = 0; j < y.shape[0]; j++)
                                nd.Data<int>()[i] += x.Data<int>(i, j) * y.Data<int>(j);
                        break;
                }
                return nd;
            }
            else if (x.ndim == 2 && y.ndim == 2)
            {
                return np.matmul(x, y);
            }

            throw new NotImplementedException($"dot {x.ndim} * {y.ndim}");
        }
    }
}