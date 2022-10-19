using System.Reflection;

namespace CooperativeWordGuess.Words
{
    public class Words
    {
        // Use a HashSet for word list storage because lookup (ie spell checks, frequent!) will be O(1).
        // Generating a random word (from randmom index 1..max) will be O(N) but less frequent, and N~1000 anyway.
        private readonly Dictionary<int, HashSet<string>> _words;
        private readonly Random _random = new();

        public Words()
        {
            _words = new(GetWordLists(3, 8));
        }

        public bool IsKnown(string word)
        {
            return _words.TryGetValue(word?.Length ?? 0, out var wordList) && wordList.Contains(word!);
        }

        public string GetRandom(int length)
        {
            var wordList = _words[length];
            var index = _random.Next(wordList.Count);
            return wordList.ElementAt(index);
        }

        private IEnumerable<KeyValuePair<int, HashSet<string>>> GetWordLists(int length1, int length2)
        {
            for (var length = length1; length <= length2; length++)
                yield return new(length, GetWordList(length));
        }

        private HashSet<string> GetWordList(int length)
        {
            var resourceName = $"{typeof(Words).Namespace}.Length{length}.txt";
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var contents = reader.ReadToEnd();
            var words = contents.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var badLength = words.FirstOrDefault(w => w.Length != length);
            if (badLength != null) throw new InvalidDataException($"Word not of length {length}: {badLength}");
            return new HashSet<string>(words, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
