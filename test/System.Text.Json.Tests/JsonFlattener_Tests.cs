using CodeCapital.System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Tests.Models;
using Xunit;

namespace System.Text.Json.Tests
{
    public class JsonFlattener_Tests
    {
        private const string NullValue = "NULL";

        [Fact]
        public void Flatten_Returns_ArgumentNullException()
        {
            var serializer = new JsonFlattener();

            Assert.Throws<ArgumentNullException>(() => serializer.Flatten(null));
        }

        [Fact]
        public void Flatten_Returns_Exception_NoJson()
        {
            var serializer = new JsonFlattener();

            var exception = Assert.ThrowsAny<Exception>(() => serializer.Flatten(""));

            Assert.Contains("The input does not contain any JSON tokens", exception.Message);
        }

        [Fact]
        public void Flatten_Returns_ListOfTwoItems()
        {
            var serializer = new JsonFlattener();

            var items = new List<Person>
            {
                new Person("John", 18, DateTime.Now.AddYears(-18)),
                new Person("Lisa", 18, DateTime.Now.AddYears(-18).AddDays(1)),
                new Person("Barbara", 18, DateTime.Now.AddYears(-18).AddDays(-1)),
                new Person("Peter", 18, DateTime.Now.AddYears(-18).AddMinutes(5)),
                new Person("Carl", 23, DateTime.Now.AddYears(-23))
            };

            var result = serializer.Flatten(JsonSerializer.Serialize(items));

            Assert.Equal(5, result?.Count);
            Assert.Equal("John", result?[0].Name);
            Assert.Equal("18", result?[0].Age);
            Assert.Equal(NullValue, result?[0].MyAddress);
            Assert.Equal("[\"Red\",\"Blue\"]", result?[0].Colours);
        }

        //ToDo Change to tests
        public static void TestJson()
        {
            var people = new List<Person>
            {
                new Person("Vaso", 20, DateTime.Now.AddYears(-20), "Bethlehem House", true, null),
                new Person("John", 80, DateTime.Now.AddYears(-80), "Cable Street", false, "Email"),
                new Person("Andy", 24,DateTime.Now.AddYears(-24), null, false, "Email"),
                new Person(null, 15, DateTime.Now.AddYears(-15), "Lime House Street", true, null),
            };

            var serializer = new CodeCapital.System.Text.Json.JsonFlattener();

            var result1 = serializer.Flatten(JsonSerializer.Serialize(people), new JsonSerializerFlattenOptions { MaxDepth = 10 });

            var result2 = serializer.Flatten(JsonSerializer.Serialize(new { Humans = people }));

            var restul3 =
                serializer.Flatten(JsonSerializer.Serialize(new Person("Johny", 34, DateTime.Now.AddYears(-34), "Lloyds Avenue", true, "Email")));
        }
    }
}


