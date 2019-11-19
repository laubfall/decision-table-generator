using System;
using System.Collections.Generic;
using System.Linq;

namespace DecisionMatrix
{
    public class Program
    {
        private int _decisionSeq = 0;

        private int _choiceSeq = 0;

        private List<Decision> _decisions = new List<Decision>();

        private static string ACTION_ADD = "add";

        private static string ACTION_LINK = "link";

        static void Main(string[] args)
        {
            Console.WriteLine("Decision Matrix");
            Console.WriteLine(@"Add a decision with the following syntax: add decisionName:choice*[/]");
            Console.WriteLine("Link a decision to a choice: link idPathToChoiceDelSlash:decisionId ");
            Console.WriteLine("List all decisions with: list");
            Console.WriteLine("Remove all decisions: remove");
            Console.WriteLine("Exit application: exit");
            var prog = new Program();

            do
            {
                prog.Dispatch(Console.ReadLine());
            } while (true);
        }

        public void Dispatch(string rawInput)
        {
            if (rawInput.StartsWith(ACTION_ADD))
            {
                var indexOf = rawInput.IndexOf(ACTION_ADD);
                AddDecision(rawInput.Substring(indexOf + ACTION_ADD.Length));
            }
            else if (rawInput.StartsWith(ACTION_LINK))
            {
                var indexOf = rawInput.IndexOf(ACTION_LINK);
                Link(rawInput.Substring(indexOf + ACTION_LINK.Length));
            }
            else if (rawInput.StartsWith("list"))
            {
                List();
            }
            else if (rawInput.StartsWith("create"))
            {
                CreateDecisionTable();
            }
            else if (rawInput.StartsWith("remove"))
            {
                RemoveAllDecisions();
            }
        }

        private void AddDecision(string rawInput)
        {
            var d2c = rawInput.Split(":");
            var rawChoices = d2c[1].Split("/");
            var choices = new List<Choice>();

            foreach (var choice in rawChoices)
            {
                choices.Add(new Choice()
                {
                    _choice = choice,
                    _id = _choiceSeq++
                });
            }

            var dec = new Decision()
            {
                _decisionId = _decisionSeq++,
                _description = d2c[0],
                _Choices = choices
            };

            _decisions.Add(dec);
            Console.WriteLine("Decision: " + dec);
        }

        private void RemoveAllDecisions()
        {
            _decisions.Clear();
        }

        private void Link(string rawInput)
        {
            var p2d = rawInput.Split(":");
            var pathParts = p2d[0].Split("/");

            foreach (var dec in _decisions)
            {
                Choice choice = null;
                foreach (var part in pathParts)
                {
                    choice = FindForId(dec, int.Parse(part));

                    if (choice == null)
                    {
                        break;
                    }
                }

                if (choice != null)
                {
                    choice._when = _decisions.Find(d => d._decisionId == int.Parse(p2d[1]));
                    Console.WriteLine($"Added decision {choice._when} to choice {choice}");
                    break;
                }
            }
        }

        private void CreateDecisionTable()
        {
            Dictionary<int, List<Choice>> tab = new Dictionary<int, List<Choice>>();

            int i = 0;
            foreach (var dec in _decisions)
            {
                var l = new List<Choice>(dec._Choices);

                tab.Add(i, l);

                if (i > 0)
                {
                    for (int iCnt = i; iCnt > 0; iCnt--)
                    {
                        tab[iCnt - 1] = DoubleElements(tab[iCnt - 1], l.Count - 1);
                    }

                    var lCnt = (tab[i - 1].Count / l.Count);
                    var tmp = new List<Choice>(l);
                    for (int ln = 1; ln < lCnt; ln++)
                    {
                        l.AddRange(tmp);
                    }
                }

                i++;
            }

            for (int d = 0; d < tab.Count; d++)
            {
                var res = "|";
                foreach (var choice in tab[d])
                {
                    res += choice._choice;
                    res += "|";
                }

                ;
                Console.WriteLine(res);
            }
        }

        private void List()
        {
            _decisions.ForEach(Console.WriteLine);
        }

        private Choice FindForId(Decision dec, int id)
        {
            return dec._Choices.Find(c => c._id == id);
        }

        private List<T> DoubleElements<T>(List<T> doubleThis, int count = 1)
        {
            var res = new List<T>();
            foreach (var dt in doubleThis)
            {
                // add the original element
                res.Add(dt);
                
                // and now the wanted count of copies
                for (int i = 0; i < count; i++)
                {
                    res.Add(dt);
                }
            }

            return res;
        }
    }


    class Decision
    {
        public int _decisionId { get; set; }

        public string _description { get; set; }

        public List<Choice> _Choices { get; set; } = new List<Choice>();

        public override string ToString()
        {
            var cStr = "";
            foreach (var choice in _Choices)
            {
                cStr += choice;
                cStr += "\n";
            }

            return $"{_decisionId} : {_description}, choices: {cStr}";
        }
    }

    class Choice
    {
        public int _id { get; set; }

        public string _choice { get; set; }

        public Decision _when { get; set; }

        public override string ToString()
        {
            return $"{_id} : {_choice}";
        }
    }
}