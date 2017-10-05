using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace stratcon
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("");Console.WriteLine("");
            Console.WriteLine("**************************");
            Console.WriteLine(" Stratification Tree v0.1");
            Console.WriteLine("**************************");
            
            var s0 =
                new StratTerm[] {
                    new StratTerm { variable = "z", condition = StratTermVal.gt, constant = "50"},
                    new StratTerm { variable = "y", condition = StratTermVal.lte, constant = "20"},
                    new StratTerm { variable = "x", condition = StratTermVal.gt, constant = "10"}
                    
                };
                
            var p0 = 
                new Dictionary<string,string> [] {
                    new Dictionary<string, string>{ //T
                        { "x", "11" },
                        { "y", "20" },
                        { "z", "51" }
                    },
                    new Dictionary<string, string>{ //F
                        { "x", "10" },
                        { "y", "21" },
                        { "z", "50" }
                    }
                };

            var s1 =
                new StratTerm[] {
                    new StratTerm { variable = "q", condition = StratTermVal.gt, constant = "10"},
                    new StratTerm { variable = "z", condition = StratTermVal.lte, constant = "30"},
                    new StratTerm { variable = "x", condition = StratTermVal.lte, constant = "20"}
                };
            var p1 = 
                new Dictionary<string,string> [] {
                    new Dictionary<string, string>{ //T
                        { "q", "11" },
                        { "z", "30" },
                        { "x", "20" }
                    },
                    new Dictionary<string, string>{ //F
                        { "q", "10" },
                        { "z", "31" },
                        { "x", "21" }
                    }
                };

            var s2 =
                new StratTerm[] {
                    s1[0],
                    s1[1],
                    new StratTerm { variable = "x", condition = StratTermVal.gt, constant = "20"}
                };
            var p2 = 
                p1;

/* 
            var s1 =
                new StratTerm[] {
                    new StratTerm { variable = "lot_sqft", condition = StratTermVal.gt, constant = "5000"},
                    new StratTerm { variable = "mfla_sqft", condition = StratTermVal.gt, constant = "2200"},
                    new StratTerm { variable = "usd", condition = StratTermVal.inlist, constant = "A,B"}
                };

            var s2 =
                new StratTerm[] {
                    new StratTerm { variable = "lot_sqft", condition = StratTermVal.gt, constant = "3000"},
                    new StratTerm { variable = "mfla_sqft", condition = StratTermVal.gt, constant = "1200"},
                    new StratTerm { variable = "usd", condition = StratTermVal.inlist, constant = "C,D"}
                };

            var s3 =
                new StratTerm[] {
                    new StratTerm { variable = "lot_sqft", condition = StratTermVal.gt, constant = "3000"},
                    new StratTerm { variable = "lot_sqft", condition = StratTermVal.lte, constant = "5000"},
                    new StratTerm { variable = "mfla_sqft", condition = StratTermVal.gt, constant = "1200"},
                    new StratTerm { variable = "mfla_sqft", condition = StratTermVal.lte, constant = "2200"},
                    new StratTerm { variable = "usd", condition = StratTermVal.inlist, constant = "A,B"}
                };   
*/
            var f0 = new Finder();

            f0.AddStataDef("s0",s0);

            f0.RenderToConsole("s0");

            var r0 = new string [] {
                    f0.FindStrata(p0[0]),
                    f0.FindStrata(p0[1]),
            };
            
            Debug.Assert(r0[0] == "s0", "strata s1 was not returned");
            Debug.Assert(r0[1] == null, "a null strata should have been returned");

            var f1 = new Finder();
            f1.AddStataDef("s1",s1);

            f1.RenderToConsole("s1");

            var r1 = new string [] {
                    f1.FindStrata(p1[0]),
                    f1.FindStrata(p1[1]),
            };

            Debug.Assert(r1[0] == "s1", "strata s1 was not returned");
            Debug.Assert(r1[1] == null, "a null strata should have been returned");

            var f2 = new Finder();
            f2.AddStataDef("s2",s2);

            f2.RenderToConsole("s2");


            var r2 = new string [] {
                    f2.FindStrata(p2[0]),
                    f2.FindStrata(p2[1]),
            };


            return;
        }
    }


    struct StratTermVal 
    {
        public const string gt =">";
        public const string lte ="<=";
        public const string inlist ="IN";
    }

    public struct StratTerm 
    {
        public string variable;
        public string condition;
        public string constant;
        public string RenderAsString() 
        { 
            return variable + " " + condition + " " + constant;
        } 

        public bool TryEval(string val, out bool result)
        {
            double dval=0;
            double dconst=0;
            bool isValNumeric = Double.TryParse(val, out dval);
            bool isConstNumeric = Double.TryParse(constant, out dconst);
            result = false;

            switch(condition) 
            {
                case StratTermVal.gt:
                case StratTermVal.lte:
                    if (isConstNumeric == false || isValNumeric == false)
                        return false;
                    else 
                        goto default;//can't miss a shot to throw in goto in 2017 LOL
                default:
                    switch(condition) 
                        {
                            case StratTermVal.gt:
                                result = dval > dconst;
                                return true;
                            case StratTermVal.lte:
                                result = dval <= dconst;
                                return true;
                            case StratTermVal.inlist: //IN not yet supported 
                            default:
                                return false;
                        }    
            }
        }   
    }

    
    public class StratTree 
    {
        private string refName;

        public StratTree(string refname, StratTerm st)
        {
            refName = refname;
            Term = st;
            Left = null;
            Right = null;
            Parent = null;
        }
        public StratTerm Term { get; private set;}
        public StratTree Left { get; private set;}
        public StratTree Right { get; private set;}
        public StratTree Parent { get; private set;}
        
        public bool TryEval(Dictionary<string,string> parcelData, out bool strataWasFound, out string resultStrata)
        {
            StratTree target = this;
            string val;
            bool result=false;

            while(true)
            {//walk tree
                if ( parcelData.TryGetValue( target.Term.variable, out val ) )
                {//there is a condition for this variable, we can check it
                    result=false;
                    if( target.Term.TryEval(val, out result))
                    {//evaluation succeeded
                        if (result) 
                        {//eval'd to T 
                            if (target.Right != null)
                            {//go right
                                target = target.Right;
                                continue;
                            }
                            else
                            {//leaf
                                break;
                            }
                        }
                        else
                        {//eval'd to F
                            if (target.Left != null)
                            {//go left
                                target = target.Left;
                                continue;
                            }
                            else
                            {//leaf
                                break;
                            }
                        }
                    }
                    else
                    {//evaluation failed
                        resultStrata=null;
                        strataWasFound=false;
                        //this is an error; something went wrong in the evaluation of a tree node ie. data type error in operations
                        return false;
                    }
                    
                }
                else
                {//nothing to check; truth
                    if (target.Right != null)
                    { //nothing to check; must be true; go right
                        target = target.Right;
                        continue;
                    }
                    else 
                    {//nothing to check; hit leaf; done, return strata 
                        break;
                    }
                }
            }//end tree walk

            strataWasFound=result;
            if (result)
            //last node visit Eval'd to T 
                resultStrata = target.refName;
            else
            //last node visit Eval'd to F
                resultStrata = null;

            return true;
        }

        public void InsertAtTop( StratTree node)
        {//insert a new decision tree node at the top
            StratTree target = this;
            Debug.Assert( node.Left == null && node.Right == null, "Not leaf node passed as node");
            Debug.Assert( target.Parent == null , "Called on not root node");

            //DRY locals
            void SwapNodeData()
            {
                StratTerm t = node.Term; //struct so copy by value
                node.Term = target.Term;
                target.Term = t;  
                string n = node.refName;
                node.refName = target.refName;
                target.refName = n;
            }
            
            void SwapNodeLinks()
            {//node is the initial root, target is now the item being inserted on top, since SwapNodeData is called before this
                node.Parent = target;
                node.Right = target.Right;
                node.Left = target.Left;
                target.Right = node;
                target.Left = node.Copy();
            }


            if (String.Compare(target.Term.variable, node.Term.variable) < 0)
            {//target-var precedes node-var in the sort order
                SwapNodeData();
                SwapNodeLinks();
            }
            else if(String.Compare(target.Term.variable, node.Term.variable) == 0)
            {//same var
                SwapNodeData();
                SwapNodeLinks();
                
                /* 
                    the <= / lte are sorted in order of decreasing constant
                    x < 10
                    x < 5

                **
                    the > / gt are sorted in order of increasing constant
                    x > 8
                    x > 2

                ** 
                    x < 10 
                    x > 8 
                    x < 5
                    x > 2
                */
            }
            else throw new Exception("Variable names must be inserted in Sorted Order"); 
        }

        public StratTree Copy()
        {
            StratTree target = this;
            StratTree copy = new StratTree(target.refName, target.Term);

            if (target.Right != null)
            {
                copy.Right = target.Right.Copy();
                copy.Right.Parent = copy;
            }

            if(target.Left != null)
            {
                copy.Left = target.Left.Copy();
                copy.Left.Parent = copy;
            }

            return copy;
        }

        public void InsertSort( StratTree node)
        {
            StratTree target = this;

            //DRY utils
            void SwapNodeData()
            {
                StratTerm t = node.Term; //struct so copy by value
                node.Term = target.Term;
                target.Term = t;  
                string n = node.refName;
                node.refName = target.refName;
                target.refName = n;
            }
            
            while(true) 
            {
                if(node.Term.variable == target.Term.variable)
                {//case: same var

                    if (node.Term.condition != target.Term.condition)
                    {//same var, diff cond
                        if (target.Right != null) 
                        {
                            target = target.Right;
                            continue;
                        }
                        else
                        {
                            target.Right = node;
                            break;
                        }
                    }
                    else
                    {//same var, same cond
                        if (node.Term.condition == StratTermVal.gt ||
                            node.Term.condition == StratTermVal.lte)
                        {//numer constant
                            double nodec=0.0;
                            double targc=0.0;
                            try 
                            { 
                                nodec = Double.Parse(node.Term.constant);
                                targc = Double.Parse(target.Term.constant); 
                            }
                            catch 
                            {
                                break; //skip invalid - correctness requires aprior validation i.e no type op issues
                            }
                            if (target.Right != null) 
                            {
                                target = target.Right;
                                continue;
                            }
                            else
                            {
                                target.Right = node;
                                break;
                            }
                        }
                        else
                        {//categ constant i.e. IN { A,B }
                            //ignored
                        }
                    }  
                }
                else
                {//case: diff var
                    if (target.Right != null) 
                    {//case: interior node
                        if (String.Compare(target.Term.variable, node.Term.variable) > 0)
                        {//target.refName follows node.refName in the sort order
                            target = target.Right;
                        }
                        
                        else
                        {
                            target = target.Right;
                        }

                        continue;
                    }
                    else
                    {//case: leaf node
                        if (String.Compare(target.Term.variable, node.Term.variable) > 0)
                        {//target.refName follows node.refName in the sort order
                        // so switch target and node i.e. target ends up being the right leaf
                            if (target.Parent == null)
                            {//data swap if target is root
                                SwapNodeData();
                                target.Right = node;
                                node.Parent = target;    
                            }
                            else
                            {
                                target.Parent.Right = node;
                                node.Parent = target.Parent;
                                node.Right = target;
                            }
                            
                        }
                        else
                        {//just add as a right leaf of target
                            target.Right = node;
                            node.Parent = target;
                        }
                        
                        break;
                    }
                }
            }   
        }

        
        public void InsertSingleStrata( StratTree node) //for verification
        {
            StratTree target = this;
            while(true) 
            {
                if(node.Term.variable == target.Term.variable)
                {//case: same var

                    if (node.Term.condition != target.Term.condition)
                    {//same var, diff cond
                        if (target.Right != null) 
                        {
                            target = target.Right;
                            continue;
                        }
                        else
                        {
                            target.Right = node;
                            break;
                        }
                    }
                    else
                    {//same var, same cond
                        if (node.Term.condition == StratTermVal.gt ||
                            node.Term.condition == StratTermVal.lte)
                        {//numer constant
                            double nodec=0.0;
                            double targc=0.0;
                            try 
                            { 
                                nodec = Double.Parse(node.Term.constant);
                                targc = Double.Parse(target.Term.constant); 
                            }
                            catch 
                            {
                                break; //skip invalid - correctness requires aprior validation i.e no type op issues
                            }
                            if (target.Right != null) 
                            {
                                target = target.Right;
                                continue;
                            }
                            else
                            {
                                target.Right = node;
                                break;
                            }
                        }
                        else
                        {//categ constant i.e. IN { A,B }
                            //ignored
                        }
                    }  
                }
                else
                {//case: diff var
                    if (target.Right != null) 
                    {//case: interior node
                        target = target.Right;
                        continue;
                    }
                    else
                    {//case: leaf node
                        target.Right = node;
                        break;
                    }
                }
            }   
        }
        public void Traverse( StratTree node, int level)
        {
            if (node.Left !=null)
                Traverse(node.Left, level+1);

            Console.WriteLine(node.Term.RenderAsString());


            if (node.Right != null) 
                Traverse(node.Right, level+1);

        }        
    }
    
    public interface IFinder
    {
        string FindStrata(Dictionary<string,string> parcelData);
        void AddStataDef(string name, StratTerm [] terms);
    }

    public class Finder 
    {
        private StratTree root = null;
        private List<StratTerm> allStrata = new List<StratTerm>(); 

        public string FindStrata(Dictionary<string,string> parcelData)
        {
            string strata=null;
            bool strataWasFound=false;

             if (root != null)
                root.TryEval(parcelData, out strataWasFound, out strata);

             return strata;
        }
        public void AddStataDef(string name, StratTerm [] terms)
        {
            int termCompare(StratTerm x, StratTerm y)
            {
                bool IsNumeric(string value, out double result)
                {
                    return Double.TryParse(value, out result);
                }

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
                            Debug.Assert( isXaNumeric == false, "Type error: expected a 'constant' double for <= and > operands" );
                            if ( isXaNumeric )
                                goto default;
                            else throw new FormatException("Type error: expected a 'constant' double for <= and > operands");   
                        default:                        
                            switch(y.condition) 
                                {
                                    case StratTermVal.gt:
                                    case StratTermVal.lte:
                                        Debug.Assert( isYaNumeric == false, "Type error: expected a 'constant' double for <= and > operands" );
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

            var l = new List<StratTerm>(terms);
            l.Sort( termCompare );
        }
        public void ProcessAllStrataDefs()
        {
            string name;
            StratTerm [] terms;
            terms = new StratTerm[4];
            name = "";

            //above all hacked to compile
            int cnt = terms.Length;
            if (cnt<1) return;//return with no change for empty strata conditions

            int start=0;
            if (root==null)
                root = new StratTree(name, terms[start++]);

            for(int i=start; i<cnt; i++)
            {
                var n = new StratTree(name, terms[i]);
                root.InsertSingleStrata(n);
            }
        }



        public void RenderToConsole(string title)
        {
            if(title!=null)
                {
                    Console.WriteLine("");Console.WriteLine("");
                    Console.WriteLine(title);Console.WriteLine("");                   
                }
            if (root != null)
                root.Traverse(root,0);
        }
    }
}


