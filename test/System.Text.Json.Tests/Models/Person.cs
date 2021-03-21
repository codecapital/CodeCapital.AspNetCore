using System.Collections.Generic;

namespace System.Text.Json.Tests.Models
{
    public class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public Address? MyAddress { get; set; }
        public List<string> Colours { get; set; } = new() { "Red", "Blue" };
        public DateTime DayOfBirth { get; set; }

        public Person(string? name, int age, DateTime dayOfBirth, string? street = null, bool isLocal = false, string? deliveryType = null)
        {
            Name = name;
            Age = age;
            DayOfBirth = dayOfBirth;

            if (street != null) MyAddress = new Address(street, isLocal, deliveryType);
        }
    }
}