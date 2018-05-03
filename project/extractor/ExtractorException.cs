using System;

namespace BakaTsukiExtractor.extractor
{
    public class ExtractorException : Exception
    {
        public ExtractorException(string message) : base(message) { }
    }
}
