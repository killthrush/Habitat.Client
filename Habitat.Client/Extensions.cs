namespace Habitat.Client
{
    /// <summary>
    /// Extension methods using reflection for manipulating expected methods not included in an interface.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Attempts to retrieve property value from object.  Swallows any exceptions.
        /// </summary>
        /// <typeparam name="T">Type of property expected.</typeparam>
        /// <param name="obj">Object that is expected to have target property.</param>
        /// <param name="propertyName">Name of property.</param>
        /// <param name="propertyValue">Out value of property if found.  default(T) on exception.</param>
        /// <returns>Returns true if value retrieved, false if property not found or type is incorrect.</returns>
        public static bool TryGetProperty<T>(this object obj, string propertyName, out T propertyValue)
        {
            try
            {
                var type = obj.GetType();
                var info = type.GetProperty(propertyName);
                propertyValue=  (T) info.GetValue(obj, null);
                return true;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            {
                // The whole point of a TryDoSomething method is to attempt an action and eat any exceptions
            }
            // ReSharper restore EmptyGeneralCatchClause
            propertyValue = default(T);
            return false;
        }

        /// <summary>
        /// Tries to set property on object.  Swallows any exceptions.
        /// </summary>
        /// <param name="obj">Object to set property on.</param>
        /// <param name="propertyName">Name of property.</param>
        /// <param name="value">Value to try to set.</param>
        /// <returns>True if property found and set.  False otherwise.</returns>
        public static bool TrySetProperty(this object obj, string propertyName, object value)
        {
            try
            {
                var type = obj.GetType();
                var info = type.GetProperty(propertyName);
                if (info != null)
                {
                    info.SetValue(obj, value, null);
                    return true;
                }
                
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            {
                // The whole point of a TryDoSomething method is to attempt an action and eat any exceptions
            }
            // ReSharper restore EmptyGeneralCatchClause
            return false;
        }
    }
}
