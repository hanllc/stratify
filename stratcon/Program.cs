using System;
using System.Collections.Generic;

namespace stratcon
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Stratification Tree");

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

            var f = new Finder();
            f.AddStataDef("s1",s1);
        }
    }


    struct StratTermVal 
    {
        static public readonly string gt =">";
        static public readonly string lte ="<=";
        static public readonly string inlist ="IN";
    }

    public struct StratTerm 
    {
        public string variable;
        public string condition;
        public string constant;    
    }

    
    public class StratTree 
    {
        private string refName;
        private StratTerm term;
        private StratTree left; //false
        private StratTree right; //true

        public StratTree(string refname, StratTerm st)
        {
            refName = refname;
            term = st;
            left = null;
            right = null;
        }
        public StratTerm Term { get; }
        public StratTree Left { get; }
        public StratTree Right { get; }

//http://crsouza.com/2012/01/04/decision-trees-in-c/
//https://stackoverflow.com/questions/9860207/build-a-simple-high-performance-tree-data-structure-in-c-sharp
        public void Insert( StratTree node)
        {
            StratTree target = this;
            while(true) 
            {
                if(node.term.variable == target.term.variable)
                {
                    if (node.term.condition != target.term.condition)
                    {//same var, diff cond
                        if (target.right != null) 
                        {
                            target = target.right;
                            continue;
                        }
                        else
                        {
                            target.right = node;
                            break;
                        }
                    }
                    else
                    {
                        //same var, same cond
                        if (node.term.condition == StratTermVal.gt ||
                            node.term.condition == StratTermVal.lte)
                        {//numer constant
                            double nodec=0.0;
                            double targc=0.0;
                            try 
                            { 
                                nodec = Double.Parse(node.term.constant);
                                targc = Double.Parse(target.term.constant); 
                            }
                            catch 
                            {
                                break; //skip invalid - correctness requires aprior validation i.e no type op issues
                            }
                            //which way? how to change tree?
                            //case
                            // X < 10   :s1  
                            // X < 5    :s2 
                            // X < 5 implies X < 10 since 5 < 10 
                            //so insert X<5 as right child
                            // X < 10 :s1
                            //  R: X < 5 = s2
                            //  L: X >= 5 = s1 (implicit?)

                            // Y < 5    :s3 
                            // Y < 10   :s4
                            //Y<5 implies Y < 10 since 5<10
                            //so insert Y < 10 as left child
                            //    Y < 5    :s3 
                            //      R: Y >= 10  :s3 (implicit?)
                            //      L: Y < 10   :s4


                            // Z < 5    :s5 
                            //  R: Q < 2    : s6
                            //  L: Q >= 2   : s5 (implicit?)

                            // Z < 10   :s7
                            //Z<5 implies Z<10 since 5<10
                            //so insert Z<10 toward left
                            //    Z < 5    :s5
                            //      R: Q < 2   : s6 
                             
                            //      L: Z < 10    : s7
                            //          R: s7
                            //          L: s5 
                            if (target.right != null) 
                            {
                                target = target.right;
                                continue;
                            }
                            else
                            {
                                target.right = node;
                                break;
                            }
                        }
                        else
                        {//categ constant

                        }
                    }  
                }
                else
                {
                    //diff var
                    if (target.right != null) 
                    {
                        target = target.right;
                        continue;
                    }
                    else
                    {
                        target.right = node;
                        break;
                    }
                }
            }   
        }
    }

    public class Finder 
    {
        private StratTree root = null;

        public string FindStrata(Dictionary<string,string> parcelData)
        {
            return null;
        }
        public void AddStataDef(string name, StratTerm [] terms)
        {
            int cnt = terms.Length;
            if (root==null && cnt !=0)
            {
                root = new StratTree(name, terms[0]);
                for(int i=1; i<cnt; i++)
                {
                    var n = new StratTree(name, terms[i]);
                    root.Insert(n);
                }
            }             
        }
    }
}


