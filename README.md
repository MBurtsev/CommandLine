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
__arguments settings are specified using an additional attribute Option__
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

Abbreviated сall examples
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
>us na="Any \"good\" name" find
```


Ка получить справку под команде
Как получить справку по всем командам

Как вывести справку если команда не задана
Как вывести запрос команды если команда не задана

Как вводить массивы


# 
        
```csharp
```
