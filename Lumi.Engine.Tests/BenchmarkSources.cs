namespace Lumi.Engine.Tests;

public static class SourceStrings
{
    public const string ListManipulationWorkloadSource = @"
            let items: list -> [];

            for i in 0 to 499 step 1 {
                items.add(i);
            }

            let checksum -> 0;
            for i in 0 to 499 step 1 {
                checksum = checksum + items[i];
            }

            for i in 0 to 249 step 1 {
                items.remove(i);
            }

            print items.length();
            print checksum;
        ";

    public const string FibonacciSource = @"
            fn fib(n) {
                if (n <= 1) {
                    return n;
                } else {
                    return fib(n - 1) + fib(n - 2);
                }
            }

            print fib(25);
        ";

    public const string StructDefinitionAndFieldAccessSource = """
            struct Person {
                name: str;
                age: int;
            }
            let person: Person -> new Person;
            print person.name;
            """;

    public const string StructDefinitionAndFieldAccessWithAssignmentSource = """
            struct Person {
                name: str;
                age: int;
            }

            for i in 0 to 999 step 1 {
                let p: Person -> new Person("Alice", 30);
                p.age = 5;
                print p.name;
                print p.age;
            }
            """;
}