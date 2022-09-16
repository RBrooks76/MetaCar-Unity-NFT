using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Global : MonoBehaviour
{
    public static string JsonToString(string target, string s)
    {

        string[] newString = Regex.Split(target, s);

        return newString[1];

    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        /*
        public static void accessData(JSONObject obj)
        {
            switch (obj.type)
            {
                case JSONObject.Type.OBJECT:
                    for (int i = 0; i < obj.list.Count; i++)
                    {
                        string key = (string)obj.keys[i];
                        JSONObject j = (JSONObject)obj.list[i];
                        Debug.Log(key);
                        accessData(j);
                    }
                    break;
                case JSONObject.Type.ARRAY:
                    foreach (JSONObject j in obj.list)
                    {
                        accessData(j);
                    }
                    break;
                case JSONObject.Type.STRING:
                    Debug.Log(obj.str);
                    break;
                case JSONObject.Type.NUMBER:
                    Debug.Log(obj.n);
                    break;
                case JSONObject.Type.BOOL:
                    Debug.Log(obj.b);
                    break;
                case JSONObject.Type.NULL:
                    Debug.Log("NULL");
                    break;

            }
        }

        */

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }    
}
