using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DecisionMatrix
{
    public class Program
    {
        private int _decisionSeq = 0;

        private int _choiceSeq = 0;

        private List<Decision> _decisions = new List<Decision>();

        private List<string> _addDecisionActions = new List<string>();

        private static string ACTION_ADD = "add";

        private static string ACTION_LINK = "link";

        private static string ACTION_SAVE = "save";

        private static string ACTION_LOAD = "load";

        private static string ACTION_CREATE = "create";

        static void Main(string[] args)
        {
            Console.WriteLine("Decision Matrix");
            Console.WriteLine(@"Add a decision with the following syntax: add decisionName:choice*[/]");
            Console.WriteLine("Link a decision to a choice: link idPathToChoiceDelSlash:decisionId ");
            Console.WriteLine("List all decisions with: list");
            Console.WriteLine("Create the whole decision table with: create [decisionDescription=true/false] [decisionId=true/false] ");
            Console.WriteLine("Write decisions to disk: save pathToFilesystem");
            Console.WriteLine("Load decisions from disk: load pathToFilesystem");
            Console.WriteLine("Remove all decisions: remove");
            Console.WriteLine("Remove decision: remove decisionId");
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
                _addDecisionActions.Add(rawInput);
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
            else if (rawInput.StartsWith(ACTION_CREATE))
            {
                CreateDecisionTable(rawInput);
            }
            else if (rawInput.StartsWith("remove"))
            {
                var keyWordAndPossibleId = rawInput.Split(" ");
                if (keyWordAndPossibleId.Length > 1)
                {
                    int.TryParse(keyWordAndPossibleId[1], out var decId);
                    RemoveDecision(decId);
                    return;
                }

                RemoveAllDecisions();
            }
            else if (rawInput.StartsWith(ACTION_SAVE))
            {
                Save(rawInput);
            }
            else if (rawInput.StartsWith(ACTION_LOAD))
            {
                Load(rawInput);
            }
            else
            {
                Console.WriteLine("Unknown command: " + rawInput);
            }
        }

        private void Save(string rawInput)
        {
            var path = rawInput.Remove(rawInput.IndexOf(ACTION_SAVE), ACTION_SAVE.Length).Trim();
            var aggregate = _addDecisionActions.Aggregate((s1, s2) => s1 + "\n" + s2);
            try
            {
                File.WriteAllText(path, aggregate);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("failed to write actions to disk " + e);
            }
        }

        private void Load(string rawInput)
        {
            var path = rawInput.Remove(rawInput.IndexOf(ACTION_LOAD), ACTION_LOAD.Length).Trim();
            var readLines = File.ReadLines(path);

            _decisions.Clear();
            foreach (var rl in readLines)
            {
                Dispatch(rl);
            }
        }

        private void AddDecision(string rawInput)
        {
            var d2c = rawInput.Split(":");
            var rawChoices = d2c[1].Split("/");
            var choices = rawChoices.Select(choice => new Choice() {_choice = choice, _id = _choiceSeq++}).ToList();

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
            Console.WriteLine("Removed all decisions");
        }

        private void RemoveDecision(int id)
        {
            var decision = _decisions.Find(dec => dec._decisionId == id);
            if (decision == null)
            {
                Console.WriteLine("Did not found decision with id " + id);
                return;
            }

            _decisions.Remove(decision);
            Console.WriteLine($"Removed decision {decision._description}");
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

        private void CreateDecisionTable(string rawInput)
        {
            var tab = new Dictionary<int, List<Choice>>();
            var i = 0;
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


            //var options = rawInput.Substring(rawInput.IndexOf(ACTION_CREATE), ACTION_CREATE.Length);
            for (int d = 0; d < tab.Count; d++)
            {
                var res = $"{_decisions[d]._description}({_decisions[d]._decisionId})|";
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

            return $"{_decisionId} : {_description}, \n\t choices: {cStr}";
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