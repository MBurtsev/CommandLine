using System;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace CommandLine
{
    class Program
    {
        public static bool Work = true;
        public static Parser Parser;

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Parser = Parser.Create(args);

            // For test
            //Parser.Run("test begin");

            if (args.Length == 0)
            {
                // promt commands
                //Parser.Run("promt");

                // or help here
                Parser.Run("/?");
            }

            //Console.ReadKey();
        }

        [Command("promt", "To enter commands from console")]
        static void Promt()
        {
            while(Work)
            {
                Console.Write(">");
                var cmd = Console.ReadLine();

                Parser.Run(cmd);
            }
        }

        [Command("exit", "To exit from console")]
        static void Exit()
        {
            Work = false;
        }
    }

    public class Test
    {
        [Command("user find {name} {age}", "Command to search for a user by name and age")]
        [Option(Param = "userName", Name = "name", Help = "User name. Case sensitive.")]
        [Option(Param = "age", Default = 24, Help = "User age. The maximum value is 90 years.")]
        static void UserFind(string userName, int age)
        {
            TestObject = TestUser.CreateUser(userName, age);
        }

        [Command("product counts {counts} {prices}")]
        static void ProductCounts(int[] counts, double[] prices)
        {
            TestObject = new object[] { counts, prices };
        }

        [Command("test begin")]
        static void BeginTest()
        {
            var flag = true;

            Console.WriteLine("Begin command line engine testing");
            Console.WriteLine();

            try
            {
                flag &= CheckUser("user find Alice 24", "Alice", 24);
                flag &= CheckUser("user Alice find 24", "Alice", 24);
                flag &= CheckUser("user Alice 24 find", "Alice", 24);
                flag &= CheckUser("    user     find   Alice    24   ", "Alice", 24);
                flag &= CheckUser("user find name=Bob", "Bob", 24);
                flag &= CheckUser("u f na=Martin a=30", "Martin", 30);
                flag &= CheckUser("user name=Jeny age=35 find", "Jeny", 35);
                flag &= CheckUser("u n='Elon Musk' a=47 find", "Elon Musk", 47);
                flag &= CheckUser("us na=\"Edward\\'s Snowden\"\" 123\" age=35 find", "Edward's Snowden 123", 35);
                flag &= CheckUser("user find Ali\\\"ce 24", "Ali\"ce", 24);
                flag &= CheckCounts("product counts 10,20,30,40 20.123,40.456,60.789", new object[] { new[] { 10, 20, 30, 40 }, new[] { 20.123, 40.456, 60.789 } });


                //flag &= CheckUser("user find Alice", "Alice", 24); // maybe in future
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                flag = false;
            }

            if (flag)
            {
                Console.WriteLine("All tests done");
            }
            else
            {
                Console.WriteLine("Fail testing");
            }

            Console.WriteLine("Press any key to exit");
        }

        static object TestObject = null;

        static bool CheckUser(string cmd, string name, int age)
        {
            Program.Parser.Run(cmd);

            var obj = TestObject as TestUser;

            if (obj == null || obj.Name != name || obj.Age != age)
            {
                Console.WriteLine(cmd + " --> Fail");

                return false;
            }

            return true;
        }

        static bool CheckCounts(string cmd, object obj)
        {
            Program.Parser.Run(cmd);

            if (TestObject is object[] tmp)
            {
                if (tmp.Length != 2)
                {
                    return false;
                }

                var a0 = tmp[0] as int[];
                var a1 = tmp[1] as double[];
                var b0 = (obj as object[])[0] as int[];
                var b1 = (obj as object[])[1] as double[];

                if (a0 == null || a1 == null || a0.Length != b0.Length || a1.Length != b1.Length)
                {
                    return false;
                }

                for (var i = 0; i < a0.Length; ++i)
                {
                    if (a0[i] != b0[i])
                    {
                        return false;
                    }
                }

                for (var i = 0; i < a1.Length; ++i)
                {
                    if (a1[i] != b1[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }

    public class TestUser
    {
        public static TestUser CreateUser(string name, int age)
        {
            return new TestUser { Name = name, Age = age };
        }

        public string Name;
        public int Age;
    }
}
