using System.Collections;
using UnityEngine;

namespace Utilities
{
    public static class HelperUtilities
    {
        public static bool ValidateCheckEmptyString(Object anObject, string fieldName, string value)
        {
            if (!string.IsNullOrWhiteSpace(value)) return false;
            
            Debug.Log(fieldName + " is empty and must contain a value in object " + anObject.name);
            
            return true;
        }

        public static bool ValidateCheckEnumerableValues(Object anObject, string fieldName, IEnumerable objectsToCheck)
        {
            var error = false;
            var count = 0;

            foreach (object obj in objectsToCheck)
            {
                if (obj == null)
                {
                    Debug.Log(fieldName + " has null values in object " + anObject.name);
                    error = true;
                }
                else
                    count++;
            }

            if (count != 0) return error;
            Debug.Log(fieldName + " has no values in object " + anObject.name);

            return true;
        }
    }
}