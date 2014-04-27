using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Seminare
{
    internal class Program
    {
        public static readonly string[] key = { "AJ", "FJ", "D", "Z", "F", "CH", "NJ", "HIGE", "EVV" };

        private static void Main(string[] args)
        {
            Console.Title = "Seminare";
            Console.ForegroundColor = ConsoleColor.White;

            List<string> combos = new List<string>();
            List<string[]> input = new List<string[]>();
            List<string> altSolutions = new List<string>();

            Dictionary<string, int> numOfPeople = new Dictionary<string, int>();

            int lowestScore = 2147483647;
            int topNum = 2147483647;
            string lowestCombo = "";

            ParseIO(out combos, out input);

            Console.WriteLine("Pocet lidi: " + input.Count + "\n");
            DateTime before = DateTime.Now;

            for (int i = 0; i < combos.Count; i++)
            {
                int currentScore = 0;
                int topScore = 0;
                Dictionary<string, int> tempDict = new Dictionary<string, int>();
                for (int n = 0; n < 9; n++)
                {
                    tempDict.Add(key[n], int.Parse(combos[i][n].ToString()));
                }

                foreach (string[] s in input)
                {
                    List<int> temp;
                    int tempScore = CountScore(tempDict, s, out temp);
                    currentScore += tempScore;
                    if (tempScore > topScore)
                    {
                        topScore = tempScore;
                    }
                }

                if (lowestScore > currentScore)
                {
                    lowestScore = currentScore;
                    lowestCombo = combos[i];
                    topNum = topScore;
                }
                else if (lowestScore == currentScore && topNum > topScore) //rozprostreni konfliktu pokud mozno rovnomerne
                {
                    lowestScore = currentScore;
                    lowestCombo = combos[i];
                    topNum = topScore;
                }
                else if (lowestScore == currentScore && topNum == topScore) //alternativni reseni
                {
                    altSolutions.Add(currentScore + " " + topScore + " " + combos[i]);
                }
                if (lowestScore == 0)
                {
                    break;
                }
            }
            DateTime after = DateTime.Now;
            TimeSpan diff = after.Subtract(before);
            List<string> totalSolutions = new List<string>();
            totalSolutions.Add(lowestCombo);

            foreach (string s in altSolutions)
            {
                string[] temp = s.Split(' ');
                //1. score
                //2. nejvyssi score
                if (int.Parse(temp[0]) == lowestScore && int.Parse(temp[1]) == topNum)
                {
                    //jsou tri bloky seminaru, tudiz je vzdycky 3! permutaci tech bloku -> tady se to eliminuje
                    string[] original = ParseTable(temp[2]);
                    bool isThere = false;
                    foreach (string combo in totalSolutions)
                    {
                        string[] tempTable = ParseTable(combo);
                        if (tempTable.Intersect(original).ToArray().Length == 3)
                        {
                            isThere = true;
                            break;
                        }
                    }
                    if (!isThere)
                    {
                        totalSolutions.Add(temp[2]);
                    }
                }
            }

            int view = 0, maxView = totalSolutions.Count - 1;
            ConsoleKey conKey = ConsoleKey.A;

            do
            {
                if (maxView != 0)
                {
                    view = Scroll(view, maxView, conKey);

                    Console.Clear();
                    Console.WriteLine("Reseni: {0}/{1}", (view + 1), (maxView + 1));
                    Console.WriteLine("Pocet lidi: " + input.Count + "\n");
                }

                //Vypis jednotlivych seznamu a jejich skore
                OutInputs(input, totalSolutions[view], ref numOfPeople);

                //Vysledky
                Console.WriteLine("\nCelkem konfliktu: {0}\nPrumer: {1}", lowestScore,
                    (double)lowestScore / (double)input.Count);
                WriteTable(totalSolutions[view], numOfPeople);

                Console.WriteLine("\nRychlost: " + diff.TotalSeconds + "s");
                Console.SetWindowPosition(0, 0);

                if (maxView == 0)
                {
                    Console.ReadKey(true);
                }
                else
                {
                    conKey = Console.ReadKey(true).Key;
                }
            }
            while (conKey == ConsoleKey.LeftArrow || conKey == ConsoleKey.RightArrow);
        }

        private static int Scroll(int current, int max, ConsoleKey key)
        {
            if (key == ConsoleKey.LeftArrow)
            {
                if (current == 0)
                {
                    current = max;
                }
                else
                {
                    current--;
                }
            }
            else if (key == ConsoleKey.RightArrow)
            {
                if (current == max)
                {
                    current = 0;
                }
                else
                {
                    current++;
                }
            }
            return current;
        }

        private static void OutInputs(List<string[]> input, string combo, ref Dictionary<string, int> numOfPeople)
        {
            Dictionary<string, int> lowestDict = new Dictionary<string, int>();
            for (int n = 0; n < 9; n++)
            {
                lowestDict.Add(key[n], int.Parse(combo[n].ToString()));
            }
            numOfPeople.Clear();
            for (int i = 0; i < key.Length; i++)
            {
                numOfPeople.Add(key[i], 0);
            }
            for (int n = 0; n < input.Count; n++)
            {
                string[] s = input[n];
                List<int> chosen;

                int konflikty = CountScore(lowestDict, s, out chosen);
                if (konflikty == 1)
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                }
                else if (konflikty > 1)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                }
                Console.Write("{0}:", input[n][0]); //jmeno
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" ");
                for (int i = 1; i < s.Length; i++)
                {
                    if (chosen.Contains(i))
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        numOfPeople[s[i]]++;
                    }
                    Console.Write(s[i]);
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.White;
                    if (i != s.Length - 1)
                    {
                        Console.Write(", ");
                    }
                }

                //Console.Write("\n     ");
                Console.Write(" ");
                for (int i = 0; i < 12 - input[n][0].Length; i++)
                {
                    Console.Write(" ");
                }
                if (konflikty == 1)
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                }
                else if (konflikty > 1)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                }
                Console.WriteLine("konfliktu: " + konflikty);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void WriteTable(string combo, Dictionary<string, int> numOfPeople)
        {
            string[] table = ParseTable(combo);
            Console.WriteLine();
            Console.WriteLine("Seminar 1: " + table[0]);
            Console.WriteLine("Seminar 2: " + table[1]);
            Console.WriteLine("Seminar 3: " + table[2]);
            Console.WriteLine();
            for (int i = 0; i < key.Length; i++)
            {
                Console.WriteLine("{0}: {1}", key[i], numOfPeople[key[i]]);
            }
        }

        private static string[] ParseTable(string combo)
        {
            string a = "";
            string b = "";
            string c = "";

            for (int i = 0; i < combo.Length; i++)
            {
                switch (int.Parse(combo[i].ToString()))
                {
                    case 0:
                        a += key[i] + " ";
                        break;

                    case 1:
                        b += key[i] + " ";
                        break;

                    case 2:
                        c += key[i] + " ";
                        break;
                }
            }
            return new string[] { a, b, c };
        }

        private static int CountScore(Dictionary<string, int> current, string[] input, out List<int> chosen)
        {
            List<int> used = new List<int>();
            int score = 0;
            chosen = new List<int>();

            for (int i = 1; i < input.Length; i++)
            {
                if (used.Count == 3)
                {
                    score = i - 1;
                    break;
                }
                else if (!used.Contains(current[input[i]]))
                {
                    used.Add(current[input[i]]);
                    chosen.Add(i);
                }
            }
            return score - 3;
            //Tri iterace jsou minimum, tudiz je skore 3 minimalni
        }

        private static void ParseIO(out List<string> combos, out List<string[]> inputs)
        {
            string path = @"output.txt";
            //output.txt je jenom vygenerovany seznam vsech permutaci retezce "000111222"
            try
            {
                StreamReader s = new StreamReader(path);
                combos = new List<string>();

                string line = s.ReadLine();
                while (line != null)
                {
                    for (int i = 0; i < line.Length; i += 9)
                    {
                        combos.Add(line.Substring(i, 9));
                    }
                    line = s.ReadLine();
                }
                //eliminace duplikovanych vstupu. Neni treba, jestli je ten soubor uz zpracovany
                combos = combos.Distinct().ToList();

                s.Close();

                /*vygenerovani souboru bez duplikovanych vstupu. Staci vygenerovat jednou

                 StreamWriter wr = new StreamWriter("combos.txt");
                 for (int i = 1; i <= combos.Count; i++)
                 {
                     wr.Write(combos[i-1]);
                 }
                 wr.Close();*/
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Soubor output.txt nenalezen.");
                combos = null;
                inputs = null;
                Console.ReadKey(true);
                Environment.Exit(0);
            }
            try
            {
                //input.txt jsou jednotlivy seznamy predmetu
                path = @"input.txt";

                StreamReader s = new StreamReader(path);
                inputs = new List<string[]>();
                string line = s.ReadLine();
                while (line != null)
                {
                    line = line.Replace(" ", "");
                    string[] temp = line.Split(',');
                    if (temp.Length == 9) //jestli neni zadany jmeno
                    {
                        string[] newTemp = new string[10];
                        newTemp[0] = "ANONYM";
                        for (int i = 1; i < 10; i++)
                        {
                            newTemp[i] = temp[i - 1];
                        }
                        temp = newTemp;
                    }
                    for (int i = 1; i < temp.Length; i++)
                    {
                        temp[i] = temp[i].ToUpper();
                    }
                    inputs.Add(temp);
                    line = s.ReadLine();
                }
                s.Close();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Soubor input.txt nenalezen.");
                combos = null;
                inputs = null;
                Console.ReadKey(true);
                Environment.Exit(0);
            }
        }
    }
}