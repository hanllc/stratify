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
                    new StratTerm { variable = "x", condition = StratTermVal.gt, constant = "10"},
                    new StratTerm { variable = "y", condition = StratTermVal.lte, constant = "20"},
                    new StratTerm { variable = "z", condition = StratTermVal.gt, constant = "50"}
                };
            var s1 =
                new StratTerm[] {
                    new StratTerm { variable = "q", condition = StratTermVal.gt, constant = "10"},
                    new StratTerm { variable = "y", condition = StratTermVal.lte, constant = "30"},
                    new StratTerm { variable = "z", condition = StratTermVal.lte, constant = "20"}
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
            var f = new Finder();

            f.AddStataDef("s0",s0);

            f.RenderToConsole();

            var r0 = new string [] {
                    f.FindStrata(p0[0]),
                    f.FindStrata(p0[1]),
            };
            
            Debug.Assert(r0[0] == "s0", "strata s1 was not returned");
            Debug.Assert(r0[1] == null, "a null strata should have been returned");

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
        }
        public StratTerm Term { get; private set;}
        public StratTree Left { get; private set;}
        public StratTree Right { get; private set;}

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

        public void Insert( StratTree node)
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
    

    public class Finder 
    {
        private StratTree root = null;

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
            int cnt = terms.Length;
            if (cnt<1) return;//return with no change for empty strata conditions

            int start=0;
            if (root==null)
                root = new StratTree(name, terms[start++]);

            for(int i=start; i<cnt; i++)
            {
                var n = new StratTree(name, terms[i]);
                root.Insert(n);
            }
        }

        public void RenderToConsole()
        {
            if (root != null)
                root.Traverse(root,0);
        }
    }
}


