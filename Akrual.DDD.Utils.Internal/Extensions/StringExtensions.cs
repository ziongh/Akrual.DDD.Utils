using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Akrual.DDD.Utils.Internal.Extensions
{
     /// <summary>
    /// Extension methods for String class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Adds a char to end of given string if it does not ends with the char.
        /// </summary>
        public static string EnsureEndsWith(this string str, char c)
        {
            return EnsureEndsWith(str, c, StringComparison.Ordinal);
        }

        /// <summary>
        /// Adds a char to end of given string if it does not ends with the char.
        /// </summary>
        public static string EnsureEndsWith(this string str, char c, StringComparison comparisonType)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.EndsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return str + c;
        }

        /// <summary>
        /// Adds a char to end of given string if it does not ends with the char.
        /// </summary>
        public static string EnsureEndsWith(this string str, char c, bool ignoreCase, CultureInfo culture)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.EndsWith(c.ToString(culture), ignoreCase, culture))
            {
                return str;
            }

            return str + c;
        }

        /// <summary>
        /// Adds a char to beginning of given string if it does not starts with the char.
        /// </summary>
        public static string EnsureStartsWith(this string str, char c)
        {
            return EnsureStartsWith(str, c, StringComparison.Ordinal);
        }

        /// <summary>
        /// Adds a char to beginning of given string if it does not starts with the char.
        /// </summary>
        public static string EnsureStartsWith(this string str, char c, StringComparison comparisonType)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.StartsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return c + str;
        }

        /// <summary>
        /// Adds a char to beginning of given string if it does not starts with the char.
        /// </summary>
        public static string EnsureStartsWith(this string str, char c, bool ignoreCase, CultureInfo culture)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            if (str.StartsWith(c.ToString(culture), ignoreCase, culture))
            {
                return str;
            }

            return c + str;
        }

        /// <summary>
        /// Indicates whether this string is null or an System.String.Empty string.
        /// </summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// indicates whether this string is null, empty, or consists only of white-space characters.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
        public static string Left(this string str, int len)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            if (str.Length < len)
            {
                throw new ArgumentException("len argument can not be bigger than given string's length!");
            }

            return str.Substring(0, len);
        }

        /// <summary>
        /// Converts line endings in the string to <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string NormalizeLineEndings(this string str)
        {
            return str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Gets index of nth occurence of a char in a string.
        /// </summary>
        /// <param name="str">source string to be searched</param>
        /// <param name="c">Char to search in <see cref="str"/></param>
        /// <param name="n">Count of the occurence</param>
        public static int NthIndexOf(this string str, char c, int n)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            var count = 0;
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] != c)
                {
                    continue;
                }

                if ((++count) == n)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes first occurrence of the given postfixes from end of the given string.
        /// Ordering is important. If one of the postFixes is matched, others will not be tested.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="postFixes">one or more postfix.</param>
        /// <returns>Modified string or the same string if it has not any of given postfixes</returns>
        public static string RemovePostFix(this string str, params string[] postFixes)
        {
            if (str == null)
            {
                return null;
            }

            if (str == string.Empty)
            {
                return string.Empty;
            }

            if (postFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (var postFix in postFixes)
            {
                if (str.EndsWith(postFix))
                {
                    return str.Left(str.Length - postFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// Removes first occurrence of the given prefixes from beginning of the given string.
        /// Ordering is important. If one of the preFixes is matched, others will not be tested.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="preFixes">one or more prefix.</param>
        /// <returns>Modified string or the same string if it has not any of given prefixes</returns>
        public static string RemovePreFix(this string str, params string[] preFixes)
        {
            if (str == null)
            {
                return null;
            }

            if (str == string.Empty)
            {
                return string.Empty;
            }

            if (preFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (var preFix in preFixes)
            {
                if (str.StartsWith(preFix))
                {
                    return str.Right(str.Length - preFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// Gets a substring of a string from end of the string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
        public static string Right(this string str, int len)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            if (str.Length < len)
            {
                throw new ArgumentException("len argument can not be bigger than given string's length!");
            }

            return str.Substring(str.Length - len, len);
        }

        /// <summary>
        /// Uses string.Split method to split given string by given separator.
        /// </summary>
        public static string[] Split(this string str, string separator)
        {
            return str.Split(new[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// Uses string.Split method to split given string by given separator.
        /// </summary>
        public static string[] Split(this string str, string separator, StringSplitOptions options)
        {
            return str.Split(new[] { separator }, options);
        }

        /// <summary>
        /// Uses string.Split method to split given string by <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string[] SplitToLines(this string str)
        {
            return str.Split(Environment.NewLine);
        }

        /// <summary>
        /// Uses string.Split method to split given string by <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string[] SplitToLines(this string str, StringSplitOptions options)
        {
            return str.Split(Environment.NewLine, options);
        }

        /// <summary>
        /// Converts PascalCase string to camelCase string.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="invariantCulture">Invariant culture</param>
        /// <returns>camelCase of the string</returns>
        public static string ToCamelCase(this string str, bool invariantCulture = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return invariantCulture ? str.ToLowerInvariant() : str.ToLower();
            }

            return (invariantCulture ? char.ToLowerInvariant(str[0]) : char.ToLower(str[0])) + str.Substring(1);
        }

        /// <summary>
        /// Converts PascalCase string to camelCase string in specified culture.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="culture">An object that supplies culture-specific casing rules</param>
        /// <returns>camelCase of the string</returns>
        public static string ToCamelCase(this string str, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return str.ToLower(culture);
            }

            return char.ToLower(str[0], culture) + str.Substring(1);
        }

        /// <summary>
        /// Converts given PascalCase/camelCase string to sentence (by splitting words by space).
        /// Example: "ThisIsSampleSentence" is converted to "This is a sample sentence".
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="invariantCulture">Invariant culture</param>
        public static string ToSentenceCase(this string str, bool invariantCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return Regex.Replace(
                str,
                "[a-z][A-Z]",
                m => m.Value[0] + " " + (invariantCulture ? char.ToLowerInvariant(m.Value[1]) : char.ToLower(m.Value[1]))
            );
        }

        /// <summary>
        /// Converts given PascalCase/camelCase string to sentence (by splitting words by space).
        /// Example: "ThisIsSampleSentence" is converted to "This is a sample sentence".
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="culture">An object that supplies culture-specific casing rules.</param>
        public static string ToSentenceCase(this string str, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1], culture));
        }

        /// <summary>
        /// Converts string to enum value.
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="value">String value to convert</param>
        /// <returns>Returns enum object</returns>
        public static T ToEnum<T>(this string value)
            where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// Converts string to enum value.
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="value">String value to convert</param>
        /// <param name="ignoreCase">Ignore case</param>
        /// <returns>Returns enum object</returns>
        public static T ToEnum<T>(this string value, bool ignoreCase)
            where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        public static string ToMd5(this string str)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(str);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Converts camelCase string to PascalCase string.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="invariantCulture">Invariant culture</param>
        /// <returns>PascalCase of the string</returns>
        public static string ToPascalCase(this string str, bool invariantCulture = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return invariantCulture ? str.ToUpperInvariant(): str.ToUpper();
            }

            return (invariantCulture ? char.ToUpperInvariant(str[0]) : char.ToUpper(str[0])) + str.Substring(1);
        }

        /// <summary>
        /// Converts camelCase string to PascalCase string in specified culture.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="culture">An object that supplies culture-specific casing rules</param>
        /// <returns>PascalCase of the string</returns>
        public static string ToPascalCase(this string str, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return str.ToUpper(culture);
            }

            return char.ToUpper(str[0], culture) + str.Substring(1);
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string Truncate(this string str, int maxLength)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            return str.Left(maxLength);
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
        /// It adds a "..." postfix to end of the string if it's truncated.
        /// Returning string can not be longer than maxLength.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string TruncateWithPostfix(this string str, int maxLength)
        {
            return TruncateWithPostfix(str, maxLength, "...");
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
        /// It adds given <paramref name="postfix"/> to end of the string if it's truncated.
        /// Returning string can not be longer than maxLength.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string TruncateWithPostfix(this string str, int maxLength, string postfix)
        {
            if (str == null)
            {
                return null;
            }

            if (str == string.Empty || maxLength == 0)
            {
                return string.Empty;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            if (maxLength <= postfix.Length)
            {
                return postfix.Left(maxLength);
            }

            return str.Left(maxLength - postfix.Length) + postfix;
        }
        
        public static string ToURLSlug(this string s)
        {
            return Regex.Replace(s, @"[^a-z0-9]+", "-", RegexOptions.IgnoreCase)
                .Trim(new char[] { '-' })
                .ToLower();
        }
        public static bool IsNullOrEmptyOrWhiteSpace(this string value, params char[] moreIgnoredCharacters)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            var ignore = moreIgnoredCharacters?.Concat(new char[] { ' ' }) ?? new char[] { ' ' };

            var temp = string.Join(string.Empty, value.ToCharArray().Where(a => !ignore.Contains(a)).ToArray());

            return temp.Length == 0;
        }
        
        
        

        public static string RemoverAcentos(this string texto)
	    {
	        string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
	        string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

	        for (int i = 0; i < comAcentos.Length; i++)
	        {
	            texto = texto.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());
	        }
	        return texto;
	    }

        
        public static string ToCodigoAtivo(this string number, CodigoType docType = CodigoType.Any)
        {
            if (string.IsNullOrEmpty(number)) return null;
            var onlyLiterals = Regex.Replace(number, "[^a-zA-Z0-9]", string.Empty);

            if (docType == CodigoType.Any)
            {
                if (onlyLiterals.Length > 10)
                {
                    // is Bolsa or ISIN
                    if (onlyLiterals.Length < 12)
                    {
                        onlyLiterals = onlyLiterals.PadLeft(12, '0');
                    }

                    if (onlyLiterals.Length > 12)
                    {
                        onlyLiterals = onlyLiterals.Substring(0, 12);

                    }
                }
                else
                {
                    // is CETIP
                    if (onlyLiterals.Length < 10)
                    {
                        onlyLiterals = onlyLiterals.PadLeft(10, '0');
                    }
                }
            }
            else if (docType == CodigoType.CodigoCETIP)
            {
                // is CETIP
                if (onlyLiterals.Length < 10)
                {
                    onlyLiterals = onlyLiterals.PadLeft(10, '0');
                }

                if (onlyLiterals.Length > 10)
                {
                    onlyLiterals = onlyLiterals.Substring(0, 10);
                }
            }
            else
            {
                // is Bolsa or ISIN
                if (onlyLiterals.Length < 12)
                {
                    onlyLiterals = onlyLiterals.PadLeft(12, '0');
                }

                if (onlyLiterals.Length > 14)
                {
                    onlyLiterals = onlyLiterals.Substring(0, 12);

                }
            }



            return onlyLiterals;
        }
        
        
        static string RemoveDiacritics(this string text)
		{
			var normalizedString = text.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			foreach (var c in normalizedString)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}

			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}

		public static int DamerauLevenshteinDistanceTo(this string @string, string targetString)
		{
			return DamerauLevenshteinDistance(@string, targetString);
		}
        
		public static bool IsSimilarTo(this string string1, string string2, bool onlyLowerAndTrim = false, double closing = 1.1, HashSet<string> ignoredWords = null)
		{
		    if (ignoredWords == null)
		    {
		        ignoredWords = new HashSet<string>()
		        {
		            "cri","cra","fidc","cci","ri","bid"
		        };
		    }
		    
			string1 = string1.Replace("-", " ").Replace("_", " ").Replace("  ", " ").Replace("  ", " ").ToLower().RemoveDiacritics().Trim();
			string2 = string2.Replace("-", " ").Replace("_", " ").Replace("  ", " ").Replace("  ", " ").ToLower().RemoveDiacritics().Trim();

		    string1 = RemoverAcentos(string1);
		    string2 = RemoverAcentos(string2);

            if (string1.Equals(string2)) { return true; }

			if (onlyLowerAndTrim)
			{
				return false;
			}

			var collection1 = string1.Split(' ');
			var wordCounterString1 = collection1.Length;

			var collection2 = string2.Split(' ');
			var wordCounterString2 = collection2.Length;

			var ignoredWordsFromString1 = collection1.Intersect(ignoredWords).ToList();

			string processedstring1 = string1;
			string processedstring2 = string2;

			if (ignoredWordsFromString1.Any())
			{
				var ignoredWordsFromString2 = collection2.Intersect(ignoredWordsFromString1).ToList();

				if (ignoredWordsFromString2.Count() == ignoredWordsFromString1.Count())
				{
					processedstring1 = collection1.Except(ignoredWordsFromString1)
						.Aggregate(string.Empty, (current, temp) => temp + " " + current);

					processedstring2 = collection2.Except(ignoredWordsFromString2)
						.Aggregate(string.Empty, (current, temp) => temp + " " + current);
				}
				else
				{
					return false;
				}
			}

			double distance = DamerauLevenshteinDistance(processedstring1, processedstring2) / ((wordCounterString1 + wordCounterString2) / 2.0);

			return distance <= closing ? true : false;
		}

	    public static string TruncateToSpecificSize(this string value, int maxLength)
	    {
	        if (value == null) return string.Empty;
	        if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
	    }


	    public static string ToNumberOnly(this string number)
	    {
	        if (number == null) return string.Empty;
	        var onlynumbers = Regex.Replace(number, "[^0-9]", string.Empty);
	        return onlynumbers;
	    }
	    public static string ToLiteralsOnly(this string number)
	    {
	        if (number == null) return string.Empty;
            var onlyLiterals = Regex.Replace(number, "[^a-zA-Z0-9]", string.Empty);
            return onlyLiterals;
	    }
        
        
	    public static string FormatToCNPJOrCPF(this string cnpjCpf, DocType docType = DocType.Any)
	    {
	        var cnpjOrCPFNumberOnly = cnpjCpf.ToCNPJCPFWithoutSpecialCaracter(docType);
	        if (cnpjOrCPFNumberOnly.Length == 14)
	        {
	            // is CNPJ
	            //00.000.000/0000-00
	            var cnpj = string.Format(
	                "{0}.{1}.{2}/{3}-{4}",
	                cnpjOrCPFNumberOnly.Substring(0, 2),
	                cnpjOrCPFNumberOnly.Substring(2, 3),
	                cnpjOrCPFNumberOnly.Substring(5, 3),
	                cnpjOrCPFNumberOnly.Substring(8, 4),
	                cnpjOrCPFNumberOnly.Substring(12, 2));
	            return cnpj;

	        }
	        else if (cnpjOrCPFNumberOnly.Length == 11)
	        {
	            // is CPF
	            //000.000.000-00
	            var cpf = string.Format(
	                "{0}.{1}.{2}-{3}",
	                cnpjOrCPFNumberOnly.Substring(0, 3),
	                cnpjOrCPFNumberOnly.Substring(3, 3),
	                cnpjOrCPFNumberOnly.Substring(6, 3),
	                cnpjOrCPFNumberOnly.Substring(9, 2));
	            return cpf;
	        }
	        else
	        {
	            throw new ApplicationException("CPF or CNPJ is invalid (is not 11 characters neigther 14)");
	        }
	    }

	    public static string ToCNPJCPFWithoutSpecialCaracter(this string cnpjCpf, DocType docType = DocType.Any)
        {
            var onlynumbers = cnpjCpf.ToNumberOnly();
            if (docType == DocType.Any)
            {
                if (onlynumbers.Length > 11)
                {
                    // is CNPJ
                    if (onlynumbers.Length < 14)
                    {
                        onlynumbers = onlynumbers.PadLeft(14, '0');
                    }

                    if (onlynumbers.Length > 14)
                    {
                        onlynumbers = onlynumbers.Substring(0, 14);

                    }
                }
                else
                {
                    // is CPF
                    if (onlynumbers.Length < 11)
                    {
                        onlynumbers = onlynumbers.PadLeft(11, '0');
                    }
                    if (onlynumbers.Length > 11)
                    {
                        onlynumbers = onlynumbers.Substring(0, 11);
                    }
                }
            }
            else if (docType == DocType.CNPJ)
            {
                // is CNPJ
                if (onlynumbers.Length < 14)
                {
                    onlynumbers = onlynumbers.PadLeft(14, '0');
                }

                if (onlynumbers.Length > 14)
                {
                    onlynumbers = onlynumbers.Substring(0, 14);

                }
            }
            else if (docType == DocType.CPF)
            {
                // is CPF
                if (onlynumbers.Length < 11)
                {
                    onlynumbers = onlynumbers.PadLeft(11, '0');
                }
            }
        
	        return onlynumbers;
	    }

		public static int DamerauLevenshteinDistance(string string1, string string2)
		{
			if (String.IsNullOrEmpty(string1))
			{
				if (!String.IsNullOrEmpty(string2))
					return string2.Length;

				return 0;
			}

			if (String.IsNullOrEmpty(string2))
			{
				if (!String.IsNullOrEmpty(string1))
					return string1.Length;

				return 0;
			}

			int length1 = string1.Length;
			int length2 = string2.Length;

			int[,] d = new int[length1 + 1, length2 + 1];

			int cost, del, ins, sub;

			for (int i = 0; i <= d.GetUpperBound(0); i++)
				d[i, 0] = i;

			for (int i = 0; i <= d.GetUpperBound(1); i++)
				d[0, i] = i;

			for (int i = 1; i <= d.GetUpperBound(0); i++)
			{
				for (int j = 1; j <= d.GetUpperBound(1); j++)
				{
					if (string1[i - 1] == string2[j - 1])
						cost = 0;
					else
						cost = 1;

					del = d[i - 1, j] + 1;
					ins = d[i, j - 1] + 1;
					sub = d[i - 1, j - 1] + cost;

					d[i, j] = Math.Min(del, Math.Min(ins, sub));

					if (i > 1 && j > 1 && string1[i - 1] == string2[j - 2] && string1[i - 2] == string2[j - 1])
						d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
				}
			}

			return d[d.GetUpperBound(0), d.GetUpperBound(1)];
		}
    }
    
    public enum DocType
    {
        Any,
        CPF,
        CNPJ
    }
    public enum CodigoType
    {
        Bolsa,
        ISIN,
        CodigoCETIP,
        Any
    }
}