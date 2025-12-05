// RIMAPI/Http/RequestParser.cs
using System;
using System.Net;

namespace RIMAPI.Http
{
    /// <summary>
    /// Parses and validates HTTP request parameters from HttpListenerContext.
    /// Provides static methods for request parameter extraction.
    /// </summary>
    public static class RequestParser
    {
        /// <summary>
        /// Gets the map ID from the request query parameters.
        /// </summary>
        /// <param name="context">The HTTP listener context.</param>
        /// <returns>The parsed map ID.</returns>
        /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
        public static int GetMapId(HttpListenerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return GetIntParameter(context, "map_id");
        }

        /// <summary>
        /// Gets an integer parameter from the request query string.
        /// </summary>
        /// <param name="context">The HTTP listener context.</param>
        /// <param name="parameterName">The name of the parameter to retrieve.</param>
        /// <returns>The parsed integer value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when context or parameterName is null.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown when the parameter is missing.</exception>
        /// <exception cref="ParameterFormatException">Thrown when the parameter has invalid format.</exception>
        public static int GetIntParameter(HttpListenerContext context, string parameterName)
        {
            ValidateContextAndParameterName(context, parameterName);

            string value = GetQueryStringValue(context, parameterName);

            if (!int.TryParse(value, out int result))
            {
                throw new ParameterFormatException(parameterName, value, typeof(int));
            }

            return result;
        }

        /// <summary>
        /// Gets a string parameter from the request query string.
        /// </summary>
        /// <param name="context">The HTTP listener context.</param>
        /// <param name="parameterName">The name of the parameter to retrieve.</param>
        /// <param name="required">Whether the parameter is required.</param>
        /// <returns>The string value, or null if not required and missing.</returns>
        /// <exception cref="ArgumentNullException">Thrown when context or parameterName is null.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown when the parameter is required but missing.</exception>
        public static string GetStringParameter(
            HttpListenerContext context,
            string parameterName,
            bool required = true
        )
        {
            string value = GetQueryStringValue(context, parameterName, required);

            if (required && string.IsNullOrEmpty(value))
            {
                throw new ParameterNotFoundException(parameterName);
            }

            return value;
        }

        /// <summary>
        /// Gets a boolean parameter from the request query string.
        /// </summary>
        /// <param name="context">The HTTP listener context.</param>
        /// <param name="parameterName">The name of the parameter to retrieve.</param>
        /// <param name="defaultValue">The default value if parameter is missing.</param>
        /// <returns>The parsed boolean value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when context or parameterName is null.</exception>
        /// <exception cref="ParameterFormatException">Thrown when the parameter has invalid format.</exception>
        public static bool GetBooleanParameter(
            HttpListenerContext context,
            string parameterName,
            bool defaultValue = false
        )
        {
            ValidateContextAndParameterName(context, parameterName);

            string value = GetQueryStringValue(context, parameterName, false);

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            if (bool.TryParse(value, out bool result))
                return result;

            // Support for "1"/"0" and "true"/"false" as case-insensitive
            if (
                value.Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            )
                return true;

            if (
                value.Equals("0", StringComparison.OrdinalIgnoreCase)
                || value.Equals("false", StringComparison.OrdinalIgnoreCase)
            )
                return false;

            throw new ParameterFormatException(parameterName, value, typeof(bool));
        }

        /// <summary>
        /// Gets a long parameter from the request query string.
        /// </summary>
        /// <param name="context">The HTTP listener context.</param>
        /// <param name="parameterName">The name of the parameter to retrieve.</param>
        /// <returns>The parsed long value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when context or parameterName is null.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown when the parameter is missing.</exception>
        /// <exception cref="ParameterFormatException">Thrown when the parameter has invalid format.</exception>
        public static long GetInt64Parameter(HttpListenerContext context, string parameterName)
        {
            ValidateContextAndParameterName(context, parameterName);

            string value = GetQueryStringValue(context, parameterName);

            if (!long.TryParse(value, out long result))
            {
                throw new ParameterFormatException(parameterName, value, typeof(long));
            }

            return result;
        }

        /// <summary>
        /// Gets a float parameter from the request query string.
        /// </summary>
        /// <param name="context">The HTTP listener context.</param>
        /// <param name="parameterName">The name of the parameter to retrieve.</param>
        /// <returns>The parsed float value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when context or parameterName is null.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown when the parameter is missing.</exception>
        /// <exception cref="ParameterFormatException">Thrown when the parameter has invalid format.</exception>
        public static float GetFloatParameter(HttpListenerContext context, string parameterName)
        {
            ValidateContextAndParameterName(context, parameterName);

            string value = GetQueryStringValue(context, parameterName);

            if (!float.TryParse(value, out float result))
            {
                throw new ParameterFormatException(parameterName, value, typeof(float));
            }

            return result;
        }

        /// <summary>
        /// Gets a double parameter from the request query string.
        /// </summary>
        /// <param name="context">The HTTP listener context.</param>
        /// <param name="parameterName">The name of the parameter to retrieve.</param>
        /// <returns>The parsed double value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when context or parameterName is null.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown when the parameter is missing.</exception>
        /// <exception cref="ParameterFormatException">Thrown when the parameter has invalid format.</exception>
        public static double GetDoubleParameter(HttpListenerContext context, string parameterName)
        {
            ValidateContextAndParameterName(context, parameterName);

            string value = GetQueryStringValue(context, parameterName);

            if (!double.TryParse(value, out double result))
            {
                throw new ParameterFormatException(parameterName, value, typeof(double));
            }

            return result;
        }

        /// <summary>
        /// Checks if a parameter exists in the query string.
        /// </summary>
        /// <param name="context">The HTTP listener context.</param>
        /// <param name="parameterName">The name of the parameter to check.</param>
        /// <returns>True if the parameter exists and is not empty.</returns>
        public static bool HasParameter(HttpListenerContext context, string parameterName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (parameterName == null)
                throw new ArgumentNullException(nameof(parameterName));

            var value = context.Request.QueryString[parameterName];
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Gets all query parameters as a dictionary.
        /// </summary>
        /// <param name="context">The HTTP listener context.</param>
        /// <returns>Dictionary of query parameters.</returns>
        public static System.Collections.Specialized.NameValueCollection GetAllParameters(
            HttpListenerContext context
        )
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Request.QueryString;
        }

        private static void ValidateContextAndParameterName(
            HttpListenerContext context,
            string parameterName
        )
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (parameterName == null)
                throw new ArgumentNullException(nameof(parameterName));
        }

        private static string GetQueryStringValue(
            HttpListenerContext context,
            string parameterName,
            bool throwIfMissing = true
        )
        {
            string value = context.Request.QueryString[parameterName];

            if (throwIfMissing && string.IsNullOrEmpty(value))
            {
                throw new ParameterNotFoundException(parameterName);
            }

            return value ?? string.Empty;
        }
    }

    /// <summary>
    /// Represents errors that occur when a required parameter is not found.
    /// </summary>
    public class ParameterNotFoundException : Exception
    {
        public string ParameterName { get; }

        public ParameterNotFoundException(string parameterName)
            : base($"Required parameter '{parameterName}' was not found or is empty.")
        {
            ParameterName = parameterName;
        }

        public ParameterNotFoundException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Represents errors that occur when a parameter has invalid format.
    /// </summary>
    public class ParameterFormatException : Exception
    {
        public string ParameterName { get; }
        public string ParameterValue { get; }
        public Type ExpectedType { get; }

        public ParameterFormatException(
            string parameterName,
            string parameterValue,
            Type expectedType
        )
            : base(
                $"Parameter '{parameterName}' with value '{parameterValue}' cannot be converted to {expectedType.Name}."
            )
        {
            ParameterName = parameterName;
            ParameterValue = parameterValue;
            ExpectedType = expectedType;
        }

        public ParameterFormatException(
            string parameterName,
            string parameterValue,
            Type expectedType,
            string message
        )
            : base(message)
        {
            ParameterName = parameterName;
            ParameterValue = parameterValue;
            ExpectedType = expectedType;
        }
    }
}
