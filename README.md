# Command Line Parser for .net core and classic .net
Easy to use command line parser.

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


Как добавить описание к аргументу команды

Как запустить команду из кода

Как вызвать команду с именнованными аргументами
Как работают сокращения команд
Как работают сокращения имен аргуменов



Ка получить справку под команде
Как получить справку по всем командам

Как вывести справку если команда не задана
Как вывести запрос команды если команда не задана

Как вводить массивы


# 
        
```csharp
```
