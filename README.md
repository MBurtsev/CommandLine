# Command Line Parser for .net core and classic .net console applications

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

Как добавить описание к команде

Как добавить значение по умолчанию
Как добавить другое имя аргументу
Как добавить описание к аргументу команды

Как вызвать команду с именнованными аргументами
Как работают сокращения команд
Как работают сокращения имен аргуменов

Как запустить команду из кода

Ка получить справку под команде
Как получить справку по всем командам

Как вывести справку если команда не задана
Как вывести запрос команды если команда не задана

Как вводить массивы


# How to 
        
```csharp
```
