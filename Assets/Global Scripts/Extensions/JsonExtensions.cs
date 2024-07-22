using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace GlobalScripts.Extensions
{
    public static class JsonExtensions
    {
        public static void ReadBool(this JsonReader reader, out bool? value)
        {
            value = null;

            try
            {
                value = reader.ReadAsBoolean();
            }
            catch
            {
                Debug.LogError("Not An Integer!");
                return;
            }
        }

        public static void ReadInt(this JsonReader reader, out int? value)
        {
            value = null;

            try
            {
                value = reader.ReadAsInt32();
            }
            catch
            {
                Debug.LogError("Not An Integer!");
                return;
            }
        }

        public static void ReadFloat(this JsonReader reader, out float? value)
        {
            value = null;

            try
            {
                value = (float)reader.ReadAsDouble();
            }
            catch
            {
                Debug.LogError("Not An Integer!");
                return;
            }
        }

        public static void ReadString(this JsonReader reader, out string value)
        {
            value = null;

            try
            {
                value = reader.ReadAsString();
            }
            catch
            {
                Debug.LogError("Not An Integer!");
                return;
            }
        }

        public static bool ReadInts(this JsonReader reader, out int? x, out int? y)
        {
            x = y = 0;

            try
            {
                reader.ReadInt(out x);
                reader.ReadInt(out y);
                return true;
            }
            catch
            {
                Debug.LogError("Cannot read interger numbers!");
                return false;
            }
        }

        public static bool ReadInts(this JsonReader reader, out int? x, out int? y, out int? value)
        {
            x = y = value = 0;

            try
            {
                reader.ReadInt(out x);
                reader.ReadInt(out y);
                reader.ReadInt(out value);
                return true;
            }
            catch
            {
                Debug.LogError("Cannot read interger numbers!");
                return false;
            }
        }

        public static bool ReadInts(this JsonReader reader, out int? x, out int? y
            , out int? value1, out int? value2)
        {
            x = y = value1 = value2 = 0;

            try
            {
                reader.ReadInt(out x);
                reader.ReadInt(out y);
                reader.ReadInt(out value1);
                reader.ReadInt(out value2);
                return true;
            }
            catch
            {
                Debug.LogError("Cannot read interger numbers!");
                return false;
            }
        }

        public static bool ReadInts(this JsonReader reader, out int? x, out int? y
            , out int? value1, out int? value2, out int? value3)
        {
            x = y = value1 = value2 = value3 = 0;

            try
            {
                reader.ReadInt(out x);
                reader.ReadInt(out y);
                reader.ReadInt(out value1);
                reader.ReadInt(out value2);
                reader.ReadInt(out value3);
                return true;
            }
            catch
            {
                Debug.LogError("Cannot read interger numbers!");
                return false;
            }
        }

        public static bool ReadInts(this JsonReader reader, out int? x, out int? y
            , out int? value1, out int? value2, out int? value3, out int? value4)
        {
            x = y = value1 = value2 = value3 = value4 = 0;

            try
            {
                reader.ReadInt(out x);
                reader.ReadInt(out y);
                reader.ReadInt(out value1);
                reader.ReadInt(out value2);
                reader.ReadInt(out value3);
                reader.ReadInt(out value4);
                return true;
            }
            catch
            {
                Debug.LogError("Cannot read interger numbers!");
                return false;
            }
        }

        public static bool ReadInts(this JsonReader reader, out int? x, out int? y
            , out int? value1, out int? value2, out int? value3, out int? value4, out int? value5)
        {
            x = y = value1 = value2 = value3 = value4 = value5 = 0;

            try
            {
                reader.ReadInt(out x);
                reader.ReadInt(out y);
                reader.ReadInt(out value1);
                reader.ReadInt(out value2);
                reader.ReadInt(out value3);
                reader.ReadInt(out value4);
                reader.ReadInt(out value5);
                return true;
            }
            catch
            {
                Debug.LogError("Cannot read interger numbers!");
                return false;
            }
        }

        public static bool ReadInts(this JsonReader reader, out int? x, out int? y
            , out int? value1, out int? value2, out int? value3, out int? value4, out int? value5, out int? value6)
        {
            x = y = value1 = value2 = value3 = value4 = value5 = value6 = 0;

            try
            {
                reader.ReadInt(out x);
                reader.ReadInt(out y);
                reader.ReadInt(out value1);
                reader.ReadInt(out value2);
                reader.ReadInt(out value3);
                reader.ReadInt(out value4);
                reader.ReadInt(out value5);
                reader.ReadInt(out value6);
                return true;
            }
            catch
            {
                Debug.LogError("Cannot read interger numbers!");
                return false;
            }
        }
    }
}
