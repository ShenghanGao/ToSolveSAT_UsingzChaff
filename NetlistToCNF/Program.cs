using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ECE255HW1
{
    class Program
    {
        static int NumOfLiteral = 0;
        static List<int> ListOfIn = new List<int>();
        static List<int> ListOfOut = new List<int>();
        static Dictionary<string, int> Literal = new Dictionary<string, int>();
        static List<gate> Gates = new List<gate>();
        static List<List<string>> GeneratedClauses = new List<List<string>>();
        static void Main(string[] args)
        {
            string file = "";
            string[] lines;

            string path = ".\\C7552.sim";
            string InputFileName = (path.Split('.'))[0];
            string OutputFileName = InputFileName + "ByC#Parser.cnf";

            //Open the stream and read it back.
            FileStream fs = File.OpenRead(path);
            long length = fs.Length;
            byte[] b = new byte[length];
            UTF8Encoding temp = new UTF8Encoding(true);
            while (fs.Read(b,0,b.Length) > 0)
            {
                file = temp.GetString(b);
            }
            
            lines = file.Split('\n');

            int No = 1;
            
            foreach (string line in lines)
            {
                if ((line == "") || (line == " ")) continue;
                int Empty = 1;
                string[] lineElements;
                lineElements = line.Split();
                foreach (string i in lineElements)
                {
                    i.Replace(" ", "");
                    if (i != "") Empty = 0;
                }
                if (Empty == 1) continue;
                int LiteralNo;

                if (lineElements[0] == "name")
                {
                    Console.WriteLine("The Name!\t" + lineElements[0]);
                }
                else if (lineElements[0] == "i")
                {
                    Console.WriteLine("An input!\t" + lineElements[0]);
                    LiteralNo = ListLiteral(lineElements[1], No);
                    if (LiteralNo == No)
                    {
                        No += 1;
                        NumOfLiteral += 1;
                    }
                    ListOfIn.Add(LiteralNo);
                }
                else if (lineElements[0] == "o")
                {
                    Console.WriteLine("An Output!\t" + lineElements[0]);
                    LiteralNo = ListLiteral(lineElements[1], No);
                    if (LiteralNo == No)
                    {
                        No += 1;
                        NumOfLiteral += 1;
                    }
                    ListOfOut.Add(LiteralNo);
                }
                else if (GateName(lineElements[0]))
                {
                    gate Gate = new gate();
                    Gate.GateName = lineElements[0];
                    Gate.GateType = lineElements[1];
                    List<string> tmp = new List<string>();
                    int i = 0;
                    foreach (string ele in lineElements)
                    {
                        if (i <= 1)
                        {
                            ++i; continue;
                        }
                        tmp.Add(lineElements[i++]);
                    }
                    Boolean input = true;
                    foreach (string ele in tmp)
                    {
                        if (ele == ";")
                        {
                            input = false; continue;
                        }
                        LiteralNo = ListLiteral(ele, No);
                        if (LiteralNo == No)
                        {
                            No += 1;
                            NumOfLiteral += 1;
                        }
                        if (input)
                            Gate.Input.Add(LiteralNo);
                        else
                            Gate.Output.Add(LiteralNo);
                    }
                    Gates.Add(Gate);
                    List<string> Clauses = new List<string>();
                    Clauses = GateCNF(Gate.GateType, Gate.Input, Gate.Output);
                    GeneratedClauses.Add(Clauses);
                }
            }

            foreach (var i in Literal)
            {
                Console.WriteLine("Key : {0}, Value : {1}", i.Key, i.Value);
            }

            Console.WriteLine("ListOfIn:");

            int a = 0;

            foreach (int i in ListOfIn)
            {
                Console.WriteLine(ListOfIn[a++]);
            }

            Console.WriteLine("ListOfOut:");

            a = 0;
            foreach (int i in ListOfOut)
            {
                Console.WriteLine(ListOfOut[a++]);
            }

            int ClauseCount = 1;

            int LiteralCount = Literal.Count();

            foreach (List<string> list in GeneratedClauses)
            {
                ClauseCount += list.Count();
            }

            string Comment = "c ";
            string ClauseEnd = " 0";
            string SecondLine;
            SecondLine = "p cnf " + LiteralCount + " " + ClauseCount;
            int count = 0;
            foreach (int Out in ListOfOut)
            {
                ++count;
                if (count == 6) break;
                for (int j = 0; j<2; ++j) //j=0 Check 1; j=1 Check 0
                {
                    OutputFileName = InputFileName + "(NumOfInputIs" + ListOfIn.Count() + ")" + "_Check_No" + count.ToString() + "(Is" + Out + ")_Output_To_Be" + (1 - j).ToString() + ".In_" + InputFileName + ",_for_No" + count + "_output,_check_if_it_is_" + (1 - j) + "_justifiable" + ".cnf";
                    if (File.Exists(OutputFileName))
                    {
                        File.Delete(OutputFileName);
                    }
                    fs = File.Create(OutputFileName);
                    AddText(fs, Comment + "Input File: " + path + "     Output File:" + OutputFileName + "\n");
                    AddText(fs, SecondLine + '\n');
                    AddText(fs, Comment + "Number of inputs: " + ListOfIn.Count() + "     List: \n");

                    string Line = "c";
                    foreach (int i in ListOfIn)
                    {
                        Line += (" " + i);
                    }
                    AddText(fs, Line + '\n');

                    AddText(fs, Comment + "Number of outputs: " + ListOfOut.Count() + "     List: \n");

                    Line = "c";
                    foreach (int i in ListOfOut)
                    {
                        Line += (" " + i);
                    }
                    AddText(fs, Line + '\n');

                    AddText(fs, "\n");
                    
                    string tmp = "";

                    if (j == 0) //j=0 Check 1;
                    {
                        tmp += (Out.ToString() + " 0\n");
                        AddText(fs, tmp);
                    }

                    if (j == 1) //j=1 Check 0;
                    {
                        tmp += ((-1 * Out).ToString() + " 0\n");
                        AddText(fs, tmp);
                    }

                    AddText(fs, "\n");

                    foreach (List<String> Clauses in GeneratedClauses)
                    {
                        foreach (string Clause in Clauses)
                        {
                            AddText(fs, Clause + ClauseEnd + '\n');
                        }
                    }
                }
            }
        }

        class gate
        {
            public string GateName = "";
            public string GateType = "";
            public List<int> Input = new List<int>();
            public List<int> Output = new List<int>();
        }

        private static void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        private static Boolean GateName(string txt)
        {
            Regex rgx = new Regex(@"^g[\w]+");
            return rgx.IsMatch(txt);
        }

        private static int ListLiteral(string txt, int id)
        {
            if (Literal.ContainsKey(txt))
                return Literal[txt];
            else
                Literal.Add(txt, id);
            return id;
        }

        private static List<string> GateCNF(string type, List<int> input, List<int> output)
        {
            List<string> ClauseList = new List<string>();
            string Clause;
            string Space = " ";

            int inputCount = input.Count();
            int outputCount = output.Count();

            if (type == "not")
            {
                Clause = input[0].ToString() + Space + output[0].ToString();
                ClauseList.Add(Clause);

                Clause = (-1 * input[0]).ToString() + Space + (-1 * output[0]).ToString();
                ClauseList.Add(Clause);
            }

            else if (type == "and")
            {
                Clause = "";
                foreach (int i in input)
                {
                    Clause += ((-1 * i).ToString() + Space);
                }
                Clause += output[0].ToString();
                ClauseList.Add(Clause);

                Clause = "";
                foreach (int i in input)
                {
                    Clause = i.ToString() + Space + (-1 * output[0]).ToString();
                    ClauseList.Add(Clause);
                }
            }

            else if (type == "or")
            {
                Clause = "";
                foreach (int i in input)
                {
                    Clause += i.ToString() + Space;
                }
                Clause += (-1 * output[0]).ToString();
                ClauseList.Add(Clause);

                Clause = "";
                foreach (int i in input)
                {
                    Clause = (-1 * i).ToString() + Space + output[0].ToString();
                    ClauseList.Add(Clause);
                }
            }

            else if ((type == "nand") && (inputCount == 2) && (outputCount == 1))
            {
                Clause = (-1 * input[0]).ToString() + Space + (-1 * input[1]).ToString() + Space + (-1 * output[0]).ToString();
                ClauseList.Add(Clause);

                Clause = input[0].ToString() + Space + output[0].ToString();
                ClauseList.Add(Clause);

                Clause = input[1].ToString() + Space + output[0].ToString();
                ClauseList.Add(Clause);
            }

            else if ((type == "nor") && (inputCount == 2) && (outputCount == 1))
            {
                Clause = input[0].ToString() + Space + input[1].ToString() + Space + output[0].ToString();
                ClauseList.Add(Clause);

                Clause = (-1 * input[0]).ToString() + Space + (-1 * output[0]).ToString();
                ClauseList.Add(Clause);

                Clause = (-1 * input[1]).ToString() + Space + (-1 * output[0]).ToString();
                ClauseList.Add(Clause);
            }

            else if ((type == "xor") && (inputCount == 2) && (outputCount == 1))
            {
                Clause = input[0].ToString() + Space + input[1].ToString() + Space + (-1 * output[0]).ToString();
                ClauseList.Add(Clause);

                Clause = input[0].ToString() + Space + (-1 * input[1]).ToString() + Space + output[0].ToString();
                ClauseList.Add(Clause);

                Clause = (-1 * input[0]).ToString() + Space + input[1].ToString() + Space + output[0].ToString();
                ClauseList.Add(Clause);

                Clause = (-1 * input[0]).ToString() + Space + (-1 * input[1]).ToString() + Space + (-1 * output[0]).ToString();
                ClauseList.Add(Clause);
            }

            else throw new Exception("Gate Undefined!");

            return ClauseList;
        }
    }
}
