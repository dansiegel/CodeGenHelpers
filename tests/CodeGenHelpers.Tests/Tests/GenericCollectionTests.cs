using Xunit;

namespace CodeGenHelpers.Tests
{
    public class GenericCollectionTests
    {
        [Fact]
        public void GeneratesNoOutput()
        {
            var collection = new GenericCollection();

            Assert.Equal(string.Empty, collection.ToString());
        }

        [Fact]
        public void GeneratesSingleGenericOutput()
        {
            var collection = new GenericCollection();
            collection.Add(new GenericBuilder("T"));

            Assert.Equal("<T>", collection.ToString());
        }

        [Fact]
        public void GeneratesGenericListOutput()
        {
            var collection = new GenericCollection();
            collection.Add(new GenericBuilder("TKey"));
            collection.Add(new GenericBuilder("TValue"));

            Assert.Equal("<TKey, TValue>", collection.ToString());
        }
    }
}
