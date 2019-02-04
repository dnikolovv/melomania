using Optional;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Melomania.Utils
{
    public static class Extensions
    {
        /// <summary>
        /// Retrieves an enum's description.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns>The enum's description.</returns>
        public static string GetDescription(this Enum enumValue) =>
            enumValue
                .GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description;

        /// <summary>
        /// Replaces all sequences of whitespace characters with a single space. E.g. "asd     asd" becomes "asd asd".
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The string with sequential whitespaces removed.</returns>
        public static string RemoveSequentialWhitespaces(this string input) =>
            string.IsNullOrEmpty(input) ?
            input :
            Regex.Replace(input, @"\s+", " ");

        /// <summary>
        /// Rounds a number to the nearest 10. E.g. 16 -> 20, 23 -> 20, etc.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>The rounded number.</returns>
        public static int RoundToNearestTen(this double number) =>
            ((int)Math.Round(number / 10.0)) * 10;

        /// <summary>
        /// Takes a file name with or without an extension and adds/edits.
        /// E.g. "file-name", "mp3" becomes "file-name.mp3"
        ///      "file-name.avi", "mp3" becomes "file-name.mp3"
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="extension">The new extension.</param>
        /// <returns></returns>
        public static string SetExtension(this string fileName, string extension) =>
            Path.ChangeExtension(fileName, extension);

        public static async Task<Option<T, TExceptionResult>> MapExceptionAsync<T, TException, TExceptionResult>(
            this Task<Option<T, TException>> optionTask,
            Func<TException, TExceptionResult> mapping) =>
            (await optionTask).MapException(mapping);
    }
}