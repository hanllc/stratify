# influences
http://crsouza.com/2012/01/04/decision-trees-in-c/
https://stackoverflow.com/questions/9860207/build-a-simple-high-performance-tree-data-structure-in-c-sharp

tree learning
http://www.vincentlemaire-labs.fr/students/master_thesis_bassem_khouzam.pdf

# insertion logic cases
which way? how to change tree?
case
X < 10   :s1  
 X < 5    :s2 
 X < 5 implies X < 10 since 5 < 10 
so insert X<5 as right child

 X < 10 :s1
    R: X < 5 = s2
    L: X >= 5 = s1 (implicit?)


Y < 5    :s3 
Y < 10   :s4
Y<5 implies Y < 10 since 5<10
//so insert Y < 10 as left child
Y < 5    :s3 
R: Y >= 10  :s3 (implicit?)
L: Y < 10   :s4


Z < 5    :s5 
R: Q < 2    : s6
L: Q >= 2   : s5 (implicit?)

Z < 10   :s7
Z<5 implies Z<10 since 5<10
so insert Z<10 toward left
Z < 5    :s5
R: Q < 2   : s6 
L: Z < 10    : s7
R: s7
L: s5 
