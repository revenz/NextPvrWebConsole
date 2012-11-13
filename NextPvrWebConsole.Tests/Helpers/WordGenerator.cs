using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextPvrWebConsole.Tests.Helpers
{
    /// <summary>
    /// A Generator that generates random words, paragraphs, or character sequences
    /// </summary>
    public class WordGenerator
    {
        /// <summary>
        /// The characters to use
        /// </summary>
        public enum CharacterSet
        {
            /// <summary>
            /// Digits 0,1,2,3,4,5,6,7,8,9
            /// </summary>
            Digits = 1,
            /// <summary>
            /// Lower case letters a through z
            /// </summary>
            LowerCase = 2,
            /// <summary>
            /// Upper case letters A through Z
            /// </summary>
            UpperCase = 4,
            /// <summary>
            /// Spaces [ ]
            /// </summary>
            Space = 8,
            /// <summary>
            /// Hex numbers 0,1,2,3,4,5,7,8,9,A,B,C,D,E,F
            /// </summary>
            Hex = 16
        }
        private static Random random = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// Generates a random sequence of characters
        /// </summary>
        /// <param name="NumberOfCharacters">the number of characters to generate</param>
        /// <param name="CharacterSet">the set of characters allowed to be used</param>
        /// <param name="SpecialCharacters">any special characters to use as well</param>
        /// <returns>a random sequence of characters</returns>
        public static string GetSequence(int NumberOfCharacters, CharacterSet CharacterSet = CharacterSet.Digits | CharacterSet.LowerCase | CharacterSet.UpperCase, string[] SpecialCharacters = null)
        {
            return GetSequence(NumberOfCharacters, NumberOfCharacters + 1, CharacterSet, SpecialCharacters);
        }
        /// <summary>
        /// Generates a random sequence of characters whose length will be somewhere between the minimum and maximum number allowed (inclusive)
        /// </summary>
        /// <param name="MininumCharacters">the minimum number of characters allowed</param>
        /// <param name="MaximumCharacters">the maximum number of characters allowed (inclusive, so can be this long)</param>
        /// <param name="CharacterSet">the set of characters allowed to be used</param>
        /// <param name="SpecialCharacters">any special characters to use as well</param>
        /// <returns>a random sequence of characters</returns>
        public static string GetSequence(int MininumCharacters, int MaximumCharacters, CharacterSet CharacterSet = CharacterSet.Digits | CharacterSet.LowerCase | CharacterSet.UpperCase, string[] SpecialCharacters = null)
        {
            List<string> chars = new List<string>();
            if ((CharacterSet & WordGenerator.CharacterSet.LowerCase) == WordGenerator.CharacterSet.LowerCase)
            {
                for (char c = 'a'; c <= 'z'; c++)
                    chars.Add(c.ToString());
            }
            if ((CharacterSet & WordGenerator.CharacterSet.UpperCase) == WordGenerator.CharacterSet.UpperCase)
            {
                for (char c = 'A'; c <= 'Z'; c++)
                    chars.Add(c.ToString());
            }
            if ((CharacterSet & WordGenerator.CharacterSet.Digits) == WordGenerator.CharacterSet.Digits)
            {
                for (int i = 0; i < 10; i++)
                    chars.Add(i.ToString());
            }
            if ((CharacterSet & WordGenerator.CharacterSet.UpperCase) == WordGenerator.CharacterSet.UpperCase)
            {
                for (int i = 0; i < 10; i++)
                    chars.Add(i.ToString());
                for (char c = 'A'; c <= 'F'; c++)
                    chars.Add(c.ToString());
            }
            if (SpecialCharacters != null)
                chars.AddRange(SpecialCharacters);

            string result = "";
            int numChars = random.Next(MininumCharacters, MaximumCharacters);
            for (int i = 0; i < numChars; i++)
                result += chars[random.Next(0, chars.Count)];
            return result;
        }

        // #c1spell(off) // suspend spell checking
        #region lorem ipsum words
        private static readonly string[] words = new string[] { "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod",
    "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua",
    "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita",
    "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet",
    "lorem", "ipsum", "dolor", "sit", "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod",
    "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua",
    "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita",
    "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet",
    "lorem", "ipsum", "dolor", "sit", "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod",
    "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua",
    "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita",
    "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet", "duis",
    "autem", "vel", "eum", "iriure", "dolor", "in", "hendrerit", "in", "vulputate", "velit", "esse", "molestie",
    "consequat", "vel", "illum", "dolore", "eu", "feugiat", "nulla", "facilisis", "at", "vero", "eros", "et",
    "accumsan", "et", "iusto", "odio", "dignissim", "qui", "blandit", "praesent", "luptatum", "zzril", "delenit",
    "augue", "duis", "dolore", "te", "feugait", "nulla", "facilisi", "lorem", "ipsum", "dolor", "sit", "amet",
    "consectetuer", "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet",
    "dolore", "magna", "aliquam", "erat", "volutpat", "ut", "wisi", "enim", "ad", "minim", "veniam", "quis",
    "nostrud", "exerci", "tation", "ullamcorper", "suscipit", "lobortis", "nisl", "ut", "aliquip", "ex", "ea",
    "commodo", "consequat", "duis", "autem", "vel", "eum", "iriure", "dolor", "in", "hendrerit", "in", "vulputate",
    "velit", "esse", "molestie", "consequat", "vel", "illum", "dolore", "eu", "feugiat", "nulla", "facilisis", "at",
    "vero", "eros", "et", "accumsan", "et", "iusto", "odio", "dignissim", "qui", "blandit", "praesent", "luptatum",
    "zzril", "delenit", "augue", "duis", "dolore", "te", "feugait", "nulla", "facilisi", "nam", "liber", "tempor",
    "cum", "soluta", "nobis", "eleifend", "option", "congue", "nihil", "imperdiet", "doming", "id", "quod", "mazim",
    "placerat", "facer", "possim", "assum", "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer", "adipiscing",
    "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam",
    "erat", "volutpat", "ut", "wisi", "enim", "ad", "minim", "veniam", "quis", "nostrud", "exerci", "tation",
    "ullamcorper", "suscipit", "lobortis", "nisl", "ut", "aliquip", "ex", "ea", "commodo", "consequat", "duis",
    "autem", "vel", "eum", "iriure", "dolor", "in", "hendrerit", "in", "vulputate", "velit", "esse", "molestie",
    "consequat", "vel", "illum", "dolore", "eu", "feugiat", "nulla", "facilisis", "at", "vero", "eos", "et", "accusam",
    "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita", "kasd", "gubergren", "no", "sea",
    "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet", "lorem", "ipsum", "dolor", "sit",
    "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod", "tempor", "invidunt", "ut",
    "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua", "at", "vero", "eos", "et",
    "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita", "kasd", "gubergren", "no",
    "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet", "lorem", "ipsum", "dolor", "sit",
    "amet", "consetetur", "sadipscing", "elitr", "at", "accusam", "aliquyam", "diam", "diam", "dolore", "dolores",
    "duo", "eirmod", "eos", "erat", "et", "nonumy", "sed", "tempor", "et", "et", "invidunt", "justo", "labore",
    "stet", "clita", "ea", "et", "gubergren", "kasd", "magna", "no", "rebum", "sanctus", "sea", "sed", "takimata",
    "ut", "vero", "voluptua", "est", "lorem", "ipsum", "dolor", "sit", "amet", "lorem", "ipsum", "dolor", "sit",
    "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod", "tempor", "invidunt", "ut",
    "labore", "et", "dolore", "magna", "aliquyam", "erat", "consetetur", "sadipscing", "elitr", "sed", "diam",
    "nonumy", "eirmod", "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed",
    "diam", "voluptua", "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea",
    "rebum", "stet", "clita", "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum" };
        #endregion
        // #c1spell(on) // resume spell checking

        /// <summary>
        /// Gets the number of Lorem Ipsum words specified
        /// </summary>
        /// <param name="NumWords">the number of words to generate</param>
        /// <param name="MaxWords">if this is present NumWords become the minimum and max is the maximum and a random number of words between the two will be returned</param>
        /// <returns></returns>
        public static string GetWords(int NumWords, int MaxWords = 0)
        {
            StringBuilder Result = new StringBuilder();

            if (MaxWords > NumWords)
                NumWords = random.Next(NumWords, MaxWords);

            for (int i = 0; i < NumWords; i++)
            {
                Result.Append(words[random.Next(words.Length)] + (i < NumWords - 1 ? " " : ""));
            }

            Result.Append(".");
            return Result.ToString();
        }

        /// <summary>
        /// Generates a specified number of paragraphs with random content
        /// </summary>
        /// <param name="Paragraphs">the number of paragraphs to generate</param>
        /// <returns>the specified number of paragraphs with random content</returns>
        public static string GetParagraphs(int Paragraphs = 1)
        {
            string[] results = new string[Paragraphs];
            for (int i = 0; i < Paragraphs; i++)
            {
                results[i] = GetWords(random.Next(50, 200));
            }
            return String.Join("\n\n", results);
        }

        /// <summary>
        /// Gets a randomly generated email address
        /// </summary>
        /// <param name="Domain">[optional] the domain name to use in the email address</param>
        /// <returns>a randomly generated email address</returns>
        public static string GetEmailAddress(string Domain = null, string DomainSuffix = "test")
        {
            return String.Format("{0}@{1}.{2}", GetSequence(3, 10, CharacterSet.LowerCase), GetSequence(3, 10, CharacterSet.LowerCase), DomainSuffix);
        }
    }
}
