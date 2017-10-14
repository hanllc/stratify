using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tyler.Avm.Stratify
{

    class Program
    {
        static void Main(string[] args)
        {
            var s0 =
                new StratTerm[] {
                    new StratTerm { variable = "z", condition = StratTermVal.gt, constant = "50"},
                    new StratTerm { variable = "y", condition = StratTermVal.lte, constant = "20"},
                    new StratTerm { variable = "x", condition = StratTermVal.gt, constant = "10"}
                    
                };
            var s1 =
                new StratTerm[] {
                    new StratTerm { variable = "q", condition = StratTermVal.gt, constant = "10"},
                    new StratTerm { variable = "z", condition = StratTermVal.lte, constant = "30"},
                    new StratTerm { variable = "x", condition = StratTermVal.lte, constant = "20"}
                };

            var f = new Finder();

            f.AddStataDef("s0",s0);
            f.AddStataDef("s1",s1);

            f.Preprocess();
            f.RenderToText(Console.Out);
        }
        static void Main2(string[] args)
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
            var f0 = new BasicFinder();

            f0.AddStataDef("s0",s0);
            f0.Preprocess();
            f0.RenderToText(Console.Out);

            var r0 = new string [] {
                    f0.FindStrata(p0[0]),
                    f0.FindStrata(p0[1]),
            };
            
            Debug.Assert(r0[0] == "s0", "strata s1 was not returned");
            Debug.Assert(r0[1] == null, "a null strata should have been returned");

            var f1 = new BasicFinder();
            f1.AddStataDef("s1",s1);
            f1.Preprocess();
            f1.RenderToText(Console.Out);

            var r1 = new string [] {
                    f1.FindStrata(p1[0]),
                    f1.FindStrata(p1[1]),
            };

            Debug.Assert(r1[0] == "s1", "strata s1 was not returned");
            Debug.Assert(r1[1] == null, "a null strata should have been returned");

            var f2 = new BasicFinder();
            f2.AddStataDef("s2",s2);
            f2.Preprocess();
            f2.RenderToText(Console.Out);


            var r2 = new string [] {
                    f2.FindStrata(p2[0]),
                    f2.FindStrata(p2[1]),
            };


            return;
        }
    }

}
