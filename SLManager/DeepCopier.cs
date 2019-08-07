using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace DeepCopier
{
    class DeepCopier<T> 
    {
        private ParameterExpression _sourceExpr;
        private ParameterExpression _destExpr;

        public DeepCopier()
        {
            _sourceExpr = Expression.Parameter(typeof(T), "source");
            _destExpr = Expression.Parameter(typeof(T), "dest");
        }

        public Expression<Action<T, T>> GetDeepCopyFieldLambda(string fieldName)
            => GetDeepCopyFieldLambda(typeof(T).GetField(fieldName));

        public Expression<Action<T, T>> GetDeepCopyFieldLambda(System.Reflection.FieldInfo field)
        {
            Expression expression = GetDeepCopyFieldExpression(field);
            return Expression.Lambda<Action<T, T>>(expression, _sourceExpr, _destExpr);
        }

        internal IEnumerable<Expression> GetAllDeepCopyFieldExpressions()
        {
            return typeof(T).GetFields().Select(field => GetDeepCopyFieldExpression(field));
        }

        internal Expression<Action<T, T>> GetAllDeepCopyFieldLambda()
            => Lambda(GetAllDeepCopyFieldExpressions());

        internal Action<T, T> CompileAllDeepCopyFieldAction()
            => GetAllDeepCopyFieldLambda().Compile();

        internal Task<Action<T, T>> StartCompileAllDeepCopyFieldAction()
            => Task.Run(CompileAllDeepCopyFieldAction);

        public Expression GetDeepCopyFieldExpression(string fieldName)
            => GetDeepCopyFieldExpression(typeof(T).GetField(fieldName));

        public Expression GetDeepCopyFieldExpression(System.Reflection.FieldInfo field)
        {
            var fType = field.FieldType;
            MemberExpression sourceFieldExp = Expression.Field(_sourceExpr, field);
            var cloneExpr = GetDeepCloneExpression(fType, sourceFieldExp);

            MemberExpression destFieldExp = Expression.Field(_destExpr, field);

            BinaryExpression assignExp = Expression.Assign(destFieldExp, cloneExpr);
            return assignExp;
        }

        public Expression<Action<T, T>> Lambda(Expression expression)
        {
            return Expression.Lambda<Action<T, T>>(expression, _sourceExpr, _destExpr);
        }

        public Expression<Action<T, T>> Lambda(params Expression[] expressions)
            => Lambda((IEnumerable<Expression>)expressions);

        public Expression<Action<T, T>> Lambda(IEnumerable<Expression> expressions)
        {
            Expression blockExpr = Expression.Block(expressions);
            return Lambda(blockExpr);
        }

        public Action<T, T> Compile(Expression expression)
        {
            return Lambda(expression).Compile();
        }

        internal IEnumerable<Expression> GetAllShallowCopyFieldExpressions()
        {
            return typeof(T).GetFields().Select(field => GetShallowCopyFieldExpression(field));
        }

        internal Expression<Action<T, T>> GetShallowCopyCopyFieldLambda()
            => Lambda(GetAllShallowCopyFieldExpressions());

        internal Action<T, T> CompileAllShallowCopyFieldAction()
            => GetShallowCopyCopyFieldLambda().Compile();

        internal Task<Action<T, T>> StartCompileAllShallowCopyFieldAction()
            => Task.Run(CompileAllShallowCopyFieldAction);

        public Expression GetShallowCopyFieldExpression(System.Reflection.FieldInfo field)
        {
            MemberExpression sourceFieldExp = Expression.Field(_sourceExpr, field);
            MemberExpression destFieldExp = Expression.Field(_destExpr, field);
            BinaryExpression assignExp = Expression.Assign(destFieldExp, sourceFieldExp);
            return assignExp;
        }


        public Expression GetDeepCloneExpression(Type dataType, Expression dataExpr)
        {
            if (dataType.IsValueType || typeof(string) == dataType)
            {
                return dataExpr;
            }
            else if (dataType.IsArray)
            {
                var valueType = dataType.GetElementType();
                if (valueType.IsValueType || typeof(string) == valueType)
                {
                    var cloneArrayMethod = typeof(ExpressionHelper).GetMethod("CloneArray", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    var cloneArrayGenericMethod = cloneArrayMethod.MakeGenericMethod(valueType);
                    var cloneArrayExpr = Expression.Call(cloneArrayGenericMethod, dataExpr);
                    return cloneArrayExpr;
                }
                else
                {

                    // deepclone element
                }
            }
            else if (dataType.IsGenericType)
            {
                var genericArgTypes = dataType.GetGenericArguments();
                var genericTypeDef = dataType.GetGenericTypeDefinition();
                if (typeof(SortedDictionary<,>) == genericTypeDef)
                {
                    SortedDictionary<int, int> s = new SortedDictionary<int, int>();
                    Type keyType = genericArgTypes[0];
                    Type valueType = genericArgTypes[1];
                    if (!keyType.IsValueType && typeof(string) != keyType)
                        throw new NotSupportedException($"SortedDictionary {dataType.Name} key is not a value type");
                    if (valueType.IsValueType || typeof(string) == valueType)
                    {
                        // clone by new Dictionary<,>(IDictionary)
                        Type genericDictType = genericTypeDef.MakeGenericType(keyType, valueType);
                        Type IDictionaryType = typeof(IDictionary<,>).MakeGenericType(keyType, valueType);
                        Type IComparerType = typeof(IComparer<>).MakeGenericType(keyType);
                        var contructor = genericDictType.GetConstructor(new Type[] { IDictionaryType, IComparerType });
                        var comparerProperty = Expression.Property(dataExpr, "Comparer");
                        var newDictExpr = Expression.New(contructor, dataExpr, comparerProperty);
                        return newDictExpr;
                    }
                    else
                    {
                        ParameterExpression paramExpr = Expression.Parameter(valueType, "value");
                        var valueCloneExpr = GetDeepCloneExpression(valueType, paramExpr);
                        var valueCloneLambda = Expression.Lambda(valueCloneExpr, paramExpr);
                        var copyDictMethod = ExpressionHelper.GetCopySortedDictionaryGenericMethod(keyType, valueType);
                        var copyDictExpr = Expression.Call(copyDictMethod, dataExpr, valueCloneLambda);
                        return copyDictExpr;
                    }
                }
                if (typeof(Dictionary<,>) == genericTypeDef)
                {
                    Type keyType = genericArgTypes[0];
                    Type valueType = genericArgTypes[1];
                    if (!keyType.IsValueType && typeof(string) != keyType)
                        throw new NotSupportedException($"Dictionary {dataType.Name} key is not a value type");
                    if (valueType.IsValueType || typeof(string) == valueType)
                    {
                        // clone by new Dictionary<,>(IDictionary)
                        Type genericDictType = genericTypeDef.MakeGenericType(keyType, valueType);
                        Type IDictionaryType = typeof(IDictionary<,>).MakeGenericType(keyType, valueType);
                        var contructor = genericDictType.GetConstructor(new Type[] { IDictionaryType });
                        var newDictExpr = Expression.New(contructor, dataExpr);
                        return newDictExpr;
                    }
                    else
                    {
                        ParameterExpression paramExpr = Expression.Parameter(valueType, "value");
                        var valueCloneExpr = GetDeepCloneExpression(valueType, paramExpr);
                        var valueCloneLambda = Expression.Lambda(valueCloneExpr, paramExpr);
                        var copyDictMethod = ExpressionHelper.GetCopyDictionaryGenericMethod(keyType, valueType);
                        var copyDictExpr = Expression.Call(copyDictMethod, dataExpr, valueCloneLambda);
                        return copyDictExpr;

                        //Type kvpGenericType = ExpressionHelper.GetKeyValuePairType(keyType, valueType);
                        //ParameterExpression paramExpr = Expression.Parameter(kvpGenericType, "kvp");
                        //// key
                        //Expression keySelectExpr = Expression.Field(paramExpr, "Key");
                        //var keySelectLambda = Expression.Lambda(keySelectExpr, paramExpr);
                        //// value
                        //Expression valueSelectExpr = Expression.Field(paramExpr, "Value");
                        //var valueCloneExpr = GetDeepCloneExpression(valueType, valueSelectExpr);
                        //var valueCloneLambda = Expression.Lambda(valueCloneExpr, paramExpr);
                        //// ToDictionary
                        //var toDictMethod = ExpressionHelper.GetToDictionaryGenericMethod(kvpGenericType, keyType, valueType);
                        //var toDictExpr = Expression.Call(toDictMethod, dataExpr, keySelectLambda, valueCloneLambda);
                        //return toDictExpr;
                    }
                }
                else if (typeof(List<>) == genericTypeDef)
                {
                    Type valueType = genericArgTypes[0];
                    Type genericListType = genericTypeDef.MakeGenericType(valueType);
                    Type enumerableType = typeof(IEnumerable<>).MakeGenericType(valueType);
                    if (valueType.IsValueType || typeof(string) == valueType)
                    {
                        var contructor = genericListType.GetConstructor(new Type[] { enumerableType });
                        var newListExpr = Expression.New(contructor, dataExpr);
                        return newListExpr;
                    }
                    else
                    {
                        ParameterExpression paramExpr = Expression.Parameter(valueType, "item");
                        var valueCloneExpr = GetDeepCloneExpression(valueType, paramExpr);
                        var valueCloneLambda = Expression.Lambda(valueCloneExpr, paramExpr);


                        var selectGenericMethod = ExpressionHelper.GetSelectGenericMethod(valueType, valueType);
                        var selectCloneExpr = Expression.Call(selectGenericMethod, dataExpr, valueCloneLambda);
                        var toListMethod = ExpressionHelper.GetToListGenericMethod(valueType);
                        var toListExpr = Expression.Call(toListMethod, selectCloneExpr);
                        return toListExpr;  // source.{field}.Select(item => CloneArray(item)).ToList()
                    }
                }
            }
            throw new NotImplementedException($"{dataType.Name} is not implemented");
        }



        //public LambdaExpression GetDeepCloneLambda(string fieldName)
        //    => GetDeepCloneLambda(typeof(T).GetField(fieldName).FieldType);

        //public LambdaExpression GetDeepCloneLambda(Type dataType)
        //{

        //    ParameterExpression dataExpr = Expression.Parameter(dataType, "source");
        //    //if(_dict_typeDeepCloneExpressions.TryGetValue(data))
        //    if (dataType.IsValueType)
        //    {
        //        throw new NotImplementedException();
        //    }
        //    else if (dataType.IsArray)
        //    {
        //        var valueType = dataType.GetElementType();
        //        if (valueType.IsValueType)
        //        {
        //            var cloneArrayMethod = typeof(DeepCopyExpressionHelper).GetMethod("CloneArray", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        //            var cloneArrayGenericMethod = cloneArrayMethod.MakeGenericMethod(valueType);
        //            var cloneArrayExpr = Expression.Call(cloneArrayGenericMethod, dataExpr);
        //            return Expression.Lambda(cloneArrayExpr, dataExpr);
        //        }
        //        else
        //        {
        //            // deepclone element
        //        }
        //    }
        //    else if (dataType.IsGenericType)
        //    {
        //        var genericArgTypes = dataType.GetGenericArguments();
        //        var genericTypeDef = dataType.GetGenericTypeDefinition();
        //        if (typeof(SortedDictionary<,>) == genericTypeDef)
        //        {

        //        }
        //        if (typeof(Dictionary<,>) == genericTypeDef)
        //        {
        //            Type keyType = genericArgTypes[0];
        //            Type valueType = genericArgTypes[1];
        //            if (!keyType.IsValueType)
        //                throw new NotSupportedException($"Dictionary {dataType.Name} key is not a value type");
        //            if (valueType.IsValueType)
        //            {
        //                // clone by new Dictionary<,>(IDictionary)
        //                Type genericDictType = genericTypeDef.MakeGenericType(keyType, valueType);
        //                Type IDictionaryType = typeof(IDictionary<,>).MakeGenericType(keyType, valueType);
        //                var contructor = genericDictType.GetConstructor(new Type[] { IDictionaryType });
        //                var newDictExpr = Expression.New(contructor, dataExpr);
        //                return Expression.Lambda(newDictExpr, dataExpr);
        //            }
        //            else
        //            {
        //                Type kvpGenericType = DeepCopyExpressionHelper.GetKeyValuePairType(keyType, valueType);
        //                ParameterExpression paramExpr = Expression.Parameter(kvpGenericType, "kvp");
        //                // key
        //                Expression keySelectExpr = Expression.Field(paramExpr, "Key");
        //                var keySelectLambda = Expression.Lambda(keySelectExpr, paramExpr);
        //                // value
        //                Expression valueSelectExpr = Expression.Field(paramExpr, "Value");
        //                var valueCloneExpr = GetDeepCloneExpression(valueType, valueSelectExpr);
        //                var valueCloneLambda = Expression.Lambda(valueCloneExpr, paramExpr);
        //                // ToDictionary
        //                var toDictMethod = DeepCopyExpressionHelper.GetToDictionaryGenericMethod(kvpGenericType, keyType, valueType);
        //                var toDictExpr = Expression.Call(toDictMethod, dataExpr, keySelectLambda, valueCloneLambda);
        //                return Expression.Lambda(toDictExpr, dataExpr);
        //                // var keySelectLambda = Expression.Lambda(keySelectExpr, paramExpr);


        //                //d.ToDictionary()
        //                // Func(KeyValuePair<TKey, TValue>, TKey)


        //                //ParameterExpression paramExpr = Expression.Parameter(valueType, "item");
        //                //var valueCloneExpr = GetDeepCloneExpression(valueType, paramExpr);
        //                //var valueCloneLambda = Expression.Lambda(valueCloneExpr, paramExpr);

        //                // Dictionary => IEnumerable<KeyValuePair<TKey,TValue>>


        //                // var cloneExprOfValue = GetDeepCloneExpression(valueType);
        //            }
        //        }
        //        else if (typeof(List<>) == genericTypeDef)
        //        {
        //            Type valueType = genericArgTypes[0];
        //            Type genericListType = genericTypeDef.MakeGenericType(valueType);
        //            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(valueType);
        //            if (valueType.IsValueType)
        //            {
        //                var contructor = genericListType.GetConstructor(new Type[] { enumerableType });
        //                var newListExpr = Expression.New(contructor, dataExpr);
        //                return Expression.Lambda(newListExpr, dataExpr);
        //            }
        //            else
        //            {
        //                ParameterExpression paramExpr = Expression.Parameter(valueType, "item");
        //                var valueCloneExpr = GetDeepCloneExpression(valueType, paramExpr);
        //                var valueCloneLambda = Expression.Lambda(valueCloneExpr, paramExpr);

        //                var selectGenericMethod = DeepCopyExpressionHelper.GetSelectGenericMethod(valueType, valueType);
        //                var selectCloneExpr = Expression.Call(selectGenericMethod, dataExpr, valueCloneLambda);
        //                var toListMethod = DeepCopyExpressionHelper.GetToListGenericMethod(valueType);
        //                var toListExpr = Expression.Call(toListMethod, selectCloneExpr);
        //                return Expression.Lambda(toListExpr, dataExpr);  // source.{field}.Select(item => CloneArray(item)).ToList()
        //            }
        //        }
        //    }
        //    return null;
        //}

        // private 
    }

    class ExpressionHelper
    {
        static public System.Reflection.MethodInfo GetSelectGenericMethod(Type sourceType, Type resultType)
        {
            var selectMethod = typeof(Enumerable).GetMethods().Where(
                m => m.Name == "Select" &&
                m.GetParameters()[1].ParameterType.Name == "Func`2").Single();
            return selectMethod.MakeGenericMethod(sourceType, resultType);
        }

        static public System.Reflection.MethodInfo GetToListGenericMethod(Type sourceType)
        {
            var toListMethod = typeof(Enumerable).GetMethod("ToList");
            return toListMethod.MakeGenericMethod(sourceType);
        }

        static public Type GetKeyValuePairType(Type keyType, Type valueType)
        {
            return typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
        }

        static public System.Reflection.MethodInfo GetToDictionaryGenericMethod(Type source ,Type keyType, Type valueType)
        {
            // ToDictionary(source, keySelector, elemSelector)
            var toDictMethod = typeof(Enumerable).GetMethods().Where(
                m => m.Name == "ToDictionary" &&
                m.GetParameters().Length == 3 &&
                m.GetGenericArguments().Length == 3).Single();
            return toDictMethod.MakeGenericMethod(source, keyType, valueType);
        }

        static public System.Reflection.MethodInfo GetToDictionaryGenericMethod(Type keyType, Type valueType)
            => GetToDictionaryGenericMethod(GetKeyValuePairType(keyType, valueType), keyType, valueType);

        static public System.Reflection.MethodInfo GetCopyDictionaryGenericMethod(Type keyType, Type elementType)
        {
            var method = typeof(ExpressionHelper).GetMethod("CopyDictionary");
            return method.MakeGenericMethod(keyType, elementType);
        }

        public static Dictionary<TKey, TElement> CopyDictionary<TKey, TElement>(IDictionary<TKey, TElement> source, Func<TElement, TElement> elementCopier)
        {
            if (source == null) return null;
            Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>();
            foreach (var kvp in source)
            {
                d.Add(kvp.Key, elementCopier(kvp.Value));
            }
            return d;
        }

        public static SortedDictionary<TKey, TElement> CopySortedDictionary<TKey, TElement>(SortedDictionary<TKey, TElement> source, Func<TElement, TElement> elementCopier)
        {
            if (source == null) return null;
            SortedDictionary<TKey, TElement> d = new SortedDictionary<TKey, TElement>(source.Comparer);
            foreach (var kvp in source)
            {
                d.Add(kvp.Key, elementCopier(kvp.Value));
            }
            return d;
        }

        static public System.Reflection.MethodInfo GetCopySortedDictionaryGenericMethod(Type keyType, Type elementType)
        {
            // ToDictionary(source, keySelector, elemSelector)
            var method = typeof(ExpressionHelper).GetMethod("CopySortedDictionary");
            return method.MakeGenericMethod(keyType, elementType);
        }

        public static T[] CloneArray<T>(T[] sourceArray)
        {
            if (sourceArray == null) return null;
            T[] destArr = new T[sourceArray.Length];
            sourceArray.CopyTo(destArr, 0);
            return destArr;
        }

        public static T[] CloneArray2<T>(T[] sourceArray, Func<T, T> elementCopier)
        {
            if (sourceArray == null) return null;
            T[] destArr = new T[sourceArray.Length];
            for (int i = 0; i < sourceArray.Length; i++)
            {
                destArr[i] = elementCopier(sourceArray[i]);
            }
            return destArr;
        }
    }
}
