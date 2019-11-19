using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CommandLine
{
    public class Parser
    {
        #region ' static '

        static Parser cash = null;

        // For example /help /? or -help -?
        public static char OptionChar = '/';
        public static char ArraySeparatorChar = ',';

        // Create parser instance
        public static Parser Create(params string[] args)
        {
            // Init
            if (cash == null)
            {
                cash = new Parser();

                var asm = Assembly.GetExecutingAssembly();

                // Seach for all methods with Command attribute
                var methods = asm.GetTypes()
                      .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                      .Where(y => y.GetCustomAttributes().OfType<CommandAttribute>().Any())
                      .ToArray();

                foreach (var met in methods)
                {
                    var atr = (CommandAttribute)met.GetCustomAttribute(typeof(CommandAttribute));

                    if (atr.Path == null || atr.Path.Length == 0)
                    {
                        throw new Exception("Command pattern not set");
                    }
                    
                    var cmd = atr.Help != null ? new Command(atr.Path, met, atr.Help) : new Command(atr.Path, met);

                    cash.Add(cmd);
                }
            }

            cash.Run(args);

            return cash;
        }
        
        #endregion 

        List<Command> list;

        Parser()
        {
            list = new List<Command>();
        }

        Parser(IEnumerable<Command> list): this()
        {
            AddRange(list);
        }
        
        // Parse multiple commands
        public void Run(IEnumerable<string> list)
        {
            foreach (var cmd in list)
            {
                Run(cmd);
            }
        }         

        // Parse string command and run it if containce
        public void Run(string text)
        {
            // multy check
            var multi = text.Split(new []{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

            if (multi.Length > 1)
            {
                Run(multi);

                return;
            }

            // Parse input string
            var pos = 0;
            var mode = 0;
            var quotes = false;
            var arr = new List<string>();

            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];

                if (c == '"' || c == '\'')
                {
                    if (i > 0 && text[i - 1] == '\\')
                    {
                        if (mode == 0)
                        {
                            arr.Add(text.Substring(pos, i - pos - 1) + c);
                        }
                        else
                        {
                            arr[arr.Count - 1] += text.Substring(pos, i - pos - 1) + c;
                        }

                        mode = 1;

                        pos = i + 1;

                        continue;
                    }

                    quotes = !quotes;

                    if (mode == 0)
                    {
                        arr.Add(text.Substring(pos, i - pos));
                    }
                    else
                    {
                        arr[arr.Count - 1] += text.Substring(pos, i - pos);
                    }

                    mode = 1;

                    pos = i + 1;

                    continue;
                }

                if (quotes)
                {
                    continue;
                }

                var last = i == text.Length - 1;

                if (c == ' ' || last)
                {
                    if (last)
                    {
                        i++;
                    }

                    var str = text.Substring(pos, i - pos).Trim();

                    if (str.Length > 0)
                    {
                        if (mode == 1)
                        {
                            arr[arr.Count - 1] += str;
                        }
                        else
                        {
                            arr.Add(str);
                        }
                    }

                    mode = 0;

                    pos = i + 1;
                }
            }

            var help = false;

            // clear options
            for (var i = 0; i < arr.Count; ++i)
            {
                var itm = arr[i];

                if (itm[0] == OptionChar)
                {
                    arr.RemoveAt(i--);

                    if (itm == (OptionChar + "?") || (OptionChar + "help").IndexOf(itm) == 0)
                    {
                        help = true;
                    }
                }
            }
                                   
            // empty check
            if (arr.Count == 0)
            {
                PrintError("Command is empty", list, help);

                return;
            }

            var ht = new Dictionary<Command, Dictionary<string, object>>();
            var found = new List<Command>();

            // Adds to found if first command word has value
            foreach (var cmd in list)
            {
                if (cmd.Parts[0].Word.IndexOf(arr[0]) == 0)
                {
                    found.Add(cmd);
                    ht.Add(cmd, new Dictionary<string, object>());
                }
            }
            
            if (found.Count == 0)
            {
                PrintError($"The command starting with '{arr[0]}' not found.", found, help);

                return;
            }

            // Analize args
            var named = false;
            var args = new List<aarg>();
            var sign_count = 0;

            if (!help)
            {
                // Filter with arguments length
                for (var i = 0; i < found.Count; ++i)
                {
                    var cmd = found[i];

                    if (cmd.Parts.Count < arr.Count)
                    {
                        found.RemoveAt(i--);

                        if (found.Count == 0)
                        {
                            PrintError($"Entered too much arguments. Please use {OptionChar}? for help.", found, help);

                            return;
                        }
                    }
                }

                foreach (var itm in arr)
                {
                    var ind = itm.IndexOf("=");

                    if (ind > -1 && ind != 0)
                    {
                        named = true;

                        var arg = new aarg { key = itm.Substring(0, ind), val = itm.Substring(ind + 1), opt = null as CommandOption };

                        args.Add(arg);
                    }
                    else
                    {
                        var arg = new aarg { key = null as string, val = itm, opt = null as CommandOption };

                        args.Add(arg);

                        sign_count++;
                    }
                }
            }

            if (named)
            {
                for (var i = 0; i < found.Count; ++i)
                {
                    var cmd = found[i];

                    // check signature count
                    if (sign_count != cmd.CountOfSignatures)
                    {
                        found.RemoveAt(i--);
                        
                        continue;
                    }

                    var f = false;
                    var n = 0;
                    var k = 0;

                    // check signatures
                    while (n < cmd.Parts.Count && k < args.Count)
                    {
                        var part = cmd.Parts[n];
                        var arg = args[k];

                        if (part.Type != Command.PartType.Signature)
                        {
                            n++;
                            continue;
                        }

                        if (arg.key != null)
                        {
                            k++;
                            continue;
                        }

                        if (part.Word.IndexOf(arg.val) != 0)
                        {
                            f = true;

                            break;
                        }

                        n++;
                        k++;
                    }

                    if (f)
                    {
                        found.RemoveAt(i--);

                        continue;
                    }

                    // fill for default check
                    var hop = new HashSet<CommandOption>();

                    for (var j = 0; j < cmd.Options.Count; ++j)
                    {
                        var opt = cmd.Options[j];

                        hop.Add(opt);
                    }

                    // check args
                    for (var j = 0; j < args.Count && !f; ++j)
                    {
                        var arg = args[j];

                        if (arg.key == null)
                        {
                            continue;
                        }

                        for (var p = 0; p < cmd.Options.Count; ++p)
                        {
                            var opt = cmd.Options[p];

                            if (opt.Name.IndexOf(arg.key) == 0)
                            {
                                // Ambiguous choise args name
                                if (arg.opt != null)
                                {
                                    if (found.Count == 1)
                                    {
                                        PrintError($"Ambiguous choise args name: '{arg.key}'", found, help);
                                    }

                                    f = true;

                                    break;
                                }

                                arg.opt = opt;

                                hop.Remove(opt);
                            }
                        }
                    }

                    // check defaults
                    foreach (var opt in hop)
                    {
                        if (opt.Default == null)
                        {
                            if (found.Count == 1)
                            {
                                PrintError($"Required option '{opt.Name}' not set.", found, help);
                            }

                            f = true;
                        }
                        else
                        {
                            args.Add(new aarg { key = opt.Name, val = opt.Default.ToString(), opt = opt });
                        }
                    }

                    if (f)
                    {
                        found.RemoveAt(i--);

                        continue;
                    }
                    
                    // get vals
                    for (var j = 0; j < args.Count; ++j)
                    {
                        var arg = args[j];

                        if (arg.key == null)
                        {
                            continue;
                        }

                        var opt = arg.opt;

                        if (opt == null)
                        {
                            found.RemoveAt(i--);

                            break;
                        }

                        // check vals
                        var val = Convert(arg.val, opt.Param.Type);

                        if (val == null)
                        {
                            found.RemoveAt(i--);

                            break;
                        }
                                                
                        var dic = ht[cmd];

                        dic.Add(opt.Param.Name, val);
                    }
                }
            }
            else
            {
                for (var i = 0; i < found.Count; ++i)
                {
                    var cmd = found[i];

                    if (cmd.Parts.Count != arr.Count)
                    {

                        if (!help)
                        {
                            // check if all args has default values
                            if (cmd.AllDefault && cmd.Parts.Count - cmd.CountOfDefault == arr.Count)
                            {
                                for (var j = 1; j < arr.Count; ++j)
                                {
                                    var part = cmd.Parts[j];

                                    if (part.Type == Command.PartType.Param)
                                    {
                                        var dic = ht[cmd];

                                        // add dufault values
                                        dic.Add(part.Option.Param.Name, part.Option.Default);
                                    }
                                    else if (part.Type != Command.PartType.Signature || part.Word.IndexOf(arr[j]) != 0)
                                    {
                                        found.RemoveAt(i--);

                                        break;
                                    }
                                }
                            }
                            else
                            {
                                found.RemoveAt(i--);
                            }
                        }

                        continue;
                    }

                    // prepair param values
                    for (var j = 1; j < cmd.Parts.Count; ++j)
                    {
                        var part = cmd.Parts[j];

                        if (part.Type == Command.PartType.Signature && part.Word.IndexOf(arr[j]) != 0)
                        {
                            found.RemoveAt(i--);

                            break;
                        }

                        if (help)
                        {
                            continue;
                        }
                        
                        if (part.Type == Command.PartType.Param)
                        {
                            var val = Convert(arr[j], part.Option.Param.Type);

                            if (val == null || !part.Option.Param.Type.IsArray && val.ToString().Length != arr[j].Length)
                            {
                                found.RemoveAt(i--);

                                if (found.Count == 0)
                                {
                                    PrintError($"Incorrect argument: {part.Word}, type must be {part.Option.Param.Type.Name}. Please use {OptionChar}? for help.", found, help);

                                    return;
                                }

                                break;
                            }
                            else
                            {
                                var dic = ht[cmd];

                                dic.Add(part.Option.Param.Name, val);
                            }
                        }
                    }
                }
            }

            // clear some
            var hash = new HashSet<Command>();

            for (var i = 0; i < found.Count; ++i)
            {
                var cmd = found[i];

                if (hash.Contains(cmd))
                {
                    found.RemoveAt(i--);
                }
                else
                {
                    hash.Add(cmd);
                }
            }

            // print help
            if (help)
            {
                PrintHelp(found);

                return;
            }

            // 
            if (found.Count == 0)
            {
                PrintError($"Command not found. Please use {OptionChar}? for help. query = {text}", found, help);

                return;
            }

            //
            if (found.Count > 9)
            {
                PrintError($"Ambiguous choise command > 9. If you have shortened commands, please add more characters. query = {text}", found, help);

                return;
            }

            var selected = null as Command;
            
            if (found.Count > 1)
            {
                Console.WriteLine($"Ambiguous choise command. Please select what did you mean 1-{found.Count}:");

                for (var i = 0; i < found.Count; ++i)
                {
                    var cmd = found[i];

                    Console.Write((i + 1) + ": ");

                    for (var j = 0; j < cmd.Parts.Count; ++j)
                    {
                        var part = cmd.Parts[j];

                        if (part.Type == Command.PartType.Signature)
                        {
                            Console.Write(part.Word + " ");
                        }
                        else if (part.Type == Command.PartType.Param)
                        {
                            Console.Write(part.Option.Name + "=" + arr[j] + " ");
                        }
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.Write($"Type number 1-{found.Count} for choise or 0 for exit: ");

                var num = Console.Read() - 48;

                if (num == 0)
                {
                    return;
                }

                selected = found[--num];
            }

            if (found.Count == 1)
            {
                selected = found[0];
            }
            else
            {
                Console.WriteLine($"Command not found. Enter {OptionChar}? for help.");
            }

            if (selected != null)
            {
                selected.Run(ht[selected]);
            }

        }
        
        // Add command
        public void Add(Command cmd)
        {
            list.Add(cmd);
        }

        // Add multiple commands
        public void AddRange(IEnumerable<Command> list)
        {
            this.list.AddRange(list);
        }

        // Convert from string
        public static object Convert(object value, Type type)
        {
            if (value == null)
            {
                throw new NullReferenceException();
            }

            if (type.IsArray && value is string val)
            {
                var typ = type.GetElementType();
                var arr = val.Split(ArraySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                var ret = Array.CreateInstance(typ, arr.Length);
                var tmp = Array.ConvertAll(arr, x => Convert(x, typ));

                Array.Copy(tmp, ret, arr.Length);

                return ret;
            }

            if (type == typeof(bool))
            {
                if (value == "1" || "true".IndexOf(value.ToString(), StringComparison.Ordinal) == 0)
                {
                    return true;
                }

                if (value == "0" || "false".IndexOf(value.ToString(), StringComparison.Ordinal) == 0)
                {
                    return false;
                }
            }
            
            return System.Convert.ChangeType(value, type);
        }

        // Print error
        void PrintError(string msg, List<Command> found, bool help)
        {
            if (help)
            {
                if (found == null)
                {
                    found = list;
                }

                PrintHelp(found);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }
        
        // Print help for cmd's
        void PrintHelp(List<Command> list)
        {
            foreach (var cmd in list)
            {
                PrintHelp(cmd);
            }
        }

        // Print help for cmd
        void PrintHelp(Command cmd)
        {
            Console.WriteLine(cmd.Path);

            if (cmd.Help != null && cmd.Help.Length > 0)
            {
                Console.WriteLine("".PadLeft(2) + cmd.Help);
            }

            Console.WriteLine();

            if (cmd.Options.Count > 0)
            {
                foreach (var opt in cmd.Options)
                {
                    //var h = "";

                    Console.WriteLine("".PadLeft(4) + opt.Name.PadRight(10) + opt.Param.Type.Name.PadRight(10) + (opt.Default != null ? $"not required. Default value: {opt.Default}" : "required") + "  " );

                    if (cmd.Help != null && cmd.Help.Length > 0)
                    {
                        Console.WriteLine("".PadLeft(6) + opt.Help);
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }

        // for analize args
        private class aarg
        {
            public string key;
            public string val;
            public CommandOption opt;
        }
    }

    // class, an exhaustive description of the console command, according to which the command parser is oriented for parsing lines and calling functions
    public class Command
    {
        // Command Path
        public string Path { get; }
        // Options list
        public List<CommandOption> Options { get; }
        // Options by name
        public Dictionary<string, CommandOption> OptionByKey { get; }
        // Options list
        public List<CommandParam> Params { get; }
        // Options by name
        public Dictionary<string, CommandParam> ParamByKey { get; }
        // Parts of command
        public List<Part> Parts { get; } = new List<Part>();
        // How much params has default vulue
        public int CountOfDefault { get; private set; }
        // How much signature words has command
        public int CountOfSignatures { get; }
        // All params have default values
        public bool AllDefault { get; }
        //
        MethodInfo method { get; }
        // Help text 
        public string Help { get; }

        public Command(string path, MethodInfo method, string help): this(path, method)
        {
            Help = help;
        }


        public Command(string path, MethodInfo method)
        {
            Options = new List<CommandOption>();
            OptionByKey = new Dictionary<string, CommandOption>();
            Params = new List<CommandParam>();
            ParamByKey = new Dictionary<string, CommandParam>();

            Path = path;
            Help = "";
            this.method = method;

            // parameters
            var pars = method.GetParameters();

            foreach (var par in pars)
            {
                var param = new CommandParam(par);

                var t = param.Type;

                if (t.IsArray)
                {
                    t = t.GetElementType();
                }
                
                if (!typeof(IConvertible).IsAssignableFrom(t))
                {
                    throw new Exception($"Param '{param.Name}' has not IConvertible Type. Method: '{method}'");
                }

                AddParam(param);
            }

            // options
            var opts = method.GetCustomAttributes<OptionAttribute>();

            foreach (var opt in opts)
            {
                if (opt.Param == null)
                {
                    throw new Exception("The option name not set");
                }

                if (!ParamByKey.ContainsKey(opt.Param))
                {
                    throw new Exception($"Option name '{opt.Param}' does not match with parameter name. Check 'Param' property.");
                }

                var pbk = ParamByKey[opt.Param];
                var option = new CommandOption(pbk, opt);


                AddOption(option);
            }

            if (CountOfDefault == Params.Count)
            {
                AllDefault = true;
            }

            // Parse pattern
            var arr = path.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            foreach (var itm in arr)
            {
                if (itm[0] == '{')
                {
                    if (itm[itm.Length - 1] != '}')
                    {
                        throw new Exception("Closed brace '}' not found");
                    }

                    var key = itm.Substring(1, itm.Length - 2);

                    if (!OptionByKey.ContainsKey(key))
                    {
                        if (ParamByKey.ContainsKey(key))
                        {
                            var param = ParamByKey[key];
                            var option = new CommandOption(param);

                            AddOption(option);
                        }
                        else
                        {
                            throw new Exception("Option with param name '" + key + "' not present");
                        }
                    }

                    var opt = OptionByKey[key];

                    Parts.Add(new Part { Word = key, Type = PartType.Param, Option = opt });
                }
                else
                {
                    Parts.Add(new Part { Word = itm, Type = PartType.Signature });

                    CountOfSignatures++;
                }
            }
        }

        // Call function
        public void Run(Dictionary<string, object> args)
        {
            if (args.Count != Params.Count)
            {
                throw new Exception("Params count are not equal parsed values");
            }
            
            var arr = new object[Params.Count];

            for (var i = 0; i < arr.Length;  ++i)
            {
                var par = Params[i];

                arr[i] = args[par.Name];
            }

            method.Invoke(null, arr);
        }

        // Add option
        public void AddOption(CommandOption option)
        {
            Options.Add(option);
            OptionByKey.Add(option.Name, option);

            if (option.Default != null)
            {
                CountOfDefault++;
            }
        }

        // add param
        public void AddParam(CommandParam param)
        {
            if (!ParamByKey.ContainsKey(param.Name))
            {
                Params.Add(param);
                ParamByKey.Add(param.Name, param);
            }
        }

        public class Part
        {
            public string Word { get; set; }
            public PartType Type { get; set; }
            public CommandOption Option { get; set; }
        }

        public enum PartType
        {
            Signature,
            Param
        }
    }

    // Param of command method
    public class CommandParam
    {
        public CommandParam(ParameterInfo info)
        {
            Info = info;
            Name = info.Name;
            Type = Info.ParameterType;
        }

        public ParameterInfo Info { get; private set; }
        public string Name { get; private set; }
        public Type Type { get; private set; }
    }

    // Command option
    public class CommandOption
    {
        // Option for this param
        public CommandParam Param { get; }
        // Not requred alias name. By default Name equal Param.Name
        public string Name { get; }
        // Default value. 
        public object Default { get; }
        // Help text
        public string Help { get; set; } = "";
        
        public CommandOption(CommandParam param, OptionAttribute atr)
        {
            Param = param;

            if (atr.Name != null)
            {
                Name = atr.Name;
            }
            else
            {
                Name = param.Name;
            }

            if (atr.Default != null)
            {
                Default = Parser.Convert(atr.Default, param.Type);
            }

            if (atr.Help != null)
            {
                Help = atr.Help;
            }
        }

        public CommandOption(CommandParam param)
        {
            Param = param;
            Name = param.Name;
        }
    }

    // Methods marked with this attribute become console commands.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class CommandAttribute: Attribute
    {
        public CommandAttribute()
        {

        }

        public CommandAttribute(string path)
        {
            Path = path;
        }

        public CommandAttribute(string path, string help)
        {
            Path = path;
            Help = help;
        }

        /// <summary>
        /// Full name of command. Can be include spaces.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Help text for this comand. 
        /// </summary>
        public string Help { get; set; }
    }


    // Console command option 
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class OptionAttribute: Attribute
    {
        /// <summary>
        /// Option name must be the same as the method parameter name.
        /// </summary>
        public string Param { get; set; }

        /// <summary>
        /// Option alias name. Not required
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a command line option is required.
        /// </summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// Default value if no set
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// Help text for this option
        /// </summary>
        public string Help { get; set; }
    }
}
