using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

public static class Program
{
    public static IEnumerable<int> EnterIndexes(this string text, string elem)
    {
        for (int i = 0; i < text.Length - elem.Length; i++)
        {
            if (string.Join("", text.Skip(i).Take(elem.Length)) == elem)
            {
                yield return i;
            }
        }
    }

    public static IEnumerable<string> ReadLines(string file)
    {
        StreamReader sr = new StreamReader(file, Encoding.Unicode);
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            yield return line;
        }
        sr.Close();
    }

    public static IEnumerable<string> WriteLines(this IEnumerable<string> seq, string file, bool append = true)
    {
        StreamWriter sw = new StreamWriter(file, append, Encoding.Unicode);
        foreach (string elem in seq)
        {
            sw.WriteLine(elem);
        }
        sw.Close();
        return seq;
    }

    public static IEnumerable<T> PrintLines<T>(this IEnumerable<T> seq)
    {
        foreach (T item in seq)
        {
            Console.WriteLine(item);
        }
        return seq;
    }

    public static bool Contain<T>(this T[] arr, T elem1)
    {
        foreach (T elem2 in arr)
        {
            if (elem1.Equals(elem2))
            {
                return true;
            }
        }
        return false;
    }

    public static IEnumerable<string> ToWords(this string str, char[] delim = null)
    {
        if (delim == null)
        {
            delim = new char[] { ' ' };
        }
        string s = "";
        foreach (char c in str)
        {
            if (delim.Contain(c))
            {
                yield return s;
                s = "";
            }
            else
            {
                s += c;
            }
        }
        if (s != "")
        {
            yield return s;
        }
    }

    public static string MyReplace(this string str, string toChange, string forChange)
    {
        char[] delims = new char[] { ' ', '(', ')', '{', '}', '+', '=', ',', '.', '<', '>', '\n', ';', '[', ']' };
        bool withoutFirst = false;
        bool withoutLast = false;
        bool isBreak = false;

        foreach (int i in str.EnterIndexes(toChange).Reverse())
        {
            if (i >= 0)
            {
                switch (str[i - 1])
                {
                    case var c when (delims.Contain(c)):
                        break;
                    case var c when (c == '`'):
                        withoutFirst = true;
                        break;
                    default:
                        isBreak = true;
                        break;
                }
            }
            if (i + toChange.Length < str.Length)
            {
                switch (str[i + toChange.Length])
                {
                    case var c when (delims.Contain(c)):
                        break;
                    case var c when (c == '`'):
                        withoutLast = true;
                        break;
                    default:
                        isBreak = true;
                        break;
                }
            }
            if (isBreak)
            {
                isBreak = false;
                continue;
            }
            str = string.Join("", str.Take(i - (withoutFirst ? 1 : 0))) + forChange + string.Join("", str.Skip(i + toChange.Length + (withoutLast ? 1 : 0)));
        }

        return str;
    }

    public static IEnumerable<string> MyRepeate(this IEnumerable<string> text, IEnumerable<string> input)
    {
        if (input.First().ToWords(new char[] { ':' }).ToArray().Length > 2)
        {
            throw new Exception("Неверное разделение параметров замены");
        }
        string[] inText = text.ToArray();
        string[] output = new string[0];
        string[] toChange = input.First().ToWords(new char[] { ':' }).First().ToWords(new char[] { '`' }).ToArray();
        string[] forChange = input.First().ToWords(new char[] { ':' }).Last().ToWords(new char[] { '`' }).ToArray();

        if (forChange.Length % toChange.Length != 0)
        {
            throw new Exception("Неверное количество параметров");
        }

        if (input.ToArray().Length == 1)
        {
            for (int i = 0; i < forChange.Length / toChange.Length; i++)
            {
                string[] outputTemp = inText;
                for (int j = 0; j < toChange.Length; j++)
                {
                    outputTemp = outputTemp.Select(s => s.MyReplace(toChange[j], forChange[j + toChange.Length * i])).ToArray();
                }
                foreach (string item in outputTemp)
                {
                    Array.Resize(ref output, output.Length + 1);
                    output[output.Length - 1] = item;
                }
            }

            return output;
        }
        else
        {
            string[] nextParams = new string[input.ToArray().Length - 1];
            for (int i = 1; i < input.ToArray().Length; i++)
            {
                nextParams[i - 1] = input.ToArray()[i];
            }

            for (int i = 0; i < forChange.Length / toChange.Length; i++)
            {
                string[] outputTemp = inText;
                for (int j = 0; j < toChange.Length; j++)
                {
                    outputTemp = outputTemp.Select(s => s.MyReplace(toChange[j], forChange[j + toChange.Length * i])).ToArray();
                }
                foreach (string item in outputTemp)
                {
                    Array.Resize(ref output, output.Length + 1);
                    output[output.Length - 1] = item;
                }
            }

            return output.MyRepeate(nextParams);
        }
    }

    public static void Main(string[] args)
    {
        string[] read = ReadLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Function.txt")).ToArray();
        try
        {
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overloads.txt"));
        }
        catch
        { }
        string[] param = new string[0];
        try
        {
            for (int i = 0; read[i].ToLower().Trim() != "programm"; i++)
            {
                Array.Resize(ref param, param.Length + 1);
                param[i] = read[i];
            }
        }
        catch
        {
            throw new Exception("Отсутствует слово \"programm\". Возможно, кодировка не соответствует Unicode");
        }
        string[] text = new string[0];
        for (int i = 0; i < read.Length - param.Length - 1; i++)
        {
            Array.Resize(ref text, text.Length + 1);
            text[i] = read[i + param.Length + 1];
        }
        text.MyRepeate(param).WriteLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overloads.txt"));
    }
}