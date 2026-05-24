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

    public const string FileIoWorkloadSource = """
            for i in 0 to 99 step 1 {
                let path: str -> "tempfile_" + i + ".txt";
                File.writeText(path, "Hello, world!");
                let contents -> File.readText(path);
                // print contents; // printing this results in a HUGE standard test output
            }
            """;

    public const string StructDefinitionWithMethodSource = """
            struct Counter {
                count: int;
                fn increment() {
                    this.count = this.count + 1;
                }
            }

            let counter: Counter -> new Counter(count: 0);

            for i in 0 to 999 step 1 {
                counter.increment();
                // print counter.count;
            }

            """;

    public const string ArrayMethodsWorkloadSource = """
            let arr: list -> [];

            for i in 0 to 99 step 1 {
                arr.add(i);
            }

            for i in 0 to 99 step 1 {
                arr.contains(i);
            }
            for i in 0 to 99 step 1 {
                arr.remove(i);
            }
            """;

    public const string MixedWorkloadSource = """
            struct Point {
                x: int;
                y: int;

                fn move(dx, dy) {
                    this.x = this.x + dx;
                    this.y = this.y + dy;
                }
            }

            let points: list -> [];

            for i in 0 to 99 step 1 {
                points.add(new Point(x: i, y: i));
            }

            for i in 0 to points.length() - 1 {
                let p: Point -> points[i];
                p.move(1, 1);
                File.appendText("point.txt", "xoxo");
            }

            let pointCount -> points.length();
            for i in 0 to pointCount - 1 step 1 {
                points.remove(points[0]);
            }

            let content -> File.readLines("point.txt");
            // print content;
            """;

    public const string HeapReferenceWorkloadSource = """
            struct Bucket {
                items: list;
                sum: int;

                fn addValue(value) {
                    this.items.add(value);
                    this.sum = this.sum + value;
                }
            }

            let buckets: list -> [];

            for i in 0 to 199 step 1 {
                let items: list -> [];
                let bucket: Bucket -> new Bucket(items: items, sum: 0);
                buckets.add(bucket);
            }

            for i in 0 to buckets.length() - 1 step 1 {
                let bucket: Bucket -> buckets[i];
                let alias: list -> bucket.items;

                for j in 0 to 49 step 1 {
                    bucket.addValue(i + j);
                }

                for j in 0 to 24 step 1 {
                    alias.add(j);
                }
            }

            let total -> 0;
            for i in 0 to buckets.length() - 1 step 1 {
                let bucket: Bucket -> buckets[i];
                total = total + bucket.sum;
                total = total + bucket.items.length();
                total = total + bucket.items[0];
                total = total + bucket.items[bucket.items.length() - 1];
            }

            print total;
            """;
}