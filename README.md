# Command Line Parser
Easy to use command line parser for console applications based on .net core or classic .net

# Features:
- A simple description of commands and arguments using attributes
- Strong typing
- Automatic type casting
- Arrays input support
- Named Arguments
- Abbreviations for command and argument names
- Default values
- Help Support

# Getting Started
- Include file CommandProcessor.cs to your project
- Make parser call in entry point function
```csharp
        static void Main(string[] args)
        {
            Parser.Create(args);
        }
```
- Add an attribute to any static function.
```csharp
        [Command("create user {name} {age}")]
        static void CteateUser(string name, int age)
        {
        }
```
- Run it
```
        create user Alice 24
```

# How to add a description to the command
```csharp
using System;
using CommandLine;

namespace ConsoleApp14
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Create(args);
        }

        [Command("user find {name} {age}", "Command to search for a user by name and age")]
        static void UserFind(string name, int age)
        {
        }
    }
}
```

# How to add default value
__Arguments settings are specified using an additional attribute Option__
```csharp
using System;
using CommandLine;

namespace ConsoleApp14
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Create(args);
        }

        [Command("user find {name} {age}", "Command to search for a user by name and age")]
        [Option(Param = "age", Default = 24)]
        static void UserFind(string name, int age)
        {
        }
    }
}
```

# How to set a another name for the argument
__The specified name in brackets {name} must match the name of the method parameter or of the name given in the Option__
```csharp
using System;
using CommandLine;

namespace ConsoleApp14
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Create(args);
        }

        [Command("user find {name} {age}", "Command to search for a user by name and age")]
        [Option(Param = "userName", Name = "name")]
        static void UserFind(string userName, int age)
        {
        }
    }
}
```

# How to add a description to the command argument
```csharp
using System;
using CommandLine;

namespace ConsoleApp14
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Create(args);
        }

        [Command("user find {name} {age}", "Command to search for a user by name and age")]
        [Option(Param = "userName", Name = "name", Help = "User name. Case sensitive.")]
        [Option(Param = "age", Default = 24, Help = "User age. The maximum value is 90 years.")]
        static void UserFind(string userName, int age)
        {
        }
    }
}
```

# How to run a command from code
```csharp
using System;
using CommandLine;

namespace ConsoleApp14
{
    class Program
    {
        public static Parser Parser;

        static void Main(string[] args)
        {
            Parser = Parser.Create(args);

            Parser.Run("user find Alice 24");
        }

        [Command("user find {name} {age}", "Command to search for a user by name and age")]
        [Option(Param = "userName", Name = "name", Help = "User name. Case sensitive.")]
        [Option(Param = "age", Default = 24, Help = "User age. The maximum value is 90 years.")]
        static void UserFind(string userName, int age)
        {
        }
    }
}
```

# How commands are called
The order of the сommand signatures and the order of the arguments must be preserved
```
>user find Alice 24
```

However, command signatures can be set anywhere.
```
>user Alice find 24
>user Alice 24 find
```

Example of named arguments
```
>user find name=Alice age=24
```

Call examples with default age value. Attention: Such calls can be made only if the arguments are specified by name!
```
>user find name=Alice
```
If all arguments are defaults

```
>user find
```

Abbreviated сall examples. When reducing, avoid ambiguous interpretations.
```
>u f Alice 24
>us f Alice 24
>use f Alice 24
>us fin Alice 24

>u f n=Alice a=24
>us f na=Alice a=24
>use fin nam=Alice ag=24

```

Using quotes
```
>u n='Elon Musk' a=47 find
>us na="Edward\'s Snowden" age=35 find
>u n="Any \"good\" name" f
```

Getting help on the command
```
>user /?
>user find /?
>u /?
```

Getting help for all commands
```
>/?
```

# How to display help if the command is not set
```csharp
using System;
using CommandLine;

namespace ConsoleApp14
{
    class Program
    {
        public static Parser Parser;

        static void Main(string[] args)
        {
            Parser = Parser.Create(args);

            if (args.Length == 0)
            {
                Parser.Run("/?");
            }
        }

        [Command("user find {name} {age}", "Command to search for a user by name and age")]
        [Option(Param = "userName", Name = "name", Help = "User name. Case sensitive.")]
        [Option(Param = "age", Default = 24, Help = "User age. The maximum value is 90 years.")]
        static void UserFind(string userName, int age)
        {
        }
    }
}
```

# How to make the command promt if the command is not set
        
```csharp
using System;
using CommandLine;

namespace ConsoleApp14
{
    class Program
    {
        public static bool Work = true;
        public static Parser Parser;

        static void Main(string[] args)
        {
            Parser = Parser.Create(args);

            if (args.Length == 0)
            {
                Parser.Run("promt");
            }
        }

        [Command("promt", "To enter commands from console")]
        static void Promt()
        {
            while (Work)
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

        [Command("user find {name} {age}", "Command to search for a user by name and age")]
        [Option(Param = "userName", Name = "name", Help = "User name. Case sensitive.")]
        [Option(Param = "age", Default = 24, Help = "User age. The maximum value is 90 years.")]
        static void UserFind(string userName, int age)
        {
        }
    }
}
```

# How to enter arrays
```csharp
using System;
using System.Globalization;
using System.Threading;
using CommandLine;

namespace ConsoleApp14
{
    class Program
    {
        public static Parser Parser;

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Parser.Create(args);
        }

        [Command("product counts {counts} {prices}")]
        static void ProductCounts(int[] counts, double[] prices)
        {
        }
    }
}
```
Usage:
```
>product counts 10,20,30,40 20.123,40.456,60.789
```
__Note: The default item separator is comma. You can change this in Parser.ArraySeparatorChar__

