﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace JsonConfig
{
    public static class Merger
    {
        /// <summary>
        /// Merge the specified obj2 and obj1, where obj1 has precendence and
        /// overrules obj2 if necessary.
        /// </summary>
        /// <exception cref='TypeMissmatchException'>
        /// Is thrown when the type missmatch exception.
        /// </exception>
        public static dynamic Merge(dynamic mObj1, dynamic mObj2)
        {
            dynamic obj1 = mObj1;
            dynamic obj2 = mObj2;

            // make sure we only deal with ConfigObject but not ExpandoObject as currently
            // return from JsonFX
            if (obj1 is ExpandoObject) obj1 = ConfigObject.FromExpando(obj1);
            if (obj2 is ExpandoObject) obj2 = ConfigObject.FromExpando(obj2);

            // if both objects are NullExceptionPreventer, return a ConfigObject so the
            // user gets an "Empty" ConfigObject
            if (obj1 is NullExceptionPreventer && obj2 is NullExceptionPreventer)
                return new ConfigObject();

            // if any object is of NullExceptionPreventer, the other object gets precedence / overruling
            if (obj1 is NullExceptionPreventer && obj2 is ConfigObject)
                return obj2;
            if (obj2 is NullExceptionPreventer && obj1 is ConfigObject)
                return obj1;

            // handle what happens if one of the args is null
            if (obj1 == null && obj2 == null)
                return new ConfigObject();

            if (obj2 == null) return obj1;
            if (obj1 == null) return obj2;

            if (obj1.GetType() != obj2.GetType())
                throw new TypeMissmatchException();

            // changes in the dictionary WILL REFLECT back to the object
            var dict1 = (IDictionary<string, object>)(obj1);
            var dict2 = (IDictionary<string, object>)(obj2);

            dynamic result = new ConfigObject();
            var rdict = (IDictionary<string, object>)result;

            // first, copy all non colliding keys over
            foreach (var kvp in dict1)
                if (!dict2.Keys.Contains(kvp.Key))
                    rdict.Add(kvp.Key, kvp.Value);
            foreach (var kvp in dict2)
                if (!dict1.Keys.Contains(kvp.Key))
                    rdict.Add(kvp.Key, kvp.Value);

            // now handle the colliding keys	
            foreach (var kvp1 in dict1)
            {
                // skip already copied over keys
                if (!dict2.Keys.Contains(kvp1.Key) || dict2[kvp1.Key] == null)
                    continue;

                var kvp2 = new KeyValuePair<string, object>(kvp1.Key, dict2[kvp1.Key]);

                // some shortcut variables to make code more readable		
                var key = kvp1.Key;
                var value1 = kvp1.Value;
                var value2 = kvp2.Value;
                var type1 = value1.GetType();
                var type2 = value1.GetType();

                // check if both are same type
                if (type1 != type2)
                    throw new TypeMissmatchException();

                if (value1 is ConfigObject[])
                {
                    rdict[key] = CollectionMerge(value1, value2);
                    /*var d1 = val1 as IDictionary<string, object>;
					var d2 = val2 as IDictionary<string, object>;
					rdict[key] = CollectionMerge (val1, val2); */
                }
                else if (value1 is ConfigObject)
                {
                    rdict[key] = Merge(value1, value2);
                }
                else if (value1 is string)
                {
                    rdict[key] = value1;
                }
                else if (value1 is IEnumerable)
                {
                    rdict[key] = CollectionMerge(value1, value2);
                }
                else
                {
                    rdict[key] = value1;
                }

                //else if (kvp.Value.GetType ().IsByRef) {
                // recursively merge it	
                //}
            }
            return result;
        }
        /// <summary>
        /// Merges the multiple ConfigObjects, accepts infinite list of arguments
        /// First named objects overrule preceeding objects.
        /// </summary>
        /// <returns>
        /// The merged ConfigObject.
        /// </returns>
        /// <param name='objects'>
        /// List of objects which are to be merged.
        /// </param>
        public static dynamic MergeMultiple(params object[] objects)
        {
            if (objects.Length == 1)
                return objects[0];

            else if (objects.Length == 2)
                return Merge(objects[0], objects[1]);

            else
            {
                object head = objects.First();
                object[] tail = objects.Skip(1).Take(objects.Length - 1).ToArray();

                return Merge(head, MergeMultiple(tail));
            }
        }
        public static dynamic CollectionMerge(dynamic obj1, dynamic obj2)
        {
            var x = new ArrayList();
            x.AddRange(obj1);
            x.AddRange(obj2);

            var obj1Type = obj1.GetType().HasElementType ? obj1.GetType().GetElementType() : null;


            if (obj1Type != null) return x.ToArray(obj1Type == typeof(ConfigObject) ? typeof(ConfigObject) : obj1Type);
            try
            {
                return x.ToArray(typeof(string));
            }
            catch
            {
                return x.ToArray();
            }
        }
    }
    /// <summary>
    /// Get thrown if two types do not match and can't be merges
    /// </summary>	
    public class TypeMissmatchException : Exception
    {
    }
}
