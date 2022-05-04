using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal class Program
{
   static List<string> commandHistory = new List<string>();
    static int historyPointer = 0;
    const int WINDOW_HEIGHT = 30;
    const int WINDOW_WIDTH = 120;
    private static string currentDir = Directory.GetCurrentDirectory();
    static void Main(string[] args)
    {
        
        
       

        try
        {
            if (File.Exists("state.txt"))
            {
                currentDir = File.ReadAllText("state.txt");
            }
            Console.Title = "FileManeger";

            Console.SetWindowSize(WINDOW_WIDTH, WINDOW_HEIGHT);
            Console.SetWindowSize(WINDOW_WIDTH, WINDOW_HEIGHT);

            DrawWindow(0, 0, WINDOW_WIDTH, 18);
            DrawWindow(0, 18, WINDOW_WIDTH, 8);
            UpdateConsole();


            Console.ReadKey(true);

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Directory.CreateDirectory("Errors");
            File.AppendAllText("Errors\\Errors.txt", e.Message);
        }

    }


    
   
    static void UpdateConsole()
    {
        DrawConsole(currentDir, 0, 26, WINDOW_WIDTH, 3);
        ProcessEnterCommand(WINDOW_WIDTH);

    }
    static (int left,int top) GetCursorPosition()
    {
        return(Console.CursorLeft,Console.CursorTop);

    }
    static void ProcessEnterCommand(int width)
    {
        (int left,int top) = GetCursorPosition();
        StringBuilder command = new StringBuilder();
        ConsoleKey key;
        do
        {
            var keyPressed = Console.ReadKey();
            char charPressed = keyPressed.KeyChar;
            key = keyPressed.Key;


            if (key != ConsoleKey.Backspace && key!=ConsoleKey.Enter && key!= ConsoleKey.UpArrow && key!= ConsoleKey.DownArrow)
                command.Append(charPressed);
         
            (int currentLeft,int currentTop) = GetCursorPosition();
            if(currentLeft == width-2)
            {
                Console.SetCursorPosition(currentLeft,top);
                Console.Write(" ");
                Console.SetCursorPosition(currentLeft, top);
                   
            }
            if(key == ConsoleKey.UpArrow)
            {
                int pointer = historyPointer - 1;
                if (pointer < 0)
                {
                    pointer = commandHistory.Count()-1;
                }

                historyPointer = pointer;
                string prevCommand = commandHistory[historyPointer];
                command.Clear();
                command.Append(prevCommand);

                
                Console.SetCursorPosition(left, top);
                Console.Write("          ");
                Console.SetCursorPosition(left, top);
                Console.Write(prevCommand, command);
            }
            else if (key == ConsoleKey.DownArrow)
            {
                int pointer = historyPointer + 1;
                if(pointer >= commandHistory.Count())
                {
                    pointer = 0 ;
                }

                historyPointer = pointer;
                string prevCommand = commandHistory[historyPointer];
                command.Clear();
                command.Append(prevCommand);
                Console.SetCursorPosition(left, top);
                Console.Write("        ");
                Console.SetCursorPosition(left, top);
                Console.Write(prevCommand, command);
            }
            if(key == ConsoleKey.Backspace)
                    {
                if (command.Length > 0)
                    command.Remove(command.Length-1,1);
                if(currentLeft>=left)
                {
                    Console.SetCursorPosition(Console.CursorLeft, top);
                    Console.Write(" ");
                    Console.SetCursorPosition(Console.CursorLeft -1, top);
                }
                else
                {
                    Console.SetCursorPosition (left , top);
                }
            }
        }

        while (key != ConsoleKey.Enter);
        ParseCommandString(command.ToString());

    }
    static void ParseCommandString(string command)
    {
        commandHistory.Add(command);


            string[] commandParams = command.ToLower().Split(' ');
        if (commandParams.Length > 0)
        {
            switch(commandParams[0])
            {

                case "cd":

                    if (commandParams.Length > 1)
                    {
                        if (Directory.Exists(commandParams[1]))
                        {
                            currentDir = commandParams[1];
                            File.WriteAllText("state.txt", currentDir);
                        }
                    }
                    
                    break;
                case "ls":
                    if (commandParams.Length > 1 && Directory.Exists(commandParams[1]))
                    {
                        if (commandParams.Length>3 && commandParams[2]=="-p"&&int.TryParse(commandParams[3],out int n))
                        {
                            DrawTree(new DirectoryInfo(commandParams[1]), n);
                        }
                        else
                        {
                            DrawTree(new DirectoryInfo(commandParams[1]), 1);
                        }
                    }
                    break;
                case "rm":
                    if (commandParams.Length > 0 && (Directory.Exists(commandParams[1]) || File.Exists(commandParams[1])))
                    {
                        string path = commandParams[1];
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        else
                        {
                            File.Delete(path);
                        }
                    }
                    break;
                case "file":
                    if (commandParams.Length > 0 && File.Exists(commandParams[1]))
                    {
                        String content = File.ReadAllText(commandParams[1], Encoding.UTF8);
                        DrawWindow(0, 0, WINDOW_WIDTH, 18);
                        (int currentLeft, int currentTop) = GetCursorPosition();
                        Console.SetCursorPosition(currentLeft + 1, currentTop + 1);
                        Console.WriteLine(content);
                    }
                        break;
                case "help":
                    DrawWindow(0, 0, WINDOW_WIDTH, 18);
                    (int tempLeft, int tempTop) = GetCursorPosition();
                    Console.SetCursorPosition(tempLeft + 1, tempTop + 1);
                    Console.WriteLine("help [Список команд]");
                    Console.SetCursorPosition(tempLeft + 1, tempTop + 2);
                    Console.WriteLine("ls [ВЫвод на экран все дерево древа]");
                    Console.SetCursorPosition(tempLeft + 1, tempTop + 3);
                    Console.WriteLine("rm [Удаление файла]");
                    Console.SetCursorPosition(tempLeft + 1, tempTop + 4);
                    Console.WriteLine("cd [Переход в деректорию]");
                    
                    break;
            }
        }
        UpdateConsole();
        
    }
    

    static void DrawTree(DirectoryInfo dir,int page)
    {
        StringBuilder tree = new StringBuilder();
        GetTree(tree,dir,"",true);
        DrawWindow(0, 0, WINDOW_WIDTH, 18);
        (int currentLeft,int currentTop)=GetCursorPosition();
        int pageLines = 16;
        string[] line = tree.ToString().Split(new char[] { '\n' });
        int pageTotal = (line.Length + pageLines - 1) / pageLines;

        if(page>pageTotal)
            page=pageTotal;
        for (int i=(page-1)*pageLines,counter=0;i<page*pageLines;i++,counter++)
        {
            if(line.Length-1>i)
            {
                Console.SetCursorPosition(currentLeft + 1, currentTop + 1 + counter);
                Console.WriteLine(line[i]);
            }
        }

        string footer = $"╠{page}of{pageTotal}╣";
        Console.SetCursorPosition(WINDOW_WIDTH / 2 - footer.Length / 2, 17);
        Console.WriteLine(footer);
    }
    static void GetTree(StringBuilder tree,DirectoryInfo dir,string indent,bool lastDirectory)
    {
        tree.Append(indent);
        if (lastDirectory)
        {
            tree.Append("∟");
            indent += " ";
        }
        else
        {
            tree.Append("├");
            indent += "│";
        }
        tree.Append($"{dir.Name} - {dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length)}\n");

       



        FileInfo[] subfiles = dir.GetFiles();
        for (int i=0;i<subfiles.Length;i++)
        {
            FileAttributes attributes = subfiles[i].Attributes;
            bool isReadonly = (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            string isReadonlyText = isReadonly ? "readonly" : "rewritable";
            
            if (i == subfiles.Length-1)
            {
                tree.Append($"{indent}∟{subfiles[i].Name} - {subfiles[i].Length} - {isReadonlyText}\n");
            }
            else
            {

                tree.Append($"{indent}├{subfiles[i].Name} - {subfiles[i].Length} - {isReadonlyText}\n");
            }
        }





        DirectoryInfo[]subDirects = dir.GetDirectories();
        for (int i = 0; i < subDirects.Length; i++)
            GetTree(tree,subDirects[i],indent,i==subDirects.Length-1);
    }

    static void DrawConsole(string dir   ,int x, int y, int width, int height)
    {
        DrawWindow(x, y, width, height);
        Console.SetCursorPosition(x + 1, y + height / 2);
        Console.Write($"{dir}>");
    }

    static void DrawWindow(int x, int y, int widtg, int heigth)
    {
        Console.SetCursorPosition(x, y);
        Console.Write("╔");
        for (int i = 0; i < widtg - 2; i++)
            Console.Write("═");
        Console.Write("╗");

        Console.SetCursorPosition(x, y + 1);
        for (int i = 0; i < heigth - 2; i++)
        {
            Console.Write('║');
            for (int j = x + 1; j < x + widtg - 1; j++)
            {
                Console.Write(" ");

            }
            Console.Write('║');
        }
        Console.Write("╚");
        for (int i = 0; i < widtg - 2; i++)
            Console.Write("═");
        Console.Write("╝");

        Console.SetCursorPosition(x, y);

    }
}