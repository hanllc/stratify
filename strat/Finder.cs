using System;
using System.Collections.Generic;
using System.Diagnostics;

 
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Tyler.Avm.StratifyTest")]


namespace Tyler.Avm.Stratify
{
    
    public interface IFinder
    {
        void AddStataDef(string name, StratTerm [] terms);
        void Preprocess();
        string FindStrata(Dictionary<string,string> parcelData);//improve this signature
    }

    public class BasicFinder : IFinder
    {
        protected List<StratTree> stratForest = new List<StratTree>(); 
        protected Dictionary<string, StratTerm[]> stratCatalog = new Dictionary<string, StratTerm[]>();
        virtual public string FindStrata(Dictionary<string,string> parcelData)
        {
            string strata;
            bool strataWasFound=false;
            foreach(StratTree t in stratForest)
            {
                if ( t.TryEval(parcelData, out strataWasFound, out strata) )
                    return strata; //FINDS FIRST ONE ONLY

            }
            return null;
        }
        virtual public void AddStataDef(string name, StratTerm [] terms)
        {
            if ( stratCatalog.ContainsKey(name) )
                stratCatalog.Remove(name);//adding the same name replaces the existing

            stratCatalog.Add(name, terms);
        }
        virtual public void Preprocess()
        {
            stratForest.Clear();
            foreach (KeyValuePair<string, StratTerm[]> kvp in stratCatalog)
            {
                string sn = kvp.Key;
                StratTerm[] ta = kvp.Value;
                StratTree root = null;
                
                for(int i=0; i<ta.Length; i++)
                {
                    if(root==null)
                        root = new StratTree(sn, ta[0]);
                    else
                        root.InsertSingleStrata(new StratTree(sn, ta[i]));//for a forest of single decision trees
                }
                stratForest.Add(root);
            }
        }
        virtual public void RenderToText(System.IO.TextWriter twriter)
        {

            Action<StratTree> nodeAction = 
                (nodeArg) => { twriter.WriteLine(nodeArg.Term.RenderAsString()); };

            foreach (StratTree t in stratForest)
            {
                twriter.WriteLine("");twriter.WriteLine("");
                twriter.WriteLine("BEGIN: " + t.StratRef);twriter.WriteLine("");
                t.Traverse( t, 0, nodeAction);
                twriter.WriteLine("");
                twriter.WriteLine("END: " + t.StratRef);twriter.WriteLine("");    
            }
        }
    }

    public class Finder : BasicFinder
    {
        override public string FindStrata(Dictionary<string,string> parcelData)
        {
            return null;//LEFT OFF HERE
        }

        override public void Preprocess()
        {
            stratForest.Clear();
            
            var l = new List<Tuple<string, StratTerm>>();

            foreach (KeyValuePair<string, StratTerm[]> kvp in stratCatalog)
            {
                string sn = kvp.Key;
                StratTerm[] ta = kvp.Value;                
                for(int i=0; i<ta.Length; i++)
                    l.Add( new Tuple<string, StratTerm>(sn,ta[i]) );
            }
            var sl = SortTermList(l);

            bool first=true;
            StratTree root = null;
            foreach (Tuple<string, StratTerm> tuple in sl)
            {   
                if(first)
                    root = new StratTree(tuple.Item1, tuple.Item2);
                else
                    root.InsertAtTop(new StratTree(tuple.Item1, tuple.Item2));//for a single composite decision tree
 
                if (first && root != null) 
                    stratForest.Add(root);

                first=false;
            }
            
            Debug.Assert(stratForest.Count == 1 || stratForest.Count == 0, "Bad forest size in advanced Finder class");
        }
        private List<Tuple<string, StratTerm>> SortTermList(List<Tuple<string, StratTerm>> l)
        {   
            int termCompare(Tuple<string, StratTerm> xt, Tuple<string, StratTerm> yt)
            {
                bool IsNumeric(string value, out double result)
                {
                    return Double.TryParse(value, out result);
                }

                StratTerm x = xt.Item2;
                StratTerm y = yt.Item2;

                int d = x.variable.CompareTo(y.variable); 

                if (d != 0) return d;
                else 
                {
                    double xd, yd;
                    bool isXaNumeric = IsNumeric(x.constant, out xd );
                    bool isYaNumeric = IsNumeric(y.constant, out yd );
                    switch(x.condition) 
                    {
                        case StratTermVal.gt:
                        case StratTermVal.lte:
                            Debug.Assert( isXaNumeric == true, "Type error: expected a 'constant' double for <= and > operands" );
                            if ( isXaNumeric )
                                goto default;
                            else throw new FormatException("Type error: expected a 'constant' double for <= and > operands");   
                        default:                        
                            switch(y.condition) 
                                {
                                    case StratTermVal.gt:
                                    case StratTermVal.lte:
                                        Debug.Assert( isYaNumeric == true, "Type error: expected a 'constant' double for <= and > operands" );
                                        if ( isYaNumeric )
                                            goto default;
                                        else throw new FormatException("Type error: expected a 'constant' double for <= and > operands");
                                    default:
                                        if ( xd - yd < 0.0 - 0.00000001)
                                            return -1;
                                        else if (xd - yd > 0.0 + 0.00000001)
                                            return 1;
                                        else 
                                            return 0;                      
                                }
                    }

                }
            }
            l.Sort( termCompare );
            return l;
        }
    }
}
