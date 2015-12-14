using System;
using System.Runtime.Serialization;

namespace CrawlerWeb
{
    [Serializable]
    internal class CrawlerDataError : Exception
    {
        public CrawlerDataError()
        {
        }

        public CrawlerDataError(string message) : base(message)
        {
        }

        public CrawlerDataError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CrawlerDataError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}