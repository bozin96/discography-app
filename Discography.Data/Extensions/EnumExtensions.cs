using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Discography.Data.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        ///     A generic extension method that aids in reflecting 
        ///     and retrieving any attribute that is applied to an `Enum`.
        /// </summary>
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
                where TAttribute : Attribute
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<TAttribute>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetNameAttribute(this Enum enumValue)
        {
            // This will throw an exception if the document type is null
            // This can happen if you do an AML screen via api/v1/amlscreen
            return enumValue.GetAttribute<DisplayAttribute>().Name;
        }


        /// <summary>
        /// Returns all flags from enum flags input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<Enum> GetFlags(this Enum input)
        {
            foreach (Enum value in Enum.GetValues(input.GetType()))
                if (input.HasFlag(value))
                    yield return value;
        }

        /// <summary>
        /// Returns flags as string separated with '|' character
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetFlagsAsString(this Enum input, string delimiter = "|")
        {
            List<string> parts = new List<string>();
            foreach (Enum value in Enum.GetValues(input.GetType()))
                if (input.HasFlag(value) && value.GetNameAttribute() != "Empty")
                    parts.Add(value.GetNameAttribute());

            return string.Join(delimiter, parts);
        }

        /// <summary>
        ///  Returns flags as list of strings
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> GetFlagsAsListOfStrings(this Enum input)
        {
            List<string> parts = new List<string>();
            foreach (Enum value in Enum.GetValues(input.GetType()))
                if (input.HasFlag(value) && value.GetNameAttribute() != "Empty")
                    parts.Add(value.GetNameAttribute());

            return parts;
        }

        /// <summary>
        /// Returns Enum flags value from list of strings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static T GetFlagsValueFromStringList<T>(List<string> strings) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            if (strings == null)
                return default;

            T flags;
            List<string> enumFlagsAsList = new List<string>();
            foreach (var stringValue in strings)
                foreach (Enum enumValue in Enum.GetValues(typeof(T)))
                    if (enumValue.GetNameAttribute() == stringValue)
                        enumFlagsAsList.Add(enumValue.ToString());

            // enumFlagsAsList.RemoveAll(c => !Enum.TryParse(c, true, out flags));
            var commaSeparatedFlags = string.Join(",", enumFlagsAsList);
            Enum.TryParse(commaSeparatedFlags, true, out flags);
            return flags;
        }

        /// <summary>
        /// Returns list of enums generated from input strings based on name attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static List<T> GetEnumListFromStringList<T>(List<string> strings) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            List<T> enumsList = new List<T>();
            foreach (var stringValue in strings)
                foreach (Enum enumValue in Enum.GetValues(typeof(T)))
                    if (enumValue.GetNameAttribute() == stringValue)
                    {
                        enumsList.Add((T)(object)enumValue);
                        break;
                    }
            return enumsList;
        }

        /// <summary>
        /// Return Enum value from Name Attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumNameValue"></param>
        /// <returns></returns>
        public static T GetValueFromName<T>(string enumNameValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            foreach (Enum enumValue in Enum.GetValues(typeof(T)))
                if (enumValue.GetNameAttribute().ToLower() == enumNameValue.ToLower())
                    return (T)(object)enumValue;

            return default;

        }
    }
}
