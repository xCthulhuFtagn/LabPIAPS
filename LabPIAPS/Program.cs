using System;
using System.IO;
using System.Collections.Generic;

namespace ResRegV1cons
{
    class NoSuchRequest : Exception { }
    class ResAreBusy : Exception { }
    class ResIdInvalid : Exception { }
    class UnRecommended : Exception { }
    class ResIsBusy : Exception { }
    class ResWasFree : Exception { }
    static class SetUp
    {
        public static string Path; //путь к файлу, сохраняющему модель
        public static void Save()
        {
            File.WriteAllText(Path, "");
            using StreamWriter file = new StreamWriter(Directory.GetCurrentDirectory() + @"\Resmod00");
            file.WriteLine(Convert.ToString(Model.timer));
            file.WriteLine(string.Join("", Model.vRes_s));
            foreach((int, List<int>) el in Model.vReq_s)
            {
                file.Write(Convert.ToString(el.Item1));
                for(int i = 0; i < el.Item2.Count; ++i)
                {
                    file.Write(" " + Convert.ToString(el.Item2[i]));
                }
                file.Write('\n');
            }
        }
        private static void ClearModel()
        {
            Console.WriteLine("Укажите срок предоставления ресурса: ");
            Model.timer = Convert.ToUInt16(Console.ReadLine());
            Console.WriteLine("Укажите количество ресурсов:");
            try
            {
                Model.vRes_s = new List<string>();
                int n = Convert.ToInt32(Console.ReadLine());
                for (int i = 0; i < n; i++)
                {
                    Model.vRes_s.Add("F");
                }
            }
            catch
            {
                Console.WriteLine("Введено некорректное число!");
                ClearModel();
            }
        }
        private static void GetModel()
        {
            Console.WriteLine("Обновить файл?");
            if (Console.ReadLine().ToUpper() == "Y") ClearModel();
            else
            {
                Model.vRes_s.Clear();
                string[] lines = File.ReadAllLines(Path);
                Model.timer = Convert.ToInt32(lines[0]);
                for (int i = 0; i < lines[1].Length; ++i) Model.vRes_s.Add(Convert.ToString(lines[1][i]));
                for(int i = 2; i < lines.Length; ++i)
                {
                    string[] req = lines[i].Split(" ");
                    int timer = Convert.ToInt32(req[0]);
                    List<int> res_s = new List<int>();
                    int j;
                    for(j = 1; j < req.Length; ++j)
                    {
                        res_s.Add(Convert.ToInt32(req[j]));
                    }
                    Model.vReq_s.Add((timer, res_s));
                }
            }
        }
        public static bool On()
        {
            try
            {
                if (File.Exists(Directory.GetCurrentDirectory() + @"\Resmod00"))
                {
                    Console.WriteLine("Использовать существующий стандартный файл Resmod00?");
                    if (Console.ReadLine().ToUpper() == "Y")
                    {
                        Path = Directory.GetCurrentDirectory() + @"\Resmod00";
                        GetModel();
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine("Создать стандартный файл?");
                    if (Console.ReadLine().ToUpper() == "Y")
                    {
                        Path = Directory.GetCurrentDirectory() + @"\Resmod00";
                        ClearModel();
                        return true;
                    }
                };
                Console.WriteLine("Введите полный адрес нестандартного файла:");
                Path = Console.ReadLine();
                if (File.Exists(Path))
                {
                    GetModel();
                    return true;
                }
                else
                {
                    ClearModel();
                    return true;
                }
            }
            catch (IOException) { Console.WriteLine("Файл не открылся."); return false; }
            catch (Exception) { Console.WriteLine("Ошибка ввода-вывода."); return false; }
        }
    }
    static class Model
    {
        public static List<string> vRes_s = new List<string>();//Модель набора ресурсов
        public static List<(int, List<int>)> vReq_s = new List<(int, List<int>)>();
        public static int timer;
        public static void Satisfy(string cn)
        {
            int ind = Convert.ToInt32(cn) - 1;
            if (vReq_s.Count <= ind || ind < 0) throw new NoSuchRequest();
            foreach(ushort el in vReq_s[ind].Item2)
            {
                vRes_s[el] = "F";
            }
            vReq_s.RemoveAt(ind);
        }
        public static void Free(string cn) //fix!
        {
            if ((Convert.ToInt16(cn) > vRes_s.Count) | (Convert.ToInt16(cn) < 0)) throw new ResIdInvalid();
            if (vRes_s[Convert.ToInt16(cn)] == "F") throw new ResWasFree();
            vRes_s[Convert.ToInt16(cn)] = "F";
            bool found = false;
            for(int i = 0; i < vReq_s.Count && !found; ++i)
            {
                for(int j = 0; j < vReq_s[i].Item2.Count && !found; ++j)
                {
                    if(vReq_s[i].Item2[j] == Convert.ToInt16(cn))
                    {
                        vReq_s[i].Item2.RemoveAt(j);
                        if (vReq_s[i].Item2.Count == 0) Console.WriteLine("Запрос " + Convert.ToString(i + 1) + " теперь лишен каких-либо ресурсов." );
                        found = true;
                    }
                }
            }
        }
        public static string Request()
        {
            Tick();
            for (int i = 0; i < vRes_s.Count; i++)
            {
                if (vRes_s[i] == "F") {
                    vReq_s.Add((timer, new List<int>(){i}));
                    vRes_s[i] = "B";
                    return Convert.ToString(i + 1); 
                }
            }
            throw new ResAreBusy();
        }
        private static void Tick()
        {
            for (int i = 0; i < vReq_s.Count; ++i)
            {
                (int, List<int>) tmp = vReq_s[i];
                tmp.Item1 -= 1;
                if (tmp.Item1 == 0)
                {
                    if (vRes_s.FindAll(x => x.Equals("F")).Count > 1)
                    {
                        int ind = vRes_s.FindIndex(0, vRes_s.Count, x => x.Equals("F"));
                        vRes_s[ind] = "B";
                        tmp.Item2.Add(ind);
                    } 
                    else
                    {
                        vRes_s.Add("B");
                        tmp.Item2.Add(vRes_s.Count - 1);
                    }
                    tmp.Item1 = timer;
                }
                vReq_s[i] = tmp; 
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string Command;
            while (!SetUp.On()) ;
            do
            {
                //сохранение модели
                SetUp.Save();
                Console.WriteLine("Введите команду:");
                Command = Console.ReadLine();
                Command = Command.ToUpper();
                try
                {
                    if (Command == "REQUEST") Console.WriteLine(Model.Request());
                    if (Command == "SATISFY") {
                        Console.WriteLine("Введите номер удовлетворяемого запроса:");
                        Model.Satisfy(Console.ReadLine()); 
                    }
                    if (Command == "FREE")
                    {
                        Console.WriteLine("Введите номер ресурса:");
                        Model.Free(Console.ReadLine());
                        Console.WriteLine("Ресурс освобождён.");
                    };
                }
                catch (NoSuchRequest) { Console.WriteLine("Такого запроса нет.");  }
                catch (OverflowException) { Console.WriteLine("Такого ресурса нет."); }
                catch (FormatException) { Console.WriteLine("Такого ресурса нет."); }
                catch (ResIdInvalid) { Console.WriteLine("Такого ресурса нет."); }
                catch (ResWasFree) { Console.WriteLine("Ресурс был свободен."); }
                catch (ResAreBusy) { Console.WriteLine("Все ресурсы заняты."); }
            }
            while (Command != "");
        }
    }
}
