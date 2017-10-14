using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tyler.Avm.Stratify
{
    
    public struct StratTermVal 
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
        
        public string StratRef { get { return refName; } }
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
        
        public void Traverse( StratTree node, int level, Action<StratTree> inFixAction)
        {
            if (node.Left !=null)
                Traverse(node.Left, level+1, inFixAction);

            inFixAction(node);

            if (node.Right != null) 
                Traverse(node.Right, level+1, inFixAction);
        }        
    }

}
